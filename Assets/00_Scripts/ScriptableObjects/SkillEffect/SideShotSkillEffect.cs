using UnityEngine;

[CreateAssetMenu(fileName = "SideShot", menuName = "Skill Effect/SideShot")]
public class SideShotSkillEffect : SkillEffect
{
    public float[] angles = new float[3] { 0, 10f, -10f };
    public float damageMultiplier = 1f;

    public override void Execute(EntityContext context, int stack)
    {
        Vector2 baseDirection = context.direction;

        Vector2 leftDirection = Quaternion.Euler(0, 0, 90) * baseDirection;
        Vector2 rightDirection = Quaternion.Euler(0, 0, -90) * baseDirection;

        int shotCount = (stack > angles.Length) ? angles.Length : stack;
        for (int i = 0; i < shotCount; i++)
        {
            float damage = context.damage * damageMultiplier;

            // left
            Vector2 leftRotated = Quaternion.Euler(0, 0, -angles[i]) * leftDirection;
            SpawnProjectile(context.source, damage, leftRotated);

            // right
            Vector2 rightRotated = Quaternion.Euler(0, 0, angles[i]) * rightDirection;
            SpawnProjectile(context.source, damage, rightRotated);
        }
    }

    private void SpawnProjectile(IAttackable source, float damage, Vector2 direction)
    {
        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(source.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            if (source is Entity entity)
                p.Initialize(damage, source, entity.ricochetCount, entity.piercingCount, entity.reflectCount);
            else
                p.Initialize(damage, source);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }
}
