using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneMoveButton : MonoBehaviour
{
    public string sceneName;
    public float fadeInTime = 0.1f;
    public float fadeOutTime = 0.2f;
    public bool autoLink = true;

    private Button button;

    private void Start()
    {
        if (autoLink)
        {
            Link();
        }
    }

    public void Link()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(MoveToScene);
    }

    public void Unlink()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.RemoveListener(MoveToScene);
    }

    public void MoveToScene()
    {
        if (sceneName.Equals("")) return;
        StartCoroutine(MoveToSceneCoroutine(sceneName));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(fadeInTime);

        yield return new WaitForSecondsRealtime(fadeInTime + 0.1f);

        LoadingSceneManager.LoadScene(sceneName, fadeOutTime);
    }
}
