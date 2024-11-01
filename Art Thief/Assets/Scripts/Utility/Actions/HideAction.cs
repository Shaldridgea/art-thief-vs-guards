using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HideAction : UtilityAction
{
    private HidingArea targetArea;

    private NavMeshPath targetPath;

    private bool reachedSpot;

    public const float CHECK_RADIUS = 15f;

    private const float FEAR_WAIT_LENGTH = 5f;

    private float fearWaitStart;

    public HideAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        // Find nearby hiding areas
        Collider[] overlaps = Physics.OverlapSphere(thief.transform.position, CHECK_RADIUS,
            LayerMask.GetMask("Hide"),
            QueryTriggerInteraction.Collide);

        List<(HidingArea area, float distance)> areaList = new List<(HidingArea area, float distance)>();

        // Create a list of tuples of our found areas and the distances to them
        foreach (Collider c in overlaps)
            if (c.TryGetComponent(out HidingArea area))
                areaList.Add(( area, Vector3.Distance(thief.transform.position, area.transform.position) ));

        // Sort our areas by distance, shortest to farthest
        areaList.Sort(delegate((HidingArea area, float distance) x, (HidingArea area, float distance) y)
        {
            float xDist = x.distance;
            float yDist = y.distance;
            var hidingSpots = thief.CurrentRoom.HidingSpots;
            // If a hiding area is safe, reduce its stored
            // distance to weight it more favourably,
            // and sort it higher in the list if it's in our current room
            if (x.area.AreaType == Consts.HidingAreaType.Safe)
            {
                xDist /= 2f;
                if (hidingSpots != null && hidingSpots.Count > 0)
                    if (hidingSpots.Contains(x.area))
                        return -1;
            }

            if (y.area.AreaType == Consts.HidingAreaType.Safe)
            {
                yDist /= 2f;
                if (hidingSpots != null && hidingSpots.Count > 0)
                    if (hidingSpots.Contains(y.area))
                        return 1;
            }
            
            // Return sort for which area is closer
            if (xDist < yDist)
                return -1;
            else if (xDist > yDist)
                return 1;
            else
                return 0;
        });

        ThiefSensoryModule senses = thief.ThiefSenses;

        bool beingChased = thief.AgentBlackboard.GetVariable<bool>("inChase");
        // Check if any of these areas are safe to go to, preferring closest first
        foreach (var (area, distance) in areaList)
        {
            // If we're being chased then we don't care about conditional/unsafe hiding spots
            if (area.AreaType == Consts.HidingAreaType.Conditional && beingChased)
                continue;

            area.CheckForSafety(senses.AwareGuards);
            if (area.IsSafe)
            {
                var newPath = Consts.GetNewPath(thief.GetNavMeshSafePosition(), area.transform.position);
                if (IsPathSafe(thief, newPath, senses.AwareGuards))
                {
                    targetArea = area;
                    targetPath = newPath;
                    break;
                }
            }
        }

        // If we couldn't find a valid hiding spot, increase danger
        if (targetArea == null)
            thief.AgentBlackboard.SetVariable("danger", thief.AgentBlackboard.GetVariable<float>("danger")+0.5f);
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetArea != null && !thief.NavAgent.hasPath && !reachedSpot)
        {
            thief.MoveAgent(targetPath);
            // Mark ourself as hiding so we stay in our hiding spot when we reach it
            thief.AgentBlackboard.SetVariable("hiding", 1f);
            GameEventLog.Log("Thief started hiding");
        }

        if (thief.NavAgent.hasPath && !reachedSpot)
            if (thief.NavAgent.remainingDistance <= 1f)
            {
                thief.TurnBody(180f, 1.5f);
                fearWaitStart = Time.time;
                reachedSpot = true;
                GameEventLog.Log("Thief reached hiding spot");
            }

        // Wait for a bit to be cautious before thinking about leaving our hiding spot
        if(reachedSpot)
            if(Time.time >= fearWaitStart + FEAR_WAIT_LENGTH)
                thief.AgentBlackboard.SetVariable("hiding", 0f);
    }

    public override void ExitAction(ThiefAgent thief)
    {
        targetArea = null;
        thief.AgentBlackboard.SetVariable("hiding", 0f);
        thief.NavAgent.ResetPath();
        reachedSpot = false;
    }

    private bool IsPathSafe(ThiefAgent thief, NavMeshPath path, List<GuardAgent> guardThreats)
    {
        if(path == null)
        {
            Debug.LogError("IsPathSafe was passed a null path");
            return false;
        }

        // For every guard we're aware of, simulate the future positions of the guards and the thief
        // and check for whether there is a potential of being in the guard's line of sight
        // if this path is followed
        for (int i = 0; i < guardThreats.Count; ++i)
        {
            GuardAgent guard = guardThreats[i];

            // Ignore stunned guards who aren't a threat to us
            if (guard.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            // Store our initial guard position before simulation
            Vector3 guardPosition = guard.transform.position;
            // Simulate in steps of 2 seconds, for 6 seconds into the future
            for(int d = 2; d <= 6; d += 2)
            {
                // Get the simulated position of where the guard will be in however many seconds on its path
                bool terminated = guard.NavAgent.SamplePathPosition(guard.NavAgent.areaMask,
                    Consts.GetPathDistance(guard.NavAgent.path), out var hit);

                // Get the simulated position along the supplied path however many seconds into the future
                Vector3 thiefPosition = GetPositionAlongPath(path, thief.NavAgent.speed * d);

                // Get the direction the guard will be looking in
                Vector3 guardDirection = (hit.position - guardPosition).normalized;
                // If guard is standing still for whatever reason we still want
                // to get where its looking at from its transform
                if (guardPosition == hit.position)
                    guardDirection = guard.transform.forward;

                // If guard has line of sight to thief, report this path as being unsafe
                if (guard.GuardSenses.IsSeen(thiefPosition, guardDirection))
                    return false;

                // Store current simulated position for next loop
                guardPosition = hit.position;

                // If terminated that means we reached the end of the current path
                // and any further checks will be redundant so we break out early here
                if (terminated)
                    break;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the position along a supplied NavMesh path by a specific distance
    /// </summary>
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
}
