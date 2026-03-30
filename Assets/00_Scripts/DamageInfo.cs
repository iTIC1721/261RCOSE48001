using System;
using UnityEngine;

[Serializable]
public class DamageInfo
{
    public float damage;
    public Transform damageSource;

    public DamageInfo() { 
        damage = 0;
        damageSource = null;
    }

    public DamageInfo(float damage, Transform damageSource)
    {
        this.damage = damage;
        this.damageSource = damageSource;
    }
}
