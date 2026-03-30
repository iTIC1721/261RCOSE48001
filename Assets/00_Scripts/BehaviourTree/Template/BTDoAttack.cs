using UnityEngine;
using UnityEngine.AI;

public class BTDoAttack : BTNode
{
    private Monster monster;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private string attackTriggerName;

    public BTDoAttack(Monster monster, string attackTriggerName)
    {
        this.monster = monster;
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
        this.attackTriggerName = attackTriggerName;
    }

    public override BTNodeState Evaluate()
    {
        animator.SetFloat("Speed", 0);
        animator.SetTrigger(attackTriggerName);
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        return BTNodeState.Success;
    }
}
