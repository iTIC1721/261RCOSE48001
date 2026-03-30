using UnityEngine;

public class BTRepeatDecorator : BTDecoratorNode
{
    private int repeatCount;
    private int currentCount = 0;

    public BTRepeatDecorator(BTNode child, int repeatCount) : base(child)
    {
        this.repeatCount = repeatCount;
    }

    public override BTNodeState Evaluate()
    {
        if (currentCount < repeatCount)
        {
            BTNodeState result = child.Evaluate();

            if (result == BTNodeState.Success || result == BTNodeState.Failure)
            {
                currentCount++;
            }

            return BTNodeState.Running;
        }

        return BTNodeState.Success;
    }
}
