using UnityEngine;

public class AttackHelper : MonoBehaviour
{
    private Entity source;
    private AttackObjectSpawner attackObjectSpawner;

    private void Awake()
    {
        source = GetComponent<Entity>();
        attackObjectSpawner = GetComponent<AttackObjectSpawner>();
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
            //Log.LogWarning("AttackObjectSpawner가 없습니다.");
            return;
        }

        attackObjectSpawner.SpawnAttackObject();
    }

    private void TriggerSkill()
    {
        if (source == null || source.skillManager == null) return;

        EntityContext context = source.BuildContext();
        source.skillManager.TriggerSkills(SkillTriggerType.OnAttack);
    }
}
