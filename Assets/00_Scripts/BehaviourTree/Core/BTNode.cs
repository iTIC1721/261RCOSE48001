using System.Collections.Generic;
using UnityEngine;

public enum BTNodeState
{
    Running,
    Failure,
    Success
}

public abstract class BTNode
{
    protected BTNodeState state;
    public BTNode parentNode;
    protected List<BTNode> childrenNode = new List<BTNode>();

    public BTNode()
    {
        parentNode = null;
    }

    public BTNode(List<BTNode> children)
    {
        foreach (var child in children)
        {
            AttatchChild(child);
        }
    }

    public void AttatchChild(BTNode child)
    {
        childrenNode.Add(child);
        child.parentNode = this;
    }

    public abstract BTNodeState Evaluate();
}