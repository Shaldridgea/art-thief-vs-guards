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

    [SerializeField]
    private GameObject stealingProgressContainer;

    public GameObject StealingProgressContainer => stealingProgressContainer;

    [SerializeField]
    private UnityEngine.UI.Image stealingProgressImage;

    public UnityEngine.UI.Image StealingProgressImage => stealingProgressImage;

    private bool usingOffMeshLink;

    private float dangerVelocity;
    
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

        // Set true or false as a float for whether we're near enough to the art to steal it
        AgentBlackboard.SetVariable("nearToArt",
            Vector3.Distance(transform.position.ZeroY(), ArtGoal.transform.position.ZeroY()) <= 1f ? 1f : 0f);

        // Motive calculation for Danger and Aggression
        float danger = 0f;
        float aggression = AgentBlackboard.GetVariable<float>("aggro");
        var guards = ThiefSenses.AwareGuards;
        int guardThreats = 0;
        bool beingChased = false;
        foreach(var g in guards)
        {
            // Ignore guards who aren't a threat to us
            if (g.AgentBlackboard.GetVariable<bool>(Consts.AGENT_STUN_STATUS))
                continue;

            ++guardThreats;
            bool seenByGuard = g.GuardSenses.IsSeen(transform.position);
            float distanceToGuard = Vector3.Distance(transform.position, g.transform.position);
            bool isGuardChasing = g.IsInChase();
            // Add more danger the closer a guard is to us
            danger += Mathf.InverseLerp(dangerDistanceMax, dangerDistanceMin, distanceToGuard);

            // Add aggression if we're able to attack currently and
            // there's a guard close enough who we can see
            bool wantToAttack = false;
            if (CanAttackEnemy())
            {
                if (distanceToGuard <= aggroRadius)
                {
                    if (Vector3.Angle(transform.forward, (g.transform.position - transform.position).normalized) <= aggroAngle)
                    {
                        if (senses.IsInLOS(g.transform.position))
                        {
                            wantToAttack = true;

                            if (isGuardChasing)
                                aggression += 1.2f * Time.deltaTime;
                            else
                                aggression += 0.6f * Time.deltaTime;
                        }
                    }
                }
            }
            if (!wantToAttack)
                aggression = 0f;

            // Add more danger if we're currently seen by a guard
            if (seenByGuard)
                danger += 0.5f;

            // Flag whether we're being chased by any guard
            if (isGuardChasing)
                beingChased = true;
        }
        // Average our danger by the number of guards and add a bit extra
        // to reflect the danger of multiple guards being nearby
        danger /= Mathf.Max(guardThreats, 1);
        danger += guardThreats * 0.1f;

        // Reflect being chased in our blackboard and increase our danger significantly if so
        AgentBlackboard.SetVariable(Consts.THIEF_CHASE_STATUS, beingChased);
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

    private void LateUpdate()
    {
        // Guards win the simulation if thief is stunned
        if (AgentBlackboard.GetVariable<bool>(Consts.AGENT_STUN_STATUS))
        {
            GameController.Instance.EndGame(Consts.Team.GUARD);
            enabled = false;
        }
    }

    /// <summary>
    /// Manually move and rotate through the points of an OffMeshLink
    /// </summary>
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

        // Move and rotate the agent through an OffMeshLink
        do
        {
            // Set our goal position based on if we went to the start yet
            Vector3 goalPos = reachedStartFirst ? endPos : startPos;

            frameMovementSpeed = navAgent.speed * Time.deltaTime;

            transform.SetPositionAndRotation
            (
                Vector3.MoveTowards(transform.position, goalPos, frameMovementSpeed),

                Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(goalPos.ZeroY() - transform.position.ZeroY(), Vector3.up),
                navAgent.angularSpeed * Time.deltaTime)
            );

            // Go to the start of the link if we weren't close enough first
            if (!reachedStartFirst && Vector3.Distance(transform.position.ZeroY(), goalPos.ZeroY()) <= frameMovementSpeed)
                reachedStartFirst = true;

            // Yield to run every frame
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
            // Art is either a painting or a statue. We take the mesh
            // of a statue with us, otherwise we replace the painting texture
            if (art.ShouldTakeObject)
            {
                art.TargetMesh.transform.SetParent(artHolderTransform, false);
                art.TargetMesh.transform.localPosition = Vector3.zero;
            }
            else
                art.RemoveArtImage();

            // The stolen art should be marked as suspicious to alert guards
            if (ArtGoal.TryGetComponent(out VisualInterest visual))
                visual.SetSuspicious(true);
        }
        
        AgentBlackboard.SetVariable("artStolen", 1f);
    }

    public override void AttackAgent(Agent targetAgent)
    {
        if(targetAgent.CanAttackBack(this))
        {
            // Decide who the winner of a struggle is.
            // We're the winner if the enemy can't win,
            // otherwise it's a 50/50
            bool winnerIsMe = false;
            if(CanWinStruggle())
            {
                if (!targetAgent.CanWinStruggle())
                    winnerIsMe = true;
                else if (Random.value <= 0.5f)
                    winnerIsMe = true;
            }

            // Start animated fight interaction between thief and guard
            SetupAttack(this, targetAgent);
            PlayStruggleSequence(winnerIsMe);
            targetAgent.PlayStruggleSequence(!winnerIsMe);
            // Mark everyone as interacting and disable agent logic to not interrupt attack
            targetAgent.AgentBlackboard.SetVariable(Consts.AGENT_INTERACT_STATUS, true);
            AgentBlackboard.SetVariable(Consts.AGENT_INTERACT_STATUS, true);
            DeactivateAgent();
        }
    }

    public override bool CanAttackBack(Agent attacker)
    {
        // Thief can only attack back if they're allowed to attack at all
        // and if they're facing towards their attacker enough
        return CanAttackEnemy() && Vector3.Dot(attacker.transform.forward, transform.forward) < -0.4f;
    }

    public override bool CanWinStruggle()
    {
        return true;
    }

    /// <summary>
    /// Gets the position of the agent or the closest equivalent
    /// that is always a valid position on the NavMesh for pathfinding
    /// </summary>
    public Vector3 GetNavMeshSafePosition()
    {
        if (!navAgent.isOnNavMesh)
        {
            // Sample position will find the nearest position on the nav mesh from where we are
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navAgent.height * 3f, navAgent.areaMask))
            {
                return hit.position;
            }
            else // If SamplePosition somehow doesn't find a position we use the centre of the room as a failsafe
            {
                return CurrentRoom.transform.position;
            }
        }
        else // If we're on the nav mesh anyway we just return our normal position
            return transform.position;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, HideAction.CHECK_RADIUS);
        Handles.DrawWireDisc(transform.position + new Vector3(0f, 0.5f), Vector3.up, dangerDistanceMin);
        Handles.DrawWireDisc(transform.position + new Vector3(0f, 0.5f), Vector3.up, dangerDistanceMax);
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position + new Vector3(0f, 0.5f), Vector3.up, aggroRadius);

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