using UnityEngine;

[CreateAssetMenu(fileName = "Test", menuName = "Projectile Effect/Test")]
public class TestProjectileEffect : ProjectileEffect
{
    public override void Execute(AttackProjectile projectile, Collider2D hitCollider, Vector2 hitDirection)
    {
        Log.LogMessage("테스트 투사체 효과");
    }
}
