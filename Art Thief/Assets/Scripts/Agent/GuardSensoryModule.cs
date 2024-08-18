﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardSensoryModule : SensoryModule
{
    public override void NotifySound(SenseInterest sound)
    {
        base.NotifySound(sound);
        // Ignore un-suspicious sounds (usually friendly sounds)
        if (!sound.IsSuspicious)
            return;

        owner.AgentBlackboard.SetVariable("lastHeardSound", sound);

        (owner as GuardAgent).Suspicion.OnSuspicionSensed(sound, Consts.SuspicionType.Sound);
    }

    public override void NotifyVisualFound(SenseInterest visual)
    {
        base.NotifyVisualFound(visual);
        // Exit if this interest belongs to a guard and it's not flagged as suspicious
        if (!visual.IsSuspicious)
            return;

        (owner as GuardAgent).Suspicion.OnSuspicionSensed(visual, Consts.SuspicionType.Visual);
    }

    public override void NotifyVisualLost(SenseInterest visual)
    {
        base.NotifyVisualLost(visual);
        // Exit if this interest belongs to a guard and it's not flagged as suspicious
        if (!visual.IsSuspicious)
            return;

        (owner as GuardAgent).Suspicion.OnVisualSuspectLost(visual);
    }
}
