

[CreateNodeMenu("Selector")]
public class BTSelectorNode : BTCompositeNode
{
    protected override void Init()
    {
        type = Consts.BehaviourType.Selector;
        base.Init();
    }
}