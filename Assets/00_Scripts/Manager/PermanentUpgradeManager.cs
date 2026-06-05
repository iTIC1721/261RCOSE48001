using System.Collections.Generic;
using UnityEngine;

public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager Instance { get; private set; }

    [SerializeField] private PermanentUpgradeData upgradeData;
    public PermanentUpgradeData UpgradeData => upgradeData;

    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ───────── 조회 ─────────

    public int GetLevel(string upgradeId)
    {
        return upgradeLevels.TryGetValue(upgradeId, out int level) ? level : 0;
    }

    public float GetTotalValue(UpgradeStatType statType)
    {
        float total = 0f;
        foreach (var entry in upgradeData.upgrades)
        {
            if (entry.statType != statType) continue;

            int level = GetLevel(entry.upgradeId);
            for (int i = 0; i < level; i++)
                total += entry.valuePerLevel[i];
        }
        return total;
    }

    public bool CanUpgrade(string upgradeId, out string reason)
    {
        var entry = GetEntry(upgradeId);
        if (entry == null) { reason = "존재하지 않는 업그레이드"; return false; }

        int level = GetLevel(upgradeId);
        if (level >= entry.MaxLevel) { reason = "최대 레벨 도달"; return false; } // MaxLevel 프로퍼티 사용

        int cost = entry.costPerLevel[level];
        int money = SaveSystem.LoadPlayerData().money;
        if (money < cost) { reason = $"재화 부족 ({money}/{cost})"; return false; }

        reason = string.Empty;
        return true;
    }

    // ───────── 업그레이드 ─────────

    public bool TryUpgrade(string upgradeId)
    {
        if (!CanUpgrade(upgradeId, out string reason))
        {
            Debug.LogWarning($"[PermanentUpgrade] 업그레이드 실패: {reason}");
            return false;
        }

        var entry = GetEntry(upgradeId);
        int level = GetLevel(upgradeId);
        int cost = entry.costPerLevel[level];

        var data = SaveSystem.LoadPlayerData();
        data.money -= cost;
        SaveSystem.SavePlayerData(data);

        upgradeLevels[upgradeId] = level + 1;
        Save();

        Debug.Log($"[PermanentUpgrade] {entry.displayName} → Lv.{level + 1}");
        return true;
    }

    // ───────── Player 적용 ─────────

    public void ApplyToPlayer(Player player)
    {
        // MaxHp
        player.maxHp += GetTotalValue(UpgradeStatType.MaxHp);
        player.hp = player.maxHp;

        // MoveSpeed
        player.moveSpeed += GetTotalValue(UpgradeStatType.MoveSpeed);

        // AttackDamage
        float damageBonus = GetTotalValue(UpgradeStatType.AttackDamage);
        if (damageBonus != 0f)
            player.baseDamage += damageBonus;

        // AttackDelay
        float delayReduction = GetTotalValue(UpgradeStatType.AttackDelay);
        if (delayReduction != 0f)
            player.baseAttackDelay = Mathf.Max(0.1f, player.baseAttackDelay - delayReduction);
    }

    // ───────── 세이브 / 로드 ─────────

    private void Save()
    {
        var data = SaveSystem.LoadPlayerData();
        data.upgradeKeys.Clear();
        data.upgradeValues.Clear();

        foreach (var kv in upgradeLevels)
        {
            data.upgradeKeys.Add(kv.Key);
            data.upgradeValues.Add(kv.Value);
        }

        SaveSystem.SavePlayerData(data);
    }

    private void Load()
    {
        upgradeLevels.Clear();
        var data = SaveSystem.LoadPlayerData();

        for (int i = 0; i < data.upgradeKeys.Count; i++)
            upgradeLevels[data.upgradeKeys[i]] = data.upgradeValues[i];
    }

    private UpgradeEntry GetEntry(string upgradeId)
        => upgradeData.upgrades.Find(e => e.upgradeId == upgradeId);
}