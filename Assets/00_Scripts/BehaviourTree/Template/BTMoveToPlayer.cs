using UnityEngine;
using UnityEngine.AI;

public class BTMoveToPlayer : BTNode
{
    private Transform player;
    private Transform monster;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    public BTMoveToPlayer(Transform player, Monster monster)
    {
        this.player = player;
        this.monster = monster.transform;
        animator = monster.GetComponentInChildren<Animator>();
        navMeshAgent = monster.GetComponent<NavMeshAgent>();
    }

    public override BTNodeState Evaluate()
    {
        animator.SetBool("1_Move", true);
        navMeshAgent.isStopped = false;

        navMeshAgent.SetDestination(player.position);

        if (navMeshAgent.velocity.x > 0)
        {
            monster.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            monster.localScale = new Vector3(1, 1, 1);
        }

        return state = BTNodeState.Running;
    }
}
