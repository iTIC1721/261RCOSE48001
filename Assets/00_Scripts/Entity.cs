using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IAttackable, IDamageable
{
    public Transform Transform => this.transform;

    [Header("HP")]
    public float hp;
    public float maxHp;

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

    public virtual EntityContext BuildContext(float damage)
    {
        EntityContext context = new EntityContext()
        {
            source = this,
            damage = damage
        };
        return context;
    }
}
