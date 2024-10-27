using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePoolLoader : MonoBehaviour
{
    [SerializeField]
    private BTRuntimeViewNode nodeTemplate;

    [SerializeField]
    private int poolSize;

    GameObjectPool<BTRuntimeViewNode> nodePool;

    void Start()
    {
        nodePool = new(nodeTemplate.gameObject, poolSize);
    }

    public GameObjectPool<BTRuntimeViewNode> GetNodePool() => nodePool;
}
