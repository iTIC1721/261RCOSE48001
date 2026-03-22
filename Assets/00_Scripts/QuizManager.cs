using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] GameObject diePanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] TextMeshProUGUI leftHpText;
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] Image timeBarFill;
    [SerializeField] Button nextButton; 
    [SerializeField] Button[] choices = new Button[4];

    [Header("Entity")]
    [SerializeField] Transform damageLayer;
    [SerializeField] Transform enemy;

    [Header("DB")]
    [SerializeField] QuizSetting easySetting;
    [SerializeField] QuizSetting normalSetting;
    [SerializeField] QuizSetting hardSetting;

    [Header("Setting")]
    [SerializeField] float baseDamage = 1000;

    private Dictionary<StageDifficulty, QuizSetting> quizSettingDict = null;
    private float timeLimit = 5;
    private int hp = 5;
    private int combo = 0;

    private float totalDamage = 0;

    private WordState currentWord = null;
    private int currentAnswer = -1;

    private bool corrected = false;
    private float questionStartTime = 0;
    private float questionResponseTime = 0;

    private bool isSolvingQuiz = false;

    private void Awake()
    {
        quizSettingDict = new()
        {
            {StageDifficulty.Easy, easySetting},
            {StageDifficulty.Normal, normalSetting},
            {StageDifficulty.Hard, hardSetting},
        };
    }

    private void Start()
    {
        timeLimit = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].timeLimit;
        SetHp(quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].maxHp);
        SetCombo(0);

        StartQuiz();
    }

    private void Update()
    {
        if (isSolvingQuiz)
        {
            float time = Time.time - questionStartTime;
            timeBarFill.fillAmount = Mathf.Clamp01((timeLimit - time) / timeLimit);

            if (timeLimit - time <= 0)
            {
                // TODO: 오답 처리
                SelectAnswer(-1, currentAnswer);
            }
        }
    }
    
    public void StartQuiz()
    {
        StartCoroutine(CountDownCoroutine());
    }

    private IEnumerator CountDownCoroutine()
    {
        wordText.text = "준비";
        yield return new WaitForSeconds(1);

        for (int i = 3; i >= 1; i--)
        {
            wordText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        wordText.text = "시작!";
        yield return new WaitForSeconds(1);

        ShowNextWord();
    }

    public void ShowNextWord()
    {
        WordState nextWord = MANAGER.StudyManager.GetNextWord();
        currentWord = nextWord;

        if (currentWord != null)
        {
            wordText.text = currentWord.word;
            meaningText.text = currentWord.meaning;

            // 선택지
            string[] wrongMeanings = MANAGER.StudyManager.GetRandomMeanings(3, currentWord.meaning);
            currentAnswer = UnityEngine.Random.Range(0, 4);
            string[] meanings = new string[4];
            for (int i = 0, j = 0; i < 4; i++)
            {
                if (i == currentAnswer) meanings[i] = currentWord.meaning;
                else meanings[i] = wrongMeanings[j++];
            }
            SetChoices(meanings, currentAnswer);

            questionStartTime = Time.time;
            isSolvingQuiz = true;
        }
        else
        {
            // 끝내기
            CompleteStage();
        }
    }

    public void SetChoices(string[] meanings, int answerIndex)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].GetComponentInChildren<TextMeshProUGUI>().text = meanings[i];
            choices[i].interactable = true;

            int tmp = i;
            choices[i].onClick.RemoveAllListeners();
            choices[i].onClick.AddListener(() => SelectAnswer(tmp, answerIndex));
        }
    }

    private void SelectAnswer(int selectIndex, int answerIndex)
    {
        questionResponseTime = Time.time - questionStartTime;
        isSolvingQuiz = false;

        float stayTime = 0;
        if (selectIndex == answerIndex)
        {
            Log.LogMessage("정답!");
            corrected = true;
            SetCombo(combo + 1);
            stayTime = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].correctStayTime;

            // 적 데미지 입음
            float damage = baseDamage + GetAdditionalDamage(questionResponseTime) * baseDamage * 0.8f;
            EnemyHurt(damage);
        }
        else
        {
            Log.LogMessage($"오답");
            corrected = false;
            SetCombo(0);
            stayTime = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].incorrectStayTime;

            // 플레이어 데미지 입음
            PlayerHurt();
        }

        // 결과 기록
        ReviewResult result = new ReviewResult()
        {
            word = currentWord,
            correct = corrected,
            responseTime = questionResponseTime,
        };
        MANAGER.StudyManager.SubmitAnswer(result);

        // 잠시동안 정답 제외 버튼들을 비활성화하여 정답을 표시
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].onClick.RemoveAllListeners();

            if (i == answerIndex) continue;
            choices[i].interactable = false;
        }

        StartCoroutine(ShowNextWordCoroutine(stayTime));
    }

    private IEnumerator ShowNextWordCoroutine(float stayTime)
    {
        yield return new WaitForSeconds(stayTime);
        ShowNextWord();
    }

    private void EnemyHurt(float damage)
    {
        // TODO: 적 데미지 입음
        Log.LogMessage("적 데미지 입음");

        // 기본 데미지를 입히고, (콤보 / 5)번의 추가 데미지를 입힘
        GiveDamage(damage);
        for (int i = 1; i <= combo / 5; i++)
        {
            GiveDamage(damage * (float)i * 0.125f);
        }
    }

    private void GiveDamage(float damage)
    {
        totalDamage += damage;

        var damageTMP = MANAGER.Pool.PoolingObj("StudyDamageTMP").Get((value) => {
            value.GetComponent<DamageTMP>().Initialize(damageLayer, enemy, Vector3.zero, damage, Color.white);
        });
    }

    private void PlayerHurt()
    {
        // TODO: 플레이어 데미지 입음
        SetHp(hp - 1);

        if (hp <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        // TODO: 플레이어 사망 - 게임 오버
        Log.LogMessage("플레이어 사망");

        MANAGER.StudyManager.ClearStageProgress();
        diePanel.SetActive(true);
    }

    private void CompleteStage()
    {
        Log.LogMessage("학습이 종료되었습니다.");

        DisplayResult();
    }

    private void DisplayResult()
    {
        // 정답률
        StageProgress stageProgress = MANAGER.StudyManager.GetStageProgress(MANAGER.StudyManager.currentStageDifficulty);
        int correctCount = 0;
        foreach (var item in stageProgress.results)
        {
            if (item.correct) correctCount++;
        }
        float correctRate = (float)correctCount / stageProgress.results.Count;
        resultPanel.descTexts[0].text = $"정답률: {(correctRate * 100f).ToString("F0")}%";

        // 총 데미지
        resultPanel.descTexts[1].text = $"총 데미지: {Mathf.FloorToInt(totalDamage)}";

        // 총 진행도
        int totalCount = MANAGER.StudyManager.words.Count;
        int studiedCount = MANAGER.StudyManager.words.Where(w => w.isLearned).Count() + MANAGER.StudyManager.currentDaySession.totalWords.Count;
        resultPanel.descTexts[2].text = $"학습 진행도: {studiedCount}/{totalCount}";

        // TODO: 입힌 데미지나 최대 콤보도 표시해도 좋을듯?

        resultPanel.resultPanel.SetActive(true);
    }

    public void Back()
    {
        SceneManager.LoadScene("StudyDungeon_StageSelect");
    }

    private void SetCombo(int combo)
    {
        this.combo = combo;
        comboText.text = $"콤보: {combo}";
    }

    private void SetHp(int hp)
    {
        this.hp = hp;
        leftHpText.text = $"남은 체력: {hp}";
    }


    private float GetAdditionalDamage(float t)
    {
        if (t <= 1f)
            return 1f;

        float x = Mathf.Clamp01((t - 1f) / 4f);
        return Mathf.Exp(-6f * x * x);
    }
}
