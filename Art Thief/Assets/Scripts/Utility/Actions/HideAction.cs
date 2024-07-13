using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HideAction : UtilityAction
{
    private HidingArea targetArea;

    private Vector3 targetPosition;

    private NavMeshPath targetPath;

    private bool turnedAround;

    public HideAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        turnedAround = false;
        Collider[] overlaps = Physics.OverlapSphere(thief.transform.position, 15f,
            LayerMask.GetMask("Hide"),
            QueryTriggerInteraction.Collide);
        ThiefSensoryModule senses = thief.ThiefSenses;

        List<(HidingArea area, float distance)> areaList = new List<(HidingArea area, float distance)>();

        // Get a list of tuples of our found areas and the distances to them
        foreach (Collider c in overlaps)
            if (c.TryGetComponent(out HidingArea area))
                areaList.Add(( area, Vector3.Distance(thief.transform.position, area.transform.position) ));

        // Sort our areas by distance, shortest to farthest
        areaList.Sort(delegate((HidingArea area, float distance) x, (HidingArea area, float distance) y)
        {
            if (x.distance < y.distance)
                return -1;
            else if (x.distance > y.distance)
                return 1;
            else
                return 0;
        });

        bool beingChased = thief.AgentBlackboard.GetVariable<bool>("inChase");
        // Check if any of these areas are safe to go to, preferring closest first
        foreach (var (area, distance) in areaList)
        {
            if (area.AreaType == Consts.HidingAreaType.Conditional && beingChased)
                continue;

            area.CheckForSafety(senses.AwareGuards);
            if (area.IsSafe)
            {
                var newPath = Consts.GetNewPath(thief.transform.position, area.transform.position);
                if (IsPathSafe(thief, newPath, senses.AwareGuards))
                {
                    targetArea = area;
                    targetPosition = targetArea.transform.position;
                    targetPath = newPath;
                    break;
                }
            }
        }
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetArea != null && !thief.NavAgent.hasPath)
        {
            thief.MoveAgent(targetPath);
            thief.AgentBlackboard.SetVariable("hiding", 1f);
        }

        if (thief.NavAgent.hasPath && !turnedAround)
            if (thief.NavAgent.remainingDistance <= 1f)
            {
                thief.TurnBody(180f, 1.5f);
                thief.AgentBlackboard.SetVariable("hiding", 0f);
                turnedAround = true;
            }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        targetArea = null;
        thief.AgentBlackboard.SetVariable("hiding", 0f);
        thief.NavAgent.ResetPath();
    }

    private bool IsPathSafe(ThiefAgent thief, NavMeshPath path, List<GuardAgent> guardThreats)
    {
        // For every guard we're aware of, simulate the future positions of the guards and the thief
        // and check for whether there is a potential of being in the guard's line of sight
        // if this path is followed
        for (int i = 0; i < guardThreats.Count; ++i)
        {
            GuardAgent guard = guardThreats[i];

            if (guard.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            // Store our initial guard position before simulation
            Vector3 guardPosition = guard.transform.position;
            // Simulate in steps of 2 seconds, for 8 seconds into the future
            for(int d = 2; d <= 6; d += 2)
            {
                // Get the simulated position of where the guard will be in however many seconds on its path
                bool terminated = guard.NavAgent.SamplePathPosition(guard.NavAgent.areaMask, Consts.GetPathDistance(guard.NavAgent.path), out var hit);

                // Get the simulated position of the thief however many seconds into the future
                Vector3 thiefPosition = GetPositionAlongPath(path, thief.NavAgent.speed * d);

                // Get the direction the guard will be looking in
                Vector3 guardDirection = (hit.position - guardPosition).normalized;
                // If guard is standing still for whatever reason we still want
                // to get where its looking at from its transform
                if (guardPosition == hit.position)
                    guardDirection = guard.transform.forward;

                // If guard has line of sight to thief, report this path as being unsafe
                if (guard.GuardSenses.IsInLOS(thiefPosition, guardDirection))
                    return false;

                guardPosition = hit.position;

                // If terminated that means we reached the end of the current path
                // and any further checks will be redundant so we break out early here
                if (terminated)
                    break;
            }
        }
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
                totalDistance -= thisDistance;
                float moveDistance = sampleDistance - totalDistance;
                return start + ((end - start).normalized * moveDistance);
            }
        }
        return corners[^1];
    }

    public override void OnSceneGUI()
    {
        return;
    }
}
