using UnityEngine;

public enum SkillTriggerType
{
    Passive, OnAttack, OnHit, OnKill, TimeBased
}

public abstract class SkillEffect : ScriptableObject
{
    [Header("발동 조건")]
    public SkillTriggerType triggerType;

    public abstract void Execute(int stack);

    public virtual bool CanTrigger(SkillTriggerType currentTrigger)
       => triggerType == currentTrigger;
}

public abstract class PassiveSkillEffect : SkillEffect
{
    public override void Execute(int stack) { }

    public abstract void ApplyPassive(int stack);
}
