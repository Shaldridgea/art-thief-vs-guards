using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspicionModule : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How long in seconds for a suspicious thing to be spotted")]
    private float reactionTimerMax;

    private float reactionTimer;

    private Dictionary<SenseInterest, (bool Visible, float Awareness)> visualSuspectMap = new();

    private List<SenseInterest> visualSuspectList = new();

    private List<SenseInterest> ignoreList = new();

    private int suspicionPriority;

    private SenseInterest currentSuspicion;

    private GuardAgent owner;

    // Start is called before the first frame update
    void Start()
    {
        suspicionPriority = -1;
        reactionTimer = -1f;
        if (TryGetComponent(out GuardAgent myAgent))
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

            // Awareness delta is how fast the guard becomes aware/suspicious of something
            // 1 is the baseline of taking 1 second to become aware
            // 2 would take 2 seconds, 0.5 would be half a second etc.
            float awarenessDelta = 0.5f;
            // Add more time to awareness if we're in peripheral vision
            if (!owner.GuardSenses.IsInCentralVision(key.gameObject))
                awarenessDelta += 1f;

            // Change awareness factor based on distance
            awarenessDelta *= Mathf.Lerp(0.25f, 1.5f,
                Mathf.InverseLerp(5f, 20f,
                Vector3.Distance(transform.position.ZeroY(), key.transform.position.ZeroY())));

            VisualInterest visual = (key as VisualInterest);

            // Reduce the reaction time if the visual interest is moving
            if (visual.IsMoving)
                awarenessDelta *= 0.5f;

            // Change awareness factor to take 3 times as long if interest is in the dark
            if (!visual.IsLitUp)
                awarenessDelta *= 3f;

            suspectValues.Awareness =
                Mathf.Clamp(suspectValues.Awareness +
                (Time.deltaTime / (suspectValues.Visible ? awarenessDelta : -1f)), 0f, 2f);

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
            }

            if (currentSuspicion == key)
                if (suspectValues.Awareness >= 2f)
                    owner.AgentBlackboard.SetVariable("suspicionStatus", "confirmed");
                else if(checkReaction)
                    owner.AgentBlackboard.SetVariable("suspicionStatus", "unconfirmed");
        }
        if (newSuspicionSet)
            CullSuspects();
    }

    private void SetSuspicion(SenseInterest newInterest)
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

    public bool OnSuspicionSensed(SenseInterest newInterest, Consts.SuspicionType suspicionType)
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

    public void OnVisualSuspectLost(SenseInterest lostInterest)
    {
        if (!visualSuspectList.Contains(lostInterest))
            return;

        var value = visualSuspectMap[lostInterest];
        value.Visible = false;
        visualSuspectMap[lostInterest] = value;
    }

    public Dictionary<SenseInterest, (bool Visible, float Awareness)> GetSuspectData() => visualSuspectMap;
}
