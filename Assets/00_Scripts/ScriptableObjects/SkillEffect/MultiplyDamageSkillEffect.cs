using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiplyDamage", menuName = "Skill Effect/MultiplyDamage")]
public class MultiplyDamageSkillEffect : PassiveSkillEffect
{
    public List<float> damageMultipliers = new List<float>() { 1 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > damageMultipliers.Count) return;

        if (context.source is Entity entity)
        {
            entity.AddDamageMultiplier(damageMultipliers[stack - 1]);
        }
    }
}
