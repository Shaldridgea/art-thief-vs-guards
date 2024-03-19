public class Selector : Composite
{
    public Selector(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree, parameters)
    {
        exitStatus = Consts.NodeStatus.SUCCESS;
    }
}