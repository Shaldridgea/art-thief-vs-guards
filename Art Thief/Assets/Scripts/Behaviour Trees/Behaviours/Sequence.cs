public class Sequence : Composite
{
    public Sequence(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree, parameters)
    {
        exitStatus = Consts.NodeStatus.FAILURE;
    }
}
