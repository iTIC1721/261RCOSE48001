using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DefaultMonsterBT : MonsterBT
{
    private Animator animator;

    private bool isPreparingSkill = false;
    private Coroutine prepareSkillCoroutine;
    private DangerTrail dangerTrail;

    private Vector2 attackDirection;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    protected override BTNode SetupBehaviorTree()
    {
        BTNode root = new BTSelectorNode(new List<BTNode>
        {
            // 플레이어 사망 시 Idle
            new BTConditionDecorator(
                new BTMoveStop(monster), 
                () => Player.Instance.IsDied),
            new BTConditionDecorator(new BTSelectorNode(new List<BTNode>
            {
                // 스킬 사용 중이 아닐 때
                new BTConditionDecorator(new BTSequenceNode(new List<BTNode>
                {
                    new BTCheckAnimationEnd(monster, "DAMAGED"),
                    new BTCheckAnimationEnd(monster, "ATTACK"),
                    new BTSelectorNode(new List<BTNode>
                    {
                        // 쿨타임마다 스킬 시전 시도
                        new BTCooldownDecorator(new BTSequenceNode(new List<BTNode>
                        {
                            new BTCheckPlayerIsInRange(monster, 7.5f, true),
                            new BTMoveStop(monster),
                            new BTInvoke(PrepareSkill)
                        }), 3),
                        // 스킬 쿨타임이 안 찼을 땐 플레이어에게로 이동
                        new BTSequenceNode(new List<BTNode>
                        {
                            new BTCheckPlayerIsInRange(monster, 10),
                            new BTMoveToPlayer(Player.Instance.transform, monster)
                        })
                    })
                }), () => !isPreparingSkill),
            }), () => !monster.IsDied),
            // 스킬 시전 중 죽으면 취소
            new BTConditionDecorator(
                new BTInvoke(StopSkill),
                () => monster.IsDied && isPreparingSkill),
            new BTMoveStop(monster)
        });

        return root;
    }

    private void PrepareSkill()
    {
        isPreparingSkill = true;       

        prepareSkillCoroutine = StartCoroutine(PrepareSkillCoroutine(0.5f));
    }

    private IEnumerator PrepareSkillCoroutine(float time)
    {
        attackDirection = (Player.Instance.transform.position - transform.position).normalized;

        GameObject dangerTrailObj = MANAGER.Pool.PoolingObj("DangerTrail").Get(value => {
            value.GetComponent<DangerTrail>().Initialize(
                startPosition: transform.position,
                direction: attackDirection,
                lifeTime: time);
        });
        dangerTrail = dangerTrailObj.GetComponent<DangerTrail>();

        yield return new WaitForSeconds(time);

        InvokeSkill();
        prepareSkillCoroutine = null;
    }

    private void StopSkill()
    {
        isPreparingSkill = false;

        if (prepareSkillCoroutine != null)
        {
            StopCoroutine(prepareSkillCoroutine);
            prepareSkillCoroutine = null;
            dangerTrail.Return();
        }
    }

    private void InvokeSkill()
    {
        isPreparingSkill = false;

        monster.Attack();
    }

    public override void AttackAnimation()
    {
        animator.SetTrigger("2_Attack");
    }

    public override void GetDamagedAnimation()
    {
        if (!isPreparingSkill) animator.SetTrigger("3_Damaged");
    }

    public override void DieAnimation()
    {
        if (!animator.GetBool("isDeath"))
        {
            animator.SetBool("isDeath", true);
            animator.SetTrigger("4_Death");
        }
    }

    public void SpawnAttackObject()
    {
        if (Player.Instance == null) return;

        Vector2 direction = attackDirection;

        MANAGER.Pool.PoolingObj("DefaultMonsterProjectile").Get(monster.GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(10, monster);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            value.transform.localScale = (monster.spriteRoot.transform.localScale.x < 0) ? new Vector3(-1, 1, 1) : Vector3.one;
            p.direction = direction;
            p.speed = 10;
        });
    }
}
