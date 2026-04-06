using System.Collections;
using UnityEngine;

public class Monster : Entity
{
    [Header("Stat")]
    public bool invulnerable = false;

    [Header("FX")]
    public string damageTMPName;
    public GameObject targetEffect;

    [Header("Ref")]
    public Transform spriteRoot;

    private Animator animator;
    private MonsterBT monsterBT;

    public bool IsDied { get; private set; }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        monsterBT = GetComponent<MonsterBT>();

        Initialize();
    }

    public override void Initialize()
    {
        IsDied = false;
        hp = maxHp;

        animator.SetBool("isDeath", false);
        SetColliderEnabled(true);
        if (targetEffect != null) targetEffect.SetActive(false);
    }

    public void SetTargetEffectEnabled(bool enabled)
    {
        if (targetEffect != null) targetEffect.SetActive(enabled);
    }

    private void SetColliderEnabled(bool enabled)
    {
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in cols)
        {
            col.enabled = enabled;
        }
    }

    public override void Attack()
    {
        monsterBT.AttackAnimation();
    }

    public override void GetDamaged(params DamageInfo[] damageInfos)
    {
        monsterBT.GetDamagedAnimation();

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

        OnDamaged?.Invoke(hp, maxHp);
    }

    public override void Die()
    {
        monsterBT.DieAnimation();

        SetColliderEnabled(false);

        IsDied = true;
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }
}
