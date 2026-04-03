using UnityEngine;

public class Monster : PoolObject, IEntity
{
    public Transform Transform => this.transform;

    [Header("Stat")]
    public float maxHp = 10f;
    public bool invulnerable = false;

    private float hp = 0;

    [Header("FX")]
    public string damageTMPName;
    public GameObject targetEffect;

    private Animator animator;

    private bool isDied = false;
    public bool IsDied { get { return isDied; } }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        Initialize();
    }

    public void Initialize()
    {
        isDied = false;
        hp = maxHp;

        animator.SetBool("isDeath", false);
        if (targetEffect != null) targetEffect.SetActive(false);
    }

    public void EnableTargetEffect()
    {
        if (targetEffect != null) targetEffect.SetActive(true);
    }

    public void DisableTargetEffect()
    {
        if (targetEffect != null) targetEffect.SetActive(false);
    }

    public void Attack()
    {
        animator.SetTrigger("2_Attack");
    }

    public void GetDamaged(params DamageInfo[] damageInfos)
    {
        animator.SetTrigger("3_Damaged");

        for (int i = 0; i < damageInfos.Length; i++)
        {
            // 데미지 계산
            float damage = damageInfos[i].damage;

            // 데미지 부여
            if (!invulnerable)
            {
                hp -= damage;
                if (hp <= 0)
                {
                    Die();
                    break;
                }
            }

            // damageTMP 출력
            if (BaseCanvas.Instance != null && BaseCanvas.Instance.damageLayer != null)
            {
                if (damageTMPName.Length > 0)
                {
                    var damageTMP = MANAGER.Pool.PoolingObj(damageTMPName).Get((value) => {
                        value.GetComponent<DamageTMP>().Initialize(BaseCanvas.Instance.damageLayer, Transform, Vector3.zero, damage, Color.white);
                    });
                }
            }
            else
            {
                Log.LogWarning("BaseCanvas 또는 damageLayer가 없습니다.");
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

        isDied = true;
        Return();
    }
}
