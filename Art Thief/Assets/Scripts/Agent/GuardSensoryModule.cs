using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardSensoryModule : SensoryModule
{
    [SerializeField]
    private VisionCone[] visionCones;

    private List<SenseInterest> inConeObjects = new List<SenseInterest>();

    private Dictionary<SenseInterest, bool> visibilityMap = new Dictionary<SenseInterest, bool>();

    private Dictionary<SenseInterest, int> entryCountMap = new Dictionary<SenseInterest, int>();

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
