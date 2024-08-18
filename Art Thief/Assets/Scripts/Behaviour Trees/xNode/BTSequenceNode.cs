

[NodeTint(0.2f, 0.3f, 0.7f)]
[CreateNodeMenu("Sequence")]
public class BTSequenceNode : BTCompositeNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Sequence;
        base.Init();
    }
}