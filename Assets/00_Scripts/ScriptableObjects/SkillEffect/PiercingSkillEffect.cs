using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Piercing", menuName = "Skill Effect/Piercing")]
public class PiercingSkillEffect : PassiveSkillEffect
{
    public List<int> piercingCounts = new List<int>() { 1, 2, 3 };

    public override void ApplyPassive(EntityContext context, int stack)
    {
        if (stack > piercingCounts.Count) return;

        if (context.source is Entity entity)
        {
            entity.piercingCount = piercingCounts[stack - 1];
        }
    }
}
