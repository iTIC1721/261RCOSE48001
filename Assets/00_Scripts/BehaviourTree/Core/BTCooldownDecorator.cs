using UnityEngine;

public class BTCooldownDecorator : BTDecoratorNode
{
    private float cooldownTime;
    private float lastTime = -Mathf.Infinity;

    public BTCooldownDecorator(BTNode child, float cooldownTime) : base(child)
    {
        this.cooldownTime = cooldownTime;
    }

    public override BTNodeState Evaluate()
    {
        if (Time.time - lastTime < cooldownTime)
            return BTNodeState.Failure;

        BTNodeState result = child.Evaluate();

        if (result == BTNodeState.Success)
            lastTime = Time.time;

        return result;
    }
}
