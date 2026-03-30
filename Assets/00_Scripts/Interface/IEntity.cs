using UnityEngine;

public interface IEntity
{
    Transform Transform { get; }
    GameObject GameObject { get; }

    public void Intialize();

    public void Attack();

    public void GetDamaged(params DamageInfo[] damageInfos);

    public void Die();
}
