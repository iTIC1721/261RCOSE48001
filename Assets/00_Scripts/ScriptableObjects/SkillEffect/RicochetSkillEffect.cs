using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ricochet", menuName = "Skill Effect/Ricochet")]
public class RicochetSkillEffect : PassiveSkillEffect
{
    public List<int> ricochetCounts = new List<int>() { 1, 2, 3 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > ricochetCounts.Count) return;

        if (context.source is Entity entity)
        {
            entity.ricochetCount = ricochetCounts[stack - 1];
        }
    }
}
