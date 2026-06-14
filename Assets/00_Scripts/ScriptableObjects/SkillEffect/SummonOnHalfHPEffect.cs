using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Skill Effect/SummonOnHalfHPEffect")]
public class SummonOnHalfHPEffect : SkillEffect
{
    [Header("소환 설정")]
    public GameObject monsterPrefab;
    public int summonCount = 2;
    public float spawnRadius = 1.5f;

    public static event Action<Monster> OnMonsterSummoned;

    public override bool Execute(EntityContext context, int stack)
    {
        if (context.source is not Entity entity) return false;

        Transform parent = entity.transform.parent;
        int spawned = 0;
        int maxAttempts = summonCount * 10;

        for (int attempt = 0; attempt < maxAttempts && spawned < summonCount; attempt++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 candidate = entity.transform.position + (Vector3)offset;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                continue;

            Vector3 spawnPos = hit.position;
            spawnPos.z = 0f;

            GameObject go = Instantiate(monsterPrefab, spawnPos, Quaternion.identity, parent);
            Monster monster = go.GetComponent<Monster>();

            if (monster != null)
                OnMonsterSummoned?.Invoke(monster);

            spawned++;
        }

        return spawned > 0;
    }
}