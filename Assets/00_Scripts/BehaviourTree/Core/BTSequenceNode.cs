using System.Collections.Generic;
using UnityEngine;

public class BTSequenceNode : BTNode
{
    public BTSequenceNode() : base() { }

    public BTSequenceNode(List<BTNode> children) : base(children) { }

    public override BTNodeState Evaluate()
    {
        bool bNowRunning = false;
        foreach (BTNode node in childrenNode)
        {
            switch (node.Evaluate())
            {
                case BTNodeState.Failure:
                    return state = BTNodeState.Failure;
                case BTNodeState.Success:
                    continue;
                case BTNodeState.Running:
                    bNowRunning = true;
                    continue;
                default:
                    continue;
            }
        }

        return state = bNowRunning ? BTNodeState.Running : BTNodeState.Success;
    }
}
