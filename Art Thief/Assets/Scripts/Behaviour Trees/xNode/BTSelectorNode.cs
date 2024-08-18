

[NodeTint(0.3f, 0.2f, 0.7f)]
[CreateNodeMenu("Selector")]
public class BTSelectorNode : BTCompositeNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Selector;
        base.Init();
    }
}