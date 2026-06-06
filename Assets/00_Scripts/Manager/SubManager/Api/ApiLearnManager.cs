using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// LearnManager의 ApiManager 기반 재구현.
/// StudyManager 의존성을 제거하고 ApiManager를 통해 서버와 통신합니다.
/// 씬: StudyDungeon_Learn
/// </summary>
public class ApiLearnManager : MonoBehaviour
{
    // ── UI 참조 (기존 LearnManager와 동일) ──
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Button answerButton;
    [SerializeField] TextMeshProUGUI[] nextDueTexts; // 1~4 레이팅별 다음 복습 예정 텍스트

    // ── 로딩 UI (ApiManager 통신 대기 표시용) ──
    [Header("로딩")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    // ── 학습 설정 ──
    [Header("학습 설정")]
    [SerializeField][Range(10, 300)] private int dailyLimit = 100;

    // ── 세션 카운터 (기존 LearnManager와 동일한 역할) ──
    int newCount = 0;
    int reviewCount = 0;
    int studiedCount = 0;

    // ── 세션 상태 ──
    private List<DailyScheduleWord> _wordQueue = new List<DailyScheduleWord>();
    private List<SessionAnswer> _answers = new List<SessionAnswer>();
    private int _currentIndex = 0;
    private int _pendingRating = 0;

    // 현재 표시 중인 단어 (기존 LearnManager의 currentCard 역할)
    private DailyScheduleWord _currentWord;

    private static readonly string[] RatingLabels = { "", "Again", "Hard", "Good", "Easy" };

    // ══════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════

    private void Start()
    {
        ShowLoading("오늘의 단어 불러오는 중...");
        StartCoroutine(LoadScheduleAndStart());
    }

    // ══════════════════════════════════════════
    // 스케줄 로드 (기존 StudyManager.session 역할)
    // ══════════════════════════════════════════

    private IEnumerator LoadScheduleAndStart()
    {
        yield return ApiManager.Instance.GetTodaySchedule(
            dailyLimit: dailyLimit,
            onSuccess: schedule => {
                // 큐 구성: 신규 → 복습 → 보충 순
                _wordQueue.Clear();
                _wordQueue.AddRange(schedule.new_words);
                _wordQueue.AddRange(schedule.review_words);
                _wordQueue.AddRange(schedule.db_supplement);

                // 카운터 초기화 (기존 Start()의 session 값 읽기와 동일한 역할)
                newCount = schedule.stats.new_count;
                reviewCount = schedule.stats.review_count + schedule.stats.supplement_count;
                studiedCount = 0;

                Debug.Log($"[ApiLearnManager] 스케줄 로드 완료 — " +
                          $"신규: {schedule.stats.new_count}, " +
                          $"복습: {schedule.stats.review_count}, " +
                          $"보충: {schedule.stats.supplement_count}");

                if (_wordQueue.Count == 0)
                {
                    Debug.Log("[ApiLearnManager] 오늘 학습할 단어 없음 → 결과 화면 표시");
                    HideLoading();
                    DisplayResult(null, reason: "오늘 학습할 단어가 없습니다.");
                    return;
                }

                _currentIndex = 0;
                _answers.Clear();

                HideLoading();
                RefreshProgressText();
                ShowNextWord();
            },
            onError: err => {
                Debug.LogError($"[ApiLearnManager] 스케줄 로드 실패: {err}");
                HideLoading();
                DisplayResult(null, reason: $"단어 로드 실패\n{err}");
            }
        );
    }

    // ══════════════════════════════════════════
    // 단어 표시 (기존 ShowNextWord와 동일한 구조)
    // ══════════════════════════════════════════

    public void ShowNextWord()
    {
        if (_currentIndex >= _wordQueue.Count)
        {
            // 끝내기 (기존 CompleteStage 호출과 동일)
            StartCoroutine(CompleteStage());
            return;
        }

        _currentWord = _wordQueue[_currentIndex];

        meaningText.gameObject.SetActive(false);
        answerButton.gameObject.SetActive(true);

        wordText.text = _currentWord.word;
        meaningText.text = string.IsNullOrEmpty(_currentWord.meaning)
            ? "(뜻 정보 없음)"
            : _currentWord.meaning;

        // 기존: FSRSScheduler.PreviewNextDues() → 서버 기반이므로 레이팅 레이블로 대체
        // TODO: 서버에 /api/schedule/preview-dues 엔드포인트가 생기면 여기서 교체
        UpdateNextDueTexts();
    }

    // 레이팅 레이블 표시 (기존 FormatDue 역할, 서버 응답 전까지 레이블만 표시)
    private void UpdateNextDueTexts()
    {
        if (nextDueTexts == null) return;

        // 서버에 실제 due 계산 API가 없으므로 레이팅 이름만 표시
        // 추후 서버 API 연동 시 이 부분을 교체하면 됩니다.
        string[] labels = { "다시", "어려움", "적당", "쉬움" };
        for (int i = 0; i < nextDueTexts.Length && i < labels.Length; i++)
        {
            if (nextDueTexts[i] != null)
                nextDueTexts[i].text = labels[i];
        }
    }

    // ══════════════════════════════════════════
    // 정답 보기 (기존 ShowAnswer와 동일)
    // ══════════════════════════════════════════

    public void ShowAnswer()
    {
        meaningText.gameObject.SetActive(true);
        answerButton.gameObject.SetActive(false);
    }

    // ══════════════════════════════════════════
    // 레이팅 (기존 RateCard와 동일한 구조)
    // ══════════════════════════════════════════

    public void RateCard(int rating)
    {
        if (_currentWord == null) return;

        // 카운터 차감 (기존: currentCard.state == CardState.New 분기와 동일한 역할)
        // DailyScheduleWord.type: "new" | "review" | "supplement"
        bool isNew = _currentWord.type == "new";
        if (isNew)
            newCount--;
        else
            reviewCount--;

        // 답변 기록
        _answers.Add(new SessionAnswer
        {
            word = _currentWord.word,
            rating_given = rating
        });

        // Again(1)이면 큐 맨 뒤에 재삽입 (기존 requeued 로직과 동일)
        bool requeued = rating == 1;
        if (requeued)
        {
            _wordQueue.Add(_currentWord);
            reviewCount++;
            Debug.Log($"[ApiLearnManager] '{_currentWord.word}' Again → 큐 재삽입 " +
                      $"(남은 단어: {_wordQueue.Count - _currentIndex - 1}개)");
        }
        else
        {
            studiedCount++;
        }

        RefreshProgressText();

        _currentIndex++;
        answerButton.gameObject.SetActive(true);
        ShowNextWord();
    }

    // ══════════════════════════════════════════
    // 진행도 텍스트 (기존 RefreshProgressText와 동일)
    // ══════════════════════════════════════════

    private void RefreshProgressText()
    {
        progressText.text =
            $"<color=#0000FF>{newCount}</color>  " +
            $"<color=#FF0000>{reviewCount}</color>  " +
            $"<color=#00FF00>{studiedCount}</color>";
    }

    // ══════════════════════════════════════════
    // 세션 종료 (기존 CompleteStage와 동일한 구조)
    // ══════════════════════════════════════════

    private IEnumerator CompleteStage()
    {
        ShowLoading("결과 저장 중...");
        Debug.Log("[ApiLearnManager] 학습이 종료되었습니다.");

        bool submitted = false;
        SessionResultResponse sessionResult = null;

        yield return ApiManager.Instance.SubmitSessionResult(
            answers: _answers.ToArray(),
            onSuccess: result => {
                sessionResult = result;
                submitted = true;
                Debug.Log($"[ApiLearnManager] 결과 제출 완료 — " +
                          $"새 Rating: {result.user_rating}, K-factor: {result.k_factor}");
            },
            onError: err => {
                Debug.LogError($"[ApiLearnManager] 결과 제출 실패: {err}");
            }
        );

        // 제출 성공 시에만 보상 지급 및 learnCompleted 저장, 실패해도 결과 화면은 표시
        if (submitted)
        {
            SaveTodayWords();
            SaveLearnCompleted();
            GetReward();
        }
        HideLoading();
        DisplayResult(sessionResult);
    }

    // ══════════════════════════════════════════
    // 결과 화면 (기존 DisplayResult와 동일한 구조)
    // ══════════════════════════════════════════

    private void DisplayResult(SessionResultResponse sessionResult, string reason = "")
    {
        int totalCount = _answers.Count;
        int[] count = new int[5];
        foreach (var a in _answers) count[a.rating_given]++;

        if (resultPanel != null && resultPanel.descTexts != null && resultPanel.descTexts.Length > 0)
        {
            // 단어가 없거나 오류인 경우 reason만 표시
            if (!string.IsNullOrEmpty(reason) && totalCount == 0)
            {
                resultPanel.descTexts[0].text = reason;
            }
            else
            {
                //string ratingLine = sessionResult != null
                //    ? $"새 Rating : {sessionResult.user_rating}\n\n"
                //    : "새 Rating : (저장 실패)\n\n";

                resultPanel.descTexts[0].text =
                    $"학습 완료! 총 {totalCount}개";
            }
        }

        if (resultPanel != null)
            resultPanel.resultPanel.SetActive(true);
    }

    // ══════════════════════════════════════════
    // 보상 (기존 GetReward와 동일)
    // ══════════════════════════════════════════

    // ── PlayerPrefs 키 (ApiQuizManager와 동일한 형식) ──
    private string TodayWordsKey
        => $"studied_words_{ApiManager.Instance.UserId}_{DateTime.Today:yyyy-MM-dd}";

    /// <summary>오늘 학습한 단어를 PlayerPrefs에 저장합니다. 기존 저장분과 합산됩니다.</summary>
    private void SaveTodayWords()
    {
        // 기존 저장분 불러오기
        var savedWords = new Dictionary<string, DailyScheduleWord>();
        string existingJson = PlayerPrefs.GetString(TodayWordsKey, "");
        if (!string.IsNullOrEmpty(existingJson))
        {
            try
            {
                var existing = JsonUtility.FromJson<DailyScheduleWordListWrapper>(existingJson);
                foreach (var w in existing.words ?? new DailyScheduleWord[0])
                    savedWords[w.word] = w;
            }
            catch { }
        }

        // 오늘 학습한 단어로 갱신 (최신 정보로 덮어쓰기)
        foreach (var w in _wordQueue)
            savedWords[w.word] = w;

        // 이전 날짜 키 정리 (최근 30일)
        for (int i = 1; i <= 30; i++)
        {
            string oldKey = $"studied_words_{ApiManager.Instance.UserId}_{DateTime.Today.AddDays(-i):yyyy-MM-dd}";
            if (PlayerPrefs.HasKey(oldKey))
                PlayerPrefs.DeleteKey(oldKey);
        }

        // 저장
        var wrapper = new DailyScheduleWordListWrapper
        {
            words = new List<DailyScheduleWord>(savedWords.Values).ToArray()
        };
        PlayerPrefs.SetString(TodayWordsKey, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
        Debug.Log($"[ApiLearnManager] 오늘 학습 단어 {savedWords.Count}개 저장 완료");
    }

    private void SaveLearnCompleted()
    {
        // 과거 날짜 키 정리 (최근 30일)
        for (int i = 1; i <= 30; i++)
        {
            string oldKey = $"learnCompleted_{ApiManager.Instance.UserId}_{DateTime.Today.AddDays(-i):yyyy-MM-dd}";
            if (PlayerPrefs.HasKey(oldKey))
                PlayerPrefs.DeleteKey(oldKey);
        }

        PlayerPrefs.SetInt($"learnCompleted_{ApiManager.Instance.UserId}_{DateTime.Today:yyyy-MM-dd}", 1);
        PlayerPrefs.Save();
        Debug.Log($"[ApiLearnManager] learnCompleted 저장 — 날짜: {DateTime.Today:yyyy-MM-dd}");
    }

    private void GetReward()
    {
        // 기존 보상 시스템 그대로 유지
        int reward = RewardSystem.CalculateLearnReward();
        MANAGER.Inventory.AddMoney(reward);
    }

    // ══════════════════════════════════════════
    // 씬 이동 (기존 Back과 동일)
    // ══════════════════════════════════════════

    public void Back()
    {
        // 기존: SaveSystem.SaveDeck() → 서버 기반이므로 저장 생략
        // 중간에 나갈 때 현재까지의 답변을 제출할지 여부는 기획에 따라 결정
        StartCoroutine(MoveToSceneCoroutine("StudyDungeon_ApiStageSelect"));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);
        yield return new WaitForSecondsRealtime(0.2f);
        LoadingSceneManager.LoadScene(sceneName);
    }

    // ══════════════════════════════════════════
    // 로딩 UI 헬퍼
    // ══════════════════════════════════════════

    private void ShowLoading(string msg)
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (loadingText) loadingText.text = msg;
        Debug.Log($"[ApiLearnManager] {msg}");
    }

    private void HideLoading()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
    }
}