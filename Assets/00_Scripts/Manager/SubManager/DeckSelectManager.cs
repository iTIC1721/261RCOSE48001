using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckSelectManager : MonoBehaviour
{
    public void MoveToScene(string sceneName)
    {
        StartCoroutine(MoveToSceneCoroutine(sceneName));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);

        yield return new WaitForSecondsRealtime(0.2f);

        LoadingSceneManager.LoadScene(sceneName);
    }
}
