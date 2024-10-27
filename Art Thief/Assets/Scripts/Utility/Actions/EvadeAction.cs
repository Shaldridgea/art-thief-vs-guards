using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EvadeAction : UtilityAction
{
    private DoorwayArea targetDoorway;

    private Vector3 targetPoint;

    private Room currentRoom;

    private Queue<Room> recentRooms = new();

    public EvadeAction(ActionData newData) : base(newData) { }

    public override void EnterAction(ThiefAgent thief)
    {
        FindSafeExit(thief);
    }

    public override void PerformAction(ThiefAgent thief)
    {
        if (targetDoorway != null && !thief.NavAgent.hasPath)
            thief.MoveAgent(targetPoint);

        // Find the next exit to run to if we've reached our current one in another room
        if (!thief.NavAgent.pathPending && thief.NavAgent.remainingDistance < thief.NavAgent.radius)
        {
            FindSafeExit(thief);
            thief.NavAgent.ResetPath();
        }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        targetDoorway = null;
        recentRooms.Clear();
        thief.NavAgent.ResetPath();
    }

    /// <summary>
    /// Calculates the safest exit out of the current room to try and avoid guards
    /// </summary>
    private void FindSafeExit(ThiefAgent thief)
    {
        targetDoorway = null;
        Room room = thief.CurrentRoom;
        if (room == null)
            return;

        // If we've changed the room we're in
        if (room != currentRoom)
        {
            // Store our current room as being a past room
            if(currentRoom != null)
                recentRooms.Enqueue(currentRoom);

            // Don't remember more than 3 rooms at once
            if (recentRooms.Count > 3)
                recentRooms.Dequeue();

            currentRoom = room;
        }

        // If we're in a dead end we no longer care about avoiding previous rooms
        if (currentRoom.Doorways.Count == 1)
            recentRooms.Clear();

        List<DoorwayArea> exitList = new(room.Doorways.Count);
        List<DoorwayArea> validCandidates = new();
        // Calculate risk of every doorway in the room, make a list of them,
        // and make a list of doorways that don't go to recent rooms or a dead end
        foreach (var d in room.Doorways)
        {
            d.CalculateRisk(thief, thief.ThiefSenses.AwareGuards);
            exitList.Add(d);
            if (IsValidDoorway(d))
                validCandidates.Add(d);
        }

        // Sort our doorways by least risk to most risk
        exitList.Sort(delegate(DoorwayArea x, DoorwayArea y)
        {
            if (x.Risk < y.Risk)
                return -1;
            else if (x.Risk > y.Risk)
                return 1;
            else
                return 0;
        });

        // If we have no doorways to new rooms, cull our queue to only the most recent
        if (validCandidates.Count == 0)
        {
            while(recentRooms.Count > 1)
                recentRooms.Dequeue();
        }
        else
        {
            // Remove doorways that aren't valid exits
            for (int i = exitList.Count - 1; i >= 0; --i)
            {
                if (!validCandidates.Contains(exitList[i]))
                    exitList.RemoveAt(i);
            }
        }

        // Choose our least risky doorway
        if (exitList.Count > 0)
            targetDoorway = exitList[0];

        // Set to go to the point of the doorway that's in the next room
        if (targetDoorway != null)
            targetPoint = targetDoorway.GetFurthestPoint(thief.transform.position);
    }

    /// <summary>
    /// Checks whether a door is valid, that being it doesn't go to a room passed through recently, and isn't a dead end
    /// </summary>
    private bool IsValidDoorway(DoorwayArea doorway)
    {
        foreach (var r in doorway.ConnectedRooms)
            if (recentRooms.Contains(r) || r.Doorways.Count == 1)
                return false;

        return true;
    }

#if UNITY_EDITOR
    public override void OnSceneGUI()
    {
        if (targetDoorway == null || currentRoom == null)
            return;

        var color = Color.black;
        Handles.color = color;
        GUI.color = color;

        foreach(var d in currentRoom.Doorways)
            if (IsValidDoorway(d))
                Handles.Label(d.transform.position, $"Risk: {d.Risk}");
    }
#endif
}