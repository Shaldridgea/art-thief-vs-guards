using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BTRuntimeViewNode : MonoBehaviour, INodeGuid, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private BTRuntimeView viewOwner;

    [SerializeField]
    private Image nodeImage;

    [SerializeField]
    private Outline nodeOutline;

    [SerializeField]
    private TextMeshProUGUI nodeTitle;

    public TextMeshProUGUI NodeTitle => nodeTitle;

    [SerializeField]
    private TextMeshProUGUI typeText;

    public TextMeshProUGUI TypeText => typeText;

    [SerializeField]
    private TextMeshProUGUI dataText;

    public TextMeshProUGUI DataText => dataText;

    [SerializeField]
    private TextMeshProUGUI liveText;

    public TextMeshProUGUI LiveText => liveText;

    [SerializeField]
    private List<Image> lineConnectors;

    public Vector2 GridPosition { get; private set; }

    private System.Guid guid;

    public bool InitNodeInfoVisuals(BTGraphNode dataNode, BehaviourNode logicNode)
    {
        nodeTitle.text = dataNode.name;
        typeText.text = dataNode.GetBehaviourTypeText();

        string detailsText = dataNode.GetNodeDetailsText();
        if (!string.IsNullOrEmpty(detailsText))
        {
            dataText.text = detailsText;
            dataText.gameObject.SetActive(true);
        }
        else
            dataText.gameObject.SetActive(false);

        liveText.gameObject.SetActive( !string.IsNullOrEmpty( logicNode.GetLiveVisualsText() ) );

        return liveText.gameObject.activeSelf;
    }

    public void UpdateLiveNodeVisuals(bool isRecentNode, BehaviourNode logicNode = null)
    {
        nodeOutline.enabled = isRecentNode;
        if (logicNode == null)
            return;

        nodeImage.color = GetStatusColor(logicNode.Status);
        if (liveText.gameObject.activeSelf)
            liveText.text = logicNode.GetLiveVisualsText();
    }

    public void SetLineConnectorVisuals(int lineIndex, BTRuntimeViewNode connectToNode, BTGridBasedGraph gridGraph)
    {
        Image lineImage;
        if (lineIndex < lineConnectors.Count)
            lineImage = lineConnectors[lineIndex];
        else
        {
            lineImage = Instantiate(lineConnectors[0], transform);
            lineConnectors.Add(lineImage);
        }

        // Do grid math to find our start and end positions for the line
        Vector2 cellSize = gridGraph.CellSize;
        Vector2 spacing = gridGraph.Spacing;

        Vector2 startPoint = GridPosition * cellSize;
        Vector2 endPoint = connectToNode.GridPosition * cellSize;
        startPoint += new Vector2(cellSize.x / 2f, 0f);
        endPoint -= new Vector2(cellSize.x / 2f, 0f);
        endPoint.x += spacing.x * Mathf.Abs(connectToNode.GridPosition.x - GridPosition.x);

        // Maths for spacing between vertical nodes
        float gridHeightDiff = Mathf.Abs(connectToNode.GridPosition.y - GridPosition.y);
        if (endPoint.y > startPoint.y)
            endPoint.y += spacing.y * gridHeightDiff;
        else
            endPoint.y -= spacing.y * gridHeightDiff;

        if (!lineImage.gameObject.activeSelf)
            lineImage.gameObject.SetActive(true);

        RectTransform lineRect = lineImage.transform as RectTransform;

        lineRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            Vector3.Distance(startPoint, endPoint));

        lineRect.rotation = Quaternion.Euler(0f, 0f,
            Vector3.SignedAngle((endPoint - startPoint).normalized, Vector3.right, Vector3.forward));
    }

    public void ResetLineConnectors(int startIndex = 0)
    {
        for (int i = startIndex; i < lineConnectors.Count; ++i)
            lineConnectors[i].gameObject.SetActive(false);
    }

    public void SetGuid(System.Guid newGuid) { guid = newGuid; }

    public System.Guid GetGuid() => guid;

    public void SetGridPosition(Vector2 gridPos) => GridPosition = gridPos;

    private Color GetStatusColor(Consts.NodeStatus status)
    {
        return status switch
        {
            Consts.NodeStatus.SUCCESS => Color.green,
            Consts.NodeStatus.FAILURE => Color.red,
            Consts.NodeStatus.RUNNING => Color.yellow,
            _ => Color.white,
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        viewOwner.InspectTooltip.gameObject.SetActive(true);
        viewOwner.InspectTooltip.UpdateInspectTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        viewOwner.InspectTooltip.gameObject.SetActive(false);
    }
}
