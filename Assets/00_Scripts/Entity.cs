using UnityEngine;

public interface IEntity
{
    Transform Transform { get; }
    GameObject GameObject { get; }

    public void Attack(float damage);

    public void GetDamaged(float damage);

    public void Die();
}
