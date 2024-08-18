using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

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
        bool doorBeingUsed = controller.IsDoorBeingUsed(doorName);
        controller.AgentEnter(doorName);
        if (doorBeingUsed)
            return;

        controller.SwingDoor(doorName, swingAngle);
        Agent agent = other.GetComponent<Agent>();
        if (agent == null)
            return;

        bool inChase = agent.AgentBlackboard.GetVariable<bool>("inChase");
        SoundInterest playSound;
        if (inChase)
            playSound = agent.DoorSlamSound;
        else
            playSound = agent.DoorOpenSound;

        PlaceDoorSound(playSound);
        playSound.PlaySound();
    }

    private void OnTriggerExit(Collider other)
    {
        controller.AgentExit(doorName);
        if (controller.IsDoorBeingUsed(doorName))
            return;

        controller.SwingDoor(doorName, 0f);
        Agent agent = other.GetComponent<Agent>();
        if(agent != null)
        {
            PlaceDoorSound(agent.DoorCloseSound);
            agent.DoorCloseSound.PlaySound();
        }
    }

    private void PlaceDoorSound(SoundInterest doorSound)
    {
        PositionConstraint soundConstraint = doorSound.GetComponent<PositionConstraint>();
        if (soundConstraint.sourceCount == 0)
            soundConstraint.AddSource(new ConstraintSource());
        soundConstraint.SetSource(0, new ConstraintSource() { sourceTransform = controller.transform, weight = 1f });
        soundConstraint.constraintActive = true;
    }

    private void Reset()
    {
        controller = GetComponentInParent<DoorController>();
    }
}
