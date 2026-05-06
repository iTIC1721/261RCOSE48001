using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    [SerializeField] GameObject stageSelectPanel;
    [SerializeField] RectTransform buttonPanel;

    [Space]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] string title;

    [Space] 
    [SerializeField] Button learnStageButton;
    [SerializeField] Button[] stageSelectButtons;   // 0: Easy, 1: Hard
    [SerializeField] GameObject[] rewardDecoration;
    [SerializeField] float panelMoveTime = 0.5f;

    Coroutine panelMoveCoroutine = null;

    public void ShowStageSelectPanel()
    {
        if (panelMoveCoroutine != null) return;

        int day = MANAGER.StudyManager.deck.GetCurrentDay();
        titleText.text = $"{day + 1}{title}";

        if (MANAGER.StudyManager.deck.lastLearnDate.Date != CustomTime.GetTimeNow().Date)   // ¿À´Ã °øºÎ ¾È³¡³ÂÀ¸¸é
        {
            rewardDecoration[0].SetActive(true);
            learnStageButton.interactable = true;

            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                rewardDecoration[d + 1].SetActive(true);
                stageSelectButtons[d].interactable = false;
            }
        }
        else
        {
            rewardDecoration[0].SetActive(false);
            learnStageButton.interactable = false;

            bool isCleared = false;
            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                if (isCleared || MANAGER.StudyManager.deck.quizCompleted[d])
                {
                    isCleared = true;
                    rewardDecoration[d + 1].SetActive(false);
                    stageSelectButtons[d].interactable = false;
                }
                else
                {
                    rewardDecoration[d + 1].SetActive(true);
                    stageSelectButtons[d].interactable = true;
                }
            }
        }

        stageSelectPanel.SetActive(true);
        panelMoveCoroutine = StartCoroutine(ShowPanelCoroutine());
    }

    private IEnumerator ShowPanelCoroutine()
    {
        CanvasGroup group = stageSelectPanel.GetComponent<CanvasGroup>();
        float startAlpha = 0;
        float endAlpha = 1;

        Vector2 startPos = new Vector2(buttonPanel.rect.width, 0);
        Vector2 destPos = Vector2.zero;

        group.alpha = startAlpha;
        buttonPanel.anchoredPosition = startPos;

        float time = 0;
        while (time < panelMoveTime)
        {
            yield return null;
            time += Time.deltaTime;

            group.alpha = Mathf.Lerp(startAlpha, endAlpha, MyMath.EaseIn(time / panelMoveTime));
            buttonPanel.anchoredPosition = Vector2.Lerp(startPos, destPos, MyMath.EaseInOut(time / panelMoveTime));
        }

        group.alpha = endAlpha;
        buttonPanel.anchoredPosition = destPos;

        panelMoveCoroutine = null;
    }

    public void HideStageSelectPanel()
    {
        if (panelMoveCoroutine != null) return;

        panelMoveCoroutine = StartCoroutine(HidePanelCoroutine());
    }

    private IEnumerator HidePanelCoroutine()
    {
        CanvasGroup group = stageSelectPanel.GetComponent<CanvasGroup>();
        float startAlpha = 1;
        float endAlpha = 0;

        Vector2 startPos = Vector2.zero;
        Vector2 destPos = new Vector2(buttonPanel.rect.width, 0);

        group.alpha = startAlpha;
        buttonPanel.anchoredPosition = startPos;

        float time = 0;
        while (time < panelMoveTime)
        {
            yield return null;
            time += Time.deltaTime;

            group.alpha = Mathf.Lerp(startAlpha, endAlpha, MyMath.EaseOut(time / panelMoveTime));
            buttonPanel.anchoredPosition = Vector2.Lerp(startPos, destPos, MyMath.EaseInOut(time / panelMoveTime));
        }

        group.alpha = endAlpha;
        buttonPanel.anchoredPosition = destPos;
        stageSelectPanel.SetActive(false);

        panelMoveCoroutine = null;
    }

    public void SetDifficulty(int diff)
    {
        MANAGER.StudyManager.currentStageDifficulty = (StageDifficulty)diff;
    }
}
