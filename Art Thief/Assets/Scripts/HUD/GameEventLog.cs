using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEventLog : MonoBehaviour
{
    private static GameEventLog instance;

    [SerializeField]
    private TextMeshProUGUI logTemplate;

    [SerializeField]
    private int logLimit;

    [SerializeField]
    private Button visibilityButton;

    [SerializeField]
    private float hidePositionY;

    private GameObjectPool<TextMeshProUGUI> logPool;

    private int logCount;

    private Queue<string> messageQueue = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if(logPool == null)
            logPool = new GameObjectPool<TextMeshProUGUI>(logTemplate.gameObject, 5);

        visibilityButton.onClick.RemoveAllListeners();
        visibilityButton.onClick.AddListener(ToggleVisibility);
    }

    private void ToggleVisibility()
    {
        var rect = transform as RectTransform;
        if (rect.anchoredPosition.y == hidePositionY)
            rect.anchoredPosition -= new Vector2(0f, hidePositionY);
        else
            rect.anchoredPosition += new Vector2(0f, hidePositionY);
        visibilityButton.transform.GetChild(0).Rotate(new Vector3(0f, 0f, 180f));
    }

    public static void Log(string newLogMessage)
    {
        if(instance == null)
        {
            instance = FindObjectOfType<GameEventLog>();
            instance.Awake();
            if (instance == null)
                return;
        }

        instance.LogMessage(newLogMessage);
    }

    private void LogMessage(string newLogMessage)
    {
        // We're not interested in potential duplicate messages clogging up the feed
        if (messageQueue.Count > 0 && messageQueue.Peek() == newLogMessage)
            return;

        messageQueue.Enqueue(newLogMessage);

        Transform logParent = logTemplate.transform.parent;
        TextMeshProUGUI newLog = logPool.GetFromPool();
        if (newLog.transform.parent != logParent)
            newLog.transform.SetParent(logParent, false);

        // Event log has most recent messages at the bottom,
        // so we make it the last sibling in the layout
        newLog.transform.SetAsLastSibling();
        newLog.text = newLogMessage;
        ++logCount;

        int i = 0;
        while (logCount > logLimit)
        {
            // Return the oldest message entry boxes to our pool
            // so they can be gotten again for new entries
            logPool.ReturnToPool(logParent.GetChild(i + 1).gameObject);
            messageQueue.Dequeue();
            --logCount;
            ++i;
        }
    }
}
