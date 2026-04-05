using System;
using UnityEngine;
using UnityEngine.AI;

public class BTInvoke : BTNode
{
    private Monster monster;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Action action;

    public BTInvoke(Monster monster, Action action)
    {
        this.monster = monster;
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
        this.action = action;
    }

    public override BTNodeState Evaluate()
    {
        animator.SetBool("1_Move", false);
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;

        action?.Invoke();

        return BTNodeState.Success;
    }
}
