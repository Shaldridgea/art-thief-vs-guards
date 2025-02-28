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
        IsTurningHead,
        HasLineOfSight,
        CallMethod,
        SetDistanceFromPoint,
        CopyVariablesToSelf,
        IsThiefHeard,
        CanAttackThief,
        AttackThief,
        TurnBody,
        LockVariable
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
        THIEF,
        NEUTRAL
    }

    public enum SuspicionType
    {
        Visual,
        Sound
    }

    public enum PatrolPathType
    {
        Regular,
        Perimeter,
        Room
    }

    public enum PatrolGetType
    {
        Follow,
        Random
    }

    public enum OffsetType
    {
        WORLD,
        LOCAL
    }

    public enum HidingAreaType
    {
        Safe,
        Conditional
    }

    public enum UtilityActionType
    {
        FindArt,
        FindExit,
        Steal,
        Hide,
        Evade,
        Attack
    }

    public static UtilityAction GetUtilityAction(ActionData actionData)
    {
        switch (actionData.Action)
        {
            case UtilityActionType.FindArt:
                return new FindArtAction(actionData);

            case UtilityActionType.FindExit:
                return new FindExitAction(actionData);

            case UtilityActionType.Steal:
                return new StealArtAction(actionData);

            case UtilityActionType.Hide:
                return new HideAction(actionData);

            case UtilityActionType.Evade:
                return new EvadeAction(actionData);

            case UtilityActionType.Attack:
                return new AttackAction(actionData);
        }
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

    public static bool ParseVector3(string vectorString, out Vector3 parsedVector)
    {
        if (!vectorString.Contains("Vector3") || !vectorString.Contains("(") ||
                !vectorString.Contains(")") || !vectorString.Contains(","))
        {
            parsedVector = Vector3.zero;
            return false;
        }

        int leftBracketIndex = vectorString.IndexOf('(') + 1;
        int rightBracketIndex = vectorString.IndexOf(')');
        string vectorValueString = vectorString[leftBracketIndex..rightBracketIndex];
        string[] vectorSplit = vectorValueString.Split(',');
        float[] vectorComponents = new float[3];
        for (int i = 0; i < Mathf.Min(vectorSplit.Length, 3); ++i)
        {
            if (float.TryParse(vectorSplit[i].Trim(), out float vecComponent))
                vectorComponents[i] = vecComponent;
        }
        parsedVector = new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]);
        return true;
    }

    public const string THIEF_CHASE_STATUS = "inChase";

    public const string GUARD_PASSIVE_MODE = "passive";

    public const string GUARD_ALERT_MODE = "alert";

    public const string GUARD_CHASE_MODE = "chase";

    public const string GUARD_MODE_STATUS = "guardMode";

    public const string AGENT_STUN_STATUS = "isStunned";

    public const string AGENT_INTERACT_STATUS = "isInteracting";
}