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
    [SerializeField] RectTransform particle;
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

        float particleStartPos = particle.anchoredPosition.x;
        float particleDestPos = -1000f * index;

        Color bgStartColor = bgColors[currentPanelIndex];
        Color bgDestColor = bgColors[index];

        float time = 0;
        while (time < panelMoveTime)
        {
            yield return null;
            time += Time.deltaTime;

            float t = MyMath.EaseInOut(time / panelMoveTime);
            mainPanel.anchoredPosition = new Vector2(Mathf.Lerp(panelStartPos, panelDestPos, t), mainPanel.anchoredPosition.y);
            particle.anchoredPosition = new Vector2(Mathf.Lerp(particleStartPos, particleDestPos, t), particle.anchoredPosition.y);
            bgImage.color = Color.Lerp(bgStartColor, bgDestColor, t);
        }

        mainPanel.anchoredPosition = new Vector3(panelDestPos, mainPanel.anchoredPosition.y);
        particle.anchoredPosition = new Vector3(particleDestPos, particle.anchoredPosition.y);
        bgImage.color = bgDestColor;
        currentPanelIndex = index;

        movePanelCoroutine = null;
    }
}
