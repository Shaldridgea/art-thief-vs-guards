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

    public GuardSensoryModule GuardSenses => (GuardSensoryModule)senses;

    private BehaviourTree agentTree;

    public BehaviourTree BehaviourTree => agentTree;

    private Transform targetPoint;

    private int patrolIndex;

    private float treeUpdateTimer;

    public SuspicionModule Suspicion { get; private set; }

    static GuardAgent debuggingAgent;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (TryGetComponent(out SuspicionModule susModule))
            Suspicion = susModule;

        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);
    }

    private void Update()
    {
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

    private void OnMouseDown()
    {
        debuggingAgent = this;
    }

    private void OnGUI()
    {
        if (debuggingAgent != this)
            return;

        GUIStyle style = new GUIStyle("box");
        style.fontSize = 20;
        GUILayout.Box(name, style);
        style.fontSize = 15;
        foreach (var i in AgentBlackboard.GetData())
            GUILayout.Box($"{i.Key}: {i.Value}", style);
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

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Color gizColor = Color.yellow;
        gizColor.a = 0.25f;
        Gizmos.color = gizColor;
        Gizmos.DrawSphere(transform.position, SensoryModule.INTEREST_RADIUS);
    }
}