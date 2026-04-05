using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image fill;
    public TextMeshProUGUI hpText;

    private Entity parent;

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
        fill.fillAmount = currentHP / maxHP;
        hpText.text = currentHP.ToString();
    }
}
