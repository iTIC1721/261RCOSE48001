using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneMoveButton : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        if (sceneName.Equals("")) return;

        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => MoveToScene(sceneName));
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
