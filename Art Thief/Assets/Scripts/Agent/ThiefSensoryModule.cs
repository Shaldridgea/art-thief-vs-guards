using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefSensoryModule : SensoryModule
{
    [SerializeField]
    [Tooltip("How long after we last knew where a guard was do we lose track of them")]
    private float loseGuardTime;

    [SerializeField]
    private List<GuardAgent> awareGuards;

    public List<GuardAgent> AwareGuards => awareGuards;

    private Dictionary<GuardAgent, float> loseGuardTimerMap = new();

    override protected void Start()
    {
        base.Start();
    }

    private void Update()
    {
        // Check if we start losing track of where a guard is or not
        // and count down timer for losing track
        for(int i = awareGuards.Count - 1; i >= 0; --i)
        {
            GuardAgent guard = awareGuards[i];

            // If we still know where the guard is then skip over them
            if (IsInLOS(guard.transform.position))
                continue;

            // If the guard is not moving and we hadn't already started to lose them
            // then we assume it's in the last place we knew and skip over them
            if (!(guard.NavAgent.hasPath && guard.NavAgent.remainingDistance > 1f) &&
                loseGuardTimerMap[guard] == loseGuardTime)
                continue;

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
}