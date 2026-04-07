using UnityEngine;

public class AttackHelper : MonoBehaviour
{
    private AttackObjectSpawner attackObjectSpawner;

    private void Awake()
    {
        attackObjectSpawner = GetComponent<AttackObjectSpawner>();
    }

    public void SpawnAttackObject()
    {
        if (attackObjectSpawner == null)
        {
            Log.LogWarning("AttackObjectSpawner가 없습니다.");
            return;
        }

        attackObjectSpawner.SpawnAttackObject();
    }
}
