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
    private VisualInterest stunnedVisualInterest;

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

        // Cache other angles for the walkie talkie animation
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
            (float value) => walkieTalkieTransform.localPosition = walkieTalkiePath.point(value), 0f, 1f, 1.5f);
        LeanTween.rotateLocal(walkieTalkieTransform.gameObject, walkieGoalLocalAngles, 1.2f).setDelay(0.3f);

        LeanTween.value(walkieTalkieTransform.gameObject,
            (float value) => walkieTalkieTransform.localPosition = walkieTalkiePath.point(value), 1f, 0f, 1.5f).setDelay(2.5f);
        LeanTween.rotateLocal(walkieTalkieTransform.gameObject, walkieStartAngles, 1.2f).setDelay(2.5f);
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
        // If thief is going to attack back
        if(targetAgent.CanAttackBack(this))
        {
            // Decide if we win this struggle or the thief does
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
            // Mark everyone as interacting and disable agent logic to not interrupt attack
            AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.DeactivateAgent();
        }
        else // If thief can't attack back then we tackle them
        {
            // End any animations that may already be playing on thief
            // and end animation on any guard that may have been attacking the thief already
            targetAgent.EndAgentAnimation();
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
            // Mark everyone as interacting and disable agent logic to not interrupt attack
            AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.DeactivateAgent();
        }
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

    public void EnableStunnedSuspicionFocus()
    {
        stunnedVisualInterest.gameObject.SetActive(true);
    }

    public void DisableStunnedSuspicionFocus()
    {
        stunnedVisualInterest.gameObject.SetActive(false);
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