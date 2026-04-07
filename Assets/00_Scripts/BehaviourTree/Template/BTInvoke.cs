using System;
using UnityEngine;
using UnityEngine.AI;

public class BTInvoke : BTNode
{
    private Action action;

    public BTInvoke(Action action)
    {
        this.action = action;
    }

    public override BTNodeState Evaluate()
    {
        action?.Invoke();

        return BTNodeState.Success;
    }
}
