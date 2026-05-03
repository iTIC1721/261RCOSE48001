using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudyStageSelectManager : MonoBehaviour
{
    public void Back()
    {
        StartCoroutine(MoveToSceneCoroutine("StudyDungeon_DeckSelect"));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);

        yield return new WaitForSecondsRealtime(0.2f);

        LoadingSceneManager.LoadScene(sceneName);
    }
}
