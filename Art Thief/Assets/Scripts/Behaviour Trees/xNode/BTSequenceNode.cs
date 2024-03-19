

[CreateNodeMenu("Sequence")]
public class BTSequenceNode : BTCompositeNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Sequence;
        base.Init();
    }
}