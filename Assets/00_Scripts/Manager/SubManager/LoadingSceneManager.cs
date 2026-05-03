using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    static string nextScene;
    static float fadeOutTime = 0.2f;

    public static void LoadScene(string sceneName, float fadeOutTime = 0.2f)
    {
        LoadingSceneManager.nextScene = sceneName;
        LoadingSceneManager.fadeOutTime = fadeOutTime;
        SceneManager.LoadScene("Loading");
    }

    [SerializeField] Image progressBar;
    private float sceneChangeWaitTime = 0.3f;

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            yield return null;

            if (progressBar != null)
            {
                progressBar.fillAmount = op.progress;
            }

            if (op.progress >= 0.9f)
            {
                Log.LogMessage($"Scene Moved: {nextScene}");
                GLOBAL_CANVAS.Fade.FadeOut(fadeOutTime, null, sceneChangeWaitTime);

                op.allowSceneActivation = true;
            }
        }
    }
}
