using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles senses for seeing and hearing, sensing visual/audio interests
/// </summary>
public abstract class SensoryModule : MonoBehaviour
{
    [SerializeField]
    protected LayerMask losMask;

    public LayerMask LosMask => losMask;

    [SerializeField]
    protected float losCheckInterval;

    [SerializeField]
    private VisionCone[] visionCones;

    private List<SenseInterest> inConeObjects = new();

    private Dictionary<SenseInterest, bool> visibilityMap = new();

    private Dictionary<SenseInterest, int> entryCountMap = new();

    private Dictionary<GameObject, bool> centralVisionMap = new();

    public delegate void SenseDelegate(SenseInterest interest);

    public event SenseDelegate EnemyFound;

    public event SenseDelegate EnemyLost;

    public event SenseDelegate VisualFound;

    public event SenseDelegate VisualLost;

    public event SenseDelegate SoundHeard;

    public const float VIEW_ANGLE = 75f;

    public const float INTEREST_RADIUS = 6f;

    protected Agent owner;

    protected float awareness;

    protected float losTimer;

    private float losCheckTimer;

    private LayerMask interestMask;

    protected virtual void Start()
    {
        interestMask = LayerMask.GetMask("Interest", "Thief", "Guard");

        owner = GetComponent<Agent>();
        foreach (VisionCone v in visionCones)
        {
            v.TriggerEnter += HandleVisionEnter;
            v.TriggerExit += HandleVisionExit;
        }
    }

    public bool IsSoundHeard(SenseInterest sound)
    {
        // We model sound being muffled by first checking if we'd be
        // beyond the sound radius were it 30% smaller, and if we are,
        // check if there's an object obstructing the sound
        float distanceToSound = Vector3.Distance(sound.transform.position.ZeroY(),
            owner.AgentView.AgentHeadRoot.position.ZeroY());

        if (distanceToSound > (sound as SoundInterest).TriggerRadius * 0.7f)
        {
            if (Physics.Linecast(sound.transform.position, owner.AgentView.AgentHeadRoot.position,
             losMask, QueryTriggerInteraction.Collide))
                return false;
        }

        return true;
    }

    public virtual void NotifySound(SenseInterest sound)
    {
        SoundHeard?.Invoke(sound);
    }

    /// <summary>
    /// Makes a line of sight check from the agent's eye point to the target position.
    /// Does not take into account whether the agent is facing the target
    /// </summary>
    public bool IsInLOS(Vector3 checkPosition)
    {
        return !Physics.Linecast(checkPosition,
            owner.AgentView.AgentEyeRoot.position, losMask, QueryTriggerInteraction.Collide);
    }

    /// <summary>
    /// Checks whether the target position is within the agent's viewing angle
    /// as well as doing a line of sight check from the eye point
    /// </summary>
    /// <param name="agentForward">Optional override for the agent's facing direction for the check</param>
    public bool IsSeen(Vector3 checkPosition, Vector3 agentForward = default)
    {
        if (agentForward == default)
            agentForward = owner.AgentView.AgentEyeRoot.forward;

        float lookAngle = Vector3.Angle(agentForward,
            (checkPosition.ZeroY() - transform.position.ZeroY()).normalized);

        if (lookAngle <= VIEW_ANGLE)
            if (!Physics.Linecast(checkPosition, owner.AgentView.AgentEyeRoot.position, losMask, QueryTriggerInteraction.Collide))
                return true;

        return false;
    }

    /// <summary>
    /// Whether an interest is detected to be in the centre of our vision rather than peripheral
    /// </summary>
    public bool IsInCentralVision(GameObject interest) => centralVisionMap.TryGetValue(interest, out bool value) ? value : false;

    public virtual void NotifyVisualFound(SenseInterest visual)
    {
        VisualFound?.Invoke(visual);
    }

    public virtual void NotifyVisualLost(SenseInterest visual)
    {
        VisualLost?.Invoke(visual);
    }

    /// <summary>
    /// Handles checking whether an object that has entered a vision cone is a VisualInterest and keeping track
    /// </summary>
    /// <param name="origin">Vision cone that detected the object</param>
    private void HandleVisionEnter(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out VisualInterest visualInterest))
            if (!inConeObjects.Contains(visualInterest))
            {
                // Initialise data about this object if it wasn't in cones already
                inConeObjects.Add(visualInterest);
                visibilityMap[visualInterest] = false;
                centralVisionMap[other] = origin.IsCentralVision;
                entryCountMap[visualInterest] = 1;
            }
            else
            {
                entryCountMap[visualInterest]++;
                if (origin.IsCentralVision)
                    centralVisionMap[other] = true;
            }
    }

    /// <summary>
    /// Handles checking whether an object that has exited a vision cone is a VisualInterest and keeping track
    /// </summary>
    /// <param name="origin">Vision cone that detected the object</param>
    private void HandleVisionExit(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out VisualInterest visualInterest))
            if (inConeObjects.Contains(visualInterest))
            {
                if (origin.IsCentralVision)
                    centralVisionMap[other] = false;

                if (--entryCountMap[visualInterest] == 0)
                {
                    if (visibilityMap[visualInterest])
                        NotifyVisualLost(visualInterest);

                    inConeObjects.Remove(visualInterest);
                    visibilityMap.Remove(visualInterest);
                    entryCountMap.Remove(visualInterest);
                }
            }
    }

    private void FixedUpdate()
    {
        if (!owner.AgentActivated)
            return;

        // Make checks for every visual interest within our vision cones
        // for whether they're visible via line of sight
        if (losCheckTimer <= 0f)
        {
            for (int i = 0; i < inConeObjects.Count; ++i)
            {
                SenseInterest target = inConeObjects[i];
                bool isSeen = IsInLOS(target.transform.position);

                if (!visibilityMap[target] && isSeen)
                    NotifyVisualFound(target);
                else if (visibilityMap[target] && !isSeen)
                    NotifyVisualLost(target);

                visibilityMap[target] = isSeen;
            }

            losCheckTimer = losCheckInterval;
        }
        else
            losCheckTimer -= Time.fixedDeltaTime;
    }

    /// <summary>
    /// Find nearby interest objects that have the supplied tag
    /// </summary>
    protected List<GameObject> FindNearbyInterests(string interestTag)
    {
        Collider[] nearbyInterests =
        Physics.OverlapSphere(
        transform.position,
        INTEREST_RADIUS,
        interestMask,
        QueryTriggerInteraction.Collide);

        List<GameObject> desiredInterests = new();
        foreach(Collider c in nearbyInterests)
        {
            // Only try to add things that have the right tag applied
            if (c.CompareTag(interestTag))
            {
                bool shouldAdd = true;
                GameObject addObject = c.gameObject;

                // Don't add ourselves to our interests
                // and don't add duplicates
                if (addObject == owner.gameObject || desiredInterests.Contains(addObject))
                    shouldAdd = false;
                else if(addObject.TryGetComponent<SenseInterest>(out var interest))
                {
                    if(interest.Owner != null)
                        addObject = interest.Owner;

                    // If we found an actual sense interest, don't add
                    // if it belongs to us and don't add a duplicate
                    if (addObject == owner.gameObject || desiredInterests.Contains(addObject))
                        shouldAdd = false;
                }

                if(shouldAdd)
                    desiredInterests.Add(addObject);
            }
        }
        return desiredInterests;
    }

    public bool StoreVisibleInterests(string interestTag, Blackboard targetBoard, string listKey)
    {
        var interests = FindNearbyInterests(interestTag);
        if(interests.Count > 0)
        {
            // Remove interests that we can't see
            for(int i = interests.Count - 1; i >= 0; --i)
            {
                if(!IsInLOS(interests[i].transform.position))
                {
                    interests.RemoveAt(i);
                }
            }

            var existingList = targetBoard.GetVariable<List<GameObject>>(listKey);
            if (existingList == null)
                targetBoard.SetVariable(listKey, interests);
            else
            {
                existingList.Clear();
                existingList.AddRange(interests);
            }
            return true;
        }
        return false;
    }
}