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

    private void Update()
    {
        for(int i = awareGuards.Count - 1; i >= 0; --i)
        {
            GuardAgent guard = awareGuards[i];
            // If out of ear shot so we don't know where they are
            if (Vector3.Distance(transform.position, guard.transform.position) > 30f)
                awareGuards.RemoveAt(i);
        }
    }
}