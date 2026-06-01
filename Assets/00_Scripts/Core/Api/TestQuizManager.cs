using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>DailyScheduleWord 배열을 JsonUtility로 직렬화하기 위한 래퍼.</summary>
[System.Serializable]
public class DailyScheduleWordListWrapper
{
    public DailyScheduleWord[] words;
}

/// <summary>
/// 전체 앱 흐름 + 직접 퀴즈 풀기를 통합한 테스트용 매니저.
/// 헬스체크 → 초기화 → 온보딩(O/X) → 스케줄 로드 → 단어 퀴즈(1~4) → 결과 제출
/// </summary>
public class TestQuizManager : MonoBehaviour
{
    // ── UI: 로딩 ──
    [Header("로딩 패널")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    // ── UI: 온보딩 퀴즈 (O/X) ──
    [Header("온보딩 패널 (O/X)")]
    [SerializeField] private GameObject onboardingPanel;
    [SerializeField] private TextMeshProUGUI onboardingWordText;
    [SerializeField] private TextMeshProUGUI onboardingProgressText;
    [SerializeField] private Button knowButton;    // O (안다)
    [SerializeField] private Button dontKnowButton; // X (모른다)

    // ── UI: 오늘의 단어 퀴즈 (1~4 레이팅) ──
    [Header("퀴즈 패널 (1~4 레이팅)")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI ratingLabelText;
    [SerializeField] private Button againButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button goodButton;
    [SerializeField] private Button easyButton;

    // ── UI: 뜻 확인 ──
    [Header("뜻 확인 패널 (레이팅 클릭 후 표시)")]
    [SerializeField] private GameObject meaningPanel;
    [SerializeField] private TextMeshProUGUI meaningWordText;
    [SerializeField] private TextMeshProUGUI meaningPosText;
    [SerializeField] private TextMeshProUGUI meaningText;
    [SerializeField] private TextMeshProUGUI selectedRatingText;
    [SerializeField] private Button nextButton;

    // ── UI: 4지선다 퀴즈 ──
    [Header("4지선다 퀴즈 패널")]
    [SerializeField] private GameObject mcPanel;
    [SerializeField] private TextMeshProUGUI mcWordText;        // 영단어
    [SerializeField] private TextMeshProUGUI mcProgressText;    // 진행도
    [SerializeField] private Button[] mcChoiceButtons;          // 버튼 4개 (Inspector에서 연결)
    [SerializeField] private TextMeshProUGUI[] mcChoiceTexts;   // 각 버튼의 TextMeshPro (4개)

    // ── UI: 4지선다 결과 확인 ──
    [Header("4지선다 결과 확인 패널")]
    [SerializeField] private GameObject mcResultPanel;
    [SerializeField] private TextMeshProUGUI mcResultWordText;   // 단어
    [SerializeField] private TextMeshProUGUI mcResultCorrectText; // 정답 표시
    [SerializeField] private TextMeshProUGUI mcResultFeedbackText; // O/X 피드백
    [SerializeField] private Button mcNextButton;                // 다음 문제

    // ── UI: 결과 ──
    [Header("결과 패널")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    // ── CAT 온보딩 상태 ──
    private CatQuestion _catCurrentQuestion;
    private int _catQuestionNum = 0;
    private int _catMaxQuestions = 0;
    private System.DateTime _catQuestionStartTime;

    // ── 설정 ──
    [Header("학습 설정")]
    [SerializeField][Range(10, 300)] private int dailyLimit = 100;

    // ── 오늘의 단어 퀴즈 상태 ──
    private List<DailyScheduleWord> _wordQueue = new List<DailyScheduleWord>();
    private List<SessionAnswer> _answers = new List<SessionAnswer>();
    private int _currentIndex = 0;
    private int _pendingRating = 0;

    private static readonly string[] RatingLabels = { "", "Again", "Hard", "Good", "Easy" };

    // ── 4지선다 퀴즈 상태 ──
    private List<DailyScheduleWord> _mcQueue = new List<DailyScheduleWord>();
    private int _mcIndex = 0;
    private int _mcCorrect = 0;
    private int _mcWrong = 0;
    private System.Random _rng = new System.Random();

    // ══════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════
    private void Start()
    {
        // 온보딩 버튼
        knowButton.onClick.AddListener(() => OnOnboardingAnswer(true));
        dontKnowButton.onClick.AddListener(() => OnOnboardingAnswer(false));

        // 4지선다 버튼
        for (int i = 0; i < mcChoiceButtons.Length; i++)
        {
            int idx = i;
            mcChoiceButtons[idx].onClick.AddListener(() => OnMcChoiceClicked(idx));
        }
        if (mcNextButton) mcNextButton.onClick.AddListener(OnMcNextClicked);

        // 단어 퀴즈 버튼
        againButton.onClick.AddListener(() => OnRatingClicked(1));
        hardButton.onClick.AddListener(() => OnRatingClicked(2));
        goodButton.onClick.AddListener(() => OnRatingClicked(3));
        easyButton.onClick.AddListener(() => OnRatingClicked(4));
        nextButton.onClick.AddListener(OnNextClicked);

        SetPanels(loading: true);
        StartCoroutine(AppStartFlow());
    }

    // ══════════════════════════════════════════
    // [1단계] 헬스 체크 → user_id 확인
    // ══════════════════════════════════════════
    private IEnumerator AppStartFlow()
    {
        ShowLoading("서버 연결 확인 중...");

        yield return ApiManager.Instance.CheckHealth(
            onSuccess: health => {
                if (health.status != "ok")
                {
                    ShowLoading("⚠️ 서버 상태 이상. 잠시 후 다시 시도하세요.");
                    return;
                }
                StartCoroutine(EnsureUserId());
            },
            onError: err => ShowLoading($"서버 연결 실패: {err}\nhttps://memorix-api-production.up.railway.app 에 접근 가능한지 확인하세요.")
        );
    }

    // ══════════════════════════════════════════
    // [1-1단계] user_id 확인 및 발급
    // ══════════════════════════════════════════
    private IEnumerator EnsureUserId()
    {
        if (string.IsNullOrEmpty(ApiManager.Instance.UserId))
        {
            ShowLoading("유저 등록 중...");
            yield return ApiManager.Instance.CreateUser(
                onSuccess: profile => {
                    Debug.Log($"[Memorix] user_id 발급: {profile.user_id}");
                    StartCoroutine(CheckUserReady());
                },
                onError: err => ShowLoading($"유저 등록 실패: {err}")
            );
        }
        else
        {
            Debug.Log($"[Memorix] 기존 user_id 사용: {ApiManager.Instance.UserId}");
            StartCoroutine(CheckUserReady());
        }
    }

    // ══════════════════════════════════════════
    // [1-2단계] 유저 초기화 여부 확인
    // ══════════════════════════════════════════
    private IEnumerator CheckUserReady()
    {
        yield return ApiManager.Instance.GetUserProfile(
            onSuccess: profile => {
                if (!profile.onboarding_completed)
                    StartCoroutine(FirstInstallFlow());
                else
                    StartCoroutine(LoadAndStartQuiz());
            },
            onError: _ => {
                // 프로필이 없으면 최초 설치
                StartCoroutine(FirstInstallFlow());
            }
        );
    }

    // ══════════════════════════════════════════
    // [2단계] 최초 설치 흐름
    // ══════════════════════════════════════════
    private IEnumerator FirstInstallFlow()
    {
        string csvPath = Application.persistentDataPath + "/my_words.csv";

        if (System.IO.File.Exists(csvPath))
        {
            ShowLoading("단어 목록 업로드 중...");
            yield return ApiManager.Instance.UploadCsv(
                filePath: csvPath,
                onSuccess: result => {
                    Debug.Log($"업로드 완료: 전체 {result.total_words}개, Oxford 매칭 {result.oxford_matched}개");
                    StartCoroutine(OnboardingFlow());
                },
                onError: err => ShowLoading($"CSV 업로드 실패: {err}")
            );
        }
        else
        {
            Debug.Log($"[Memorix] CSV 없음 ({csvPath}) — Oxford 기본 단어로 초기화합니다.");
            ShowLoading("기본 단어 DB 초기화 중...");
            yield return ApiManager.Instance.InitDefault(
                onSuccess: result => {
                    Debug.Log($"기본 단어 DB 초기화 완료 (status: {result.status})");
                    StartCoroutine(OnboardingFlow());
                },
                onError: err => ShowLoading($"기본 초기화 실패: {err}")
            );
        }
    }

    // ══════════════════════════════════════════
    // [3단계] CAT 온보딩 (적응형, 평균 15문항)
    // ══════════════════════════════════════════
    private IEnumerator OnboardingFlow()
    {
        ShowLoading("온보딩 퀴즈 준비 중...");

        yield return ApiManager.Instance.CatStart(
            onSuccess: q => {
                _catCurrentQuestion = q;
                _catMaxQuestions = q.max_questions;
                _catQuestionNum = q.question_num;
            },
            onError: err => ShowLoading($"온보딩 시작 실패: {err}")
        );

        if (_catCurrentQuestion == null)
        {
            ShowLoading("온보딩 시작 실패.");
            yield break;
        }

        SetPanels(onboarding: true);
        ShowCatQuestion();
    }

    private void ShowCatQuestion()
    {
        onboardingWordText.text = _catCurrentQuestion.word;
        onboardingProgressText.text = $"{_catCurrentQuestion.question_num} / {_catMaxQuestions}";
        _catQuestionStartTime = System.DateTime.Now;
    }

    private void OnOnboardingAnswer(bool correct)
    {
        // 버튼 비활성화 (중복 클릭 방지)
        knowButton.interactable = false;
        dontKnowButton.interactable = false;

        int responseMs = (int)(System.DateTime.Now - _catQuestionStartTime).TotalMilliseconds;
        StartCoroutine(SubmitCatAnswer(correct, responseMs));
    }

    // CAT 완료 여부를 멤버 변수로 관리 (코루틴-콜백 간 상태 공유)
    private bool _catDone = false;

    private IEnumerator SubmitCatAnswer(bool correct, int responseMs)
    {
        ShowLoading("분석 중...");

        _catDone = false;
        string answeredWord = _catCurrentQuestion.word;
        CatQuestion nextQuestion = null;
        CatResult finalResult = null;

        yield return ApiManager.Instance.CatAnswer(
            word: answeredWord,
            correct: correct,
            responseTimeMs: responseMs,
            onQuestion: next => {
                nextQuestion = next;
            },
            onDone: result => {
                finalResult = result;
                _catDone = true;
            },
            onError: err => ShowLoading($"답변 제출 실패: {err}")
        );

        if (_catDone && finalResult != null)
        {
            Debug.Log($"CAT 온보딩 완료! 문항 수: {finalResult.question_num}, " +
                      $"초기 userRating: {finalResult.user_profile.user_rating}");
            StartCoroutine(LoadAndStartQuiz());
        }
        else if (nextQuestion != null)
        {
            _catCurrentQuestion = nextQuestion;
            _catQuestionNum = nextQuestion.question_num;
            knowButton.interactable = true;
            dontKnowButton.interactable = true;
            SetPanels(onboarding: true);
            ShowCatQuestion();
        }
        else
        {
            // 에러 케이스 — 버튼 복구
            knowButton.interactable = true;
            dontKnowButton.interactable = true;
        }
    }

    // ══════════════════════════════════════════
    // [4단계] 스케줄 로드 → 퀴즈 시작
    // ══════════════════════════════════════════
    private IEnumerator LoadAndStartQuiz()
    {
        ShowLoading("오늘의 단어 불러오는 중...");

        yield return ApiManager.Instance.GetTodaySchedule(
            dailyLimit: dailyLimit,
            onSuccess: schedule => {
                _wordQueue.Clear();
                _wordQueue.AddRange(schedule.new_words);
                _wordQueue.AddRange(schedule.review_words);
                _wordQueue.AddRange(schedule.db_supplement);

                Debug.Log($"스케줄 로드 완료 — 신규: {schedule.stats.new_count}, " +
                          $"복습: {schedule.stats.review_count}, " +
                          $"보충: {schedule.stats.supplement_count}");

                if (_wordQueue.Count == 0)
                {
                    Debug.Log("[Memorix] 오늘 학습할 단어 없음 → 4지선다 퀴즈로 이동");
                    StartCoroutine(StartMcQuizOnly());
                    return;
                }

                _currentIndex = 0;
                _answers.Clear();
                SetPanels(quiz: true);
                ShowCurrentWord();
            },
            onError: err => ShowLoading($"스케줄 로드 실패: {err}")
        );
    }

    // ══════════════════════════════════════════
    // [5단계] 오늘의 단어 퀴즈 (1~4 레이팅)
    // ══════════════════════════════════════════
    private void ShowCurrentWord()
    {
        if (_currentIndex >= _wordQueue.Count)
        {
            StartCoroutine(FinishSession());
            return;
        }

        var current = _wordQueue[_currentIndex];
        wordText.text = current.word;
        ratingLabelText.text = $"난이도: {current.rating}";
        progressText.text = $"{_currentIndex + 1} / {_wordQueue.Count}";
    }

    private void OnRatingClicked(int rating)
    {
        _pendingRating = rating;
        var current = _wordQueue[_currentIndex];

        meaningWordText.text = current.word;
        meaningPosText.text = string.IsNullOrEmpty(current.pos) ? "" : current.pos;
        meaningText.text = string.IsNullOrEmpty(current.meaning) ? "(뜻 정보 없음)" : current.meaning;
        selectedRatingText.text = $"선택한 레이팅: {RatingLabels[rating]} ({rating})";

        SetPanels(meaning: true);
    }

    private void OnNextClicked()
    {
        var current = _wordQueue[_currentIndex];

        _answers.Add(new SessionAnswer
        {
            word = current.word,
            rating_given = _pendingRating
        });

        // Again(1)이고 due_date가 오늘이면 큐 맨 뒤로 재삽입
        if (_pendingRating == 1 && IsToday(current.fsrs_due))
        {
            _wordQueue.Add(current);
            Debug.Log($"[Memorix] '{current.word}' 오늘 복습 → 큐 맨 뒤로 재삽입 (현재 큐: {_wordQueue.Count - _currentIndex - 1}개 남음)");
        }

        _currentIndex++;
        SetPanels(quiz: true);
        ShowCurrentWord();
    }

    /// <summary>날짜 문자열이 오늘 날짜인지 확인합니다.</summary>
    private bool IsToday(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return false;
        if (System.DateTime.TryParse(dateStr, out System.DateTime due))
            return due.Date == System.DateTime.Today;
        return false;
    }

    // ══════════════════════════════════════════
    // [6단계] 세션 결과 제출
    // ══════════════════════════════════════════
    private IEnumerator FinishSession()
    {
        ShowLoading("결과 저장 중...");

        bool submitted = false;
        string finalResultMsg = "";

        yield return ApiManager.Instance.SubmitSessionResult(
            answers: _answers.ToArray(),
            onSuccess: result => {
                int[] count = new int[5];
                foreach (var a in _answers) count[a.rating_given]++;

                finalResultMsg =
                    $"학습 완료!  총 {_answers.Count}개\n\n" +
                    $"새 Rating : {result.user_rating}\n" +
                    $"K-factor  : {result.k_factor}\n\n" +
                    $"Again (1) : {count[1]}개\n" +
                    $"Hard  (2) : {count[2]}개\n" +
                    $"Good  (3) : {count[3]}개\n" +
                    $"Easy  (4) : {count[4]}개";

                Debug.Log(finalResultMsg);
                submitted = true;
            },
            onError: err => ShowLoading($"결과 저장 실패: {err}")
        );

        if (!submitted) yield break;

        // 오늘 학습한 단어 PlayerPrefs에 저장 (기존 저장분과 합산)
        var alreadySaved = LoadTodayWords();
        var savedWords = new Dictionary<string, DailyScheduleWord>();
        foreach (var w in alreadySaved) savedWords[w.word] = w;
        foreach (var w in _wordQueue) savedWords[w.word] = w; // 최신 정보로 덮어쓰기
        SaveTodayWords(new List<DailyScheduleWord>(savedWords.Values));

        // 4지선다: 오늘 저장된 전체 단어 중 뜻 있는 단어만 (중복 제거)
        var seen = new HashSet<string>();
        _mcQueue.Clear();
        foreach (var w in savedWords.Values)
        {
            if (!string.IsNullOrEmpty(w.meaning) && seen.Add(w.word))
                _mcQueue.Add(w);
        }

        if (_mcQueue.Count < 2)
        {
            // 4지선다를 낼 단어가 너무 적으면 바로 결과 화면
            SetPanels(result: true);
            resultText.text = finalResultMsg;
            yield break;
        }

        // 4지선다 시작
        _sessionResultMsg = finalResultMsg;
        _mcIndex = 0;
        _mcCorrect = 0;
        _mcWrong = 0;
        SetPanels(mc: true);
        ShowMcQuestion();
    }

    // ══════════════════════════════════════════
    // 단어 없을 때 4지선다만 시작
    // ══════════════════════════════════════════
    private IEnumerator StartMcQuizOnly()
    {
        ShowLoading("오늘 학습 이력 확인 중...");
        yield return null;

        // PlayerPrefs에서 오늘 학습한 단어 불러오기
        var todayWords = LoadTodayWords();
        Debug.Log($"[Memorix] 오늘 학습 이력: {todayWords.Count}개");

        // 뜻 있는 단어만 4지선다 큐에 추가
        var seen = new HashSet<string>();
        _mcQueue.Clear();
        foreach (var w in todayWords)
        {
            if (!string.IsNullOrEmpty(w.meaning) && seen.Add(w.word))
                _mcQueue.Add(w);
        }

        _mcIndex = 0;
        _mcCorrect = 0;
        _mcWrong = 0;
        _sessionResultMsg = "오늘의 신규 학습 없음\n\n── 오늘 학습 단어 복습 퀴즈 ──";

        if (_mcQueue.Count < 2)
        {
            SetPanels(result: true);
            resultText.text = "오늘 학습한 단어가 없거나\n뜻 정보가 부족하여 퀴즈를 낼 수 없습니다.";
            yield break;
        }

        SetPanels(mc: true);
        ShowMcQuestion();
    }

    // ══════════════════════════════════════════
    // [7단계] 4지선다 퀴즈
    // ══════════════════════════════════════════
    private string _sessionResultMsg = "";

    // ── PlayerPrefs 키 ──
    private string TodayWordsKey => $"studied_words_{ApiManager.Instance.UserId}_{System.DateTime.Today:yyyy-MM-dd}";

    // ──────────────────────────────────────────
    // 오늘 학습 단어 저장 / 불러오기
    // ──────────────────────────────────────────

    /// <summary>오늘 학습한 단어 목록을 PlayerPrefs에 저장하고, 이전 날짜 키를 정리합니다.</summary>
    private void SaveTodayWords(List<DailyScheduleWord> words)
    {
        // 오늘 이전 날짜 키 정리 (최근 30일치만 검사)
        for (int i = 1; i <= 30; i++)
        {
            string oldKey = $"studied_words_{ApiManager.Instance.UserId}_{System.DateTime.Today.AddDays(-i):yyyy-MM-dd}";
            if (PlayerPrefs.HasKey(oldKey))
            {
                PlayerPrefs.DeleteKey(oldKey);
                Debug.Log($"[Memorix] 이전 날짜 키 삭제: {oldKey}");
            }
        }

        // 오늘 단어 저장
        var wrapper = new DailyScheduleWordListWrapper { words = words.ToArray() };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(TodayWordsKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[Memorix] 오늘 학습 단어 {words.Count}개 저장 완료 (key: {TodayWordsKey})");
    }

    /// <summary>오늘 학습한 단어 목록을 PlayerPrefs에서 불러옵니다.</summary>
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

    private void ShowMcQuestion()
    {
        if (_mcIndex >= _mcQueue.Count)
        {
            // 4지선다 완료 → 최종 결과
            SetPanels(result: true);
            resultText.text =
                _sessionResultMsg +
                $"\n\n── 4지선다 결과 ──\n" +
                $"정답: {_mcCorrect}개 / 오답: {_mcWrong}개";
            return;
        }

        var current = _mcQueue[_mcIndex];
        mcWordText.text = current.word;
        mcProgressText.text = $"{_mcIndex + 1} / {_mcQueue.Count}";

        // 오답 후보: 현재 단어 제외한 나머지에서 최대 3개 랜덤 추출
        // TODO: 서버에 /api/words/distractors?word=xxx 엔드포인트가 생기면 여기서 교체
        var distractors = GetLocalDistractors(current, 3);

        // 정답 위치를 랜덤으로 결정
        int correctPos = _rng.Next(0, 4);
        int dIdx = 0;
        for (int i = 0; i < 4; i++)
        {
            string label;
            if (i == correctPos)
                label = current.meaning;
            else
            {
                label = dIdx < distractors.Count ? distractors[dIdx] : "(알 수 없음)";
                dIdx++;
            }
            if (mcChoiceTexts != null && i < mcChoiceTexts.Length)
                mcChoiceTexts[i].text = label;

            // 버튼 색상 초기화
            var colors = mcChoiceButtons[i].colors;
            colors.normalColor = Color.white;
            mcChoiceButtons[i].colors = colors;
            mcChoiceButtons[i].interactable = true;
        }

        // 정답 위치를 버튼 태그에 저장
        mcPanel.GetComponent<McQuizState>()?.SetCorrectIndex(correctPos);
    }

    private List<string> GetLocalDistractors(DailyScheduleWord current, int count)
    {
        var pool = new List<string>();
        foreach (var w in _mcQueue)
        {
            if (w.word != current.word && !string.IsNullOrEmpty(w.meaning))
                pool.Add(w.meaning);
        }

        // 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Count >= count ? pool.GetRange(0, count) : pool;
    }

    private void OnMcChoiceClicked(int choiceIdx)
    {
        var state = mcPanel.GetComponent<McQuizState>();
        if (state == null) return;

        int correctIdx = state.CorrectIndex;
        bool isCorrect = choiceIdx == correctIdx;

        if (isCorrect) _mcCorrect++;
        else _mcWrong++;

        // 버튼 비활성화
        foreach (var btn in mcChoiceButtons) btn.interactable = false;

        // 결과 패널 표시
        var current = _mcQueue[_mcIndex];
        mcResultWordText.text = current.word;
        mcResultCorrectText.text = $"정답: {current.meaning}";
        mcResultFeedbackText.text = isCorrect ? "정답!" : "오답";
        SetPanels(mcResult: true);
    }

    private void OnMcNextClicked()
    {
        _mcIndex++;
        SetPanels(mc: true);
        ShowMcQuestion();
    }

    // ══════════════════════════════════════════
    // UI 헬퍼
    // ══════════════════════════════════════════
    private void SetPanels(bool loading = false, bool onboarding = false,
                           bool quiz = false, bool meaning = false,
                           bool mc = false, bool mcResult = false, bool result = false)
    {
        if (loadingPanel) loadingPanel.SetActive(loading);
        if (onboardingPanel) onboardingPanel.SetActive(onboarding);
        if (quizPanel) quizPanel.SetActive(quiz);
        if (meaningPanel) meaningPanel.SetActive(meaning);
        if (mcPanel) mcPanel.SetActive(mc);
        if (mcResultPanel) mcResultPanel.SetActive(mcResult);
        if (resultPanel) resultPanel.SetActive(result);
    }

    private void ShowLoading(string msg)
    {
        SetPanels(loading: true);
        if (loadingText) loadingText.text = msg;
        Debug.Log($"[Memorix] {msg}");
    }
}