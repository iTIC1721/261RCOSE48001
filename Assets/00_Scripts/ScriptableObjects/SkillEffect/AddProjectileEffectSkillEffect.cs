using UnityEngine;

[CreateAssetMenu(fileName = "AddProjectileEffect", menuName = "Skill Effect/AddProjectileEffect")]
public class AddProjectileEffectSkillEffect : PassiveSkillEffect
{
    public ProjectileEffect projectileEffect;

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (context.source is Entity entity)
            entity.AddOnHitEffect(projectileEffect);
    }
}
