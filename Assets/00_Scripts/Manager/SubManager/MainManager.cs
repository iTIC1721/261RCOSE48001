using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public float panelHorizontalMoveTime = 0.5f;
    public float panelVerticalMoveTime = 0.3f;
    [SerializeField] List<Button> panelButtons = new List<Button>();
    [SerializeField] RectTransform mainPanel;
    [SerializeField] RectTransform dungeonPanel;
    [SerializeField] int dungeonCount = 3;
    [SerializeField] RectTransform particle;
    [SerializeField] Image bgImage;

    [Space]
    [SerializeField] List<Color> bgColors = new List<Color>();
    [SerializeField] Color studyBgColor;

    private int currentPanelIndex = 0;
    private int currentDungeonIndex = 0;
    private Coroutine movePanelHorizontalCoroutine = null;
    private Coroutine movePanelVerticalCoroutine = null;

    private void Start()
    {
        for (int i = 0; i < panelButtons.Count; i++)
        {
            int tmp = i;
            panelButtons[i].onClick.AddListener(() => MovePanelHorizontal(tmp));
        }

        bgImage.color = bgColors[0];
    }

    private void MovePanelHorizontal(int index)
    {
        if (movePanelHorizontalCoroutine != null) return;
        if (currentPanelIndex == index) return;

        movePanelHorizontalCoroutine = StartCoroutine(MovePanelHorizontalCoroutine(index));
    }

    private IEnumerator MovePanelHorizontalCoroutine(int index)
    {
        float panelStartPos = mainPanel.anchoredPosition.x;
        float panelDestPos = -1040f * index;

        float particleStartPos = particle.anchoredPosition.x;
        float particleDestPos = -1000f * index;

        Color bgStartColor = (index == 1) ? bgColors[currentDungeonIndex] : studyBgColor;
        Color bgDestColor = (index == 0) ? bgColors[currentDungeonIndex] : studyBgColor;

        float time = 0;
        while (time < panelHorizontalMoveTime)
        {
            yield return null;
            time += Time.deltaTime;

            float t = MyMath.EaseInOut(time / panelHorizontalMoveTime);
            mainPanel.anchoredPosition = new Vector2(Mathf.Lerp(panelStartPos, panelDestPos, t), mainPanel.anchoredPosition.y);
            particle.anchoredPosition = new Vector2(Mathf.Lerp(particleStartPos, particleDestPos, t), particle.anchoredPosition.y);
            bgImage.color = Color.Lerp(bgStartColor, bgDestColor, t);
        }

        mainPanel.anchoredPosition = new Vector3(panelDestPos, mainPanel.anchoredPosition.y);
        particle.anchoredPosition = new Vector3(particleDestPos, particle.anchoredPosition.y);
        bgImage.color = bgDestColor;
        currentPanelIndex = index;

        movePanelHorizontalCoroutine = null;
    }

    private void MovePanelVertical(int index)
    {
        if (movePanelVerticalCoroutine != null) return;
        if (currentDungeonIndex == index) return;

        movePanelVerticalCoroutine = StartCoroutine(MovePanelVerticalCoroutine(index));
    }

    private IEnumerator MovePanelVerticalCoroutine(int index)
    {
        float panelStartPos = dungeonPanel.anchoredPosition.y;
        float panelDestPos = 1600f * index;

        Color bgStartColor = bgColors[currentDungeonIndex];
        Color bgDestColor = bgColors[index];

        float time = 0;
        while (time < panelVerticalMoveTime)
        {
            yield return null;
            time += Time.deltaTime;

            float t = MyMath.EaseInOut(time / panelVerticalMoveTime);
            dungeonPanel.anchoredPosition = new Vector2(dungeonPanel.anchoredPosition.x, Mathf.Lerp(panelStartPos, panelDestPos, t));
            bgImage.color = Color.Lerp(bgStartColor, bgDestColor, t);
        }

        dungeonPanel.anchoredPosition = new Vector3(dungeonPanel.anchoredPosition.x, panelDestPos);
        bgImage.color = bgDestColor;
        currentDungeonIndex = index;

        movePanelVerticalCoroutine = null;
    }

    public void MovePanelPrev()
    {
        if (currentDungeonIndex <= 0) return;

        MovePanelVertical(currentDungeonIndex - 1);
    }

    public void MovePanelNext()
    {
        if (currentDungeonIndex >= dungeonCount - 1) return;

        MovePanelVertical(currentDungeonIndex + 1);
    }
}
