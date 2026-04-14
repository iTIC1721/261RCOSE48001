using UnityEngine;

[CreateAssetMenu(fileName = "Spinner", menuName = "Skill Effect/Spinner")]
public class SpinnerSkillEffect : PassiveSkillEffect
{
    public string spinnerName = "TestSpinner";

    public override void ApplyPassive(EntityContext context, int stack)
    {
        SpawnSpinner(context.source, context.damage);
    }

    private void SpawnSpinner(IAttackable source, float damage)
    {
        MANAGER.Pool.PoolingObj(spinnerName).Get(source.GetAttackPosition(), value => {
            AttackSpinner attackSpinner = value.GetComponent<AttackSpinner>();
            attackSpinner.Initialize(damage, source);
        });
    }
}
