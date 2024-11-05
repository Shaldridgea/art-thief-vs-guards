using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GuardAgent : Agent
{
    [SerializeField]
    private BTGraph behaviourTreeGraph;

    [SerializeField]
    private float treeUpdateInterval;

    [Header("Guard Data")]
    [SerializeField]
    private PatrolPath regularPatrol;

    [SerializeField]
    private PatrolPath perimeterPatrol;

    [SerializeField]
    private float sprintSpeedIncrease = 0.8f;

    [SerializeField]
    [Tooltip("How long the sprint speed takes to decay back to default speed")]
    private float sprintDecayTime = 5f;

    [SerializeField]
    [Tooltip("How long until sprint speed starts to decay")]
    private float sprintDecayDelay = 5f;

    [SerializeField]
    private VisualInterest suspicionVisualFocus;

    [SerializeField]
    private Transform walkieTalkieTransform;

    [SerializeField]
    private Transform walkieTalkieGoalTransform;

    [SerializeField]
    private GameObject torchLight;

    public GuardSensoryModule GuardSenses => (GuardSensoryModule)senses;

    public SuspicionModule Suspicion { get; private set; }

    private BehaviourTree agentTree;

    public BehaviourTree BehaviourTree => agentTree;

    private float treeUpdateTimer;

    private bool hasFoughtThief;

    private float defaultAgentSpeed;

    private Vector3 walkieStartPosition;

    Vector3 walkieStartAngles;

    Vector3 walkieGoalLocalAngles;

    private LTBezierPath walkieTalkiePath;

    protected override void Start()
    {
        base.Start();

        if (TryGetComponent(out SuspicionModule susModule))
            Suspicion = susModule;

        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);

        // Deactivate our torch collision at the start so we don't have
        // issues if this agent is turned on and off when setting
        // up the simulation, since OnTriggerExit isn't called
        // when disabling a collider
        torchLight.SetActive(false);

        defaultAgentSpeed = navAgent.speed;

        // Create spline points moving the walkie talkie in local space up to its goal position
        Vector3 localAgentForward = walkieTalkieTransform.parent.InverseTransformDirection(transform.forward);
        Vector3[] points = new Vector3[]
        {
            walkieTalkieTransform.localPosition,
            walkieTalkieTransform.localPosition + localAgentForward * 0.25f,
            walkieTalkieTransform.localPosition + localAgentForward * 0.35f + walkieTalkieGoalTransform.up * 0.2f,
            walkieTalkieTransform.parent.InverseTransformPoint(walkieTalkieGoalTransform.position)
        };

        walkieTalkiePath = new LTBezierPath(points);

        // Cache starting angles for the walkie talkie animation
        walkieStartAngles = walkieTalkieTransform.localEulerAngles;
        walkieStartPosition = walkieTalkieTransform.localPosition;

        Vector3 localGoalForward = walkieTalkieTransform.parent.InverseTransformDirection(walkieTalkieGoalTransform.forward);
        walkieGoalLocalAngles = Quaternion.LookRotation(localGoalForward).eulerAngles;
    }

    private void Update()
    {
        if (!AgentActivated)
            return;

        // Update our behaviour tree
        treeUpdateTimer -= Time.deltaTime;
        if (treeUpdateTimer <= 0f)
        {
            treeUpdateTimer = treeUpdateInterval;
            agentTree.Update();
            return;
        }
    }

    public override void ActivateAgent()
    {
        base.ActivateAgent();
        torchLight.SetActive(true);
    }

    public PatrolPath GetPatrol(Consts.PatrolPathType pathType) =>
        pathType == Consts.PatrolPathType.Regular ? regularPatrol : perimeterPatrol;

    public void StartChaseSprint()
    {
        navAgent.speed += sprintSpeedIncrease;
        // When starting a tween we can attach it to a specific GameObject for later
        // cancelling it (generally a good idea). We attach this one to the body root
        // because it is unused for any other tweens, so we can cancel this one safely
        LeanTween.value(agentView.AgentBodyRoot.gameObject, navAgent.speed, defaultAgentSpeed, sprintDecayTime)
            .setOnUpdate((f) => navAgent.speed = f).setDelay(sprintDecayDelay);
    }

    public void CancelChaseSprint()
    {
        LeanTween.cancel(agentView.AgentBodyRoot.gameObject);
        navAgent.speed = defaultAgentSpeed;
    }

    #region ANIMATION
    /// <summary>
    /// Animate the walkie talkie moving up to the guard's face and back down to the belt
    /// </summary>
    public void PlayReportAnimation()
    {
        LeanTween.value(walkieTalkieTransform.gameObject,
            (float value) => walkieTalkieTransform.localPosition = walkieTalkiePath.point(value), 0f, 1f, 1f);
        LeanTween.rotateLocal(walkieTalkieTransform.gameObject, walkieGoalLocalAngles, 0.7f).setDelay(0.3f);

        LeanTween.value(walkieTalkieTransform.gameObject,
            (float value) => walkieTalkieTransform.localPosition = walkieTalkiePath.point(value), 1f, 0f, 1f).setDelay(2f);
        LeanTween.rotateLocal(walkieTalkieTransform.gameObject, walkieStartAngles, 0.7f).setDelay(2f);
        GameEventLog.Log($"{name} is reporting in about the thief!");
    }

    public void EndReportAnimation()
    {
        LeanTween.cancel(walkieTalkieTransform.gameObject);
        walkieTalkieTransform.localEulerAngles = walkieStartAngles;
        walkieTalkieTransform.localPosition = walkieStartPosition;
    }

    public override void EndAgentAnimation()
    {
        EndReportAnimation();
        base.EndAgentAnimation();
        PlayWakeupAnimation();
    }
    #endregion

    #region ATTACKING
    public override bool CanAttackEnemy()
    {
        if (!base.CanAttackEnemy())
            return false;

        ThiefAgent thief = Level.Instance.Thief;

        if (thief.AgentBlackboard.GetVariable<bool>("isCaught"))
            return false;

        if (Vector3.Distance(transform.position, thief.transform.position) <= aggroRadius)
        {
            if(Vector3.Angle(transform.forward, (thief.transform.position - transform.position).normalized) <= aggroAngle)
            {
                return true;
            }
        }

        return false;
    }

    public override void AttackAgent(Agent targetAgent)
    {
        if(targetAgent.CanAttackBack(this))
        {
            bool winnerIsMe = false;
            if(CanWinStruggle())
            {
                if (!targetAgent.CanWinStruggle())
                    winnerIsMe = true;
                else if (Random.value <= 0.5f)
                    winnerIsMe = true;
            }

            // Start animated fight interaction between thief and guard
            SetupAttack(this, targetAgent);
            PlayStruggleSequence(winnerIsMe);
            targetAgent.PlayStruggleSequence(!winnerIsMe);
            // Mark everyone as interacting, which is used elsewhere to avoid
            // interrupting whatever is going on
            AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.DeactivateAgent();
        }
        else
        {
            // If a guard was already attacking the thief,
            // end the animations on that guard
            Agent attacker = targetAgent.AgentBlackboard.GetVariable<Agent>("attackingAgent");
            if (attacker != null)
                attacker.EndAgentAnimation();

            targetAgent.AgentBlackboard.SetVariable("isCaught", true);
            // Start animated fight interaction between thief and guard
            SetupAttack(this, targetAgent);
            // Turn thief around to face away from the guard for tackle
            targetAgent.transform.Rotate(Vector3.up, 180f);
            PlayTackleSequence(true);
            targetAgent.PlayTackleSequence(false);
            // Mark everyone as interacting, which is used elsewhere to avoid
            // interrupting whatever is going on
            AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.DeactivateAgent();
        }
        GameEventLog.Log($"{name} started attacking the thief!");
    }

    public override bool CanAttackBack(Agent attacker)
    {
        return true;
    }

    public override bool CanWinStruggle()
    {
        return hasFoughtThief;
    }
    #endregion

    public void EnableSuspicionFocus()
    {
        suspicionVisualFocus.SetSuspicious(true);
    }

    public void DisableSuspicionFocus()
    {
        suspicionVisualFocus.SetSuspicious(false);
    }

    [Button("Test head turn", EButtonEnableMode.Playmode)]
    private void TestHeadTurn()
    {
        TurnHeadToPoint(regularPatrol.GetPoint(0), 2f);
    }

    [Button("Test body turn", EButtonEnableMode.Playmode)]
    private void TestBodyTurn()
    {
        TurnBodyToPoint(regularPatrol.GetPoint(0), 2f);
    }

    [Button("Test report animation", EButtonEnableMode.Playmode)]
    private void TestReport()
    {
        PlayReportAnimation();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Color gizColor = Color.yellow;
        gizColor.a = 0.25f;
        Gizmos.color = gizColor;
        Gizmos.DrawSphere(transform.position, SensoryModule.INTEREST_RADIUS);
    }
}