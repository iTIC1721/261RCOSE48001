using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] Button nextButton;

    private WordState currentWord = null;

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
                correct = true,
                responseTime = 1f,
            };

            MANAGER.StudyManager.SubmitAnswer(result);
        }

        WordState nextWord = MANAGER.StudyManager.GetNextWord();
        currentWord = nextWord;

        if (currentWord != null)
        {
            wordText.text = currentWord.word;
            meaningText.text = currentWord.meaning;
        }
        else
        {
            // TODO: 끝내기
            Log.LogMessage("오늘의 학습이 종료되었습니다.");
        }
    }
}
