using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // ── UI: 결과 ──
    [Header("결과 패널")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    // ── 온보딩 상태 ──
    private OnboardingQuiz _onboardingQuiz;
    private List<QuizAnswer> _onboardingAnswers = new List<QuizAnswer>();
    private int _onboardingIndex = 0;

    // ── 설정 ──
    [Header("학습 설정")]
    [SerializeField][Range(10, 300)] private int dailyLimit = 100;

    // ── 오늘의 단어 퀴즈 상태 ──
    private List<DailyScheduleWord> _wordQueue = new List<DailyScheduleWord>();
    private List<SessionAnswer> _answers = new List<SessionAnswer>();
    private int _currentIndex = 0;
    private int _pendingRating = 0;

    private static readonly string[] RatingLabels = { "", "Again", "Hard", "Good", "Easy" };

    // ══════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════
    private void Start()
    {
        // 온보딩 버튼
        knowButton.onClick.AddListener(() => OnOnboardingAnswer(true));
        dontKnowButton.onClick.AddListener(() => OnOnboardingAnswer(false));

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
    // [1단계] 헬스 체크
    // ══════════════════════════════════════════
    private IEnumerator AppStartFlow()
    {
        ShowLoading("서버 연결 확인 중...");

        yield return ApiManager.Instance.CheckHealth(
            onSuccess: health => {
                if (!health.ai3_ready)
                {
                    ShowLoading("⚠️ AI3 임베딩 DB 미준비.\n서버에서 ai3_refine_ratings.py를 먼저 실행하세요.");
                    return;
                }

                if (!health.user_ready)
                    StartCoroutine(FirstInstallFlow());
                else
                    StartCoroutine(LoadAndStartQuiz());
            },
            onError: err => ShowLoading($"서버 연결 실패: {err}\n127.0.0.1:8000이 실행 중인지 확인하세요.")
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
    // [3단계] 온보딩 퀴즈 (O/X 직접 풀기)
    // ══════════════════════════════════════════
    private IEnumerator OnboardingFlow()
    {
        ShowLoading("온보딩 퀴즈 준비 중...");

        yield return ApiManager.Instance.GetOnboardingQuiz(
            onSuccess: q => _onboardingQuiz = q,
            onError: err => ShowLoading($"퀴즈 로드 실패: {err}")
        );

        if (_onboardingQuiz == null || _onboardingQuiz.questions.Length == 0)
        {
            ShowLoading("온보딩 퀴즈 로드 실패.");
            yield break;
        }

        _onboardingIndex = 0;
        _onboardingAnswers.Clear();
        SetPanels(onboarding: true);
        ShowOnboardingWord();
    }

    private void ShowOnboardingWord()
    {
        if (_onboardingIndex >= _onboardingQuiz.questions.Length)
        {
            StartCoroutine(SubmitOnboarding());
            return;
        }

        var q = _onboardingQuiz.questions[_onboardingIndex];
        onboardingWordText.text = q.word;
        onboardingProgressText.text = $"{_onboardingIndex + 1} / {_onboardingQuiz.questions.Length}";
    }

    private void OnOnboardingAnswer(bool correct)
    {
        var q = _onboardingQuiz.questions[_onboardingIndex];
        _onboardingAnswers.Add(new QuizAnswer
        {
            order = q.order,
            word = q.word,
            correct = correct,
            response_time_ms = 0
        });
        _onboardingIndex++;
        ShowOnboardingWord();
    }

    private IEnumerator SubmitOnboarding()
    {
        ShowLoading("온보딩 결과 분석 중...");

        yield return ApiManager.Instance.SubmitOnboardingAnswers(
            answers: _onboardingAnswers.ToArray(),
            onSuccess: profile => {
                Debug.Log($"온보딩 완료! 초기 userRating: {profile.user_rating}");
                StartCoroutine(LoadAndStartQuiz());
            },
            onError: err => ShowLoading($"온보딩 제출 실패: {err}")
        );
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
                    ShowLoading("오늘 학습할 단어가 없습니다.");
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
        _answers.Add(new SessionAnswer
        {
            word = _wordQueue[_currentIndex].word,
            rating_given = _pendingRating
        });

        _currentIndex++;
        SetPanels(quiz: true);
        ShowCurrentWord();
    }

    // ══════════════════════════════════════════
    // [6단계] 세션 결과 제출
    // ══════════════════════════════════════════
    private IEnumerator FinishSession()
    {
        ShowLoading("결과 저장 중...");

        yield return ApiManager.Instance.SubmitSessionResult(
            answers: _answers.ToArray(),
            onSuccess: result => {
                int[] count = new int[5];
                foreach (var a in _answers) count[a.rating_given]++;

                string resultMsg =
                    $"학습 완료!  총 {_answers.Count}개\n\n" +
                    $"새 Rating : {result.user_rating}\n" +
                    $"K-factor  : {result.k_factor}\n\n" +
                    $"Again (1) : {count[1]}개\n" +
                    $"Hard  (2) : {count[2]}개\n" +
                    $"Good  (3) : {count[3]}개\n" +
                    $"Easy  (4) : {count[4]}개";

                Debug.Log(resultMsg);
                SetPanels(result: true);
                resultText.text = resultMsg;
            },
            onError: err => ShowLoading($"결과 저장 실패: {err}")
        );
    }

    // ══════════════════════════════════════════
    // UI 헬퍼
    // ══════════════════════════════════════════
    private void SetPanels(bool loading = false, bool onboarding = false,
                           bool quiz = false, bool meaning = false, bool result = false)
    {
        if (loadingPanel) loadingPanel.SetActive(loading);
        if (onboardingPanel) onboardingPanel.SetActive(onboarding);
        if (quizPanel) quizPanel.SetActive(quiz);
        if (meaningPanel) meaningPanel.SetActive(meaning);
        if (resultPanel) resultPanel.SetActive(result);
    }

    private void ShowLoading(string msg)
    {
        SetPanels(loading: true);
        if (loadingText) loadingText.text = msg;
        Debug.Log($"[Memorix] {msg}");
    }
}