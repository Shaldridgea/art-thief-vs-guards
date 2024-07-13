using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThiefUtilityView : MonoBehaviour
{
    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private GameObject entryTemplate;

    private ThiefAgent target;

    private UtilityBehaviour utilityScorer;

    private Dictionary<UtilityAction, Transform> actionEntryMap = new();

    private List<UtilityAction> sortedActions = new();

    // Start is called before the first frame update
    void Start()
    {

    }

    private void CreateEntries()
    {
        foreach(var a in utilityScorer.ActionList)
        {
            // Create box entries for each action
            var newEntry = Instantiate(entryTemplate, contentParent);
            actionEntryMap.Add(a, newEntry.transform);
            newEntry.SetActive(true);

            TextMeshProUGUI actionText = newEntry.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            actionText.text = a.Name;

            // Get the VerticalLayoutGroup that holds our contributing motives for the action
            Transform motiveLayout = newEntry.transform.GetChild(1);
            // The first motive score text already exists,
            // so we only need to make clones of it after the first
            for (int i = 1; i < a.ScoreCurves.Length; ++i)
                Instantiate(motiveLayout.GetChild(0), motiveLayout);
        }
        UpdateView();
    }

    private void UpdateView()
    {
        // Sort our actions by their score for visualisation purposes
        sortedActions.Sort((x, y) => y.Score.CompareTo(x.Score));
        for(int i = 0; i < sortedActions.Count; ++i)
        {
            var a = sortedActions[i];
            Transform entry = actionEntryMap[a];
            // Re-order our actions in the layout from highest to lowest score
            entry.SetSiblingIndex(i);

            Transform motiveLayout = entry.transform.GetChild(1);
            for (int j = 0; j < a.ScoreCurves.Length; ++j)
            {
                var curve = a.ScoreCurves[j];

                // Set each motive name and score
                motiveLayout.GetChild(j).GetComponent<TextMeshProUGUI>().text =
                    $"{curve.MotiveSource.Motive}: {curve.GetValue(target.AgentBlackboard)}";
            }

            // Total action score text
            TextMeshProUGUI scoreText = entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            scoreText.text = a.Score.ToString();
        }
    }

    public void SetTarget(ThiefAgent newThief)
    {
        target = newThief;
        // Don't perform set up if we've already done it
        // Since we only have one Thief to care about
        if (sortedActions.Count > 0)
            return;

        utilityScorer = target.GetComponent<UtilityBehaviour>();
        sortedActions.AddRange(utilityScorer.ActionList);
        CreateEntries();
    }

    private void LateUpdate()
    {
        UpdateView();
    }
}
