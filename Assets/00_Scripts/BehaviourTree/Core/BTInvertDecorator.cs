using UnityEngine;

public class BTInvertDecorator : BTDecoratorNode
{
    public BTInvertDecorator(BTNode child) : base(child) { }

    public override BTNodeState Evaluate()
    {
        BTNodeState result = child.Evaluate();

        switch (result)
        {
            case BTNodeState.Success:
                return BTNodeState.Failure;

            case BTNodeState.Failure:
                return BTNodeState.Success;

            default:
                return BTNodeState.Running;
        }
    }
}
