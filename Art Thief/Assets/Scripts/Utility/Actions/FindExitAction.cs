using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Utility action that handles the thief agent pathing to the nearest level exit
/// </summary>
public class FindExitAction : UtilityAction
{
    private NavMeshPath targetPath;

    public FindExitAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        float checkDist = float.MaxValue;
        targetPath = null;
        // Find the exit with the shortest path from us
        for(int i = 0; i < Level.Instance.LevelExits.Count; ++i)
        {
            NavMeshPath path = Consts.GetNewPath(
                thief.GetNavMeshSafePosition(),
                Level.Instance.LevelExits[i].position);

            if (path == null)
                continue;

            float pathDistance = Consts.GetPathDistance(path);
            if(pathDistance < checkDist)
            {
                checkDist = pathDistance;
                targetPath = path;
            }
        }
        GameEventLog.Log("Thief started heading to the exit");
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if(!thief.NavAgent.hasPath)
            thief.MoveAgent(targetPath);
        else
        {
            // Thief wins simulation if they reach the exit
            if (thief.NavAgent.remainingDistance < 1f)
                GameController.Instance.EndGame(Consts.Team.THIEF);
        }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }
}
