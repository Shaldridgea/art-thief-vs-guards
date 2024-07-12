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

    [SerializeField]
    private Button closeButton;

    private ThiefAgent target;

    private UtilityBehaviour utilityScorer;

    private Dictionary<UtilityAction, Transform> actionEntryMap = new();

    private List<UtilityAction> sortedActions = new();

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(CloseView);
    }

    private void CreateEntries()
    {
        foreach(var a in utilityScorer.ActionList)
        {
            var newEntry = Instantiate(entryTemplate, contentParent);
            actionEntryMap.Add(a, newEntry.transform);
            newEntry.SetActive(true);
            TextMeshProUGUI actionText = newEntry.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            actionText.text = a.Name;
            Transform motiveLayout = newEntry.transform.GetChild(1);
            for(int i = 0; i < a.ScoreCurves.Length; ++i)
            {
                var curve = a.ScoreCurves[i];
                if (i > 0)
                    Instantiate(motiveLayout.GetChild(0), motiveLayout);

                motiveLayout.GetChild(i).GetComponent<TextMeshProUGUI>().text = 
                    $"{curve.MotiveSource.Motive}: {curve.GetValue(target.AgentBlackboard)}";
            }
            TextMeshProUGUI scoreText = newEntry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            scoreText.text = a.Score.ToString();
        }
    }

    private void UpdateView()
    {
        // Sort our actions by their score for visualisation purposes
        sortedActions.Sort((x, y) => y.Score.CompareTo(x.Score));
        for(int i = 0; i < sortedActions.Count; ++i)
        {
            var a = sortedActions[i];
            Transform entry = actionEntryMap[a];
            entry.SetSiblingIndex(i+2);
            Transform motiveLayout = entry.transform.GetChild(1);
            for (int j = 0; j < a.ScoreCurves.Length; ++j)
            {
                var curve = a.ScoreCurves[j];

                motiveLayout.GetChild(j).GetComponent<TextMeshProUGUI>().text =
                    $"{curve.MotiveSource.Motive}: {curve.GetValue(target.AgentBlackboard)}";
            }
            TextMeshProUGUI scoreText = entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            scoreText.text = a.Score.ToString();
        }
    }

    public void SetTarget(ThiefAgent newThief)
    {
        target = newThief;
        if (sortedActions.Count > 0)
            return;

        utilityScorer = target.GetComponent<UtilityBehaviour>();
        sortedActions.AddRange(utilityScorer.ActionList);
        CreateEntries();
    }

    private void CloseView()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        UpdateView();
    }
}
