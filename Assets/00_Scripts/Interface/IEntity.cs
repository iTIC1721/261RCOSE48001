using UnityEngine;

public interface IEntity
{
    Transform Transform { get; }
    GameObject GameObject { get; }

    public void Intialize();

    public void Attack(float damage);

    public void GetDamaged(params float[] damage);

    public void Die();
}
