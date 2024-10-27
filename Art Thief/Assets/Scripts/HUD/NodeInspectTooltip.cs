using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeInspectTooltip : MonoBehaviour
{
    [SerializeField]
    private RectTransform targetViewport;

    [SerializeField]
    private TextMeshProUGUI nodeTitle;

    [SerializeField]
    private TextMeshProUGUI typeText;

    [SerializeField]
    private TextMeshProUGUI dataText;

    [SerializeField]
    private TextMeshProUGUI liveText;

    [SerializeField]
    private float awayFromTopPivot;

    [SerializeField]
    private float awayFromLeftPivot;

    [SerializeField]
    private float awayFromRightPivot;

    [SerializeField]
    private float awayFromBottomPivot;

    private RectTransform tooltipRectTransform;

    private Vector2 viewportEdge;

    private BTRuntimeViewNode targetCopyNode;

    public void UpdateInspectTooltip(BTRuntimeViewNode copyNode)
    {
        targetCopyNode = copyNode;

        if (tooltipRectTransform == null)
        {
            tooltipRectTransform = transform as RectTransform;
            viewportEdge = targetViewport.rect.size;
        }

        // Copy the text of our targeted node
        nodeTitle.text = copyNode.NodeTitle.text;
        typeText.text = copyNode.TypeText.text;
        dataText.text = copyNode.DataText.text;
        dataText.gameObject.SetActive(copyNode.DataText.gameObject.activeSelf);
        liveText.text = copyNode.LiveText.text;
        liveText.gameObject.SetActive(copyNode.LiveText.gameObject.activeSelf);

        Update();
    }

    private void Update()
    {
        // Place our tooltip to follow our mouse position
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetViewport, Input.mousePosition, null, out Vector2 point))
        {
            // Move the pivot point of the tooltip to
            // avoid going off the edges of the viewport
            Vector2 absPoint = point;
            absPoint.y = Mathf.Abs(point.y);
            float horizontalPivot, verticalPivot;
            if (absPoint.x - tooltipRectTransform.sizeDelta.x/2f <= 0f)
            {
                horizontalPivot = awayFromLeftPivot;
            }
            else
            if (absPoint.x + tooltipRectTransform.sizeDelta.x/2f >= viewportEdge.x)
            {
                horizontalPivot = awayFromRightPivot;
            }
            else
                horizontalPivot = 0.5f;

            if (absPoint.y + tooltipRectTransform.sizeDelta.y >= viewportEdge.y)
            {
                verticalPivot = awayFromBottomPivot;
            }
            else if (horizontalPivot != 0.5f && absPoint.y > tooltipRectTransform.sizeDelta.y/2f)
                verticalPivot = 0.5f;
            else
                verticalPivot = awayFromTopPivot;

            tooltipRectTransform.pivot = new Vector2(horizontalPivot, verticalPivot);
            tooltipRectTransform.anchoredPosition = point;
        }

        if (liveText.gameObject.activeSelf)
            liveText.text = targetCopyNode.LiveText.text;
    }
}
