using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HasArrived : Condition
{
    private float stoppingDistance;

    public HasArrived(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree, parameters)
    {
        stoppingDistance = parameters[0];
    }

    public override Consts.NodeStatus Update()
    {
        // Return whether we have arrived at the navigation destination
        NavMeshAgent agent = ParentTree.Owner.NavAgent;
        return (!agent.pathPending && agent.remainingDistance < agent.radius + stoppingDistance) ? Consts.NodeStatus.SUCCESS : Consts.NodeStatus.FAILURE;
    }
}