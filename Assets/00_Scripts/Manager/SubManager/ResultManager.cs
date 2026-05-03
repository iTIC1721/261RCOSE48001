using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;

    private void Start()
    {
        Time.timeScale = 1.0f;

        bool isCleared = MANAGER.Game.isCleared;
        if (isCleared)
        {
            titleText.text = "게임 클리어!";
        }
        else
        {
            titleText.text = "게임 오버...";
        }
    }

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
