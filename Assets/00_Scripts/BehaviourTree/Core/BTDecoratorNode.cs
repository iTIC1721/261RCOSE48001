using UnityEngine;

public abstract class BTDecoratorNode : BTNode
{
    protected BTNode child;

    public BTDecoratorNode(BTNode child)
    {
        this.child = child;
    }
}
