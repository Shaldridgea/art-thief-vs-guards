using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HideAction : UtilityAction
{
    private HidingArea targetArea;

    private Vector3 targetPosition;

    private NavMeshPath targetPath;

    private bool touchedEdge;

    public HideAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        touchedEdge = false;
        Collider[] overlaps = Physics.OverlapSphere(thief.transform.position, 15f, LayerMask.GetMask("Hide"), QueryTriggerInteraction.Collide);
        foreach (Collider c in overlaps)
            if (c.TryGetComponent(out HidingArea area))
                if (area.IsSafe)
                {
                    var newPath = Consts.GetNewPath(thief.transform.position, area.transform.position);
                    if (IsPathSafe(thief, newPath, (thief.Senses as ThiefSensoryModule).AwareGuards))
                    {
                        targetArea = area;
                        targetPosition = targetArea.transform.position;
                        targetPath = newPath;
                        break;
                    }
                }
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if(targetArea != null && !thief.NavAgent.hasPath)
            thief.MoveAgent(targetPath);

        if (thief.NavAgent.hasPath && !touchedEdge)
            if (thief.NavAgent.remainingDistance <= 1f)
            {
                thief.TurnBody(180f, 1.5f);

                touchedEdge = true;
            }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        return;
    }

    private bool IsPathSafe(ThiefAgent thief, NavMeshPath path, List<GuardAgent> guardThreats)
    {
        // For every guard we're aware of, simulate the future positions of the guards and the thief
        // and check for whether there is a potential of being in the guard's line of sight
        // if this path is followed
        Debug.Log($"Starting angle to thief: {Vector3.Angle(guardThreats[0].transform.forward, thief.transform.position - guardThreats[0].transform.position)}");
        for(int i = 0; i < guardThreats.Count; ++i)
        {
            GuardAgent guard = guardThreats[i];

            // Store our initial guard position before simulation
            Vector3 guardPosition = guard.transform.position;
            // Simulate in steps of 2 seconds, for 6 seconds into the future
            for(int d = 0; d < 6; d += 2)
            {
                // Get the simulated position of where the guard will be in 2 seconds along its path
                bool terminated = guard.NavAgent.SamplePathPosition(guard.NavAgent.areaMask, guard.NavAgent.speed * i, out var hit);
                // If terminated this may mean we reached the end of the path, we'll decide what to do about that later

                // Get the simulated position of the thief in 2 seconds along its path
                Vector3 thiefPosition = GetPositionAlongPath(path, thief.NavAgent.speed * i);
                Debug.Log(thiefPosition);

                // Get the direction the guard will be looking in
                Vector3 guardDirection = (hit.position - guardPosition).normalized;
                if (guardPosition == hit.position)
                    guardDirection = guard.transform.forward;

                // Get the angle from guard's looking vector to thief position
                float angleToThief = Vector3.Angle(guardDirection, thiefPosition-guardPosition);
                Debug.Log($"Angle to thief: {angleToThief}");

                // If thief is in viewing angle to guard
                if(angleToThief <= 40f)
                {
                    // If guard has line of sight to thief, report this path as being unsafe
                    if (!Physics.Linecast(guardPosition, thiefPosition, guard.Senses.LosMask, QueryTriggerInteraction.Ignore))
                        return false;
                }
                guardPosition = hit.position;
            }
        }
        Debug.Log("Path is safe");
        return true;
    }

    private Vector3 GetPositionAlongPath(NavMeshPath path, float sampleDistance)
    {
        Vector3[] corners = path.corners;
        float totalDistance = 0f;
        for(int i = 1; i < corners.Length; ++i)
        {
            Vector3 start = corners[i - 1];
            Vector3 end = corners[i];
            float thisDistance = Vector3.Distance(start, end);
            totalDistance += thisDistance;
            if(totalDistance >= sampleDistance)
            {
                totalDistance -= sampleDistance;
                return (start + (end - start).normalized) * totalDistance;
            }
        }
        return corners[^1];
    }
}
