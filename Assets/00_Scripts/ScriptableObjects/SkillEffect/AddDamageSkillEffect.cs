using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddDamage", menuName = "Skill Effect/AddDamage")]
public class AddDamageSkillEffect : PassiveSkillEffect
{
    public List<float> damageAdders = new List<float>() { 0 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > damageAdders.Count) return;

        if (context.source is Entity entity)
        {
            entity.AddDamageAdder(damageAdders[stack - 1]);
        }
    }
}
