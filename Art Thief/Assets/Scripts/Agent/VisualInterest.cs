using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualInterest : SenseInterest
{
    [SerializeField]
    [Tooltip("Mask for lighting LOS checks to see whether we're illuminated by a light")]
    private LayerMask raycastMask;

    [SerializeField]
    [Tooltip("Turn on to track whether we are moving about or not. Use this for interests that will be moving themselves")]
    private bool trackMovement;

    [SerializeField]
    [Tooltip("Turn on to track our illumination in lights every frame or not. Use this for interests that move around a lot")]
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

    /// <summary>
    /// Lights that this interest is inside of
    /// </summary>
    private List<Light> lightSources = new();

    /// <summary>
    /// Map of whether a light is able to illuminate this interest
    /// </summary>
    private Dictionary<Light, bool> lightVisibleMap = new();

    private bool checkLightingNextUpdate;

    public void EnteredLight(Light newSource)
    {
        lightSources.Add(newSource);
        lightVisibleMap.Add(newSource, false);

        checkLightingNextUpdate = true;
    }

    public void ExitedLight(Light newSource)
    {
        if(lightVisibleMap[newSource])
            --litCount;
        lightSources.Remove(newSource);
        lightVisibleMap.Remove(newSource);

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

        // Track dynamic lighting means we check if we're illuminated every frame no matter what.
        // checkLightingNextUpdate is set every time we enter or exit a light collider
        // and only does one check for being illuminated, since every frame doesn't matter for us
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
