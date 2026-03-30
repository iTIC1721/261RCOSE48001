using UnityEngine;

public interface IDamageable
{
    public void GetDamaged(params DamageInfo[] damageInfos);
}
