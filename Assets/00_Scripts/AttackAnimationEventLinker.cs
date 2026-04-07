using UnityEngine;

public class AttackAnimationEventLinker : MonoBehaviour
{
    private AttackHelper attackHelper;

    private void Awake()
    {
        attackHelper = GetComponentInParent<AttackHelper>();
    }

    public void SpawnAttackObject()
    {
        if (attackHelper != null) attackHelper.SpawnAttackObject();
    }
}
