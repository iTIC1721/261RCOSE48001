using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LearnManager : MonoBehaviour
{
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI[] nextDueTexts;

    int newCount = 0;
    int reviewCount = 0;
    int studiedCount = 0;

    Card currentCard;

    private void Start()
    {
        newCount = MANAGER.StudyManager.session.newCount;
        reviewCount = MANAGER.StudyManager.session.reviewCount;
        studiedCount = MANAGER.StudyManager.session.studiedCount;

        RefreshProgressText();

        ShowNextWord();
    }

    public void ShowNextWord()
    {
        currentCard = MANAGER.StudyManager.GetNextWord();

        if (currentCard != null)
        {
            meaningText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            wordText.text = currentCard.front;
            meaningText.text = currentCard.back;

            DateTime[] dues = FSRSScheduler.PreviewNextDues(currentCard, MANAGER.StudyManager.deck);
            for (int rating = 1; rating <= 4; rating++)
            {
                if (rating - 1 < nextDueTexts.Length && nextDueTexts[rating - 1] != null) 
                    nextDueTexts[rating - 1].text = FormatDue(dues[rating - 1]);
            }
        }
        else
        {
            // 끝내기
            CompleteStage();
        }
    }

    string FormatDue(DateTime due)
    {
        TimeSpan diff = due - CustomTime.GetTimeNow();

        if (diff.TotalMinutes < 60)
            return $"< {(int)diff.TotalMinutes}분";
        if (diff.TotalHours < 24)
            return $"< {(int)diff.TotalHours}시간";
        return $"< {(int)diff.TotalDays}일";
    }

    public void RateCard(int rating)
    {
        // MainScheduler.RateCard 호출 전에 카운터 차감
        if (currentCard.state == CardState.New)
            newCount--;
        else
            reviewCount--;

        // MainScheduler.RateCard 호출 (due 갱신)
        bool requeued = MANAGER.StudyManager.SubmitAnswer(rating);

        // requeue되었는지 확인
        if (requeued)
            reviewCount++;
        else
            studiedCount++;

        RefreshProgressText();

        meaningText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }

    private void RefreshProgressText()
    {
        progressText.text = $"<color=#0000FF>{newCount}</color>  <color=#FF0000>{reviewCount}</color>  <color=#00FF00>{studiedCount}</color>";
    }

    public void NextCard()
    {
        ShowNextWord();
    }

    private void CompleteStage()
    {
        Log.LogMessage("학습이 종료되었습니다.");

        GetReward();

        MANAGER.StudyManager.deck.lastLearnDate = CustomTime.GetTimeNow();
        SaveSystem.SaveDeck(MANAGER.StudyManager.deck);

        DisplayResult();
    }

    private void DisplayResult()
    {
        // 총 진행도
        int totalCount = MANAGER.StudyManager.deck.cards.Count;
        int newCount = MANAGER.StudyManager.deck.cards.Where(w => w.state == CardState.New).Count();
        int studiedCount = totalCount - newCount;
        resultPanel.descTexts[0].text = $"학습 진행도: {studiedCount}/{totalCount}";

        resultPanel.resultPanel.SetActive(true);
    }

    private void GetReward()
    {
        int reward = RewardSystem.CalculateLearnReward();

        MANAGER.Inventory.AddMoney(reward);
    }

    public void Back()
    {
        SaveSystem.SaveDeck(MANAGER.StudyManager.deck);
        SceneManager.LoadScene("StudyDungeon_StageSelect");
    }
}
