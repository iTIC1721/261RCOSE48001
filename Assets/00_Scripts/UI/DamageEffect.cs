using UnityEngine;
using UnityEngine.UI;

public class DamageEffect : MonoBehaviour
{
    public Image damageImage;
    public float flashSpeed = 5f;
    public float maxAlpha = 0.1f;

    private float currentAlpha = 0f;

    void Update()
    {
        // 서서히 사라짐
        currentAlpha = Mathf.Lerp(currentAlpha, 0f, flashSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }

    public void OnDamage()
    {
        currentAlpha = maxAlpha;
        SetAlpha(currentAlpha);
    }

    void SetAlpha(float a)
    {
        Color c = damageImage.color;
        c.a = a;
        damageImage.color = c;
    }
}
