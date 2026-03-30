using System;
using UnityEngine;

public class BTConditionDecorator : BTDecoratorNode
{
    private Func<bool> condition;

    public BTConditionDecorator(BTNode child, Func<bool> condition) : base(child)
    {
        this.condition = condition;
    }

    public override BTNodeState Evaluate()
    {
        if (condition()) 
            return child.Evaluate();

        return BTNodeState.Failure;
    }
}
