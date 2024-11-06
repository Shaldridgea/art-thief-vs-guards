using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Trigger for controlling a single door
/// </summary>
public class DoorTrigger : MonoBehaviour
{
    [SerializeField]
    private DoorwayController controller;

    [SerializeField]
    private string doorName;

    [SerializeField]
    private float swingAngle;

    [SerializeField]
    private bool ignoresGuards;

    private void OnTriggerEnter(Collider other)
    {
        if (ignoresGuards)
            if (other.CompareTag("Guard"))
                return;

        // If door is already in use register the agent as being there
        // and then exit out, since the door is already open
        bool doorBeingUsed = controller.IsDoorBeingUsed(doorName);
        controller.AgentEnter(doorName);
        if (doorBeingUsed)
            return;

        controller.SwingDoor(doorName, swingAngle);

        Agent agent = other.GetComponent<Agent>();
        if (agent == null)
            return;

        // Check if our agent is in chase (thief only) and play the corresponding door opening sound
        bool inChase = agent.IsInChase();
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
        if (ignoresGuards)
            if (other.CompareTag("Guard"))
                return;

        // Register the agent as leaving the door and don't continue if still used
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

    /// <summary>
    /// Place the supplied sound emitter at the location of this door and used a constraint to keep it there
    /// </summary>
    private void PlaceDoorSound(SoundInterest doorSound)
    {
        // The door sounds are per-agent and part of their hierarchy, so that every
        // door doesn't need to have its own sound to take care of. Take the sound we're
        // given and use a PositionConstraint to fix it to the position of this door
        PositionConstraint soundConstraint = doorSound.GetComponent<PositionConstraint>();
        if (soundConstraint.sourceCount == 0)
            soundConstraint.AddSource(new ConstraintSource());
        soundConstraint.SetSource(0, new ConstraintSource() { sourceTransform = controller.transform, weight = 1f });
        soundConstraint.constraintActive = true;
    }

    private void Reset()
    {
        controller = GetComponentInParent<DoorwayController>();
    }
}
