using UnityEngine;
using UnityEngine.AI;

public class BTIdle : BTNode
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    public BTIdle(Monster monster)
    {
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
    }

    public override BTNodeState Evaluate()
    {
        animator.SetBool("1_Move", false);
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
        }
        return BTNodeState.Success;
    }
}
