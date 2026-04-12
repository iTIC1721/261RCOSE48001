using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public Sprite skillSprite;
    public string skillName;
    public string skillDesc;
}
