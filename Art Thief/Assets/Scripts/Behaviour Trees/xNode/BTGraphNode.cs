using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NaughtyAttributes;

[NodeWidth(250)]
public abstract class BTGraphNode : Node, INodeGuid
{
    [System.Serializable]
    protected class Empty { }

    [Input]
    [SerializeField]
    protected Empty parentNode;

    [SerializeField]
    [HideInInspector]
    protected Consts.BehaviourType type;

    public Consts.BehaviourType BehaviourType => type;

    private System.Guid guid;

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port) {
        return this;
    }

    /// <summary>
    /// Returns whether this node type is a leaf node (branch ending)
    /// </summary>
    public abstract bool IsLeaf { get; }

    public abstract NodeParameter[] GetParameters();

    public void UpdateName<T>(T myType)
    {
        name = SpacedCapitalisation(myType.ToString());
    }

    private string SpacedCapitalisation(string startString)
    {
        List<char> wordList = new List<char>(startString.ToCharArray());
        int i = 0;
        while (i < wordList.Count)
        {
            if (i > 0 && char.IsUpper(wordList[i]) && char.IsLower(wordList[i - 1]))
                wordList.Insert(i, ' ');
            ++i;
        }
        return new string(wordList.ToArray());
    }

    public string GetBehaviourTypeText() => $"Type: {type.ToString()}";

    public virtual string GetNodeDetailsText()
    {
        return string.Empty;
    }

    public void SetGuid(System.Guid newGuid) { guid = newGuid; }

    public System.Guid GetGuid() => guid;
}