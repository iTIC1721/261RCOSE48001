using UnityEngine;

public interface IEntity : IDamageable
{
    Transform Transform { get; }
    GameObject GameObject { get; }

    public void Intialize();

    public void Attack();

    public void Die();
}
