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
    private bool trackDynamicLighting;

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

    private bool checkLightingNextUpdate;

    public void EnteredLight(Light newSource)
    {
        lightSources.Add(newSource);
        lightVisibleMap.Add(newSource, false);

        if (!trackDynamicLighting)
            checkLightingNextUpdate = true;
    }

    public void ExitedLight(Light newSource)
    {
        if(lightVisibleMap[newSource])
            --litCount;
        lightSources.Remove(newSource);
        lightVisibleMap.Remove(newSource);

        if (!trackDynamicLighting)
            checkLightingNextUpdate = true;
    }

    private void FixedUpdate()
    {
        if (trackMovement)
            if (transform.position != lastPosition)
            {
                IsMoving = true;
                lastPosition = transform.position;
            }
            else
                IsMoving = false;

        if (trackDynamicLighting || checkLightingNextUpdate)
        {
            foreach (var l in lightSources)
            {
                bool lightBlocked = Physics.Linecast(l.transform.position,
                    transform.position, raycastMask.value, QueryTriggerInteraction.Collide);

                if (lightVisibleMap[l] && lightBlocked)
                    --litCount;
                else if (!lightVisibleMap[l] && !lightBlocked)
                    ++litCount;
            }

            checkLightingNextUpdate = false;
        }
    }

    private void OnDisable()
    {
        lightSources.Clear();
        lightVisibleMap.Clear();
    }
}
