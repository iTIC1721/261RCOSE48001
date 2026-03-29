using UnityEngine;

public class Monster : MonoBehaviour, IEntity
{
    public Transform Transform => this.transform;
    public GameObject GameObject => this.gameObject;

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
