using UnityEngine;

public class AttackHelper : MonoBehaviour
{
    private Entity source;
    private AttackObjectSpawner attackObjectSpawner;
    private SkillManager skillManager;

    private void Awake()
    {
        source = GetComponent<Entity>();
        attackObjectSpawner = GetComponent<AttackObjectSpawner>();
        skillManager = GetComponent<SkillManager>();
    }

    public void Attack()
    {
        SpawnAttackObject();
        TriggerSkill();
    }

    private void SpawnAttackObject()
    {
        if (attackObjectSpawner == null)
        {
            Log.LogWarning("AttackObjectSpawner가 없습니다.");
            return;
        }

        attackObjectSpawner.SpawnAttackObject();
    }

    private void TriggerSkill()
    {
        if (source == null || skillManager == null) return;

        EntityContext context = source.BuildContext(10);
        skillManager.TriggerSkills(context, SkillTriggerType.OnAttack);
    }
}
