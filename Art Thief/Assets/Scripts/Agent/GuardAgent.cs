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

    [SerializeField]
    private PatrolPath regularPatrol;

    [SerializeField]
    private PatrolPath perimeterPatrol;

    [SerializeField]
    private VisualInterest stunnedVisualInterest;

    [SerializeField]
    private Transform walkieTalkieTransform;

    [SerializeField]
    private Transform walkieTalkieUseTransform;

    [SerializeField]
    private GameObject torchLight;

    public GuardSensoryModule GuardSenses => (GuardSensoryModule)senses;

    private BehaviourTree agentTree;

    public BehaviourTree BehaviourTree => agentTree;

    private float treeUpdateTimer;

    public SuspicionModule Suspicion { get; private set; }

    private bool hasFoughtThief;

    private Vector3 walkieLocalAngles;

    private Vector3 walkieLocalPosition;

    private float agentSpeed;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        GameController.Instance.GlobalBlackboard.SetVariable("breakroomList", Level.Instance.BreakroomMarkerList);
        GameController.Instance.GlobalBlackboard.SetVariable("toiletList", Level.Instance.ToiletMarkerList);

        if (TryGetComponent(out SuspicionModule susModule))
            Suspicion = susModule;

        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);

        torchLight.SetActive(false);

        walkieLocalAngles = walkieTalkieTransform.localEulerAngles;
        walkieLocalPosition = walkieTalkieTransform.localPosition;

        agentSpeed = navAgent.speed;
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
        navAgent.speed += 0.8f;
        LeanTween.value(agentView.AgentBodyRoot.gameObject, navAgent.speed, agentSpeed, 5f)
            .setOnUpdate((f) => navAgent.speed = f).setDelay(5f);
    }

    public void CancelChaseSprint()
    {
        LeanTween.cancel(agentView.AgentBodyRoot.gameObject);
        navAgent.speed = agentSpeed;
    }

    public void PlayReportAnimation()
    {
        LTBezierPath walkieTalkiePath = new LTBezierPath(new Vector3[]{
            walkieTalkieTransform.position,
            walkieTalkieTransform.position + transform.forward * 0.2f,
            walkieTalkieTransform.position + transform.forward * 0.3f + walkieTalkieUseTransform.up * 0.2f,
            walkieTalkieUseTransform.position });

        Vector3 walkieStartAngles = walkieTalkieTransform.eulerAngles;
        LeanTween.value(walkieTalkieTransform.gameObject,
            (float value) => walkieTalkieTransform.position = walkieTalkiePath.point(value), 0f, 1f, 1.5f);
        LeanTween.rotate(walkieTalkieTransform.gameObject, walkieTalkieUseTransform.eulerAngles, 1.5f);

        LeanTween.value(walkieTalkieTransform.gameObject,
            (float value) => walkieTalkieTransform.position = walkieTalkiePath.point(value), 1f, 0f, 1.5f).setDelay(2.5f);
        LeanTween.rotate(walkieTalkieTransform.gameObject, walkieStartAngles, 1.5f).setDelay(2.5f);
    }

    public void EndReportAnimation()
    {
        LeanTween.cancel(walkieTalkieTransform.gameObject);
        walkieTalkieTransform.localEulerAngles = walkieLocalAngles;
        walkieTalkieTransform.localPosition = walkieLocalPosition;
    }

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
            // Mark everyone as interacting and unable to interrupt attack
            AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.AgentBlackboard.SetVariable("isInteracting", true);
            targetAgent.DeactivateAgent();
        }
        else
        {
            targetAgent.EndAgentAnimation();
            Agent attacker = targetAgent.AgentBlackboard.GetVariable<Agent>("attackingAgent");
            if (attacker != null)
                attacker.EndAgentAnimation();
            targetAgent.AgentBlackboard.SetVariable("isCaught", true);
            // Start animated fight interaction between thief and guard
            SetupAttack(this, targetAgent);
            targetAgent.transform.Rotate(Vector3.up, 180f);
            PlayTackleSequence(true);
            targetAgent.PlayTackleSequence(false);
            // Mark everyone as interacting and unable to interrupt attack
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

    public override void EndAgentAnimation()
    {
        EndReportAnimation();
        base.EndAgentAnimation();
        PlayWakeupAnimation();
    }

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