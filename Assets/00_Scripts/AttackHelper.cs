using UnityEngine;

public class AttackHelper : MonoBehaviour
{
    private Entity source;

    private void Awake()
    {
        source = GetComponent<Entity>();
    }

    public void Attack()
    {
        TriggerSkill(SkillTriggerType.OnAttack);
    }

    private void TriggerSkill(SkillTriggerType type)
    {
        if (source == null || source.skillManager == null) return;

        EntityContext context = source.BuildContext();
        source.skillManager.TriggerSkills(type);
    }
}
