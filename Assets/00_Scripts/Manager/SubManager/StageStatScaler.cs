using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지 진행에 따라 몬스터의 HP와 공격력을 스케일링합니다.
/// MapManager와 같은 GameObject에 붙여서 사용하세요.
///
/// 공식 교체 방법:
///   인스펙터의 Hp Formula / Damage Formula 슬롯에
///   ScalingFormula 애셋(Linear, Exponential, Step, S-Curve 등)을 연결합니다.
///   새 공식이 필요하면 ScalingFormula를 상속해 Evaluate()만 구현하세요.
/// </summary>
public class StageStatScaler : MonoBehaviour
{
    [Header("HP 공식")]
    [Tooltip("ScalingFormula 애셋을 연결하세요.\n비워두면 HP는 스케일링하지 않습니다.")]
    public ScalingFormula hpFormula;

    [Header("공격력 공식")]
    [Tooltip("ScalingFormula 애셋을 연결하세요.\n비워두면 공격력은 스케일링하지 않습니다.")]
    public ScalingFormula damageFormula;

    [Header("디버그")]
    public bool logScaling = true;

    /// <summary>
    /// 해당 스테이지의 몬스터 목록에 스탯 스케일링을 적용합니다.
    /// MapManager.NextStage() 안에서 몬스터 리스트를 가져온 직후 호출됩니다.
    /// </summary>
    public void ApplyStats(int currentStage, int totalStages, List<Monster> monsters)
    {
        if (monsters == null || monsters.Count == 0)
            return;

        float hpMult = hpFormula != null ? hpFormula.Evaluate(currentStage, totalStages) : 1f;
        float dmgMult = damageFormula != null ? damageFormula.Evaluate(currentStage, totalStages) : 1f;

        foreach (Monster monster in monsters)
        {
            if (monster == null) continue;

            monster.maxHp = monster.maxHp * hpMult;
            monster.hp = monster.maxHp;
            monster.baseDamage = monster.baseDamage * dmgMult;

            monster.OnDamaged?.Invoke(monster.hp, monster.maxHp);
        }

        if (logScaling)
        {
            Log.LogMessage(
                $"[StatScaler] Stage {currentStage}/{totalStages} | " +
                $"HP x{hpMult:F2} | DMG x{dmgMult:F2} | " +
                $"Monsters: {monsters.Count}");
        }
    }
}