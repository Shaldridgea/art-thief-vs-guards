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
    [Tooltip("Distance thief must be within to aggro on guard")]
    private float aggroRadius = 1f;
    public float AggroRadius => aggroRadius;
    [SerializeField]
    [Tooltip("Vision angle of thief to aggro on a guard")]
    private float aggroAngle = 15f;
    public float AggroAngle => aggroAngle;

    public ThiefSensoryModule ThiefSenses => (ThiefSensoryModule)senses;

    private Transform artGoal;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (artGoal == null)
            artGoal = GameController.Instance.ArtGoal;

        AgentBlackboard.SetVariable("nearToArt", Vector3.Distance(transform.position.ZeroY(), artGoal.position.ZeroY()) <= 1f ? 1f : 0f);

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
            bool hasLos = g.GuardSenses.IsInLOS(transform.position);
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
        if (beingChased)
            danger += 1f;
        float storedDanger = AgentBlackboard.GetVariable<float>("danger");
        //storedDanger = Mathf.MoveTowards(storedDanger, danger, Time.deltaTime);
        storedDanger = danger;
        AgentBlackboard.SetVariable("danger", storedDanger);
        AgentBlackboard.SetVariable("aggro", aggression);
    }

    public void TakeArt()
    {
        if (artGoal == null)
            return;

        artGoal.SetParent(transform, true);
        AgentBlackboard.SetVariable("artStolen", 1f);
    }

    public override void HandleSoundHeard(SenseInterest sound)
    {
        // Ignore our own sounds
        if (sound.OwnerTeam == Consts.Team.THIEF)
            return;

        if (sound.Owner != null)
            if (sound.Owner.TryGetComponent(out GuardAgent guard))
            {
                var list = ThiefSenses.AwareGuards;
                if(!list.Contains(guard))
                    list.Add(guard);
            }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 15f);
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
}