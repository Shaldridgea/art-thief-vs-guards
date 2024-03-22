using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GuardAgent : Agent
{
    [SerializeField]
    private BTGraph behaviourTreeGraph;

    [SerializeField]
    private PatrolPath patrolPath;

    private BehaviourTree agentTree;

    private Transform targetPoint;

    private int patrolIndex;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);
    }

    private void Update()
    {
        // Update our behaviour tree
        agentTree.Update();
    }

    private void OnTriggerEnter(Collider other)
    {
        // If we entered the catching trigger of the spy
        if (other.CompareTag("Caught"))
        {
            GameController.Instance.GuardsWon();
        }
    }

    public Vector3 GetNextPatrolPoint()
    {
        Vector3 point = patrolPath.GetPoint(patrolIndex);
        ++patrolIndex;
        return point;
    }

    public void TurnHeadToPoint(Vector3 targetPoint)
    {
        float lookAngle = Vector3.SignedAngle(AgentView.AgentHeadRoot.forward, (targetPoint - AgentView.AgentHeadRoot.position).normalized, Vector3.up);
        TurnHead(lookAngle);
    }

    public void TurnHead(float turnAngle)
    {
        float angleResult = Vector3.SignedAngle(AgentView.AgentRoot.forward, AgentView.AgentHeadRoot.forward, Vector3.up) + turnAngle;
        // Adjust our turning angle to be clamped so the head doesn't turn all the way around like an owl
        if (Mathf.Abs(angleResult) >= 100f)
            turnAngle -= Mathf.Sign(turnAngle) * (Mathf.Abs(angleResult) - 100f);

        LeanTween.rotateAroundLocal(AgentView.AgentHeadRoot.gameObject, Vector3.up, turnAngle, 2f);
    }

    [Button("Test head turn", EButtonEnableMode.Playmode)]
    private void TestHeadTurn()
    {
        TurnHeadToPoint(patrolPath.GetPoint(0));
    }
}