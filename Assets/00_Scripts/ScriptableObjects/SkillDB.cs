using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDB", menuName = "DB/SkillDB")]
public class SkillDB : ScriptableObject
{
    public List<SkillData> skillDatas = new List<SkillData>();
}
