using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HasArrived : Condition
{
    public HasArrived(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree, parameters)
    {

    }

    public override Consts.NodeStatus Update()
    {
        // Return whether we have arrived at the navigation destination
        NavMeshAgent agent = ParentTree.Owner.NavAgent;
        return (!agent.pathPending && agent.remainingDistance < agent.radius) ? Consts.NodeStatus.SUCCESS : Consts.NodeStatus.FAILURE;
    }
}