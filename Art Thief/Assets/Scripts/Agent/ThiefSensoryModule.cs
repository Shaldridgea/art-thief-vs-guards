using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefSensoryModule : SensoryModule
{
    [SerializeField]
    [Tooltip("How long after we last knew where a guard was do we lose track of them")]
    private float loseGuardTime;

    private ThiefAgent thief;

    [SerializeField]
    private List<GuardAgent> awareGuards;

    public List<GuardAgent> AwareGuards => awareGuards;

    private Dictionary<GuardAgent, float> loseGuardTimerMap = new();

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        thief = (ThiefAgent)owner;
    }

    private void Update()
    {
        for(int i = awareGuards.Count - 1; i >= 0; --i)
        {
            GuardAgent guard = awareGuards[i];

            if ((loseGuardTimerMap[guard] -= Time.deltaTime) <= 0f)
                LoseGuard(guard);
        }
    }

    private void NoticeGuard(GuardAgent guard)
    {
        if (!awareGuards.Contains(guard))
        {
            awareGuards.Add(guard);
            loseGuardTimerMap.Add(guard, loseGuardTime);
        }
        else
            loseGuardTimerMap[guard] = loseGuardTime;
    }

    private void LoseGuard(GuardAgent guard)
    {
        awareGuards.Remove(guard);
        loseGuardTimerMap.Remove(guard);
    }

    public override void NotifySound(SenseInterest sound)
    {
        base.NotifySound(sound);

        // Ignore our own sounds
        if (sound.OwnerTeam == Consts.Team.THIEF)
            return;

        if (sound.Owner == null)
            return;

        if (sound.Owner.TryGetComponent(out GuardAgent guard))
            NoticeGuard(guard);
    }

    public override void NotifyVisualFound(SenseInterest visual)
    {
        base.NotifyVisualFound(visual);
        if (visual.OwnerTeam == Consts.Team.THIEF)
            return;

        if (visual.Owner == null)
            return;

        if(visual.Owner.TryGetComponent(out GuardAgent guard))
            NoticeGuard(guard);
    }

    public override void NotifyVisualLost(SenseInterest visual)
    {
        base.NotifyVisualLost(visual);
        if (visual.OwnerTeam == Consts.Team.THIEF)
            return;

        if (visual.Owner == null)
            return;

        if (visual.Owner.TryGetComponent(out GuardAgent guard))
            LoseGuard(guard);
    }
}