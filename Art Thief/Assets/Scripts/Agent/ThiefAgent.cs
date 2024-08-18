using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using UnityEngine.AI;

public class ThiefAgent : Agent
{
    [SerializeField]
    private float dangerDistanceMin = 9f;
    [SerializeField]
    private float dangerDistanceMax = 20f;

    [SerializeField]
    private Transform artHolderTransform;

    public ThiefSensoryModule ThiefSenses => (ThiefSensoryModule)senses;

    public Transform ArtGoal { get; set; }

    private bool usingOffMeshLink;

    private float dangerVelocity;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (!AgentActivated)
            return;

        if (!usingOffMeshLink && navAgent.hasPath && navAgent.isOnOffMeshLink)
        {
            StartCoroutine(FollowPathOffMeshLink());
            usingOffMeshLink = true;
        }

        float danger = 0f;
        float aggression = 0f;
        var guards = ThiefSenses.AwareGuards;
        int guardThreats = 0;
        bool beingChased = false;
        foreach(var g in guards)
        {
            if (g.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            ++guardThreats;
            bool hasLos = g.GuardSenses.IsSeen(transform.position);
            float distanceToGuard = Vector3.Distance(transform.position, g.transform.position);
            danger += Mathf.InverseLerp(dangerDistanceMax, dangerDistanceMin, distanceToGuard);

            if (distanceToGuard < aggroRadius)
                if (Vector3.Angle(transform.forward, (g.transform.position - transform.position).normalized) <= aggroAngle)
                    aggression += 1f;
            if (hasLos)
                danger += 0.5f;
            if (g.AgentBlackboard.GetVariable<string>("guardMode") == "chase")
                beingChased = true;
        }
        danger /= Mathf.Max(guardThreats, 1);
        danger += guardThreats * 0.1f;
        AgentBlackboard.SetVariable("inChase", beingChased);
        if (beingChased)
            danger += 1f;
        float storedDanger = AgentBlackboard.GetVariable<float>("danger");
        // Immediately reflect calculated danger in blackboard if it's higher,
        // otherwise bring it down slowly if it's lower
        if (danger > storedDanger)
        {
            storedDanger = danger;
            dangerVelocity = 0.1f;
        }
        else
            storedDanger = Mathf.SmoothDamp(storedDanger, danger, ref dangerVelocity, 4f, 0.5f);
        AgentBlackboard.SetVariable("danger", storedDanger);
        AgentBlackboard.SetVariable("aggro", aggression);
    }

    private IEnumerator FollowPathOffMeshLink()
    {
        bool reachedStartFirst = false;

        Vector3 startPos = navAgent.currentOffMeshLinkData.startPos;
        startPos.y = transform.position.y;

        Vector3 endPos = navAgent.currentOffMeshLinkData.endPos;
        if (NavMesh.SamplePosition(endPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            endPos = hit.position;

        float frameMovementSpeed;

        navAgent.velocity = Vector3.zero;

        do
        {
            Vector3 goalPos = reachedStartFirst ? endPos : startPos;

            frameMovementSpeed = navAgent.speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(
                transform.position, goalPos, frameMovementSpeed);

            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(goalPos.ZeroY() - transform.position.ZeroY(), Vector3.up),
                navAgent.angularSpeed * Time.deltaTime);

            if (!reachedStartFirst && Vector3.Distance(transform.position.ZeroY(), goalPos.ZeroY()) <= frameMovementSpeed)
                reachedStartFirst = true;

            yield return null;
        }
        while (Vector3.Distance(transform.position.ZeroY(), endPos.ZeroY()) > frameMovementSpeed
        && navAgent.isOnOffMeshLink);
        navAgent.CompleteOffMeshLink();
        usingOffMeshLink = false;
    }

    public void TakeArt()
    {
        if (ArtGoal == null)
            return;

        if (ArtGoal.TryGetComponent(out GalleryArt art))
        {
            if (art.ShouldTakeObject)
            {
                art.TargetMesh.transform.SetParent(artHolderTransform, false);
                art.TargetMesh.transform.localPosition = Vector3.zero;
            }
            else
                art.RemoveArtImage();

            if (ArtGoal.TryGetComponent(out VisualInterest visual))
                visual.SetSuspicious(true);
        }
        
        AgentBlackboard.SetVariable("artStolen", 1f);
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, HideAction.CHECK_RADIUS);
        Handles.DrawWireDisc(transform.position + new Vector3(0f, 0.5f), Vector3.up, dangerDistanceMin);
        Handles.DrawWireDisc(transform.position + new Vector3(0f, 0.5f), Vector3.up, dangerDistanceMax);

        if (!EditorApplication.isPlaying)
            return;

        Handles.color = Color.black;
        GUI.color = Color.black;
        GUIStyle style = GUIStyle.none;
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleCenter;
        Handles.Label(transform.position, $"Danger: {AgentBlackboard.GetVariable<float>("danger")}", style);
    }
#endif
}