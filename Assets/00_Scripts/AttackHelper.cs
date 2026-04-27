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
        TriggerSkill();
    }

    private void TriggerSkill()
    {
        if (source == null || source.skillManager == null) return;

        EntityContext context = source.BuildContext();
        source.skillManager.TriggerSkills(SkillTriggerType.OnAttack);
    }
}
