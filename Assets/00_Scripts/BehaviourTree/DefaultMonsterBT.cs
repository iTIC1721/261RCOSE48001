using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultMonsterBT : MonsterBT
{
    private bool isPreparingSkill = false;
    private Coroutine prepareSkillCoroutine;
    private DangerTrail dangerTrail;

    protected override BTNode SetupBehaviorTree()
    {
        BTNode root = new BTSelectorNode(new List<BTNode>
        {
            new BTConditionDecorator(new BTSelectorNode(new List<BTNode>
            {
                new BTConditionDecorator(new BTSequenceNode(new List<BTNode>
                {
                    // 스킬 사용 중이 아닐 때
                    new BTCheckAnimationEnd(monster, "DAMAGED"),
                    new BTCheckAnimationEnd(monster, "ATTACK"),
                    new BTSelectorNode(new List<BTNode>
                    {

                        // 쿨타임마다 스킬 시전 시도
                        new BTCooldownDecorator(new BTSequenceNode(new List<BTNode>
                        {
                            new BTCheckPlayerIsInRange(monster, 7.5f, true),
                            new BTInvoke(monster, PrepareSkill)
                        }), 6),
                        // 스킬 쿨타임이 안 찼을 땐 플레이어에게로 이동
                        new BTSequenceNode(new List<BTNode>
                        {
                            new BTCheckPlayerIsInRange(monster, 10),
                            new BTMoveToPlayer(Player.Instance.transform, monster)
                        })
                    })
                }), () => !isPreparingSkill),
                new BTConditionDecorator(new BTSelectorNode(new List<BTNode>
                {
                    // 스킬 사용 중일 때
                    new BTSequenceNode(new List<BTNode>
                    {
                        // 스킬 시전 중 공격받으면 취소
                        new BTInvertDecorator(new BTCheckAnimationEnd(monster, "DAMAGED")),
                        new BTInvoke(monster, StopSkill)
                    }),                  
                }), () => isPreparingSkill)
            }), () => !monster.IsDied),
            new BTIdle(monster)
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
        GameObject dangerTrailObj = MANAGER.Pool.PoolingObj("DangerTrail").Get(value => {
            value.GetComponent<DangerTrail>().Initialize(
                startPosition: transform.position,
                direction: (Player.Instance.transform.position - transform.position).normalized,
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
}
