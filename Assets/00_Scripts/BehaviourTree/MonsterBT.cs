using UnityEngine;
using UnityEngine.AI;

public abstract class MonsterBT : BehaviourTree
{
    protected Monster monster;
    private NavMeshAgent agent;

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    protected override abstract BTNode SetupBehaviorTree();

    public abstract void AttackAnimation();
    public abstract void GetDamagedAnimation();
    public abstract void DieAnimation();
}
