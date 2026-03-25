using UnityEngine;
using UnityEngine.SceneManagement;

public class StudyStageSelectManager : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("StudyDungeon_DeckSelect");
    }
}
