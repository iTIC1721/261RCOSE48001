using UnityEngine;

public interface IAttackable
{
    Transform Transform { get; }

    public void Attack();

    public Vector3 GetAttackPosition();
}
