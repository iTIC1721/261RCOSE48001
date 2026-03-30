using UnityEngine;

public interface IAttackable
{
    Transform Transform { get; }

    public void Attack();
}
