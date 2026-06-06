using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StageSelectPanel의 ApiManager 기반 재구현.
/// - deck.lastLearnDate → PlayerPrefs learnCompleted 날짜 키로 대체
/// - deck.quizCompleted[] → PlayerPrefs quizCompleted 날짜 키로 대체
/// - StudyManager.currentStageDifficulty → PlayerPrefs selectedDifficulty로 대체
/// 씬: StudyDungeon_StageSelect
/// </summary>
public class ApiStageSelectPanel : MonoBehaviour
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

    private const string LearnCompletedKeyPrefix = "learnCompleted_";
    private const string QuizCompletedKeyPrefix = "quizCompleted_";
    private const string DifficultyKey = "selectedDifficulty";

    Coroutine panelMoveCoroutine = null;

    // ── PlayerPrefs 헬퍼 ──

    private bool IsLearnCompleted()
        => PlayerPrefs.GetInt($"{LearnCompletedKeyPrefix}{DateTime.Today:yyyy-MM-dd}", 0) == 1;

    private bool IsQuizCompleted(int diffIndex)
        => PlayerPrefs.GetInt($"{QuizCompletedKeyPrefix}{diffIndex}_{DateTime.Today:yyyy-MM-dd}", 0) == 1;

    // ══════════════════════════════════════════
    // 패널 표시 (기존 ShowStageSelectPanel과 동일한 구조)
    // ══════════════════════════════════════════

    public void ShowStageSelectPanel()
    {
        if (panelMoveCoroutine != null) return;

        // currentDay는 ApiStageView에서 이미 계산되어 있으나,
        // titleText용으로만 필요하므로 PlayerPrefs에서 읽거나 별도 전달 방식을 사용.
        // 여기서는 ApiStageView가 호출 전에 PlayerPrefs에 저장한 값을 사용.
        int currentDay = PlayerPrefs.GetInt("currentDay", 1);
        titleText.text = $"{currentDay}{title}";

        if (!IsLearnCompleted())
        {
            // 오늘 Learn 미완료 → learnButton 활성, 퀴즈 전체 비활성
            if (rewardDecoration[0] != null) rewardDecoration[0].SetActive(true);
            learnStageButton.interactable = true;

            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                if (rewardDecoration[d + 1] != null) rewardDecoration[d + 1].SetActive(true);
                stageSelectButtons[d].interactable = false;
            }
        }
        else
        {
            // 오늘 Learn 완료 → learnButton 비활성, 퀴즈 클리어 여부에 따라 활성/비활성
            if (rewardDecoration[0] != null) rewardDecoration[0].SetActive(false);
            learnStageButton.interactable = false;

            bool isCleared = false;
            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                if (isCleared || IsQuizCompleted(d))
                {
                    isCleared = true;
                    if (rewardDecoration[d + 1] != null) rewardDecoration[d + 1].SetActive(false);
                    stageSelectButtons[d].interactable = false;
                }
                else
                {
                    if (rewardDecoration[d + 1] != null) rewardDecoration[d + 1].SetActive(true);
                    stageSelectButtons[d].interactable = true;
                }
            }
        }

        stageSelectPanel.SetActive(true);
        panelMoveCoroutine = StartCoroutine(ShowPanelCoroutine());
    }

    // ══════════════════════════════════════════
    // 난이도 선택 (기존 SetDifficulty와 동일, PlayerPrefs로 저장)
    // ══════════════════════════════════════════

    public void SetDifficulty(int diff)
    {
        PlayerPrefs.SetInt(DifficultyKey, diff);
        PlayerPrefs.Save();
        Debug.Log($"[ApiStageSelectPanel] 난이도 저장 — {(StageDifficulty)diff}");
    }

    // ══════════════════════════════════════════
    // 패널 애니메이션 (기존과 동일)
    // ══════════════════════════════════════════

    private IEnumerator ShowPanelCoroutine()
    {
        CanvasGroup group = stageSelectPanel.GetComponent<CanvasGroup>();
        float startAlpha = 0, endAlpha = 1;
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
        float startAlpha = 1, endAlpha = 0;
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
}