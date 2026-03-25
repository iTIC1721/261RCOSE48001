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
        }
        else
        {
            // 끝내기
            CompleteStage();
        }
    }

    public void RateCard(int rating)
    {
        if (rating == 1)
        {
            if (currentCard.state == CardState.New)
            {
                newCount--;
                reviewCount++;
            }
        }
        else
        {
            if (currentCard.state == CardState.New)
                newCount--;
            else
                reviewCount--;
            studiedCount++;
        }
        RefreshProgressText();

        // 결과 기록
        MANAGER.StudyManager.SubmitAnswer(rating);

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
