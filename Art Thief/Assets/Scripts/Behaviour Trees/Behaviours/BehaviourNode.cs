using System.Collections;

public abstract class BehaviourNode
{
    protected BehaviourTree ParentTree { get; private set; }

    public Consts.NodeStatus Status { get; private set; }

    public BehaviourNode(BehaviourTree parentTree)
    {
        ParentTree = parentTree;
    }

    public Consts.NodeStatus Tick()
    {
        if (Status != Consts.NodeStatus.RUNNING)
            OnEnter();
        Status = Update();
        if (Status != Consts.NodeStatus.RUNNING)
            OnExit();
        return Status;
    }

    public virtual void OnEnter()
    {
        ParentTree.PushRunningNode(this);
    }

    public abstract Consts.NodeStatus Update();

    public virtual void OnExit()
    {
        ParentTree.PopRunningNode();
        Reset();
    }

    public virtual void Reset()
    {
        // Set our status to something if we have to reset
        // while still running (usually because of an interrupt)
        if (Status == Consts.NodeStatus.RUNNING)
            Status = Consts.NodeStatus.FAILURE;
    }

    public virtual void AddChild(BehaviourNode addNode, string portName = "") { }

    protected Blackboard GetTargetBlackboard(Consts.BlackboardSource source) =>
     source == Consts.BlackboardSource.AGENT ? ParentTree.Owner.AgentBlackboard : ParentTree.GlobalBlackboard;

    protected Blackboard GetTargetBlackboard(NodeParameter source) =>
    ((Consts.BlackboardSource)(int)source) == Consts.BlackboardSource.AGENT ? ParentTree.Owner.AgentBlackboard : ParentTree.GlobalBlackboard;
}