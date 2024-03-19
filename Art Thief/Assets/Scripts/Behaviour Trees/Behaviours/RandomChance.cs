using UnityEngine;

public class RandomChance : Decorator
{
    private float randomChance;

    public RandomChance(BehaviourTree parentTree, NodeParameter[] parameters) : base(parentTree)
    {
        randomChance = parameters[0];
        if (randomChance == 0f)
            randomChance = -1f;
    }

    public override Consts.NodeStatus Update()
    {
        if (Status == Consts.NodeStatus.RUNNING)
            if (childNode.Status != Consts.NodeStatus.RUNNING)
                return childNode.Status;

        Consts.NodeStatus status = Consts.NodeStatus.FAILURE;
        if (Random.value <= randomChance)
            status = childNode.Tick();

        return status;
    }
}
