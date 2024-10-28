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

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        logPool = new GameObjectPool<TextMeshProUGUI>(logTemplate.gameObject, 5);

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
            if (instance == null)
                return;
        }

        var self = instance;
        Transform logParent = self.logTemplate.transform.parent;
        TextMeshProUGUI newLog = self.logPool.GetFromPool();
        if(newLog.transform.parent != logParent)
            newLog.transform.SetParent(logParent, false);
        newLog.transform.SetAsLastSibling();
        newLog.text = newLogMessage;
        ++self.logCount;

        int i = 0;
        while (self.logCount > self.logLimit)
        {
            self.logPool.ReturnToPool(logParent.GetChild(i+1).gameObject);
            --self.logCount;
            ++i;
        }
    }
}
