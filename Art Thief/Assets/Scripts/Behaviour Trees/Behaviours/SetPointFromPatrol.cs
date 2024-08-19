using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointFromPatrol : BehaviourNode
{
    private string variableKey;

    private Consts.PatrolPathType pathType;

    private string roomPointVariableKey;

    public SetPointFromPatrol(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        variableKey = parameters[0];
        pathType = (Consts.PatrolPathType)(int)parameters[1];
        roomPointVariableKey = parameters[2];
    }

    public override Consts.NodeStatus Update()
    {
        if(pathType == Consts.PatrolPathType.Room)
        {
            List<Room> rooms = Level.Instance.AllRooms;
            Vector3 roomPoint = ParentTree.Owner.AgentBlackboard.GetVariable<Vector3>(roomPointVariableKey);
            Room targetRoom = null;
            foreach(Room r in rooms)
            {
                if(r.RoomBox.bounds.Contains(roomPoint))
                {
                    targetRoom = r;
                    break;
                }
            }

            if (targetRoom)
                Debug.Log(targetRoom.name);

            if(targetRoom != null)
                ParentTree.Owner.AgentBlackboard.SetVariable(variableKey, targetRoom.RoomPatrolPath.FindNextPointFromPosition(ParentTree.Owner.transform.position));
        }
        else
            ParentTree.Owner.AgentBlackboard.SetVariable(variableKey, ParentTree.Owner.GetNextPatrolPoint(pathType));
        return Consts.NodeStatus.SUCCESS;
    }
}
