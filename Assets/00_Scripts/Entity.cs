using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IAttackable, IDamageable
{
    public Transform Transform => this.transform;

    [Header("HP")]
    public float hp;
    public float maxHp;

    [Header("Attack")]
    public float baseDamage;
    public float baseAttackDelay = 1;

    [Header("Projectile")]
    public int ricochetCount = 0;
    public int piercingCount = 0;
    public int reflectCount = 0;

    public Action<float, float> OnDamaged;
    public Action OnDeath;

    private List<float> damageAdders = new();
    public void AddDamageAdder(float amount) => damageAdders.Add(amount);
    public void RemoveDamageAdder(float amount) => damageAdders.Remove(amount);

    private List<float> damageMultipliers = new();
    public void AddDamageMultiplier(float amount) => damageMultipliers.Add(amount);
    public void RemoveDamageMultiplier(float amount) => damageMultipliers.Remove(amount);

    private List<float> attackDelayMultipliers = new();
    public void AddAttackDelayMultiplier(float amount) => attackDelayMultipliers.Add(amount);
    public void RemoveAttackDelayMultiplier(float amount) => attackDelayMultipliers.Remove(amount);

    public float Damage { 
        get
        {
            float damageAdder = damageAdders.Sum();
            float damageMultiplier = 1;
            foreach (float m in damageMultipliers) damageMultiplier *= m;

            return (baseDamage + damageAdder) * damageMultiplier;
        }
    }

    public float AttackDelay
    {
        get
        {
            float attackDelayMultiplier = 1;
            foreach (float m in attackDelayMultipliers) attackDelayMultiplier *= m;

            return baseAttackDelay * attackDelayMultiplier;
        }
    }


    public abstract void Initialize();

    public abstract void Die();

    public abstract AttackHelper AttackHelper { get; }

    // IAttackable
    public abstract void Attack();
    public abstract Vector3 GetAttackPosition();

    // IDamageable
    public abstract void GetDamaged(params DamageInfo[] damageInfos);

    public virtual EntityContext BuildContext()
    {
        EntityContext context = new EntityContext()
        {
            source = this,
            damage = Damage
        };
        return context;
    }
}
