using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerAttackObjectSpawner : AttackObjectSpawner
{
    public float attackPositionOffset = 0.2f;
    public Player player;

    public override void SpawnAttackObject()
    {
        Monster target = player.target;

        if (target == null) return;

        Vector2 direction = target.transform.position - transform.position;

        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(10, player);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }

    private Vector3 GetAttackPosition()
    {
        return transform.position + Vector3.up * attackPositionOffset;
    }
}
