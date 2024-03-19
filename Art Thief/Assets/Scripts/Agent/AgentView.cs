using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentView : MonoBehaviour
{
    [SerializeField]
    private Transform agentRoot;

    public Transform AgentRoot => agentRoot;

    [SerializeField]
    private Transform agentHeadRoot;

    public Transform AgentHeadRoot => agentHeadRoot;

    [SerializeField]
    private Transform agentBodyRoot;

    public Transform AgentBodyRoot => agentBodyRoot;
}