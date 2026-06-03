using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    static string nextScene;
    static float fadeOutTime = 0.2f;

    /// <summary>
    /// 씬 로딩 중 실행할 프리로드 작업.
    /// null이면 기존처럼 씬만 로드합니다.
    /// 코루틴이 완료되면 씬 전환이 진행됩니다.
    /// </summary>
    static Func<IEnumerator> preloadTask = null;

    /// <summary>기존 방식 — 프리로드 없이 씬만 전환합니다.</summary>
    public static void LoadScene(string sceneName, float fadeOutTime = 0.2f)
    {
        LoadingSceneManager.nextScene = sceneName;
        LoadingSceneManager.fadeOutTime = fadeOutTime;
        LoadingSceneManager.preloadTask = null;
        SceneManager.LoadScene("Loading");
    }

    /// <summary>
    /// 프리로드 포함 씬 전환.
    /// 로딩 화면이 떠 있는 동안 preloadTask 코루틴을 실행하고,
    /// 씬 로드 + 프리로드가 모두 완료되면 씬을 활성화합니다.
    /// </summary>
    public static void LoadSceneWithPreload(string sceneName, Func<IEnumerator> preloadTask, float fadeOutTime = 0.2f)
    {
        LoadingSceneManager.nextScene = sceneName;
        LoadingSceneManager.fadeOutTime = fadeOutTime;
        LoadingSceneManager.preloadTask = preloadTask;
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
        // 씬 로드와 프리로드를 병렬 실행
        bool sceneReady = false;
        bool preloadDone = preloadTask == null; // 프리로드 없으면 즉시 완료

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        // 씬 로드 진행
        StartCoroutine(WaitForSceneLoad(op, () => sceneReady = true));

        // 프리로드 작업 병렬 실행
        if (preloadTask != null)
            StartCoroutine(RunPreload(() => preloadDone = true));

        // 둘 다 완료될 때까지 대기
        yield return new WaitUntil(() => sceneReady && preloadDone);

        Log.LogMessage($"Scene Moved: {nextScene}");
        GLOBAL_CANVAS.Fade.FadeOut(fadeOutTime, null, sceneChangeWaitTime);

        op.allowSceneActivation = true;

        // 다음 호출을 위해 프리로드 초기화
        preloadTask = null;
    }

    private IEnumerator WaitForSceneLoad(AsyncOperation op, Action onReady)
    {
        while (op.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.fillAmount = op.progress;
            yield return null;
        }

        if (progressBar != null)
            progressBar.fillAmount = 1f;

        onReady?.Invoke();
    }

    private IEnumerator RunPreload(Action onDone)
    {
        yield return preloadTask?.Invoke();
        onDone?.Invoke();
    }
}