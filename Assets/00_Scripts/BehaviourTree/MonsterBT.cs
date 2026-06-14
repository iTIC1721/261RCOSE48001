using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public abstract class MonsterBT : BehaviourTree
{
    [Serializable]
    protected class SpecialSkillEntry
    {
        public string name;
        public bool onlyOnce;
        [HideInInspector] public bool used;
        public string sfxName;
    }

    protected Monster monster;
    private NavMeshAgent agent;

    public Vector2 AttackDirection { get; protected set; }

    protected float attackDelay = 1;
    private readonly float MinAttackDelay = 0.01f;

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        attackDelay = monster.AttackDelay;
    }

    protected override abstract BTNode SetupBehaviorTree();

    public abstract void AttackAnimation();
    public abstract void GetDamagedAnimation();
    public abstract void DieAnimation();

    protected void SetRandomAttackDelay(float range)
    {
        float delay = monster.AttackDelay + UnityEngine.Random.Range(-range, range);
        attackDelay = delay < MinAttackDelay ? MinAttackDelay : delay;
    }
}
