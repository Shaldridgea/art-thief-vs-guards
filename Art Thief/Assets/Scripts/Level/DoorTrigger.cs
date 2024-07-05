using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField]
    private DoorController controller;

    [SerializeField]
    private string doorName;

    [SerializeField]
    private float swingAngle;

    private void OnTriggerEnter(Collider other)
    {
        if (!controller.IsDoorBeingUsed(doorName))
            controller.SwingDoor(doorName, swingAngle);
        controller.AgentEnter(doorName);
    }

    private void OnTriggerExit(Collider other)
    {
        controller.AgentExit(doorName);
        if (!controller.IsDoorBeingUsed(doorName))
            controller.SwingDoor(doorName, 0f);
    }

    private void Reset()
    {
        controller = GetComponentInParent<DoorController>();
    }
}
