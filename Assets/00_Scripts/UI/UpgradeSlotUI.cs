using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Grid의 셀 1개. UpgradePanel이 데이터를 주입합니다.
/// </summary>
public class UpgradeSlotUI : MonoBehaviour
{
    [Header("Ref")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;       // "Lv. 2 / 5"
    public TextMeshProUGUI costText;        // "100 G" 또는 "MAX"
    public Button upgradeButton;

    private UpgradeEntry entry;

    public Action onUpgraded;

    public void Bind(UpgradeEntry upgradeEntry)
    {
        entry = upgradeEntry;
        Refresh();
    }

    public void Refresh()
    {
        if (entry == null) return;

        var mgr = PermanentUpgradeManager.Instance;
        int level = mgr.GetLevel(entry.upgradeId);
        int maxLevel = entry.MaxLevel;
        bool isMax = level >= maxLevel;

        // 아이콘
        if (icon != null && entry.icon != null)
            icon.sprite = entry.icon;

        // 텍스트
        if (nameText) nameText.text = entry.displayName;
        if (descriptionText) descriptionText.text = BuildDescription();
        if (levelText) levelText.text = $"Lv. {level}";
        if (costText) costText.text = isMax ? "MAX" : $"{entry.costPerLevel[level]} G";

        // 버튼 상태
        if (upgradeButton)
        {
            bool canUpgrade = !isMax && mgr.CanUpgrade(entry.upgradeId, out _);
            upgradeButton.interactable = canUpgrade;
        }
    }

    /// <summary>다음 레벨 수치를 설명에 포함합니다.</summary>
    private string BuildDescription()
    {
        var mgr = PermanentUpgradeManager.Instance;
        int level = mgr.GetLevel(entry.upgradeId);
        bool isMax = level >= entry.MaxLevel;

        string baseDesc = entry.description;

        if (isMax)
            return $"{baseDesc}\n<color=#aaaaaa>최대 레벨 달성</color>";

        float nextValue = entry.valuePerLevel[level];
        return $"{baseDesc}\n<color=#ffffaa>다음 단계: +{nextValue}</color>";
    }

    // ─── 버튼 OnClick에 연결 ───
    public void OnClickUpgrade()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(entry.upgradeId))
        {
            Refresh();
            onUpgraded?.Invoke();
        }
    }
}