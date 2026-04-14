using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IAttackable, IDamageable
{
    public Transform Transform => this.transform;

    [Header("HP")]
    public float hp;
    public float maxHp;

    [Header("Attack")]
    public float damage;
    public float damageAdder = 0;
    public float damageMultiplier = 1;

    [Header("Projectile")]
    public int ricochetCount = 0;
    public int piercingCount = 0;
    public int reflectCount = 0;

    public Action<float, float> OnDamaged;
    public Action OnDeath;

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
            damage = (this.damage + damageAdder) * damageMultiplier
        };
        return context;
    }
}
