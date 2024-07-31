using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GuardAgent : Agent
{
    [SerializeField]
    private List<GameObject> breakroomMarkerList;

    [SerializeField]
    private List<GameObject> toiletMarkerList;

    [SerializeField]
    private BTGraph behaviourTreeGraph;

    [SerializeField]
    private float treeUpdateInterval;

    [SerializeField]
    private PatrolPath regularPatrol;

    [SerializeField]
    private PatrolPath perimeterPatrol;

    [SerializeField]
    private Transform walkieTalkieTransform;

    [SerializeField]
    private Transform walkieTalkieUseTransform;

    public GuardSensoryModule GuardSenses => (GuardSensoryModule)senses;

    private BehaviourTree agentTree;

    public BehaviourTree BehaviourTree => agentTree;

    private Transform targetPoint;

    private int patrolIndex;

    private float treeUpdateTimer;

    public SuspicionModule Suspicion { get; private set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        AgentBlackboard.SetVariable("breakroomList", breakroomMarkerList);
        AgentBlackboard.SetVariable("toiletList", toiletMarkerList);

        if (TryGetComponent(out SuspicionModule susModule))
            Suspicion = susModule;

        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);
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

    public Vector3 GetNextPatrolPoint(Consts.PatrolPathType pathType)
    {
        PatrolPath path = pathType == Consts.PatrolPathType.Regular ? regularPatrol : perimeterPatrol;
        Vector3 point = path.GetNextPointFromPosition(transform.position);
        return point;
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