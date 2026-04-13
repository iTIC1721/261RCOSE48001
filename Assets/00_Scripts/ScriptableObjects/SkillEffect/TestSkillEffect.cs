using UnityEngine;

[CreateAssetMenu(fileName = "TestSkillEffect", menuName = "Skill Effect/TestSkillEffect")]
public class TestSkillEffect : SkillEffect
{
    public override void Execute(int stack)
    {
        Log.LogMessage("테스트 공격!");
    }
}
