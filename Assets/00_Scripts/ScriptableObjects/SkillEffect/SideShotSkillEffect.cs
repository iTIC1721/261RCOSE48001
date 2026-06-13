using UnityEngine;

[CreateAssetMenu(fileName = "SideShot", menuName = "Skill Effect/SideShot")]
public class SideShotSkillEffect : ShotSkillEffect
{
    public float[] angles = new float[3] { 0, 10f, -10f };
    public float damageMultiplier = 1f;

    public override bool Execute(EntityContext context, int stack)
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

        return true;
    }
}
