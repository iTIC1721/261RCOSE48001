using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public float panelMoveTime = 0.5f;
    [SerializeField] List<Button> panelButtons = new List<Button>();
    [SerializeField] RectTransform mainPanel;
    [SerializeField] Image bgImage;

    [Space]
    [SerializeField] List<Color> bgColors = new List<Color>();

    private int currentPanelIndex = 0;
    private Coroutine movePanelCoroutine = null;

    private void Start()
    {
        for (int i = 0; i < panelButtons.Count; i++)
        {
            int tmp = i;
            panelButtons[i].onClick.AddListener(() => MovePanel(tmp));
        }

        bgImage.color = bgColors[0];
    }

    private void MovePanel(int index)
    {
        if (movePanelCoroutine != null) return;
        if (currentPanelIndex == index) return;

        movePanelCoroutine = StartCoroutine(MovePanelCoroutine(index));
    }

    private IEnumerator MovePanelCoroutine(int index)
    {
        float panelStartPos = mainPanel.anchoredPosition.x;
        float panelDestPos = 290f - 1040f * index;

        Color bgStartColor = bgColors[currentPanelIndex];
        Color bgDestColor = bgColors[index];

        float t = 0;
        while (t < panelMoveTime)
        {
            yield return null;
            t += Time.deltaTime;

            mainPanel.anchoredPosition = new Vector2(Mathf.Lerp(panelStartPos, panelDestPos, MyMath.EaseInOut(t / panelMoveTime)), mainPanel.anchoredPosition.y);
            bgImage.color = Color.Lerp(bgStartColor, bgDestColor, MyMath.EaseInOut(t / panelMoveTime));
        }

        mainPanel.anchoredPosition = new Vector3(panelDestPos, mainPanel.anchoredPosition.y);
        bgImage.color = bgDestColor;
        currentPanelIndex = index;

        movePanelCoroutine = null;
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
