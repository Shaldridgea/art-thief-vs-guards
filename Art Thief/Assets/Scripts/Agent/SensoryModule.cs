using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float losCheckTimer;

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

    protected virtual void Start()
    {
        owner = GetComponent<Agent>();
        foreach (VisionCone v in visionCones)
        {
            v.TriggerEnter += HandleVisionEnter;
            v.TriggerExit += HandleVisionExit;
        }
    }

    public bool IsSoundHeard(SenseInterest sound)
    {
        // If there's no obstruction then we heard the sound normally
        if (!Physics.Linecast(sound.transform.position, owner.AgentView.AgentHeadRoot.position,
            losMask, QueryTriggerInteraction.Collide))
            return true;

        // If there was an obstruction we model sound being muffled by checking
        // if we'd still be in the sound radius were it 30% smaller
        float distanceToSound = Vector3.Distance(sound.transform.position, owner.AgentView.AgentHeadRoot.position);
        if (distanceToSound <= (sound as SoundInterest).TriggerRadius * 0.7f)
            return true;

        return false;
    }

    public virtual void NotifySound(SenseInterest sound)
    {
        SoundHeard?.Invoke(sound);
    }

    public bool IsInLOS(Vector3 checkPosition)
    {
        return !Physics.Linecast(checkPosition,
            owner.AgentView.AgentEyeRoot.position,losMask, QueryTriggerInteraction.Collide);
    }

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

    public bool IsInCentralVision(GameObject interest) => centralVisionMap.TryGetValue(interest, out bool value) ? value : false;

    public virtual void NotifyVisualFound(SenseInterest visual)
    {
        VisualFound?.Invoke(visual);
    }

    public virtual void NotifyVisualLost(SenseInterest visual)
    {
        VisualLost?.Invoke(visual);
    }

    private void HandleVisionEnter(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out VisualInterest visualInterest))
            if (!inConeObjects.Contains(visualInterest))
            {
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
        if (losCheckTimer <= 0f)
        {
            for (int i = 0; i < inConeObjects.Count; ++i)
            {
                SenseInterest target = inConeObjects[i];
                bool isSeen = !Physics.Linecast(owner.AgentView.AgentEyeRoot.position, target.transform.position,
                    losMask.value, QueryTriggerInteraction.Collide);

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

    protected List<GameObject> FindNearbyInterests(string interestTag)
    {
        Collider[] nearbyInterests =
        Physics.OverlapSphere(
        transform.position,
        INTEREST_RADIUS,
        LayerMask.GetMask("Interest", "Thief", "Guard"),
        QueryTriggerInteraction.Collide);

        List<GameObject> desiredInterests = new();
        foreach(Collider c in nearbyInterests)
        {
            if (c.CompareTag(interestTag) && c.gameObject != owner.gameObject)
                desiredInterests.Add(c.gameObject);
        }
        return desiredInterests;
    }

    public bool StoreNearbyInterests(string interestTag, Blackboard targetBoard, string listKey)
    {
        var interests = FindNearbyInterests(interestTag);
        if(interests.Count > 0)
        {
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