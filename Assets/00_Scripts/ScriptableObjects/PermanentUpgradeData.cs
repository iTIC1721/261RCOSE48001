using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PermanentUpgradeData", menuName = "Roguelite/PermanentUpgradeData")]
public class PermanentUpgradeData : ScriptableObject
{
    public List<UpgradeEntry> upgrades;
}
[System.Serializable]
public class UpgradeEntry
{
    public string upgradeId;
    public string displayName;
    [TextArea] public string description;
    public UpgradeStatType statType;

    /// <summary>
    /// 인덱스 = 레벨-1, 값 = 해당 레벨 달성 시 누적 적용되는 수치
    /// costPerLevel[0] → Lv.1 해금 비용, valuePerLevel[0] → Lv.1 적용 수치
    /// </summary>
    public List<float> valuePerLevel;
    public List<int> costPerLevel;

    public int MaxLevel => valuePerLevel?.Count ?? 0;

    public Sprite icon;
}

public enum UpgradeStatType
{
    MaxHp,
    MoveSpeed,
    AttackDelay,
    AttackDamage,
}