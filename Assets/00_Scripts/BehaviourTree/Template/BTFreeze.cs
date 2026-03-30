using UnityEngine;
using UnityEngine.AI;

public class BTFreeze : BTNode
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    public BTFreeze(Monster monster)
    {
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
    }

    public override BTNodeState Evaluate()
    {
        animator.speed = 0;
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        return BTNodeState.Running;
    }
}
