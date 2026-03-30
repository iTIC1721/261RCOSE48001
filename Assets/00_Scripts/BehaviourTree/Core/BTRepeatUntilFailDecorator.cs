using UnityEngine;

public class BTRepeatUntilFailDecorator : BTDecoratorNode
{
    public BTRepeatUntilFailDecorator(BTNode child) : base(child) { }

    public override BTNodeState Evaluate()
    {
        BTNodeState result = child.Evaluate();

        if (result == BTNodeState.Failure)
            return BTNodeState.Success;

        return BTNodeState.Running;
    }
}
