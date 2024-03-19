using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField]
    private AgentView ownerView;

    [SerializeField]
    private LayerMask losMask;

    public delegate void VisionDelegate(VisionCone callingCone, ThiefAgent spottedAgent);

    public event VisionDelegate TriggerEnter;

    public event VisionDelegate TriggerExit;

    private ThiefAgent targetAgent;

    public bool HasLineOfSight(ThiefAgent targetThief = null)
    {
        ThiefAgent lookAtThief = targetThief ?? targetAgent;

        if (lookAtThief == null)
            return false;

        return !Physics.Linecast(ownerView.AgentHeadRoot.position, lookAtThief.AgentView.AgentHeadRoot.position, losMask);
    }
}