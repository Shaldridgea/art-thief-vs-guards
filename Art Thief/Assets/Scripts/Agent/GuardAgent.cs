using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}