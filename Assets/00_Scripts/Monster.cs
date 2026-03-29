using UnityEngine;

public class Monster : MonoBehaviour, IEntity
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Attack(float damage)
    {
        animator.SetTrigger("2_Attack");
    }

    public void GetDamaged(float damage)
    {
        animator.SetTrigger("3_Damaged");
    }

    public void Die()
    {
        animator.SetTrigger("4_Death");
    }
}
