using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspicionModule : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How long in seconds for a suspicious thing to be spotted")]
    private float reactionTimerMax;

    private float reactionTimer;

    private Dictionary<SuspiciousInterest, (bool Visible, float Awareness)> visualSuspectMap = new();

    private List<SuspiciousInterest> visualSuspectList = new();

    private List<SuspiciousInterest> ignoreList = new();

    private int suspicionPriority;

    private SuspiciousInterest currentSuspicion;

    private Agent owner;

    // Start is called before the first frame update
    void Start()
    {
        suspicionPriority = -1;
        reactionTimer = -1f;
        if (TryGetComponent(out Agent myAgent))
            owner = myAgent;
    }

    // Update is called once per frame
    void Update()
    {
        bool checkReaction = false;
        if (reactionTimer == 0f)
        {
            checkReaction = true;
            reactionTimer = -1f;
        }
        else if (reactionTimer > 0f)
            reactionTimer = Mathf.Max(reactionTimer - Time.deltaTime, 0f);

        bool newSuspicionSet = false;
        foreach(var key in visualSuspectList)
        {
            var suspectValues = visualSuspectMap[key];
            float compareAware = suspectValues.Awareness;
            suspectValues.Awareness = Mathf.Clamp(suspectValues.Awareness + (Time.deltaTime * (suspectValues.Visible ? 1f : -1f)), 0f, 2f);
            visualSuspectMap[key] = suspectValues;

            if(suspectValues.Awareness >= 1f)
            {
                if (compareAware < 1f)
                {
                    SetSuspicion(key);
                    newSuspicionSet = true;
                    reactionTimer = reactionTimerMax;
                    break;
                }

                if(checkReaction)
                    if(suspectValues.Awareness >= 2f)
                        owner.AgentBlackboard.SetVariable("suspicionStatus", "confirmed");
                    else
                        owner.AgentBlackboard.SetVariable("suspicionStatus", "unconfirmed");
            }
        }
        if (newSuspicionSet)
            CullSuspects();
    }

    protected virtual void SetSuspicion(SuspiciousInterest newInterest)
    {
        currentSuspicion = newInterest;
        suspicionPriority = currentSuspicion.Priority;
        owner.AgentBlackboard.SetVariable("suspicious", true);
        owner.AgentBlackboard.SetVariable("suspicionStatus", "reacting");
        owner.AgentBlackboard.SetVariable("suspicion", currentSuspicion.gameObject);
        bool isThief = currentSuspicion.CompareTag("Thief");
        owner.AgentBlackboard.SetVariable("thiefFound", isThief);
        if(!isThief)
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
