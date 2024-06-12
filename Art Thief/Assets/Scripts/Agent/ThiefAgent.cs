using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.AI;

public class ThiefAgent : Agent
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public override void HandleSoundHeard(SenseInterest sound)
    {
        // Ignore our own sounds
        if (sound.OwnerTeam == Consts.Team.THIEF)
            return;

        if (sound.Owner != null)
            if (sound.Owner.TryGetComponent(out GuardAgent guard))
            {
                var list = (senses as ThiefSensoryModule).AwareGuards;
                if(!list.Contains(guard))
                    list.Add(guard);
            }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}