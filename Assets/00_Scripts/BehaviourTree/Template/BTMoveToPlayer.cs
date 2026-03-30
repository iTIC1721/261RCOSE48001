using UnityEngine;
using UnityEngine.AI;

public class BTMoveToPlayer : BTNode
{
    private Transform player;
    private Monster monster;
    private Animator animator;
    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;

    public BTMoveToPlayer(Transform player, Monster monster)
    {
        this.player = player;
        this.monster = monster;
        animator = monster.GetComponentInChildren<Animator>();
        rb = monster.GetComponent<Rigidbody>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
    }

    public override BTNodeState Evaluate()
    {
        animator.SetFloat("Speed", 1);
        navMeshAgent.isStopped = false;

        navMeshAgent.SetDestination(player.position);

        return state = BTNodeState.Running;
    }
}
