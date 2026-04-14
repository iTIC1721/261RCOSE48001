using UnityEngine;

[CreateAssetMenu(fileName = "DiagonalShot", menuName = "Skill Effect/DiagonalShot")]
public class DiagonalShotSkillEffect : SkillEffect
{
    public float[] angles = new float[3] { 0, 15f, -15f };

    public override void Execute(EntityContext context, int stack)
    {
        Vector2 baseDirection = context.direction;

        Vector2 leftDirection = Quaternion.Euler(0, 0, 45) * baseDirection;
        Vector2 rightDirection = Quaternion.Euler(0, 0, -45) * baseDirection;

        int shotCount = (stack > angles.Length) ? angles.Length : stack;
        for (int i = 0; i < shotCount; i++)
        {
            // left
            Vector2 leftRotated = Quaternion.Euler(0, 0, -angles[i]) * leftDirection;
            SpawnProjectile(context.source, context.damage, leftRotated);

            // right
            Vector2 rightRotated = Quaternion.Euler(0, 0, angles[i]) * rightDirection;
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
