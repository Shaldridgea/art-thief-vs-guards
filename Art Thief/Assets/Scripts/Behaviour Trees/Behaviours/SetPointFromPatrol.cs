using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointFromPatrol : BehaviourNode
{
    private string variableKey;

    private Consts.PatrolPathType pathType;

    private Consts.PatrolPointType pointType;

    private string roomPointVariableKey;

    public SetPointFromPatrol(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
        pathType = (Consts.PatrolPathType)(int)parameters[1];
        pointType = (Consts.PatrolPointType)(int)parameters[2];
        roomPointVariableKey = parameters[3];
    }

    public override Consts.NodeStatus Update()
    {
        Vector3 nextPosition;
        PatrolPath targetPath = null;

        if (pathType == Consts.PatrolPathType.Room)
        {
            List<Room> rooms = Level.Instance.AllRooms;
            Vector3 roomPoint = ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(roomPointVariableKey);
            Room targetRoom = null;
            foreach (Room r in rooms)
            {
                if (r.RoomBox.bounds.Contains(roomPoint))
                {
                    targetRoom = r;
                    break;
                }
            }

            if (targetRoom != null)
                targetPath = targetRoom.RoomPatrolPath;
        }
        else
            targetPath = ParentTree.Owner.GetPatrol(pathType);

        if (targetPath == null)
            return Consts.NodeStatus.FAILURE;

        if (pointType == Consts.PatrolPointType.Follow)
            nextPosition = targetPath.FindNextPointFromPosition(ParentTree.Owner.transform.position);
        else
            nextPosition = targetPath.GetRandomPosition(ParentTree.Owner.transform.position);

        ParentTree.Owner.AgentBlackboard.SetVariable(variableKey, nextPosition);

        return Consts.NodeStatus.SUCCESS;
    }
}
