using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpySensoryModule : SensoryModule
{
    private ThiefAgent spy;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        spy = (ThiefAgent)owner;
    }

    public override void SoundHeard(SoundTrigger sound)
    {
        throw new System.NotImplementedException();
    }
}