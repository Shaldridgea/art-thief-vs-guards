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
    private SoundInterest walkingSound;

    [SerializeField]
    private float walkSoundInterval;

    private Coroutine walkingCoroutine;

    [SerializeField]
    private List<string> blackboardDefaults;

    public Blackboard AgentBlackboard { get; private set; }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Make a new blackboard for this agent
        AgentBlackboard = new Blackboard();
        senses.SoundHeard += HandleSoundHeard;
        senses.VisualFound += HandleVisualFound;
        senses.VisualLost += HandleVisualLost;

        // Set our default blackboard values by parsing the strings for their keys and values
        foreach (string s in blackboardDefaults)
        {
            string[] splitResult = s.Split(',', 2);
            string left = splitResult[0].Trim();
            string right = splitResult[1].Trim();

            // Parse normally for our primitives
            if (int.TryParse(right, out int newInt))
                AgentBlackboard.SetVariable(left, newInt);
            else
            if (float.TryParse(right, out float newFloat))
                AgentBlackboard.SetVariable(left, newFloat);
            else
            if (bool.TryParse(right, out bool newBool))
                AgentBlackboard.SetVariable(left, newBool);
            else
            if (right.Contains("Vector3"))
            {
                int leftBracketIndex = right.IndexOf('(') + 1;
                int rightBracketIndex = right.IndexOf(')');
                string vectorValueString = right[leftBracketIndex..rightBracketIndex];
                string[] vectorSplit = vectorValueString.Split(',');
                float[] vectorComponents = new float[3];
                for (int i = 0; i < Mathf.Min(vectorSplit.Length, 3); ++i)
                {
                    if (float.TryParse(vectorSplit[i].Trim(), out float vecComponent))
                        vectorComponents[i] = vecComponent;
                }
                AgentBlackboard.SetVariable(left,
                new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]));
            }
            else
                AgentBlackboard.SetVariable(left, right);
        }
    }

    public virtual void HandleSoundHeard(SenseInterest sound)
    {
        
    }

    public virtual void HandleVisualFound(SenseInterest visual)
    {

    }

    public virtual void HandleVisualLost(SenseInterest visual)
    {

    }

    /// <summary>
    /// Set an agent to move to a specified position
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="updatePositionOnly"></param>
    public void MoveAgent(Vector3 newPosition, bool updatePositionOnly = false)
    {
        navAgent.SetDestination(newPosition);

        if (walkingSound == null)
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

        if(walkingSound == null)
            return;

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

    public void TurnHeadToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle((targetPoint - AgentView.AgentHeadRoot.position).normalized, AgentView.AgentRoot.forward, Vector3.up);
        TurnHead(lookAngle, time);
    }

    public void TurnHead(float turnAngle, float time)
    {
        // Adjust our turning angle to be clamped so the head doesn't turn all the way around like an owl
        turnAngle = Mathf.Clamp(turnAngle, -100f, 100f);

        LeanTween.rotateLocal(AgentView.AgentHeadRoot.gameObject, new Vector3(0f, 0f, turnAngle), time);
    }

    public void TurnBodyToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = Vector3.SignedAngle(AgentView.AgentRoot.forward, (targetPoint - AgentView.AgentRoot.position).normalized, Vector3.up);
        TurnBody(lookAngle, time);
    }

    public void TurnBody(float turnAngle, float time)
    {
        LeanTween.rotateAroundLocal(AgentView.AgentRoot.gameObject, Vector3.up, turnAngle, time);
    }

    public bool IsTweeningHead() => LeanTween.isTweening(AgentView.AgentHeadRoot.gameObject);

    protected virtual void OnDrawGizmosSelected()
    {
        if (navAgent.hasPath && !navAgent.isPathStale)
        {
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; ++i)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}