using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualInterest : SenseInterest
{
    [SerializeField]
    private LayerMask raycastMask;

    private int litCount;

    public bool IsLitUp {
        get => litCount > 0;
        set {
            if (value)
                ++litCount;
            else
                --litCount;
        }
    }

    private List<Light> lightSources = new();

    private Dictionary<Light, bool> lightVisibleMap = new();

    public void EnteredLight(Light newSource)
    {
        lightSources.Add(newSource);
        lightVisibleMap.Add(newSource, false);
    }

    public void ExitedLight(Light newSource)
    {
        if(lightVisibleMap[newSource])
            --litCount;
        lightSources.Remove(newSource);
        lightVisibleMap.Remove(newSource);
    }

    private void Update()
    {
        foreach(var l in lightSources)
        {
            bool lightBlocked = Physics.Linecast(l.transform.position,
                transform.position, raycastMask.value, QueryTriggerInteraction.Collide);

            if (lightVisibleMap[l] && lightBlocked)
                --litCount;
            else if (!lightVisibleMap[l] && !lightBlocked)
                ++litCount;
        }
    }
}
