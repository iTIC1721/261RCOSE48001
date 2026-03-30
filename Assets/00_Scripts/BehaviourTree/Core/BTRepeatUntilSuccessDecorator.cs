using UnityEngine;

public class BTRepeatUntilSuccessDecorator : BTDecoratorNode
{
    public BTRepeatUntilSuccessDecorator(BTNode child) : base(child) { }

    public override BTNodeState Evaluate()
    {
        BTNodeState result = child.Evaluate();

        if (result == BTNodeState.Success)
            return BTNodeState.Success;

        return BTNodeState.Running;
    }
}
