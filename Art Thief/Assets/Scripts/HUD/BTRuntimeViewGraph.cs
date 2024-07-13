using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BTRuntimeViewGraph : MonoBehaviour
{
    [SerializeField]
    private GameObject rowTemplate;

    [SerializeField]
    private GameObject columnTemplate;

    [SerializeField]
    private GameObject nodeTemplate;

    private GuardAgent target;

    private BehaviourTree tree;

    private List<BehaviourNode> nodeList = new();

    private List<BehaviourNode> updatedNodesList = new();

    private bool resetUpdatedNodes;

    private Dictionary<BehaviourNode, BTRuntimeNodeUI> nodeLayoutMap = new();

    // Start is called before the first frame update
    void Start()
    {

    }

    private IEnumerator CreateGraph()
    {
        if (nodeList.Count > 0)
            ClearGraph();

        ProcessNode(tree.RootNode, transform, nodeList);
        // Dirty hack to make the tree visualisation expand out correctly as
        // Unity's UI system doesn't expand everything on its own for some reason
        foreach (var n in nodeList)
        {
            ContentSizeFitter fitter = nodeLayoutMap[n].transform.parent.GetComponent<ContentSizeFitter>();
            fitter.enabled = false;
            yield return new WaitForEndOfFrame();
            fitter.enabled = true;
        }
        // Wait another frame to make sure the ui layout has settled
        yield return new WaitForEndOfFrame();

        // Set up our line connectors
        foreach (var n in nodeList)
        {
            Transform currentNode = nodeLayoutMap[n].transform;
            GameObject bottomLine = currentNode.GetChild(2).gameObject;
            GameObject leftConnector = currentNode.GetChild(3).gameObject;
            GameObject rightConnector = currentNode.GetChild(4).gameObject;

            if (bottomLine.activeSelf)
            {
                n.TryGetChildNodes(out List<BehaviourNode> children);

                if (children.Count < 2)
                    continue;

                leftConnector.SetActive(true);
                rightConnector.SetActive(true);

                // Get the position of our node within the scroll viewport
                // We need all our node positions as part of the same
                // transform for comparison
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    transform.parent as RectTransform,
                    currentNode.position, null,
                    out Vector2 currentPoint);

                float currentNodePos = currentPoint.x;
                float leftFurthestChildPos = currentNodePos;
                float rightFurthestChildPos = currentNodePos;

                // Find the child nodes that are furthest left
                // and right from this parent node
                foreach (var c in children)
                {
                    // Get the position of child nodes within
                    // scroll viewport for comparison
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                       transform.parent as RectTransform,
                       nodeLayoutMap[c].transform.position, null,
                       out Vector2 childPos);

                    if (childPos.x > rightFurthestChildPos)
                        rightFurthestChildPos = childPos.x;
                    if (childPos.x < leftFurthestChildPos)
                        leftFurthestChildPos = childPos.x;
                }

                // Calculate and set connector sizes
                float leftDiff = (currentNodePos - leftFurthestChildPos) + (bottomLine.transform as RectTransform).rect.width;
                float rightDiff = (rightFurthestChildPos - currentNodePos) + (bottomLine.transform as RectTransform).rect.width;
                (leftConnector.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, leftDiff);
                (rightConnector.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rightDiff);
            }
        }
    }

    private void ClearGraph()
    {
        Destroy(transform.GetChild(0));
        Destroy(transform.GetChild(1));
        nodeList.Clear();
        nodeLayoutMap.Clear();
        updatedNodesList.Clear();
    }

    private void ProcessNode(BehaviourNode nextNode, Transform nodeParent, List<BehaviourNode> list)
    {
        if (!list.Contains(nextNode))
        {
            list.Add(nextNode);

            // Create new UI node and set its name text
            GameObject newNode = Instantiate(nodeTemplate, nodeParent);
            newNode.SetActive(true);
            if (newNode.transform.GetChild(0).TryGetComponent(out TextMeshProUGUI text))
                text.text = $"{nextNode.Name}";
            nodeLayoutMap.Add(nextNode, newNode.GetComponent<BTRuntimeNodeUI>());

            // Create new HorizontalLayoutGroup row to hold children
            GameObject newRow = Instantiate(rowTemplate, nodeParent);
            newRow.SetActive(true);
            // Ensure these are in the correct order of node then row
            newRow.transform.SetAsFirstSibling();
            newNode.transform.SetAsFirstSibling();

            if (nextNode.TryGetChildNodes(out List<BehaviourNode> children))
            {
                // Turn on bottom line connector of current node
                newNode.transform.GetChild(2).gameObject.SetActive(true);

                foreach (var n in children)
                {
                    if (list.Contains(n))
                        continue;

                    // Create a new VerticalLayoutGroup column for each child node
                    GameObject newColumn = Instantiate(columnTemplate, newRow.transform);
                    newColumn.SetActive(true);
                    ProcessNode(n, newColumn.transform, list);
                    // Turn on top line connector of child nodes
                    nodeLayoutMap[n].transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }        
    }

    public void SetTarget(GuardAgent guard)
    {
        if (guard == target)
            return;

        if (target != null)
            target.BehaviourTree.NodeRanEvent -= OnNodeRun;
        target = guard;
        tree = target.BehaviourTree;
        tree.NodeRanEvent += OnNodeRun;
        StartCoroutine(CreateGraph());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Update status colour and recently ran outline of nodes
        foreach(var n in nodeList)
        {
            nodeLayoutMap[n].NodeOutline.enabled = updatedNodesList.Contains(n);
            nodeLayoutMap[n].NodeImage.color = GetStatusColor(n.Status);
        }
        resetUpdatedNodes = true;
    }

    private Color GetStatusColor(Consts.NodeStatus status)
    {
        switch (status)
        {
            case Consts.NodeStatus.SUCCESS:
            return Color.green;

            case Consts.NodeStatus.FAILURE:
            return Color.red;

            case Consts.NodeStatus.RUNNING:
            return Color.yellow;
        }
        return Color.white;
    }

    private void OnNodeRun(BehaviourNode node)
    {
        if(resetUpdatedNodes)
        {
            resetUpdatedNodes = false;
            updatedNodesList.Clear();
        }
        updatedNodesList.Add(node);
    }
}
