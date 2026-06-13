using UnityEngine;

[CreateAssetMenu(fileName = "DefaultShot", menuName = "Skill Effect/DefaultShot")]
public class DefaultShotSkillEffect : ShotSkillEffect
{
    public override bool Execute(EntityContext context, int stack)
    {
        Vector2 direction = context.direction;

        SpawnProjectile(context.source, context.damage, direction);

        return true;
    }
}
