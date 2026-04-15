using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Serializable]
    public class SkillItem { 
        public SkillData skillData; 
        [Range(1, 10)] public int stack = 1; 
    }

    public List<SkillItem> skills;

    private Entity entity;

    // 스킬 이름 → (SkillData, 현재 스택) 딕셔너리
    private Dictionary<string, (SkillData data, int stack)> activeSkills = new();

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    private void Start()
    {
        InitializeSkill();
    }

    public void InitializeSkill()
    {
        foreach (var item in skills)
        {
            if (item.stack < 1) item.stack = 1;

            for (int i = 0; i < item.stack; i++)
            {
                AddSkill(item.skillData);
            }
        }
    }

    // ─────────────────────────────────────────────
    // 스킬 추가 (레벨업 시 호출)
    // ─────────────────────────────────────────────
    public void AddSkill(SkillData data)
    {
        if (activeSkills.TryGetValue(data.skillName, out var existing))
        {
            if (!data.isStackable) return;
            if (existing.stack >= data.maxStack) return;

            activeSkills[data.skillName] = (existing.data, existing.stack + 1);
            Log.LogMessage($"[SkillManager] {data.skillName} 스택 {existing.stack + 1} 획득");

            // 패시브 스킬은 즉시 적용
            ApplyPassiveIfNeeded(existing.data, existing.stack + 1);
        }
        else
        {
            activeSkills[data.skillName] = (data, 1);
            Log.LogMessage($"[SkillManager] 새 스킬 획득: {data.skillName}");

            // 패시브 스킬은 즉시 적용
            ApplyPassiveIfNeeded(data, 1);
        }
    }

    // ─────────────────────────────────────────────
    // 트리거 이벤트 발동 (Player에서 호출)
    // ─────────────────────────────────────────────
    public void TriggerSkills(SkillTriggerType trigger)
    {
        foreach (var (skillName, entry) in activeSkills)
        {
            var (data, stack) = entry;
            foreach (var effect in data.skillEffects)
            {
                if (effect == null) continue;

                // 발동 조건 체크
                if (!effect.CanTrigger(trigger)) continue;

                // 패시브는 TriggerSkills로 발동하지 않음
                if (effect.triggerType == SkillTriggerType.Passive) continue;

                // 실행
                EntityContext context = entity.BuildContext();
                effect.Execute(context, stack);

                Log.LogMessage($"스킬 발동: {data.name}");
            }            
        }
    }

    // ─────────────────────────────────────────────
    // 패시브 스킬 적용 (스탯에 직접 반영)
    // ─────────────────────────────────────────────
    private void ApplyPassiveIfNeeded(SkillData data, int stack)
    {
        EntityContext context = entity.BuildContext();
        foreach (var effect in data.skillEffects)
        {
            if (effect is PassiveSkillEffect passiveEffect)
                passiveEffect.ApplyPassive(context, stack);
        }
    }

    // ─────────────────────────────────────────────
    // 유틸리티
    // ─────────────────────────────────────────────
    /// <summary>
    /// 현재 보유 스킬 목록 반환
    /// </summary>
    public IEnumerable<(SkillData data, int stack)> GetActiveSkills()
        => activeSkills.Values;

    /// <summary>현재 보유 스킬 목록 반환</summary>
    /// <param name="trigger">트리거 타입</param>
    public IEnumerable<(SkillData data, int stack)> GetActiveSkills(SkillTriggerType trigger)
        => activeSkills.Values.Where(item => item.data.skillEffects.Exists(m => m.triggerType == trigger));
}
