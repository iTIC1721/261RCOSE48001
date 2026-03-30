using UnityEngine;
using UnityEngine.AI;

public class BTResume : BTNode
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    public BTResume(Monster monster)
    {
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
    }

    public override BTNodeState Evaluate()
    {
        animator.speed = 1;
        navMeshAgent.isStopped = false;
        return BTNodeState.Success;
    }
}
