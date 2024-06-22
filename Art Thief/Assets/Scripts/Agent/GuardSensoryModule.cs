using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardSensoryModule : SensoryModule
{
    [SerializeField]
    private VisionCone[] visionCones;

    private List<SenseInterest> inConeObjects = new();

    private Dictionary<SenseInterest, bool> visibilityMap = new();

    private Dictionary<SenseInterest, int> entryCountMap = new();

    private float losCheckTimer;

    protected override void Start()
    {
        base.Start();
        foreach (VisionCone v in visionCones)
        {
            v.TriggerEnter += HandleVisionEnter;
            v.TriggerExit += HandleVisionExit;
        }
    }

    public bool IsInLOS(Vector3 checkPosition, Vector3 guardForward = default)
    {
        if (guardForward == default)
            guardForward = owner.AgentView.AgentEyeRoot.forward;

        float lookAngle = Vector3.Angle(guardForward,
            (checkPosition.ZeroY() - transform.position.ZeroY()).normalized);

        if (lookAngle <= VIEW_ANGLE)
            if (!Physics.Linecast(checkPosition, owner.AgentView.AgentEyeRoot.position, losMask, QueryTriggerInteraction.Ignore))
                return true;

        return false;
    }

    private void HandleVisionEnter(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out VisualInterest visualInterest))
            if (!inConeObjects.Contains(visualInterest))
            {
                inConeObjects.Add(visualInterest);
                visibilityMap[visualInterest] = false;
                entryCountMap[visualInterest] = 1;
            }
            else
                entryCountMap[visualInterest]++;
    }

    private void HandleVisionExit(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out VisualInterest visualInterest))
            if (inConeObjects.Contains(visualInterest))
            {
                if (--entryCountMap[visualInterest] == 0)
                {
                    if(visibilityMap[visualInterest])
                        NotifyVisualLost(visualInterest);

                    inConeObjects.Remove(visualInterest);
                    visibilityMap.Remove(visualInterest);
                    entryCountMap.Remove(visualInterest);
                }
            }
    }

    private void FixedUpdate()
    {
        if(losCheckTimer <= 0f)
        {
            for (int i = 0; i < inConeObjects.Count; ++i)
            {
                SenseInterest target = inConeObjects[i];
                bool isSeen = !Physics.Linecast(owner.AgentView.AgentEyeRoot.position, target.transform.position, losMask.value);

                if (!visibilityMap[target] && isSeen)
                    NotifyVisualFound(target);
                else if (visibilityMap[target] && !isSeen)
                    NotifyVisualLost(target);

                visibilityMap[target] = isSeen;
            }

            losCheckTimer = losCheckInterval;
        }
        else
            losCheckTimer -= Time.fixedDeltaTime;
    }
}
