using UnityEngine;

[CreateAssetMenu(fileName = "FrontShot", menuName = "Skill Effect/FrontShot")]
public class FrontShotSkillEffect : ShotSkillEffect
{
    public float[] angles = new float[3] { 0, 7.5f, 15f };
    public float damageMultiplier = 0.75f;

    public override void Execute(EntityContext context, int stack)
    {
        Vector2 baseDirection = context.direction;

        int shotCount = (stack > angles.Length) ? angles.Length : stack;
        for (int i = 0; i < shotCount; i++)
        {
            if (i == 0)
            {
                SpawnProjectile(context.source, context.damage, baseDirection);
            }
            else
            {
                float damage = context.damage * damageMultiplier;

                // left
                Vector2 leftRotated = Quaternion.Euler(0, 0, angles[i]) * baseDirection;
                SpawnProjectile(context.source, damage, leftRotated);

                // right
                Vector2 rightRotated = Quaternion.Euler(0, 0, -angles[i]) * baseDirection;
                SpawnProjectile(context.source, damage, rightRotated);
            }            
        }
    }
}
