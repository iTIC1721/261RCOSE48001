using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] Button nextButton; 
    [SerializeField] Button[] choices = new Button[4];

    private WordState currentWord = null;

    private bool corrected = false;
    private float questionStartTime = 0;
    private float resTime = 0;

    private void Start()
    {
        ShowNextWord();
    }

    public void ShowNextWord()
    {
        if (currentWord != null)
        {
            // TODO: 정답 여부 및 응답시간 측정하여 넣기
            ReviewResult result = new ReviewResult()
            {
                word = currentWord,
                correct = corrected,
                responseTime = resTime,
            };

            MANAGER.StudyManager.SubmitAnswer(result);
        }

        WordState nextWord = MANAGER.StudyManager.GetNextWord();
        currentWord = nextWord;

        if (currentWord != null)
        {
            wordText.text = currentWord.word;
            meaningText.text = currentWord.meaning;

            // 선택지
            string[] wrongMeanings = MANAGER.StudyManager.GetRandomMeanings(3, currentWord.meaning);
            int answerIndex = UnityEngine.Random.Range(0, 4);
            string[] meanings = new string[4];
            for (int i = 0, j = 0; i < 4; i++)
            {
                if (i == answerIndex) meanings[i] = currentWord.meaning;
                else meanings[i] = wrongMeanings[j++];
            }
            SetChoices(meanings, answerIndex);

            questionStartTime = Time.time;
        }
        else
        {
            // TODO: 끝내기
            Log.LogMessage("오늘의 학습이 종료되었습니다.");

            // 정답률
            StageProgress stageProgress = MANAGER.StudyManager.GetStageProgress(MANAGER.StudyManager.currentStageDifficulty);
            int correctCount = 0;
            foreach (var item in stageProgress.results)
            {
                if (item.correct) correctCount++;
            }
            float correctRate = (float)correctCount / stageProgress.results.Count;
            resultPanel.correctRateText.text = $"Correct Rate: {(correctRate * 100f).ToString("F0")}%";

            // 복습 개수
            int reviewCount = MANAGER.StudyManager.currentDaySession.reviewWords.Count;
            resultPanel.reviewCountText.text = $"Review Count: {reviewCount}";

            // 총 진행도
            int totalCount = MANAGER.StudyManager.words.Count;
            int studiedCount = MANAGER.StudyManager.words.Where(w => w.isLearned).Count() + MANAGER.StudyManager.currentDaySession.totalWords.Count;
            resultPanel.totalProgressText.text = $"Total Progress: {studiedCount}/{totalCount}";

            resultPanel.resultPanel.SetActive(true);
        }
    }

    public void SetChoices(string[] meanings, int answerIndex)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].GetComponentInChildren<TextMeshProUGUI>().text = meanings[i];
            choices[i].onClick.RemoveAllListeners();
            if (i == answerIndex)
            {
                choices[i].onClick.AddListener(() => {
                    // TODO: 정답버튼 만들기
                    Log.LogMessage("정답!");
                    corrected = true;
                    resTime = Time.time - questionStartTime;
                    ShowNextWord();
                });
            }
            else
            {
                choices[i].onClick.AddListener(() => {
                    // TODO: 오답버튼 만들기
                    Log.LogMessage($"오답 - 정답은 \"{meanings[answerIndex]}\"");
                    corrected = false;
                    resTime = Time.time - questionStartTime;
                    ShowNextWord();
                });
            }            
        }
    }

    public void Back()
    {
        SceneManager.LoadScene("StudyDungeon_StageSelect");
    }
}
