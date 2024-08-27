using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;

public abstract class Agent : MonoBehaviour
{
    [SerializeField]
    protected NavMeshAgent navAgent;

    public NavMeshAgent NavAgent => navAgent;

    [SerializeField]
    protected AgentView agentView;

    public AgentView AgentView => agentView;

    [SerializeField]
    protected SensoryModule senses;

    public SensoryModule Senses => senses;

    [SerializeField]
    [Tooltip("Distance agent must be within to aggro on enemy")]
    protected float aggroRadius = 1f;
    public float AggroRadius => aggroRadius;
    [SerializeField]
    [Tooltip("Vision angle of agent to attack enemy")]
    protected float aggroAngle = 15f;
    public float AggroAngle => aggroAngle;

    [SerializeField]
    private SoundInterest walkingSound;

    [SerializeField]
    private float walkSoundInterval;

    [SerializeField]
    private SoundInterest doorOpenSound;

    public SoundInterest DoorOpenSound => doorOpenSound;

    [SerializeField]
    private SoundInterest doorCloseSound;

    public SoundInterest DoorCloseSound => doorCloseSound;

    [SerializeField]
    private SoundInterest doorSlamSound;

    public SoundInterest DoorSlamSound => doorSlamSound;

    private Coroutine walkingCoroutine;

    [SerializeField]
    private List<string> blackboardDefaults;

    public Blackboard AgentBlackboard { get; private set; }

    private List<Room> roomList = new();

    // Get the room at the end of the list if it's not empty
    public Room CurrentRoom => roomList.Count == 0 ? null : roomList[^1];

    public bool AgentActivated { get; protected set; }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Make a new blackboard for this agent
        AgentBlackboard = new Blackboard();

        // Set our default blackboard values by parsing the strings for their keys and values
        foreach (string s in blackboardDefaults)
        {
            string[] splitResult = s.Split(',', 2);
            string left = splitResult[0].Trim();
            string right = splitResult[1].Trim();

            // Parse normally for our primitives
            if (int.TryParse(right, out int newInt))
                AgentBlackboard.SetVariable(left, newInt);
            else
            if (float.TryParse(right, out float newFloat))
                AgentBlackboard.SetVariable(left, newFloat);
            else
            if (bool.TryParse(right, out bool newBool))
                AgentBlackboard.SetVariable(left, newBool);
            else
            if (right.Contains("Vector3"))
            {
                int leftBracketIndex = right.IndexOf('(') + 1;
                int rightBracketIndex = right.IndexOf(')');
                string vectorValueString = right[leftBracketIndex..rightBracketIndex];
                string[] vectorSplit = vectorValueString.Split(',');
                float[] vectorComponents = new float[3];
                for (int i = 0; i < Mathf.Min(vectorSplit.Length, 3); ++i)
                {
                    if (float.TryParse(vectorSplit[i].Trim(), out float vecComponent))
                        vectorComponents[i] = vecComponent;
                }
                AgentBlackboard.SetVariable(left,
                new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]));
            }
            else
                AgentBlackboard.SetVariable(left, right);
        }
    }

    public virtual void ActivateAgent()
    {
        AgentActivated = true;
    }

    public void DeactivateAgent()
    {
        AgentActivated = false;
    }

    /// <summary>
    /// Set an agent to move to a specified position
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="updatePositionOnly"></param>
    public void MoveAgent(Vector3 newPosition, bool updatePositionOnly = false)
    {
        navAgent.updatePosition = true;
        navAgent.updateRotation = true;
        navAgent.updateUpAxis = true;

        navAgent.SetDestination(newPosition);
        LeanTween.cancel(AgentView.AgentRoot.gameObject);

        if (walkingSound == null)
            return;

        if(walkingCoroutine == null)
            walkingCoroutine = StartCoroutine(MakeWalkingSound());
    }

    /// <summary>
    /// Set an agent to move along a path
    /// </summary>
    /// <param name="newPath"></param>
    /// <param name="updatePositionOnly"></param>
    public void MoveAgent(NavMeshPath newPath, bool updatePositionOnly = false)
    {
        navAgent.updatePosition = true;
        navAgent.updateRotation = true;
        navAgent.updateUpAxis = true;

        navAgent.SetPath(newPath);
        LeanTween.cancel(AgentView.AgentRoot.gameObject);

        if (walkingSound == null)
            return;

        if (walkingCoroutine == null)
            walkingCoroutine = StartCoroutine(MakeWalkingSound());
    }

    private IEnumerator MakeWalkingSound()
    {
        // Make the walking sound play as long as we're travelling to our destination
        while(Vector3.Distance(transform.position, navAgent.destination) > 1f)
        {
            walkingSound.PlaySound();
            yield return new WaitForSeconds(walkSoundInterval);
        }

        walkingCoroutine = null;
        yield break;
    }

    public void TurnHeadToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle((targetPoint - AgentView.AgentHeadRoot.position).normalized, AgentView.AgentRoot.forward, Vector3.up);
        TurnHead(lookAngle, time);
    }

    public void TurnHead(float turnAngle, float time)
    {
        LeanTween.cancel(AgentView.AgentHeadRoot.gameObject);
        // Adjust our turning angle to be clamped so the head doesn't turn all the way around like an owl
        turnAngle = Mathf.Clamp(turnAngle, -100f, 100f);

        LeanTween.rotateLocal(AgentView.AgentHeadRoot.gameObject, new Vector3(0f, 0f, turnAngle), time);
    }

    public void TurnBodyToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle(AgentView.AgentRoot.forward, (targetPoint - AgentView.AgentRoot.position).normalized, Vector3.up);
        TurnBody(lookAngle, time);
    }

    public void TurnBody(float turnAngle, float time)
    {
        LeanTween.cancel(AgentView.AgentRoot.gameObject);
        LeanTween.rotateAroundLocal(AgentView.AgentRoot.gameObject, Vector3.up, turnAngle, time);
    }

    public bool IsTweeningHead() => LeanTween.isTweening(AgentView.AgentHeadRoot.gameObject);

    public abstract void AttackAgent(Agent targetAgent);

    public abstract bool CanAttackBack(Agent attacker);

    public abstract bool CanWinStruggle();

    public void PlayStruggleSequence(bool isWinner)
    {
        StartCoroutine(EnumerateStruggleSequence(isWinner));
    }

    protected void SetupAttack(Agent a, Agent b)
    {
        // Immediately stop the thief and guard from moving, make them look at each other
        // and stop the NavAgent components from moving
        // or rotating the agents during their animated interaction
        a.transform.LookAt(b.transform, Vector3.up);
        a.navAgent.ResetPath();
        a.navAgent.updatePosition = false;
        a.navAgent.updateRotation = false;
        a.navAgent.updateUpAxis = false;
        a.navAgent.velocity = Vector3.zero;

        b.transform.LookAt(a.transform, Vector3.up);
        b.navAgent.ResetPath();
        b.navAgent.updatePosition = false;
        b.navAgent.updateRotation = false;
        b.navAgent.updateUpAxis = false;
        b.navAgent.velocity = Vector3.zero;
    }

    private IEnumerator EnumerateStruggleSequence(bool isWinner)
    {
        PlayStrugglingAnimation(!isWinner);
        yield return new WaitForSeconds(3f);
        PlayStruggleOutcomeAnimation(isWinner);
        yield return new WaitForSeconds(1f);
        AgentBlackboard.SetVariable("isInteracting", false);
        if (!isWinner)
            AgentBlackboard.SetVariable("isStunned", true);
        else
            ActivateAgent();
    }

    private void PlayStrugglingAnimation(bool beingAttacked)
    {
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, beingAttacked ? 15f : -15f, 0.3f);
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, beingAttacked ? -15f : 15f, 0.6f).setFrom(beingAttacked ? 15f : -15f).setDelay(0.3f).setLoopPingPong(2);
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, 0f, 0.3f).setFrom(beingAttacked ? 15f : -15f).setDelay(2.7f);
    }

    private void PlayStruggleOutcomeAnimation(bool isWinner)
    {
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, isWinner ? 40f : -90f, isWinner ? 0.2f : 0.4f);
        if(isWinner)
            LeanTween.rotateX(AgentView.AgentRoot.gameObject, 0f, 0.5f).setDelay(0.4f);
    }

    public void PlayWakeupAnimation()
    {
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, 0f, 2f);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (navAgent.hasPath && !navAgent.isPathStale)
        {
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; ++i)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
            if(other.TryGetComponent(out Room room))
                roomList.Add(room);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
            if (other.TryGetComponent(out Room room))
                roomList.Remove(room);
    }
}