using UnityEngine;

[CreateAssetMenu(fileName = "TestSkillEffect", menuName = "Skill Effect/TestSkillEffect")]
public class TestSkillEffect : SkillEffect
{
    public override bool Execute(EntityContext context, int stack)
    {
        Log.LogMessage("테스트 공격!");

        return true;
    }
}
