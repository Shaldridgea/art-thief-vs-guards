using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindExitAction : UtilityAction
{
    private NavMeshPath targetPath;

    public FindExitAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        float checkDist = float.MaxValue;
        targetPath = null;
        for(int i = 0; i < Level.Instance.LevelExits.Count; ++i)
        {
            NavMeshPath path = Consts.GetNewPath(
                thief.transform.position,
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
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if(!thief.NavAgent.hasPath)
            thief.MoveAgent(targetPath);
        else
        {
            if (thief.NavAgent.remainingDistance < 1f)
                GameController.Instance.EndGame(Consts.Team.THIEF);
        }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        thief.NavAgent.ResetPath();
    }

    public override void OnSceneGUI()
    {
        return;
    }
}
