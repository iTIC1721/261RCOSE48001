using UnityEngine;

[CreateAssetMenu(fileName = "QuizSetting", menuName = "Study/QuizSetting")]
public class QuizSetting : ScriptableObject
{
    public float timeLimit = 5f;
    public int maxHp = 5;
    public float correctStayTime = 1f;
    public float incorrectStayTime = 3f;
    public int monsterId = 0;
}
