using UnityEngine;

public class QuizMonsterBT : MonsterBT
{
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    protected override BTNode SetupBehaviorTree()
    {
        BTNode root = new BTMoveStop(monster);
        return root;
    }

    public override void AttackAnimation()
    {
        animator.SetTrigger("2_Attack");
    }

    public override void GetDamagedAnimation()
    {
        animator.SetTrigger("3_Damaged");
    }

    public override void DieAnimation()
    {
        if (!animator.GetBool("isDeath"))
        {
            animator.SetBool("isDeath", true);
            animator.SetTrigger("4_Death");
        }
    }
}
