using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CAT 온보딩 씬 매니저.
/// 씬 시작 시 백그라운드에서 CatStart()로 첫 문항을 미리 로드.
/// 안내 패널 → 시작 버튼 클릭 → 퀴즈 시작 → 완료 시 결과 화면 → StudyDungeon_ApiStageSelect 이동
/// 씬: StudyDungeon_Onboarding
/// </summary>
public class OnboardingManager : MonoBehaviour
{
    // ── 안내 패널 ──
    [Header("안내 패널")]
    [SerializeField] GameObject guidePanel;
    [SerializeField] Button startButton;              // 다음 → 퀴즈 시작

    // ── 퀴즈 패널 ──
    [Header("퀴즈 패널")]
    [SerializeField] GameObject quizPanel;
    [SerializeField] Image progressBarFill;           // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] Button oButton;                  // 안다
    [SerializeField] Button xButton;                  // 모른다

    // ── 결과 패널 ──
    [Header("결과 패널")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI ratingText;
    [SerializeField] Button confirmButton;            // 확인 → 씬 이동

    // ── 로딩 패널 ──
    [Header("로딩")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    // ── 상태 ──
    private CatQuestion _currentQuestion;
    private float _questionStartTime;

    // 백그라운드 로딩 결과
    private bool _isConnecting = false;
    private CatQuestion _firstQuestion = null;
    private string _connectionError = null;

    // ══════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════

    private void Start()
    {
        guidePanel.SetActive(true);
        quizPanel.SetActive(true);
        resultPanel.SetActive(false);
        loadingPanel.SetActive(false);

        startButton.onClick.AddListener(OnStartButtonClicked);
        oButton.onClick.AddListener(() => OnAnswer(true));
        xButton.onClick.AddListener(() => OnAnswer(false));
        confirmButton.onClick.AddListener(OnConfirm);

        // 안내 패널 표시 중에 백그라운드에서 첫 문항 미리 로드
        StartCoroutine(PreloadFirstQuestion());
    }

    // ══════════════════════════════════════════
    // 백그라운드 로딩
    // ══════════════════════════════════════════

    private IEnumerator PreloadFirstQuestion()
    {
        _isConnecting = true;
        Debug.Log("[OnboardingManager] 첫 문항 백그라운드 로딩 시작");

        yield return ApiManager.Instance.CatStart(
            onSuccess: question => {
                _firstQuestion = question;
                Debug.Log($"[OnboardingManager] 첫 문항 로딩 완료: {question.word}");
            },
            onError: err => {
                _connectionError = err;
                Debug.LogError($"[OnboardingManager] 첫 문항 로딩 실패: {err}");
            }
        );

        _isConnecting = false;
    }

    // ══════════════════════════════════════════
    // 시작 버튼
    // ══════════════════════════════════════════

    private void OnStartButtonClicked()
    {
        guidePanel.SetActive(false);
        StartCoroutine(WaitAndStartQuiz());
    }

    private IEnumerator WaitAndStartQuiz()
    {
        if (_isConnecting)
        {
            // 아직 로딩 중 → 완료될 때까지 로딩 패널 표시
            ShowLoading("연결 중...");
            yield return new WaitUntil(() => !_isConnecting);
            HideLoading();
        }

        if (_connectionError != null)
        {
            ShowLoading($"연결 실패\n{_connectionError}");
            yield break;
        }

        // 로딩 완료 → 첫 문항 즉시 표시
        ShowQuestion(_firstQuestion);
    }

    // ══════════════════════════════════════════
    // 문항 표시
    // ══════════════════════════════════════════

    private void ShowQuestion(CatQuestion question)
    {
        _currentQuestion = question;

        oButton.interactable = true;
        xButton.interactable = true;

        wordText.text = question.word;

        if (progressBarFill != null)
            progressBarFill.fillAmount = (float)question.question_num / question.max_questions;

        _questionStartTime = Time.time;

        Debug.Log($"[OnboardingManager] 문항 {question.question_num}/{question.max_questions}: {question.word}");
    }

    // ══════════════════════════════════════════
    // 답변 제출
    // ══════════════════════════════════════════

    private void OnAnswer(bool correct)
    {
        if (_currentQuestion == null) return;

        oButton.interactable = false;
        xButton.interactable = false;

        int responseTimeMs = Mathf.RoundToInt((Time.time - _questionStartTime) * 1000);

        ShowLoading("처리 중...");
        StartCoroutine(SubmitAnswer(_currentQuestion.word, correct, responseTimeMs));
    }

    private IEnumerator SubmitAnswer(string word, bool correct, int responseTimeMs)
    {
        yield return ApiManager.Instance.CatAnswer(
            word: word,
            correct: correct,
            responseTimeMs: responseTimeMs,
            onQuestion: question => {
                HideLoading();
                ShowQuestion(question);
            },
            onDone: result => {
                HideLoading();
                ShowResult(result);
            },
            onError: err => {
                Debug.LogError($"[OnboardingManager] CatAnswer 실패: {err}");
                ShowLoading($"답변 제출 실패\n{err}");
                oButton.interactable = true;
                xButton.interactable = true;
            }
        );
    }

    // ══════════════════════════════════════════
    // 결과 화면
    // ══════════════════════════════════════════

    private void ShowResult(CatResult result)
    {
        quizPanel.SetActive(false);
        resultPanel.SetActive(true);

        titleText.text = "온보딩 완료!";
        ratingText.text = $"예상 실력: Lv. {result.user_profile?.user_rating ?? 0}";

        Debug.Log($"[OnboardingManager] 온보딩 완료 — Rating: {result.user_profile?.user_rating ?? 0}");
    }

    // ══════════════════════════════════════════
    // 확인 버튼 → 씬 이동
    // ══════════════════════════════════════════

    private void OnConfirm()
    {
        StartCoroutine(MoveToStageSelect());
    }

    private IEnumerator MoveToStageSelect()
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);
        yield return new WaitForSecondsRealtime(0.2f);
        LoadingSceneManager.LoadScene("StudyDungeon_ApiStageSelect");
    }

    // ══════════════════════════════════════════
    // 로딩 UI 헬퍼
    // ══════════════════════════════════════════

    private void ShowLoading(string msg)
    {
        if (loadingPanel) loadingPanel.SetActive(true);
        if (loadingText) loadingText.text = msg;
    }

    private void HideLoading()
    {
        if (loadingPanel) loadingPanel.SetActive(false);
    }
}