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

        if (!thief.NavAgent.pathPending && thief.NavAgent.remainingDistance < thief.NavAgent.radius)
        {
            FindSafeExit(thief);
            thief.NavAgent.ResetPath();
        }
    }

    public override void ExitAction(ThiefAgent thief)
    {
        recentRooms.Clear();
        thief.NavAgent.ResetPath();
    }

    private void FindSafeExit(ThiefAgent thief)
    {
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

        List<DoorwayArea> leastRisk = new(room.Doorways.Count);
        List<DoorwayArea> validCandidates = new();
        // Calculate risk of every doorway in the room, make a list of them,
        // and make a list of doorways that go to rooms we haven't been in recently
        foreach (var d in room.Doorways)
        {
            d.CalculateRisk(thief, thief.ThiefSenses.AwareGuards);
            leastRisk.Add(d);
            if (!GoesToRecentRoom(d))
                validCandidates.Add(d);
        }

        // Sort our doorways by least risk to most risk
        leastRisk.Sort(delegate(DoorwayArea x, DoorwayArea y)
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
            // Remove doorways that go back into rooms we've just visited
            for (int i = leastRisk.Count - 1; i >= 0; --i)
            {
                if (!validCandidates.Contains(leastRisk[i]))
                    leastRisk.RemoveAt(i);
            }
        }

        if (leastRisk.Count > 0)
            targetDoorway = leastRisk[0];

        if (targetDoorway != null)
            targetPoint = targetDoorway.GetFurthestPoint(thief.transform.position);
    }

    private bool GoesToRecentRoom(DoorwayArea doorway)
    {
        foreach (var r in doorway.ConnectedRooms)
            if (recentRooms.Contains(r))
                return true;

        return false;
    }

    public override void OnSceneGUI()
    {
        if (targetDoorway == null || currentRoom == null)
            return;

        var color = Color.black;
        Handles.color = color;
        GUI.color = color;

        foreach(var d in currentRoom.Doorways)
            if (!GoesToRecentRoom(d))
                Handles.Label(d.transform.position, $"Risk: {d.Risk}");
    }
}