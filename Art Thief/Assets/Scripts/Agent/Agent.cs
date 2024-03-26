using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    [SerializeField]
    protected NavMeshAgent navAgent;

    public NavMeshAgent NavAgent => navAgent;

    [SerializeField]
    protected AgentView agentView;

    public AgentView AgentView => agentView;

    [SerializeField]
    protected SensoryModule senses;

    public SensoryModule Senses => senses;

    [SerializeField]
    private SoundTrigger walkingSound;

    [SerializeField]
    private float walkSoundInterval;

    private Coroutine walkingCoroutine;

    public Blackboard AgentBlackboard { get; private set; }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Make a new blackboard for this agent
        AgentBlackboard = new Blackboard();
    }

    public void NotifySound(SoundTrigger sound)
    {
        // Ignore our own walking sounds
        if (sound == walkingSound)
            return;

        // Trigger sensory module to handle hearing a sound
        senses.SoundHeard(sound);
    }

    /// <summary>
    /// Set an agent to move to a specified position
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="updatePositionOnly"></param>
    public void MoveAgent(Vector3 newPosition, bool updatePositionOnly = false)
    {
        navAgent.destination = newPosition;
        return;
        // TODO: Fix walking sounds
        // Stop the walking coroutine that creates the step sound if we aren't just
        // updating position only
        if (walkingCoroutine != null && !updatePositionOnly)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }

        if(walkingCoroutine == null)
            walkingCoroutine = StartCoroutine(MakeWalkingSound());
    }

    /// <summary>
    /// Set an agent to move along a path
    /// </summary>
    /// <param name="newPath"></param>
    /// <param name="updatePositionOnly"></param>
    public void MoveAgent(NavMeshPath newPath, bool updatePositionOnly = false)
    {
        navAgent.SetPath(newPath);
        if (walkingCoroutine != null && !updatePositionOnly)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }

        if (walkingCoroutine == null)
            walkingCoroutine = StartCoroutine(MakeWalkingSound());
    }

    private IEnumerator MakeWalkingSound()
    {
        // Make the walking sound play as long as we're travelling to our destination
        while(Vector3.Distance(transform.position, navAgent.destination) > 2f)
        {
            walkingSound.PlaySound();
            yield return new WaitForSeconds(walkSoundInterval);
        }

        yield break;
    }
}