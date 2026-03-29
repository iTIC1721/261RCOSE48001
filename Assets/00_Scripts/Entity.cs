using UnityEngine;

public interface IEntity
{
    public void Attack(float damage);

    public void GetDamaged(float damage);

    public void Die();
}
