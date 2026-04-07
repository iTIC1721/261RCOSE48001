using System.Threading;
using UnityEngine;

public class DefaultMonsterAttackObjectSpawner : AttackObjectSpawner
{
    public float attackPositionOffset = 0.2f;
    public Monster monster;
    public DefaultMonsterBT bt;

    public override void SpawnAttackObject()
    {
        if (Player.Instance == null) return;

        Vector2 direction = bt.AttackDirection;

        MANAGER.Pool.PoolingObj("DefaultMonsterProjectile").Get(monster.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(10, monster);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            value.transform.localScale = (monster.spriteRoot.transform.localScale.x < 0) ? new Vector3(-1, 1, 1) : Vector3.one;
            p.direction = direction;
            p.speed = 10;
        });
    }

    private Vector3 GetAttackPosition()
    {
        return transform.position + Vector3.up * attackPositionOffset;
    }
}
