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

    [Header("Audio")]
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

    private Coroutine attackCoroutine;

    [Header("Agent Setup")]
    [SerializeField]
    private List<string> blackboardDefaults;

    public Blackboard AgentBlackboard { get; private set; }

    private List<Room> roomList = new();

    // We add the most recently entered room to the end of our list,
    // so our current room can always be gotten with the last entry
    public Room CurrentRoom => roomList.Count == 0 ? null : roomList[^1];

    public bool AgentActivated { get; protected set; }

    protected virtual void Start()
    {
        // Make a new blackboard for this agent
        AgentBlackboard = new Blackboard();

        // Every default value is a string where we
        // have the key name and then its value,
        // separated by a comma
        foreach (string s in blackboardDefaults)
        {
            string[] splitResult = s.Split(',', 2);
            string left = splitResult[0].Trim();
            string right = splitResult[1].Trim();

            if (int.TryParse(right, out int newInt))
                AgentBlackboard.SetVariable(left, newInt);
            else
            if (float.TryParse(right, out float newFloat))
                AgentBlackboard.SetVariable(left, newFloat);
            else
            if (bool.TryParse(right, out bool newBool))
                AgentBlackboard.SetVariable(left, newBool);
            else
            if (Consts.ParseVector3(right, out Vector3 newVector))
                AgentBlackboard.SetVariable(left, newVector);
            else
                AgentBlackboard.SetVariable(left, right); // Fallback sets the value as a string
        }

        // Add our GameObject to our blackboard so we
        // can be addressed by variable accessors
        AgentBlackboard.SetVariable("owner", gameObject);
    }

    public virtual void ActivateAgent()
    {
        AgentActivated = true;
    }

    public void DeactivateAgent()
    {
        AgentActivated = false;
        if(navAgent.hasPath)
            navAgent.ResetPath();
    }

    /// <summary>
    /// Set an agent to move to a specified position
    /// </summary>
    public void MoveAgent(Vector3 newPosition)
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
    /// Set an agent to move along a NavMesh path
    /// </summary>
    public void MoveAgent(NavMeshPath newPath)
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
        while(Vector3.Distance(transform.position, navAgent.destination) > 1f &&
            !AgentBlackboard.GetVariable<bool>(Consts.AGENT_INTERACT_STATUS))
        {
            walkingSound.PlaySound();
            yield return new WaitForSeconds(walkSoundInterval);
        }

        walkingCoroutine = null;
        yield break;
    }

    /// <summary>
    /// Rotate the agent's head model to look towards the supplied point over time.
    /// Head angle will be clamped to not rotate too far
    /// </summary>
    public void TurnHeadToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle(
            (targetPoint - AgentView.AgentHeadRoot.position).normalized,
            AgentView.AgentRoot.forward,
            Vector3.up);
        TurnHead(lookAngle, time);
    }

    /// <summary>
    /// Rotate head to the supplied angle over time. Head angle will be clamped to not rotate too far
    /// </summary>
    public void TurnHead(float turnAngle, float time)
    {
        LeanTween.cancel(AgentView.AgentHeadRoot.gameObject);
        // Adjust our turning angle to be clamped so the head doesn't turn all the way around like an owl
        turnAngle = Mathf.Clamp(turnAngle, -100f, 100f);

        LeanTween.rotateLocal(AgentView.AgentHeadRoot.gameObject, new Vector3(0f, 0f, turnAngle), time);
    }

    /// <summary>
    /// Rotate the whole agent to look towards the supplied point over time
    /// </summary>
    public void TurnBodyToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle(
            AgentView.AgentRoot.forward,
            (targetPoint - AgentView.AgentRoot.position).normalized,
            Vector3.up);
        TurnBody(lookAngle, time);
    }

    /// <summary>
    /// Rotate the whole agent by adding the supplied angle over time
    /// </summary>
    public void TurnBody(float turnAngle, float time)
    {
        LeanTween.cancel(AgentView.AgentRoot.gameObject);
        LeanTween.rotateAroundLocal(AgentView.AgentRoot.gameObject, Vector3.up, turnAngle, time);
    }

    public bool IsTweeningHead() => LeanTween.isTweening(AgentView.AgentHeadRoot.gameObject);

    public bool IsInChase()
    {
        return AgentBlackboard.GetVariable<string>(Consts.GUARD_MODE_STATUS) == Consts.GUARD_CHASE_MODE
           || AgentBlackboard.GetVariable<bool>(Consts.THIEF_CHASE_STATUS);
    }

    #region ATTACKING
    /// <summary>
    /// Whether this agent is allowed to attack another agent
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAttackEnemy()
    {
        return !AgentBlackboard.GetVariable<bool>(Consts.AGENT_INTERACT_STATUS);
    }

    /// <summary>
    /// Run logic for attacking the supplied agent
    /// </summary>
    public abstract void AttackAgent(Agent targetAgent);

    /// <summary>
    /// Whether this agent is allowed to fight back against the supplied agent who is attacking them
    /// </summary>
    public abstract bool CanAttackBack(Agent attacker);

    /// <summary>
    /// Whether this agent can win a struggle with another agent
    /// </summary>
    public abstract bool CanWinStruggle();

    public void PlayStruggleSequence(bool isWinner)
    {
        attackCoroutine = StartCoroutine(EnumerateStruggleSequence(isWinner));
    }

    public void PlayTackleSequence(bool isWinner)
    {
        attackCoroutine = StartCoroutine(EnumerateTackleSequence(isWinner));
    }

    /// <summary>
    /// Ends and resets any currently running animations on this agent
    /// </summary>
    public virtual void EndAgentAnimation()
    {
        LeanTween.cancel(AgentView.AgentRoot.gameObject);
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        TurnHead(0f, 0f);
    }

    /// <summary>
    /// Set up two agents for the start of an attack animation.
    /// Ends movement, animations etc. and makes them look at each other
    /// </summary>
    protected void SetupAttack(Agent a, Agent b)
    {
        // End any current animations on the agents first
        a.EndAgentAnimation();
        b.EndAgentAnimation();

        // Immediately stop the thief and guard from moving, make them look at each other
        // and stop the NavAgent components from moving
        // or rotating the agents during their animated interaction
        a.transform.LookAt(b.transform, Vector3.up);
        a.navAgent.ResetPath();
        a.navAgent.updatePosition = false;
        a.navAgent.updateRotation = false;
        a.navAgent.updateUpAxis = false;
        a.navAgent.velocity = Vector3.zero;
        a.AgentBlackboard.SetVariable("attackingAgent", b);

        b.transform.LookAt(a.transform, Vector3.up);
        b.navAgent.ResetPath();
        b.navAgent.updatePosition = false;
        b.navAgent.updateRotation = false;
        b.navAgent.updateUpAxis = false;
        b.navAgent.velocity = Vector3.zero;
        b.AgentBlackboard.SetVariable("attackingAgent", a);

        // Push our agents together because they might be a
        // bit far from each other when the animation starts
        Vector3 toA = (a.transform.position - b.transform.position);
        Vector3 toB = (b.transform.position - a.transform.position);
        Vector3 midPoint = a.transform.position + toB / 2f;
        a.transform.position = midPoint + toA.normalized * 0.4f;
        b.transform.position = midPoint + toB.normalized * 0.4f;
    }
    #endregion

    #region STRUGGLE ANIMATION
    private IEnumerator EnumerateStruggleSequence(bool isWinner)
    {
        PlayStrugglingAnimation(!isWinner);
        yield return new WaitForSeconds(3f);
        PlayStruggleOutcomeAnimation(isWinner);
        yield return new WaitForSeconds(1f);
        AgentBlackboard.SetVariable(Consts.AGENT_INTERACT_STATUS, false);
        if (!isWinner)
        {
            AgentBlackboard.SetVariable(Consts.AGENT_STUN_STATUS, true);
            GameEventLog.Log($"{name} lost the struggle!");
        }
        else
            ActivateAgent();
        attackCoroutine = null;
    }

    private void PlayStrugglingAnimation(bool beingAttacked)
    {
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, beingAttacked ? 15f : -15f, 0.3f);
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, beingAttacked ? -15f : 15f, 0.6f)
            .setFrom(beingAttacked ? 15f : -15f).setDelay(0.3f).setLoopPingPong(2);
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
        if(AgentView.AgentRoot.eulerAngles.x != 0f)
            LeanTween.rotateX(AgentView.AgentRoot.gameObject, 0f, 1f);
    }
    #endregion

    #region TACKLE ANIMATION
    private IEnumerator EnumerateTackleSequence(bool isWinner)
    {
        PlayTackleAnimation(isWinner);
        yield return new WaitForSeconds(1.6f);
        AgentBlackboard.SetVariable(Consts.AGENT_INTERACT_STATUS, false);
        if (!isWinner)
        {
            AgentBlackboard.SetVariable(Consts.AGENT_STUN_STATUS, true);
            GameEventLog.Log($"{name} was tackled to the floor!");
        }
        else
            ActivateAgent();
        attackCoroutine = null;
    }

    private void PlayTackleAnimation(bool isWinner)
    {
        LeanTween.rotateX(AgentView.AgentRoot.gameObject, isWinner ? 70f : 90f, 1.5f).setEaseInElastic();
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
            if (other.TryGetComponent(out Room room))
                roomList.Add(room);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
            if (other.TryGetComponent(out Room room))
                roomList.Remove(room);
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
}