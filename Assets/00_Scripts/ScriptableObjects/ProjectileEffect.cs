using UnityEngine;

public abstract class ProjectileEffect : ScriptableObject
{
    public abstract void Execute(AttackProjectile projectile, Collider2D hitCollider, Vector2 hitDirection);
}
