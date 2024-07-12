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
    private Button closeButton;

    private Agent target;

    private List<GameObject> entries = new();

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(CloseView);
    }

    private void LateUpdate()
    {
        UpdateView();
    }

    public void SetTarget(Agent newTarget) => target = newTarget;

    private void CloseView()
    {
        gameObject.SetActive(false);
    }

    private void UpdateView()
    {
        if (target == null)
            return;

        Blackboard board = target.AgentBlackboard;
        var allData = board.GetData();

        int i = 0;
        foreach(var d in allData)
        {
            if(entries.Count < allData.Count)
                entries.Add(Instantiate(entryTemplate, contentParent));

            entries[i].SetActive(true);
            TextMeshProUGUI keyText = entries[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI valueText = entries[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            keyText.text = d.Key;
            valueText.text = d.Value.ToString();
            ++i;
        }

        if(entries.Count > allData.Count)
        {
            int deactivateCount = entries.Count - allData.Count;
            for(int l = 0; l < deactivateCount;  ++l)
                entries[entries.Count - 1 - l].SetActive(false);
        }
    }
}
