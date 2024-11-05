using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the suspicions and awareness of guards, using visual and auditory sense information
/// </summary>
public class SuspicionModule : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How long in seconds for a suspicious thing to be spotted")]
    private float reactionTimerMax;

    [SerializeField]
    private float awarenessMinDistance = 5f;

    [SerializeField]
    private float awarenessMaxDistance = 20f;

    [Header("Awareness UI")]
    [SerializeField]
    private GameObject guardMarkerCanvas;

    [SerializeField]
    private GameObject exclamationIcon;

    [SerializeField]
    private GameObject questionIcon;

    private float reactionTimer;

    private Dictionary<SenseInterest, (bool Visible, float Awareness)> visualSuspectMap = new();

    private List<SenseInterest> visualSuspectList = new();

    private List<SenseInterest> ignoreList = new();

    private SenseInterest currentSuspicion;

    private GuardAgent owner;

    private bool IsChasing => owner.AgentBlackboard.GetVariable<string>("guardMode") == "chase";

    void Start()
    {
        reactionTimer = -1f;
        if (TryGetComponent(out GuardAgent myAgent))
            owner = myAgent;
    }

    void Update()
    {
        if (!owner.AgentActivated)
            return;

        // Check reaction when our timer hits zero, but not after
        bool checkReaction = false;
        if (reactionTimer == 0f)
        {
            checkReaction = true;
            reactionTimer = -1f;
        }
        else if (reactionTimer > 0f)
            reactionTimer = Mathf.Max(reactionTimer - Time.deltaTime, 0f);

        if (IsChasing)
            return;

        bool shouldResetInterest = false;
        int awarenessCount = 0;
        foreach(var key in visualSuspectList)
        {
            var suspectValues = visualSuspectMap[key];
            float compareAware = suspectValues.Awareness;

            // Calculate our new awareness to increase by if interest is visible
            if (suspectValues.Visible)
            {
                // Awareness delta is how fast the guard becomes aware/suspicious of something
                // 1 is the baseline of taking 1 second to become aware
                // 2 would take 2 seconds, 0.5 would be half a second etc.
                float awarenessDelta = 0.75f;
                // Add more time to awareness if interest is in peripheral vision
                if (!owner.GuardSenses.IsInCentralVision(key.gameObject))
                    awarenessDelta += 1f;

                // Mutiply awareness factor based on distance
                awarenessDelta *= Mathf.Lerp(0.25f, 1.5f,
                    Mathf.InverseLerp(awarenessMinDistance, awarenessMaxDistance,
                    Vector3.Distance(transform.position.ZeroY(), key.transform.position.ZeroY())));

                VisualInterest visual = (key as VisualInterest);

                // Reduce the reaction time if the visual interest is moving
                if (visual.IsMoving)
                    awarenessDelta *= 0.6f;

                // Multiply awareness factor to take 3 times as long if interest is in the dark
                if (!visual.IsLitUp)
                    awarenessDelta *= 3f;

                // Clamp our awareness so we don't react too fast or too slow
                awarenessDelta = Mathf.Clamp(awarenessDelta, 0.3f, 3f);

                // Increase our awareness of this interest over time
                suspectValues.Awareness =
                    Mathf.Clamp(suspectValues.Awareness + (Time.deltaTime / awarenessDelta), 0f, 2f);
            }
            else // Reduce awareness over a second if interest isn't visible
                suspectValues.Awareness = Mathf.Clamp(suspectValues.Awareness - Time.deltaTime, 0f, 2f);

            visualSuspectMap[key] = suspectValues;

            if (suspectValues.Awareness > 0f)
            {
                ++awarenessCount;
                ShowAwarenessMarker(questionIcon);

                if (suspectValues.Awareness >= 1f)
                {
                    // If our awareness is over 1 and was previously under 1
                    // then this is our new current suspicion, start reacting
                    if (compareAware < 1f)
                    {
                        SetSuspicion(key);
                        reactionTimer = reactionTimerMax;
                        break;
                    }
                }
            }
            else if (compareAware > 0f)
                --awarenessCount;
        }

        if(awarenessCount < 0)
            HideAwarenessMarker();

        if (currentSuspicion != null && currentSuspicion is VisualInterest)
        {
            // Confirm our suspicion as soon as it hits 2
            if (visualSuspectMap[currentSuspicion].Awareness >= 2f)
            {
                owner.AgentBlackboard.SetVariable("suspicionStatus", "confirmed");
                shouldResetInterest = true;
                GameEventLog.Log($"{name} saw something suspicious!");
                ShowAwarenessMarker(exclamationIcon);
            }
            else if (checkReaction)
            {
                // If our awareness hasn't reached 2 when the
                // timer stops, our suspicion is unconfirmed
                owner.AgentBlackboard.SetVariable("suspicionStatus", "unconfirmed");
                shouldResetInterest = true;
                GameEventLog.Log($"{name} thinks they saw something...");
                ShowAwarenessMarker(questionIcon);
                HideAwarenessMarker();
            }
        }

        // Sound interests can never be truly confirmed, as the guard is more interested
        // in what caused the sound, so when reaction runs out we're unconfirmed
        if (checkReaction)
            if (currentSuspicion is SoundInterest)
            {
                owner.AgentBlackboard.SetVariable("suspicionStatus", "unconfirmed");
                shouldResetInterest = true;
                GameEventLog.Log($"{name} thinks they heard something...");
                HideAwarenessMarker();
            }

        if (shouldResetInterest)
            CullSuspects();
    }

    private void SetSuspicion(SenseInterest newInterest)
    {
        currentSuspicion = newInterest;
        owner.AgentBlackboard.SetVariable("suspicious", true);
        owner.AgentBlackboard.SetVariable("suspicionStatus", "reacting");
        owner.AgentBlackboard.SetVariable("suspicion", currentSuspicion.gameObject);
        bool isThief = currentSuspicion.CompareTag("Thief");
        owner.AgentBlackboard.SetVariable("thiefFound", isThief);
        if(!newInterest.AlwaysImportant)
            ignoreList.Add(newInterest);
    }
    
    /// <summary>
    /// Cull visual suspects that aren't relevant or in use, and reset our current suspicion
    /// </summary>
    private void CullSuspects()
    {
        if (currentSuspicion is VisualInterest)
        {
            for (int i = visualSuspectList.Count - 1; i >= 0; --i)
            {
                var key = visualSuspectList[i];
                // Cull interest if it's not visible and we're not aware of it
                if (!visualSuspectMap[key].Visible && visualSuspectMap[key].Awareness == 0f)
                {
                    visualSuspectList.RemoveAt(i);
                    visualSuspectMap.Remove(key);
                }
            }
        }

        currentSuspicion = null;

        HideAwarenessMarker();
    }

    public bool OnSuspicionSensed(SenseInterest newInterest, Consts.SuspicionType suspicionType)
    {
        if (IsChasing)
            return false;

        if (ignoreList.Contains(newInterest))
            return false;

        // If we have no suspicion right now or this new suspicion is higher/equal priority
        if (currentSuspicion == null || newInterest.Priority >= currentSuspicion.Priority)
        {
            guardMarkerCanvas.SetActive(true);
            ShowAwarenessMarker(questionIcon);

            // Immediately set our suspicion if its a sound interest
            if (suspicionType == Consts.SuspicionType.Sound)
            {
                SetSuspicion(newInterest);
                reactionTimer = reactionTimerMax;
            }
            else
            {
                // If we see another guard that's chasing the thief, we want to immediately
                // respond to that and give chase as well. This isn't an ideal way of doing it
                // but using the suspicion focus guards have already and swapping out our passed
                // interest for the thief instead, we'll near instantly give chase
                bool spottedChasingGuard = false;
                if(newInterest.OwnerTeam == Consts.Team.GUARD)
                {
                    if(newInterest.Owner.TryGetComponent(out Agent agent))
                    {
                        if(agent.AgentBlackboard.GetVariable<string>("guardMode") == "chase" &&
                            !agent.AgentBlackboard.GetVariable<bool>("isStunned"))
                        {
                            newInterest = Level.Instance.Thief.GetComponentInChildren<VisualInterest>();
                            spottedChasingGuard = true;
                            SetSuspicion(newInterest);
                        }
                    }
                }

                // Add/update a visual interest as being visible
                if (visualSuspectMap.TryGetValue(newInterest, out var value))
                {
                    value.Visible = true;
                    if (spottedChasingGuard)
                        value.Awareness = 2f;
                    visualSuspectMap[newInterest] = value;
                }
                else
                {
                    visualSuspectMap[newInterest] = (true, spottedChasingGuard ? 2f : 0f);
                    visualSuspectList.Add(newInterest);
                }
            }
            GameEventLog.Log($"{name} noticed something...");
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

    /// <summary>
    /// Get whether the last heard sound was made by the thief and is still playing
    /// </summary>
    public bool IsThiefHeard()
    {
        SoundInterest sound = owner.AgentBlackboard.GetVariable<SoundInterest>("lastHeardSound");
        if (sound == null)
            return false;

        if(sound.OwnerTeam == Consts.Team.THIEF)
            return sound.IsOngoing;

        return false;
    }

    /// <summary>
    /// Show the passed marker as being the active marker over the agent's
    /// head displaying their current awareness state
    /// </summary>
    /// <param name="marker">Question mark or exclamation mark object marker</param>
    private void ShowAwarenessMarker(GameObject marker)
    {
        LeanTween.cancel(guardMarkerCanvas);
        marker.SetActive(true);
        if (marker == exclamationIcon)
            questionIcon.SetActive(false);
        else
            exclamationIcon.SetActive(false);
    }

    /// <summary>
    /// Hide the floating awareness marker after a few seconds delay
    /// </summary>
    private void HideAwarenessMarker()
    {
        LeanTween.cancel(guardMarkerCanvas);
        LeanTween.delayedCall(guardMarkerCanvas, 5f, () => guardMarkerCanvas.SetActive(false));
    }

    public Dictionary<SenseInterest, (bool Visible, float Awareness)> GetSuspectData() => visualSuspectMap;
}
