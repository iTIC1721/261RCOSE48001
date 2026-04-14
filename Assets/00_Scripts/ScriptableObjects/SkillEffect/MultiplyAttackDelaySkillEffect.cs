using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiplyAttackDelay", menuName = "Skill Effect/MultiplyAttackDelay")]
public class MultiplyAttackDelaySkillEffect : PassiveSkillEffect
{
    public List<float> attackDelayMultipliers = new List<float>() { 1 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > attackDelayMultipliers.Count) return;

        if (context.source is Entity entity)
        {
            entity.AddAttackDelayMultiplier(attackDelayMultipliers[stack - 1]);
        }
    }
}
