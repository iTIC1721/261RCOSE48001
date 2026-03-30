using UnityEngine;

public interface IEntity : IAttackable, IDamageable
{
    public void Intialize();

    public void Die();
}
