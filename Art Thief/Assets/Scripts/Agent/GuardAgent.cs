using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GuardAgent : Agent
{
    [SerializeField]
    private BTGraph behaviourTreeGraph;

    [SerializeField]
    private float treeUpdateInterval;

    [SerializeField]
    private PatrolPath regularPatrol;

    [SerializeField]
    private PatrolPath perimeterPatrol;

    [SerializeField]
    private List<string> blackboardDefaults;

    private BehaviourTree agentTree;

    private Transform targetPoint;

    private int patrolIndex;

    private float treeUpdateTimer;

    private SuspicionModule suspicion;

    static GuardAgent debuggingAgent;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (TryGetComponent(out SuspicionModule susModule))
            suspicion = susModule;

        // Create our behaviour tree based on the graph blueprint provided
        agentTree = BehaviourTreeFactory.MakeTree(behaviourTreeGraph, this);

        // Set our default blackboard values by parsing the strings for their keys and values
        foreach(string s in blackboardDefaults)
        {
            string[] splitResult = s.Split(',');
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

    private void Update()
    {
        // Update our behaviour tree
        if (treeUpdateTimer <= 0f)
        {
            treeUpdateTimer = treeUpdateInterval;
            agentTree.Update();
            return;
        }
        treeUpdateTimer -= Time.deltaTime;
    }

    public override void HandleSoundHeard(SenseInterest sound)
    {
        base.HandleSoundHeard(sound);
        // Don't treat unimportant friendly sounds as suspicious
        if (sound.OwnerTeam == Consts.Team.GUARD && !sound.IsSuspicious)
            return;

        if (!sound.TryGetComponent(out SuspiciousInterest suspect))
            return;

        suspicion.OnSuspicionSensed(suspect, Consts.SuspicionType.Sound);
    }

    public override void HandleVisualFound(SenseInterest visual)
    {
        base.HandleVisualFound(visual);
        // Check if other guards appear suspicious or not i.e. unconscious on the ground
        if (visual.OwnerTeam == Consts.Team.GUARD && !visual.IsSuspicious)
            return;

        if (!visual.TryGetComponent(out SuspiciousInterest suspect))
            return;

        suspicion.OnSuspicionSensed(suspect, Consts.SuspicionType.Visual);
    }

    public override void HandleVisualLost(SenseInterest visual)
    {
        base.HandleVisualLost(visual);
        // Check if other guards appear suspicious or not i.e. unconscious on the ground
        if (visual.OwnerTeam == Consts.Team.GUARD && !visual.IsSuspicious)
            return;

        if (!visual.TryGetComponent(out SuspiciousInterest suspect))
            return;

        suspicion.OnVisualSuspectLost(suspect);
    }

    public Vector3 GetNextPatrolPoint(Consts.PatrolPathType pathType)
    {
        PatrolPath path = pathType == Consts.PatrolPathType.Regular ? regularPatrol : perimeterPatrol;
        Vector3 point = path.GetPoint(patrolIndex);
        ++patrolIndex;
        return point;
    }

    public void TurnHeadToPoint(Vector3 targetPoint, float time)
    {
        float lookAngle = -Vector3.SignedAngle(AgentView.AgentRoot.forward, (targetPoint - AgentView.AgentHeadRoot.position).normalized, Vector3.up);
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

    private void OnMouseDown()
    {
        debuggingAgent = this;
    }

    private void OnGUI()
    {
        if (debuggingAgent != this)
            return;

        GUIStyle style = new GUIStyle("box");
        style.fontSize = 20;
        GUILayout.Box(name, style);
        style.fontSize = 15;
        foreach (var i in AgentBlackboard.GetData())
            GUILayout.Box($"{i.Key}: {i.Value}", style);
    }

    private void OnDrawGizmosSelected()
    {
        if(navAgent.hasPath && !navAgent.isPathStale)
        {
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length-1; ++i)
            {
                Vector3 start = corners[i];
                Vector3 end = corners[i + 1];
                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
            }
        }

        Color gizColor = Color.yellow;
        gizColor.a = 0.25f;
        Gizmos.color = gizColor;
        Gizmos.DrawSphere(transform.position, SensoryModule.INTEREST_RADIUS);
    }

    [Button("Test head turn", EButtonEnableMode.Playmode)]
    private void TestHeadTurn()
    {
        TurnHeadToPoint(regularPatrol.GetPoint(0), 2f);
    }

    [Button("Test body turn", EButtonEnableMode.Playmode)]
    private void TestBodyTurn()
    {
        TurnBodyToPoint(regularPatrol.GetPoint(0), 2f);
    }
}