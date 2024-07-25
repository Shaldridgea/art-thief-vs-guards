using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualInterest : SenseInterest
{
    [SerializeField]
    private LayerMask raycastMask;

    [SerializeField]
    private bool trackMovement;

    [SerializeField]
    private bool trackLighting;

    public bool IsMoving { get; private set; }

    private Vector3 lastPosition;

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
        if (!trackLighting)
            return;

        lightSources.Add(newSource);
        lightVisibleMap.Add(newSource, false);
    }

    public void ExitedLight(Light newSource)
    {
        if (!trackLighting)
            return;

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

        if (!trackMovement)
            return;

        if (transform.position != lastPosition)
        {
            IsMoving = true;
            lastPosition = transform.position;
        }
        else
            IsMoving = false;
    }
}
