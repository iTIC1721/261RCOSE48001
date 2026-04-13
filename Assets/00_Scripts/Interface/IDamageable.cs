using UnityEngine;

public interface IDamageable
{
    Transform Transform { get; }

    public void GetDamaged(params DamageInfo[] damageInfos);
}
