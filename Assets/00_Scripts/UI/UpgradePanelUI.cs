using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Ref")]
    public GameObject slotPrefab;
    public Transform gridContent;

    private readonly List<UpgradeSlotUI> slots = new();

    public Action onAnyUpgraded;

    private void OnEnable()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        foreach (var slot in slots)
            Destroy(slot.gameObject);
        slots.Clear();

        // 매니저에서 직접 참조
        var upgradeData = PermanentUpgradeManager.Instance.UpgradeData;
        if (upgradeData == null)
        {
            Debug.LogError("[UpgradePanelUI] PermanentUpgradeManager의 UpgradeData가 없습니다.");
            return;
        }

        foreach (var entry in upgradeData.upgrades)
        {
            var go = Instantiate(slotPrefab, gridContent);
            var slot = go.GetComponent<UpgradeSlotUI>();
            slot.Bind(entry);

            // 업그레이드 성공 시 전체 슬롯 갱신 + 외부 콜백 호출
            slot.onUpgraded += () => {
                RefreshAll();
                onAnyUpgraded?.Invoke();
            };

            slots.Add(slot);
        }
    }

    public void RefreshAll()
    {
        foreach (var slot in slots)
            slot.Refresh();
    }
}