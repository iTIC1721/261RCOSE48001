using UnityEngine;

public class AttackHelper : MonoBehaviour
{
    private AttackObjectSpawner attackObjectSpawner;
    private SkillManager skillManager;

    private void Awake()
    {
        attackObjectSpawner = GetComponent<AttackObjectSpawner>();
        skillManager = GetComponent<SkillManager>();
    }

    public void Attack()
    {
        SpawnAttackObject();
        if (skillManager != null) skillManager.TriggerSkills(SkillTriggerType.OnAttack);
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
}
