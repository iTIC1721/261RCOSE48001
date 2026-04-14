using UnityEngine;

[CreateAssetMenu(fileName = "FrontShot", menuName = "Skill Effect/FrontShot")]
public class FrontShotSkillEffect : SkillEffect
{
    public float[] angles = new float[3] { 7.5f, 15f, 22.5f };

    public override void Execute(EntityContext context, int stack)
    {
        Vector2 baseDirection = context.direction;

        int shotCount = (stack > angles.Length) ? angles.Length : stack;
        for (int i = 0; i < shotCount; i++)
        {
            // left
            Vector2 leftRotated = Quaternion.Euler(0, 0, angles[i]) * baseDirection;
            SpawnProjectile(context.source, context.damage, leftRotated);

            // right
            Vector2 rightRotated = Quaternion.Euler(0, 0, -angles[i]) * baseDirection;
            SpawnProjectile(context.source, context.damage, rightRotated);
        }
    }

    private void SpawnProjectile(IAttackable source, float damage, Vector2 direction)
    {
        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(source.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(damage, source);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }
}
