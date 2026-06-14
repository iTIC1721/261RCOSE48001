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
        parent.OnDamaged += SetHPBar;
    }

    private void Start()
    {
        SetHPBar(parent.hp, parent.maxHp);
    }

    public void SetHPBar(float currentHP, float maxHP)
    {
        float hp = currentHP > 0 ? currentHP : 0;
        hpText.text = hp.ToString("F0");

        float amount = hp / maxHP;
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
