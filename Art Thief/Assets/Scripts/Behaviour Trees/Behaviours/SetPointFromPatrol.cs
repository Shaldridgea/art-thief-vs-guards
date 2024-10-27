using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointFromPatrol : BehaviourNode
{
    private string variableKey;

    private Consts.PatrolPathType pathType;

    private Consts.PatrolGetType getType;

    private string roomPointVariableKey;

    public SetPointFromPatrol(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
        pathType = (Consts.PatrolPathType)(int)parameters[1];
        getType = (Consts.PatrolGetType)(int)parameters[2];
        roomPointVariableKey = parameters[3];
    }

    public override Consts.NodeStatus Update()
    {
        Vector3 nextPosition;
        PatrolPath targetPath = null;

        // Room path type requires us to get which room our supplied point
        // is in and get the path associated with that room first
        if (pathType == Consts.PatrolPathType.Room)
        {
            List<Room> rooms = Level.Instance.AllRooms;
            Vector3 roomPoint = ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(roomPointVariableKey);
            Room insideTargetRoom = null;
            Room closestRoom = null;
            float closestDistance = float.MaxValue;
            foreach (Room r in rooms)
            {
                if (r.RoomBox.bounds.Contains(roomPoint))
                {
                    insideTargetRoom = r;
                    break;
                }
                 
                // On the off chance that our room point is outside of a room bounding box,
                // we find which room's bounds are closest to that point as a failsafe
                float measureDistance = Vector3.Distance(r.RoomBox.ClosestPointOnBounds(roomPoint), roomPoint);
                if (measureDistance < closestDistance)
                {
                    closestRoom = r;
                    closestDistance = measureDistance;
                }
            }

            if (insideTargetRoom != null)
                targetPath = insideTargetRoom.RoomPatrolPath;
            else if (closestRoom != null)
                targetPath = closestRoom.RoomPatrolPath;
        }
        else
            targetPath = ParentTree.Owner.GetPatrol(pathType);

        // Exit out gracefully if we couldn't get a path for some reason
        if (targetPath == null)
            return Consts.NodeStatus.FAILURE;

        // Get either the next logical point or a random point
        // on our path to set our blackboard variable to
        if (getType == Consts.PatrolGetType.Follow)
            nextPosition = targetPath.FindNextPointFromPosition(ParentTree.Owner.transform.position);
        else
            nextPosition = targetPath.GetRandomPosition(ParentTree.Owner.transform.position);

        ParentTree.Owner.AgentBlackboard.SetVariable(variableKey, nextPosition);

        return Consts.NodeStatus.SUCCESS;
    }
}
