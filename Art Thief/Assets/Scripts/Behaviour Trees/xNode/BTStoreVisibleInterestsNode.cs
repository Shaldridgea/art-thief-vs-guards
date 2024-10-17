using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Variables/Store Visible Interests")]
public class BTStoreVisibleInterestsNode : BTActionNode
{
    [SerializeField]
    [Tag]
    private string interestTag;

    [SerializeField]
    private Consts.BlackboardSource source;

    [SerializeField]
    private string listKey;

    protected override void Init()
    {
        type = Consts.BehaviourType.StoreVisibleInterests;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { interestTag, (int)source, listKey };
    }

    public override string GetNodeDetailsText()
    {
        return "Interest tag: " + interestTag + ", List key: " + listKey;
    }
}