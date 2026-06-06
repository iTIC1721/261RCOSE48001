using System.Collections.Generic;
using UnityEngine;

public class BTSelectorNode : BTNode
{
    public BTSelectorNode() : base() { }

    public BTSelectorNode(List<BTNode> children) : base(children) { }

    public override BTNodeState Evaluate()
    {
        foreach (BTNode node in childrenNode)
        {
            switch (node.Evaluate())
            {
                case BTNodeState.Failure:
                    continue;
                case BTNodeState.Success:
                    return state = BTNodeState.Success;
                case BTNodeState.Running:
                    return state = BTNodeState.Running;
                default:
                    continue;
            }
        }

        return state = BTNodeState.Failure;
    }
}
