using UnityEngine;
using static Unity.VisualScripting.Member;

public class PlayerAttackObjectSpawner : AttackObjectSpawner
{
    public Player player;

    public override void SpawnAttackObject()
    {
        Monster target = player.target;

        if (target == null) return;

        Vector2 direction = target.transform.position - transform.position;

        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(player.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(player.damage, player, player.ricochetCount, player.piercingCount, player.reflectCount);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }
}
