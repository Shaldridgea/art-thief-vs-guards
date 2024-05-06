using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardSensoryModule : SensoryModule
{
    [SerializeField]
    private VisionCone[] visionCones;

    private List<SenseInterest> sensedObjects = new List<SenseInterest>();

    private float losCheckTimer;

    private bool isAnythingVisible;

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
        if(other.TryGetComponent(out SenseInterest visualInterest))
            if(!sensedObjects.Contains(visualInterest))
                sensedObjects.Add(visualInterest);
    }

    private void HandleVisionExit(VisionCone origin, GameObject other)
    {
        if (other.TryGetComponent(out SenseInterest visualInterest))
            if (sensedObjects.Contains(visualInterest))
                sensedObjects.Remove(visualInterest);
    }

    private void FixedUpdate()
    {
        if(losCheckTimer <= 0f)
        {
            isAnythingVisible = false;
            for(int i = 0; i < sensedObjects.Count; ++i)
            {
                if (Physics.Linecast(owner.AgentView.AgentEyeRoot.position, sensedObjects[i].transform.position, losMask.value))
                    continue;

                isAnythingVisible = true;
                if (awareness >= 1f)
                    NotifyVisual(sensedObjects[i]);
            }

            losCheckTimer = losCheckInterval;
        }
        else
            losCheckTimer -= Time.fixedDeltaTime;

        if (isAnythingVisible)
            awareness = Mathf.Min(awareness + awarenessChange * Time.fixedDeltaTime, 5f);
        else
            awareness = Mathf.Max(awareness - awarenessChange * Time.fixedDeltaTime, 0f);
    }
}
