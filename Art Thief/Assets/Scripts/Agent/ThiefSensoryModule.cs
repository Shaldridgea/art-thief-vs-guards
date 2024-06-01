using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefSensoryModule : SensoryModule
{
    private ThiefAgent thief;

    [SerializeField]
    private List<GuardAgent> awareGuards;

    public List<GuardAgent> AwareGuards => awareGuards;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        thief = (ThiefAgent)owner;
    }

    public override void NotifySound(SenseInterest sound)
    {
        return;
    }
}