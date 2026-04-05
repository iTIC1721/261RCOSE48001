using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public Image fill;
    public Image backFill;

    public float backFillSpeed = 0.5f;

    private Entity parent;

    private Coroutine backFillCoroutine;
    private float backFillTargetAmount = 0;

    private void Awake()
    {
        parent = GetComponentInParent<Entity>();
    }

    private void Start()
    {
        SetHPBar(parent.hp, parent.maxHp);

        parent.OnDamaged += SetHPBar;
    }

    public void SetHPBar(float currentHP, float maxHP)
    {
        hpText.text = currentHP.ToString("F0");

        float amount = currentHP / maxHP;
        fill.fillAmount = amount;

        if (backFill != null)
        {
            backFillTargetAmount = amount;
            if (backFillCoroutine == null) backFillCoroutine = StartCoroutine(BackFillCoroutine());
        }
    }

    private IEnumerator BackFillCoroutine()
    {
        float amount = backFill.fillAmount;

        while (amount >= backFillTargetAmount)
        {
            yield return null;

            amount -= backFillSpeed * Time.deltaTime;
            backFill.fillAmount = amount;
        }

        backFill.fillAmount = backFillTargetAmount;
        backFillCoroutine = null;
    }
}
