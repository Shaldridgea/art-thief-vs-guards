using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspicionModule : MonoBehaviour
{
    private Dictionary<SuspiciousInterest, (bool Visible, float Awareness)> visualSuspectMap = new Dictionary<SuspiciousInterest, (bool Visible, float Awareness)>();

    private List<SuspiciousInterest> visualSuspectList = new List<SuspiciousInterest>();

    private List<SuspiciousInterest> ignoreList = new List<SuspiciousInterest>();

    private int suspicionPriority;

    private SuspiciousInterest currentSuspicion;

    private Agent owner;

    // Start is called before the first frame update
    void Start()
    {
        suspicionPriority = -1;
        if (TryGetComponent(out Agent myAgent))
            owner = myAgent;
    }

    // Update is called once per frame
    void Update()
    {
        bool newSuspicionSet = false;
        foreach(var key in visualSuspectList)
        {
            if (ignoreList.Contains(key))
                continue;

            var suspectValues = visualSuspectMap[key];
            suspectValues.Awareness = Mathf.Clamp(suspectValues.Awareness + (Time.deltaTime * (suspectValues.Visible ? 1f : -1f)), 0f, 1f);
            visualSuspectMap[key] = suspectValues;

            if(visualSuspectMap[key].Awareness >= 1f)
            {
                SetSuspicion(key);
                newSuspicionSet = true;
                break;
            }
        }
        if (newSuspicionSet)
            CullSuspects();
    }

    private void SetSuspicion(SuspiciousInterest newInterest)
    {
        currentSuspicion = newInterest;
        suspicionPriority = currentSuspicion.Priority;
        owner.AgentBlackboard.SetVariable("suspicious", true);
        owner.AgentBlackboard.SetVariable("suspicionFound", true);
        owner.AgentBlackboard.SetVariable("suspicion", currentSuspicion.gameObject);
        ignoreList.Add(newInterest);
    }

    private void CullSuspects()
    {
        for (int i = visualSuspectList.Count-1; i >= 0; --i)
        {
            var key = visualSuspectList[i];
            if(key.Priority < suspicionPriority)
            {
                visualSuspectList.RemoveAt(i);
                visualSuspectMap.Remove(key);
            }
        }
    }

    public bool OnSuspicionSensed(SuspiciousInterest newInterest, Consts.SuspicionType suspicionType)
    {
        if (ignoreList.Contains(newInterest))
            return false;

        if (currentSuspicion == null || newInterest.Priority >= suspicionPriority)
        {
            if (suspicionType == Consts.SuspicionType.Sound)
            {
                SetSuspicion(newInterest);
                CullSuspects();
            }
            else
            {
                if (visualSuspectMap.TryGetValue(newInterest, out var value))
                {
                    value.Visible = true;
                    visualSuspectMap[newInterest] = value;
                }
                else
                {
                    visualSuspectMap[newInterest] = (true, 0f);
                    visualSuspectList.Add(newInterest);
                }
            }
            return true;
        }

        return false;
    }

    public void OnVisualSuspectLost(SuspiciousInterest lostInterest)
    {
        if (ignoreList.Contains(lostInterest))
            return;

        var value = visualSuspectMap[lostInterest];
        value.Visible = false;
        visualSuspectMap[lostInterest] = value;
    }

    public Dictionary<SuspiciousInterest, (bool Visible, float Awareness)> GetSuspectData() => visualSuspectMap;
}
