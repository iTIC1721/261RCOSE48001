using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public Sprite skillSprite;
    public string skillName;
    public string skillDesc;

    [Header("스킬 효과 (SO 직접 할당)")]
    public SkillEffect skillEffect;

    [Header("스택")]
    public bool isStackable = true;
    [ShowIf("isStackable", true)] public int maxStack = 5;
}
