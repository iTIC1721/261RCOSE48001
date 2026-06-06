using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// QuizManager의 ApiManager 기반 재구현.
/// StudyManager 의존성을 제거하고 오늘의 단어를 서버에서 로드합니다.
/// - 오답 보기: 로컬 생성 (_wordQueue 내 다른 단어의 뜻에서 추출)
/// - 퀴즈 결과: 서버 전송 없이 로컬 처리
/// - quizCompleted: PlayerPrefs 날짜 키 기반으로 관리
/// 씬: StudyDungeon_Quiz
/// </summary>
public class ApiQuizManager : MonoBehaviour
{
    // ── UI ──
    [Header("UI")]
    [SerializeField] ShakeUI shakeLayer;
    [SerializeField] DamageEffect damageEffect;
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] GameObject diePanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] TextMeshProUGUI leftHpText;
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Image timeBarFill;
    [SerializeField] Button nextButton;
    [SerializeField] Button[] choices = new Button[4];

    // ── 엔티티 ──
    [Header("Entity")]
    [SerializeField] Player player;
    [SerializeField] Transform monsterPos;
    [SerializeField] Transform damageLayer;

    // ── 난이도 설정 ──
    [Header("DB")]
    [SerializeField] QuizSetting easySetting;
    [SerializeField] QuizSetting hardSetting;

    // ── 기본 설정 ──
    [Header("Setting")]
    [SerializeField] float baseDamage = 1000;

    // ── 로딩 UI ──
    [Header("로딩")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    // ── 학습 설정 ──
    [Header("학습 설정")]
    [SerializeField][Range(10, 300)] private int dailyLimit = 100;

    // ── 난이도 설정 딕셔너리 ──
    private Dictionary<StageDifficulty, QuizSetting> quizSettingDict = null;

    // ── 게임 상태 ──
    private float timeLimit = 5f;
    private int hp = 5;
    private int combo = 0;

    private float totalDamage = 0;
    private int progressCount = 0;
    private int correctCount = 0;
    private int todayCount = 0;

    private Monster monster;

    // ── 단어 큐 (서버에서 로드, 기존 RandomQueue<Card> 역할) ──
    private List<DailyScheduleWord> _wordQueue = new List<DailyScheduleWord>();
    private int _queueIndex = 0;

    // ── 현재 문제 상태 ──
    private DailyScheduleWord _currentWord = null;
    private int currentAnswer = -1;

    // ── 퀴즈 기록 (기존 quizLogs 역할) ──
    private List<(DailyScheduleWord word, bool isCorrect)> quizLogs = new();

    // ── 타이머 ──
    private float questionStartTime = 0;
    private float questionResponseTime = 0;
    private bool isSolvingQuiz = false;
    private bool isDied = false;

    // ── 난이도 (PlayerPrefs에서 씬 진입 시 로드) ──
    private StageDifficulty _currentDifficulty;
    private const string DifficultyKey = "selectedDifficulty";

    // ══════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════

    private void Awake()
    {
        quizSettingDict = new()
        {
            { StageDifficulty.Easy, easySetting },
            { StageDifficulty.Hard, hardSetting },
        };
    }

    private void Start()
    {
        // 난이도 PlayerPrefs에서 로드
        _currentDifficulty = (StageDifficulty)PlayerPrefs.GetInt(DifficultyKey, 0);
        var setting = quizSettingDict[_currentDifficulty];
        timeLimit = setting.timeLimit;
        SetHp(setting.maxHp);
        SetCombo(0);

        // 플레이어 생성 (기존과 동일)
        PlayerSaveData data = SaveSystem.LoadPlayerData() ?? new PlayerSaveData();
        int characterId = data.characterId;
        player.SetCharacter();
        player.enableMove = false;
        player.enableAttack = false;
        player.invulnerable = true;

        // 몬스터 생성 (기존과 동일)
        int monsterId = setting.monsterId;
        var monsterObj = Instantiate(MANAGER.DB.monsterDB.GetMonsterData(monsterId).monster, monsterPos);
        monsterObj.transform.localPosition = new Vector3(0, 0, monsterObj.transform.localPosition.z);
        monsterObj.transform.localScale = new Vector3(1, 1, 1);
        monster = monsterObj.GetComponent<Monster>();

        ShowLoading("오늘의 단어 불러오는 중...");
        StartCoroutine(LoadScheduleAndStart());
    }

    // ══════════════════════════════════════════
    // 스케줄 로드 (기존 StartQuiz의 MainScheduler 역할을 서버로 대체)
    // ══════════════════════════════════════════

    // ── PlayerPrefs 키 (TestQuizManager와 동일한 형식) ──
    private string TodayWordsKey
        => $"studied_words_{ApiManager.Instance.UserId}_{System.DateTime.Today:yyyy-MM-dd}";

    private List<DailyScheduleWord> LoadTodayWords()
    {
        string json = PlayerPrefs.GetString(TodayWordsKey, "");
        if (string.IsNullOrEmpty(json)) return new List<DailyScheduleWord>();
        try
        {
            var wrapper = JsonUtility.FromJson<DailyScheduleWordListWrapper>(json);
            return new List<DailyScheduleWord>(wrapper.words ?? new DailyScheduleWord[0]);
        }
        catch
        {
            return new List<DailyScheduleWord>();
        }
    }

    private IEnumerator LoadScheduleAndStart()
    {
        yield return null; // 프레임 대기

        // 오늘 학습한 단어를 PlayerPrefs에서 불러옴
        var todayWords = LoadTodayWords();
        Debug.Log($"[ApiQuizManager] 오늘 학습 단어 로드: {todayWords.Count}개");

        _wordQueue.Clear();
        foreach (var w in todayWords)
            if (!string.IsNullOrEmpty(w.meaning)) _wordQueue.Add(w);

        // 학습 이력이 없으면 서버에서 현재 스케줄로 fallback
        if (_wordQueue.Count == 0)
        {
            Debug.Log("[ApiQuizManager] 오늘 학습 이력 없음 → 서버 스케줄로 fallback");
            yield return ApiManager.Instance.GetTodaySchedule(
                dailyLimit: dailyLimit,
                onSuccess: schedule => {
                    foreach (var w in schedule.new_words)
                        if (!string.IsNullOrEmpty(w.meaning)) _wordQueue.Add(w);
                    foreach (var w in schedule.review_words)
                        if (!string.IsNullOrEmpty(w.meaning)) _wordQueue.Add(w);
                    foreach (var w in schedule.db_supplement)
                        if (!string.IsNullOrEmpty(w.meaning)) _wordQueue.Add(w);
                    Debug.Log($"[ApiQuizManager] fallback 스케줄 단어: {_wordQueue.Count}개");
                },
                onError: err => {
                    Debug.LogError($"[ApiQuizManager] 스케줄 로드 실패: {err}");
                }
            );
        }

        // 랜덤 셔플
        Shuffle(_wordQueue);

        todayCount = _wordQueue.Count;
        _queueIndex = 0;

        Debug.Log($"[ApiQuizManager] 퀴즈 단어 수: {todayCount}");

        HideLoading();

        if (todayCount == 0)
        {
            DisplayResult();
            yield break;
        }

        StartCoroutine(CountDownCoroutine());
    }

    // ══════════════════════════════════════════
    // 카운트다운 (기존과 동일)
    // ══════════════════════════════════════════

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

    // ══════════════════════════════════════════
    // 단어 표시 (기존 ShowNextWord와 동일한 구조)
    // ══════════════════════════════════════════

    public void ShowNextWord()
    {
        if (isDied) return;

        _currentWord = null;
        if (_queueIndex < _wordQueue.Count)
            _currentWord = _wordQueue[_queueIndex++];

        if (_currentWord != null)
        {
            progressCount++;
            wordText.text = _currentWord.word;
            meaningText.text = _currentWord.meaning;
            progressText.text = $"진행도: {progressCount} / {todayCount}";

            // 오답 보기 로컬 생성 (기존 StudyManager.GetRandomMeanings() 역할)
            string[] wrongMeanings = GetLocalDistractors(_currentWord, 3);
            currentAnswer = UnityEngine.Random.Range(0, 4);
            string[] meanings = new string[4];
            for (int i = 0, j = 0; i < 4; i++)
            {
                if (i == currentAnswer) meanings[i] = _currentWord.meaning;
                else meanings[i] = j < wrongMeanings.Length ? wrongMeanings[j++] : "(알 수 없음)";
            }

            SetChoices(meanings, currentAnswer);

            questionStartTime = Time.time;
            isSolvingQuiz = true;
        }
        else
        {
            // 끝내기 (기존과 동일)
            CompleteStage();
        }
    }

    // ── 오답 보기 로컬 생성 (기존 StudyManager.GetRandomMeanings() 대체) ──
    private string[] GetLocalDistractors(DailyScheduleWord current, int count)
    {
        var pool = _wordQueue
            .Where(w => w.word != current.word && !string.IsNullOrEmpty(w.meaning))
            .Select(w => w.meaning)
            .Distinct()
            .ToList();

        // 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(count).ToArray();
    }

    // ══════════════════════════════════════════
    // 선택지 설정 (기존과 동일)
    // ══════════════════════════════════════════

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

    // ══════════════════════════════════════════
    // 답변 선택 (기존과 동일한 구조)
    // ══════════════════════════════════════════

    private void SelectAnswer(int selectIndex, int answerIndex)
    {
        questionResponseTime = Time.time - questionStartTime;
        isSolvingQuiz = false;

        float stayTime;
        var setting = quizSettingDict[_currentDifficulty];

        if (selectIndex == answerIndex)
        {
            Log.LogMessage("정답!");
            correctCount++;
            SetCombo(combo + 1);
            stayTime = setting.correctStayTime;

            float damage = baseDamage + GetAdditionalDamage(questionResponseTime) * baseDamage * 0.8f;
            player.Attack();
            StartCoroutine(EntityAttackCoroutine(() => EnemyHurt(damage)));
        }
        else
        {
            Log.LogMessage("오답");
            SetCombo(0);
            stayTime = setting.incorrectStayTime;

            monster.Attack();
            StartCoroutine(EntityAttackCoroutine(() => PlayerHurt()));
        }

        // 퀴즈 기록 (기존과 동일)
        quizLogs.Add((_currentWord, selectIndex == answerIndex));

        // 정답 버튼만 활성화 유지 (기존과 동일)
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].onClick.RemoveAllListeners();
            if (i != answerIndex) choices[i].interactable = false;
        }

        StartCoroutine(ShowNextWordCoroutine(stayTime));
    }

    private IEnumerator ShowNextWordCoroutine(float stayTime)
    {
        yield return new WaitForSeconds(stayTime);
        ShowNextWord();
    }

    private IEnumerator EntityAttackCoroutine(Action action)
    {
        yield return new WaitForSeconds(0.25f);
        action?.Invoke();
    }

    // ══════════════════════════════════════════
    // 전투 (기존과 동일)
    // ══════════════════════════════════════════

    private void EnemyHurt(float damage)
    {
        Log.LogMessage("적 데미지 입음");
        shakeLayer.Shake(0.5f + combo * 0.02f, true);

        List<DamageInfo> damageList = new List<DamageInfo>();
        damageList.Add(new DamageInfo(damage, player.Transform));
        for (int i = 1; i <= combo / 5; i++)
            damageList.Add(new DamageInfo(damage * (float)i * 0.125f, player.Transform));

        monster.GetDamaged(damageList.ToArray());
        for (int i = 0; i < damageList.Count; i++)
            totalDamage += damageList[i].damage;
    }

    private void PlayerHurt()
    {
        SetHp(hp - 1);
        shakeLayer.Shake(2, true);
        damageEffect.OnDamage();

        if (hp <= 0)
            PlayerDie();
        else
            player.GetDamaged(new DamageInfo(1, monster.Transform));
    }

    private void PlayerDie()
    {
        Log.LogMessage("플레이어 사망");
        isDied = true;
        player.Die();
        diePanel.SetActive(true);
    }

    // ══════════════════════════════════════════
    // 세션 종료
    // ══════════════════════════════════════════

    private void CompleteStage()
    {
        Log.LogMessage("학습이 종료되었습니다.");
        SaveQuizCompleted();
        GetReward();
        DisplayResult();
    }

    // ── quizCompleted: PlayerPrefs 날짜 키 기반 ──

    private string QuizCompletedKey(int diffIndex)
        => $"quizCompleted_{diffIndex}_{ApiManager.Instance.UserId}_{DateTime.Today:yyyy-MM-dd}";

    private bool IsQuizCompleted(int diffIndex)
        => PlayerPrefs.GetInt(QuizCompletedKey(diffIndex), 0) == 1;

    private void SaveQuizCompleted()
    {
        int diffIndex = (int)_currentDifficulty;

        // 과거 날짜 키 정리 (최근 30일)
        for (int i = 1; i <= 30; i++)
        {
            for (int d = 0; d < quizSettingDict.Count; d++)
            {
                string oldKey = $"quizCompleted_{d}_{ApiManager.Instance.UserId}_{DateTime.Today.AddDays(-i):yyyy-MM-dd}";
                if (PlayerPrefs.HasKey(oldKey))
                    PlayerPrefs.DeleteKey(oldKey);
            }
        }

        PlayerPrefs.SetInt(QuizCompletedKey(diffIndex), 1);
        PlayerPrefs.Save();
        Debug.Log($"[ApiQuizManager] quizCompleted 저장 — 난이도: {diffIndex}, 날짜: {DateTime.Today:yyyy-MM-dd}");
    }

    // ══════════════════════════════════════════
    // 결과 화면
    // ══════════════════════════════════════════

    private void DisplayResult()
    {
        if (todayCount > 0)
        {
            float correctRate = (float)correctCount / todayCount;
            resultPanel.descTexts[0].text = $"정답률: {(correctRate * 100f):F0}%";
            resultPanel.descTexts[1].text = $"총 데미지: {Mathf.FloorToInt(totalDamage)}";
            resultPanel.descTexts[2].text = $"총 문제 수: {todayCount}개";
        }
        else
        {
            resultPanel.descTexts[0].text = "오늘 학습할 단어가 없습니다.";
            resultPanel.descTexts[1].text = "";
            resultPanel.descTexts[2].text = "";
        }

        resultPanel.resultPanel.SetActive(true);
    }

    // ── GetReward: quizCompleted를 PlayerPrefs 기반으로 참조 ──
    private void GetReward()
    {
        int reward = 0;
        int diffIndex = (int)_currentDifficulty;
        for (int i = diffIndex; i >= 0; i--)
        {
            if (IsQuizCompleted(i)) continue;
            reward += RewardSystem.CalculateQuizReward((StageDifficulty)i);
        }
        MANAGER.Inventory.AddMoney(reward);
    }

    // ══════════════════════════════════════════
    // 타이머 (기존 Update와 동일)
    // ══════════════════════════════════════════

    private void Update()
    {
        if (!isSolvingQuiz) return;

        float elapsed = Time.time - questionStartTime;
        timeBarFill.fillAmount = Mathf.Clamp01((timeLimit - elapsed) / timeLimit);

        if (timeLimit - elapsed <= 0)
            SelectAnswer(-1, currentAnswer);
    }

    // ══════════════════════════════════════════
    // 씬 이동 (기존과 동일)
    // ══════════════════════════════════════════

    public void Back()
    {
        StartCoroutine(MoveToSceneCoroutine("StudyDungeon_ApiStageSelect"));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);
        yield return new WaitForSecondsRealtime(0.2f);
        LoadingSceneManager.LoadScene(sceneName);
    }

    // ══════════════════════════════════════════
    // UI 헬퍼
    // ══════════════════════════════════════════

    private void SetCombo(int value)
    {
        combo = value;
        comboText.text = $"콤보: {combo}";
    }

    private void SetHp(int value)
    {
        hp = value;
        leftHpText.text = $"남은 체력: {hp}";
    }

    private void ShowLoading(string msg)
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (loadingText) loadingText.text = msg;
        Debug.Log($"[ApiQuizManager] {msg}");
    }

    private void HideLoading()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    // ══════════════════════════════════════════
    // 유틸
    // ══════════════════════════════════════════

    private float GetAdditionalDamage(float t)
    {
        if (t <= 1f) return 1f;
        float x = Mathf.Clamp01((t - 1f) / 4f);
        return Mathf.Exp(-6f * x * x);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}