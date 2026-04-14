using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reflect", menuName = "Skill Effect/Reflect")]
public class ReflectSkillEffect : PassiveSkillEffect
{
    public List<int> reflectCounts = new List<int>() { 1, 2, 3 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > reflectCounts.Count) return;

        if (context.source is Entity entity)
        {
            entity.reflectCount = reflectCounts[stack - 1];
        }
    }
}
