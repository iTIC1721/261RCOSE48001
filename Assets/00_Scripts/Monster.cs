using UnityEngine;

public class Monster : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        animator.SetTrigger("2_Attack");
    }

    public void GetDamaged()
    {
        animator.SetTrigger("3_Damaged");
    }

    public void Die()
    {
        animator.SetTrigger("4_Death");
    }
}
