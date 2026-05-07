using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    public Image skillIcon;
    public TextMeshProUGUI stackText;

    public void SetIcon(SkillData data, int stack)
    {
        if (skillIcon != null) skillIcon.sprite = data.skillSprite;
        if (stackText != null) stackText.text = stack.ToString();
    }
}
