using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneMoveButton : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => MoveToScene());
    }

    public void MoveToScene()
    {
        if (sceneName.Equals("")) return;
        StartCoroutine(MoveToSceneCoroutine(sceneName));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);

        yield return new WaitForSecondsRealtime(0.2f);

        LoadingSceneManager.LoadScene(sceneName);
    }
}
