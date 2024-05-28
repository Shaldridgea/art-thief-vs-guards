﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Consts
{
    public enum NodeStatus
    {
        SUCCESS,
        FAILURE,
        RUNNING
    }

    public enum BehaviourType
    {
        Sequence,
        Selector,
        Repeat,
        Invert,
        ForceSuccess,
        RandomChance,
        Monitor,
        SetVariable,
        Wait,
        Condition,
        HasArrived,
        MoveToPoint,
        SetPointFromPatrol,
        Cooldown,
        StoreVisibleInterests,
        SetRandomInterest,
        SetPointFromGameObject,
        StopMoving,
        TurnHead,
        IsTurningHead
    }

    public enum RepeatCondition
    {
        UntilResult,
        NumberOfTimes
    }

    public enum BlackboardSource
    {
        AGENT,
        GLOBAL
    }

    public enum Team
    {
        GUARD,
        THIEF
    }

    public enum SuspicionType
    {
        Visual,
        Sound
    }

    public enum PatrolPathType
    {
        Regular,
        Perimeter
    }

    public enum UtilityActionType
    {
        Path,
        Steal,
        Hide,
        Run,
        Attack
    }

    public static UtilityAction GetUtilityAction(UtilityActionType actionType, MotiveValue[] motiveValues)
    {
        // TODO: Return the correct actions when they're made
        return null;
    }

    public static NavMeshPath GetNewPath(Vector3 startPos, Vector3 endPos)
    {
        // Calculate a new path from the start position to end position
        NavMeshPath newPath = new NavMeshPath();
        if (!NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, newPath))
            return null;

        return newPath;
    }

    public static float GetPathDistance(NavMeshPath calcPath)
    {
        if (calcPath == null)
            return 0f;

        // Sum up the distances between all the positions along the path to get the total distance
        Vector3[] corners = calcPath.corners;
        float distance = 0f;
        for(int i = 1; i < corners.Length; ++i)
        {
            distance += Vector3.Distance(corners[i-1], corners[i]);
        }
        return distance;
    }

    public enum AgentSource
    {
        Guard,
        Thief
    }

    public const string CHASE_KEY = "thiefChase";
}