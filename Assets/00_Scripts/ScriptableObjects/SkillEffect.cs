using System.Threading;
using UnityEngine;

public enum SkillTriggerType
{
    Passive, OnAttack, OnDamaged, OnHit, OnKill, TimeBased
}

public abstract class SkillEffect : ScriptableObject
{
    [Header("발동 조건")]
    public SkillTriggerType triggerType;

    [Header("옵션")]
    public bool onlyOnce = false;   // 전투 중 1회만 발동

    public abstract bool Execute(EntityContext context, int stack);

    public virtual bool CanTrigger(SkillTriggerType currentTrigger)
       => triggerType == currentTrigger;
}

public abstract class PassiveSkillEffect : SkillEffect
{
    public override bool Execute(EntityContext context, int stack) => true;

    public abstract void ApplyPassive(EntityContext context, int stack);
}

public abstract class ShotSkillEffect : SkillEffect
{
    [Header("투사체")]
    public bool playerProjectile = true;
    [ShowIf("playerProjectile", false)] public string projectileName = "PlayerProjectile";

    protected void SpawnProjectile(IAttackable source, float damage, Vector2 direction)
    {
        if (playerProjectile)
        {
            int characterId = Player.Instance.CharacterId;
            projectileName = MANAGER.DB.characterDB.GetCharacterData(characterId).projectileName;
        }

        MANAGER.Pool.PoolingObj(projectileName).Get(source.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();

            if (source is Entity entity)
            {
                p.Initialize(
                    damage, 
                    source,
                    entity.ricochetCount, 
                    entity.piercingCount, 
                    entity.reflectCount);

                // 스폰 시점의 스냅샷을 복사
                p.SetEffects(entity.ProjectileEffects);

                // 투사체 좌우반전
                value.transform.localScale = (entity.spriteRoot.transform.localScale.x < 0) ? new Vector3(-1, 1, 1) : Vector3.one;
            }
            else
            {
                p.Initialize(damage, source);
            }

            value.transform.rotation = Quaternion.Euler(0, 0,
                -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }
}

