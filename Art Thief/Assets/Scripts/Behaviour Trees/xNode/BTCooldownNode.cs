using UnityEngine;
using XNode;
using NaughtyAttributes;

[CreateNodeMenu("Decorator/Cooldown")]
public class BTCooldownNode : BTDecoratorNode
{
    [SerializeField]
    private float cooldownTime;

    [SerializeField]
    private bool startOnCooldown;

    protected override void Init()
    {
        type = Consts.BehaviourType.Cooldown;
        base.Init();
    }

    public override NodeParameter[] GetParameters()
    {
        return new NodeParameter[] { cooldownTime, startOnCooldown };
    }
}