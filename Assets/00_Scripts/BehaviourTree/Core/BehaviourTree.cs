using UnityEngine;

public abstract class BehaviourTree : MonoBehaviour
{
    private BTNode rootNode;

    protected virtual void Start()
    {
        rootNode = SetupBehaviorTree();
    }

    protected virtual void Update()
    {
        if (rootNode is null) return;
        rootNode.Evaluate();
    }

    protected abstract BTNode SetupBehaviorTree();
}
