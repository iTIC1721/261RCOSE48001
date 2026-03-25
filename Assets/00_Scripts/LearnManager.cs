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

    private void Start()
    {
        ShowNextWord();
    }

    public void ShowNextWord()
    {
        Card nextWord = MANAGER.StudyManager.GetNextWord();

        if (nextWord != null)
        {
            meaningText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            wordText.text = nextWord.front;
            meaningText.text = nextWord.back;
            //progressText.text = $"진행도: {MANAGER.StudyManager.GetStageProgress(MANAGER.StudyManager.currentStageDifficulty).currentIndex + 1} / {MANAGER.StudyManager.currentDaySession.totalWords.Count}";
        }
        else
        {
            // 끝내기
            CompleteStage();
        }
    }

    public void RateCard(int rating)
    {
        // 결과 기록
        MANAGER.StudyManager.SubmitAnswer(rating);

        meaningText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }

    public void NextCard()
    {
        ShowNextWord();
    }

    private void CompleteStage()
    {
        Log.LogMessage("학습이 종료되었습니다.");

        DisplayResult();
    }

    private void DisplayResult()
    {
        // 정답률
        //StageProgress stageProgress = MANAGER.StudyManager.GetStageProgress(MANAGER.StudyManager.currentStageDifficulty);
        //int correctCount = 0;
        //foreach (var item in stageProgress.results)
        //{
        //    if (item.correct) correctCount++;
        //}
        //float correctRate = (float)correctCount / stageProgress.results.Count;
        //resultPanel.descTexts[0].text = $"정답률: {(correctRate * 100f).ToString("F0")}%";

        // 총 데미지
        //resultPanel.descTexts[1].text = $"총 데미지: {Mathf.FloorToInt(totalDamage)}";

        // 총 진행도
        //int totalCount = MANAGER.StudyManager.deck.cards.Count;
        //int studiedCount = MANAGER.StudyManager.deck.cards.Where(w => w.state == CardState.Review).Count() + MANAGER.StudyManager.currentDaySession.newWords.Count;
        //resultPanel.descTexts[2].text = $"학습 진행도: {studiedCount}/{totalCount}";

        // TODO: 입힌 데미지나 최대 콤보도 표시해도 좋을듯?

        resultPanel.resultPanel.SetActive(true);
    }

    public void Back()
    {
        SaveSystem.SaveDeck(MANAGER.StudyManager.deck);
        SceneManager.LoadScene("StudyDungeon_StageSelect");
    }
}
