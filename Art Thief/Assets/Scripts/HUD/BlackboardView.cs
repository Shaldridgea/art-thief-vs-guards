using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackboardView : MonoBehaviour
{
    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private GameObject entryTemplate;

    [SerializeField]
    private GameObject lineBreakTemplate;

    private Agent target;

    private List<GameObject> entries = new();

    private List<GameObject> lineBreaks = new();

    private void LateUpdate()
    {
        UpdateView();
    }

    public void SetTarget(Agent newTarget) => target = newTarget;

    private void UpdateView()
    {
        if (target == null)
            return;

        Blackboard board = target.AgentBlackboard;
        var allData = board.GetData();

        int i = 0;
        foreach(var d in allData)
        {
            // Create new UI boxes for entries if there's less than we need
            if (entries.Count < allData.Count)
            {
                entries.Add(Instantiate(entryTemplate, contentParent));
                lineBreaks.Add(Instantiate(lineBreakTemplate, contentParent));
            }

            entries[i].SetActive(true);
            TextMeshProUGUI keyText = entries[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI valueText = entries[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            keyText.text = d.Key;
            valueText.text = d.Value.ToString();
            ++i;
        }

        // Turn off entry boxes that are going unused
        if(entries.Count > allData.Count)
        {
            int deactivateCount = entries.Count - allData.Count;
            for (int l = 0; l < deactivateCount; ++l)
            {
                entries[entries.Count - 1 - l].SetActive(false);
                lineBreaks[lineBreaks.Count - 1 - l].SetActive(false);
            }
        }
    }
}
