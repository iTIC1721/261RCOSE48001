using System.Threading;
using UnityEngine;

public class Monster : MonoBehaviour, IEntity
{
    public Transform Transform => this.transform;
    public GameObject GameObject => this.gameObject;

    public string damageTMPName;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Intialize()
    {
        animator.SetBool("isDeath", false);
    }

    public void Attack(float damage)
    {
        animator.SetTrigger("2_Attack");
    }

    public void GetDamaged(params float[] damage)
    {
        animator.SetTrigger("3_Damaged");

        // damageTMP Ãâ·Â
        if (BaseCanvas.Instance != null && BaseCanvas.Instance.damageLayer != null)
        {
            for (int i = 0; i < damage.Length; i++)
            {
                var damageTMP = MANAGER.Pool.PoolingObj(damageTMPName).Get((value) => {
                    value.GetComponent<DamageTMP>().Initialize(BaseCanvas.Instance.damageLayer, Transform, Vector3.zero, damage[i], Color.white);
                });
            }
        }
    }

    public void Die()
    {
        if (!animator.GetBool("isDeath"))
        {
            animator.SetBool("isDeath", true);
            animator.SetTrigger("4_Death");
        }
    }
}
