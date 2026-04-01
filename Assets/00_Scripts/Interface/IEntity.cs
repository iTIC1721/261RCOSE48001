using UnityEngine;

public interface IEntity : IAttackable, IDamageable
{
    public void Initialize();

    public void Die();
}
