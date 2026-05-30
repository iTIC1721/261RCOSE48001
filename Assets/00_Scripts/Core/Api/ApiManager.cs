using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ──────────────────────────────────────────────
// 데이터 모델
// ──────────────────────────────────────────────

[Serializable]
public class HealthResponse
{
    public string status;
    public bool ai3_ready;
    public bool user_ready;
    public bool schedule_ready;
}

[Serializable]
public class UploadCsvResponse
{
    public string status;
    public int total_words;
    public int oxford_matched;
    public int predicted;
}

[Serializable]
public class OnboardingQuiz
{
    public int total_questions;
    public QuizQuestion[] questions;
}

[Serializable]
public class QuizQuestion
{
    public int order;
    public string word;
    public int rating;
    public string bucket;
}

[Serializable]
public class QuizAnswer
{
    public int order;
    public string word;
    public bool correct;
    public int response_time_ms;
}

[Serializable]
public class QuizAnswerPayload
{
    public QuizAnswer[] answers;
}

[Serializable]
public class UserProfile
{
    public string user_id;
    public int user_rating;
    public int k_factor;
    public int total_sessions;
    public bool onboarding_completed;
    public string last_updated;
}

[Serializable]
public class WordEntry
{
    public string word;
    public string pos;
    public string meaning;
    public int rating;
    public string source;
    public float confidence;
    public bool learned;
}

[Serializable]
public class DailyScheduleWord
{
    public string word;
    public string pos;
    public string meaning;
    public int rating;
    public string type; // "new" | "review" | "supplement"
    public string fsrs_due; // review일 때만 존재
}

[Serializable]
public class ScheduleStats
{
    public int new_count;
    public int review_count;
    public int supplement_count;
}

[Serializable]
public class DailySchedule
{
    public string date;
    public int user_rating;
    public int total_words;
    public DailyScheduleWord[] new_words;
    public DailyScheduleWord[] review_words;
    public DailyScheduleWord[] db_supplement;
    public ScheduleStats stats;
}

[Serializable]
public class SessionAnswer
{
    public string word;
    public bool correct;
    public int rating_given; // 1(Again) 2(Hard) 3(Good) 4(Easy)
}

[Serializable]
public class SessionResultPayload
{
    public SessionAnswer[] answers;
}

[Serializable]
public class SessionResultResponse
{
    public int user_rating;
    public int k_factor;
    public int total_sessions;
}

[Serializable]
public class ApiError
{
    public string detail;
    public int status_code;
}

// ──────────────────────────────────────────────
// API 매니저
// ──────────────────────────────────────────────

/// <summary>
/// Memorix 백엔드 서버(localhost:8000)와의 모든 HTTP 통신을 담당합니다.
/// MonoBehaviour에서 StartCoroutine()으로 호출하세요.
/// </summary>
public class ApiManager : MonoBehaviour
{
    // ── 싱글톤 ──
    public static ApiManager Instance { get; private set; }

    [Header("서버 설정")]
    [SerializeField] private string baseUrl = "http://localhost:8000";
    [SerializeField] private float timeoutSeconds = 30f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ──────────────────────────────────────────
    // 공개 API 메서드 (StartCoroutine으로 호출)
    // ──────────────────────────────────────────

    /// <summary>서버 상태 확인. 앱 시작 시 호출하여 ai3_ready 등을 체크합니다.</summary>
    public IEnumerator CheckHealth(Action<HealthResponse> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<HealthResponse>("/api/health", onSuccess, onError);
    }

    [Serializable]
    public class InitDefaultResponse
    {
        public string status;
        public int total_words;
        public int oxford_matched;
        public int predicted;
    }

    /// <summary>CSV 없이 Oxford 기본 단어만으로 rated_words.json 초기화</summary>
    public IEnumerator InitDefault(Action<InitDefaultResponse> onSuccess, Action<string> onError = null)
    {
        yield return PostRequest<InitDefaultResponse>("/api/init-default", new object(), onSuccess, onError);
    }

    /// <summary>
    /// CSV 파일을 업로드하여 단어 레이팅을 계산합니다.
    /// filePath: Application.persistentDataPath 등 로컬 절대 경로
    /// </summary>
    public IEnumerator UploadCsv(string filePath, Action<UploadCsvResponse> onSuccess, Action<string> onError = null)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        string fileName = System.IO.Path.GetFileName(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, fileName, "text/csv");

        using var req = UnityWebRequest.Post(baseUrl + "/api/upload-csv", form);
        req.timeout = Mathf.RoundToInt(timeoutSeconds * 3); // CSV 처리는 더 오래 걸릴 수 있음

        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    /// <summary>온보딩 퀴즈 100문제를 가져옵니다.</summary>
    public IEnumerator GetOnboardingQuiz(Action<OnboardingQuiz> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<OnboardingQuiz>("/api/onboarding/quiz", onSuccess, onError);
    }

    /// <summary>온보딩 퀴즈 답변을 제출하고 userRating 초기값을 받습니다.</summary>
    public IEnumerator SubmitOnboardingAnswers(QuizAnswer[] answers, Action<UserProfile> onSuccess, Action<string> onError = null)
    {
        var payload = new QuizAnswerPayload { answers = answers };
        yield return PostRequest<UserProfile>("/api/onboarding/submit", payload, onSuccess, onError);
    }

    /// <summary>오늘의 학습 스케줄을 가져옵니다.</summary>
    public IEnumerator GetTodaySchedule(int dailyLimit, Action<DailySchedule> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<DailySchedule>($"/api/schedule/today?daily_limit={dailyLimit}", onSuccess, onError);
    }

    /// <summary>학습 세션 결과를 제출합니다. FSRS와 userRating이 업데이트됩니다.</summary>
    public IEnumerator SubmitSessionResult(SessionAnswer[] answers, Action<SessionResultResponse> onSuccess, Action<string> onError = null)
    {
        var payload = new SessionResultPayload { answers = answers };
        yield return PostRequest<SessionResultResponse>("/api/session/result", payload, onSuccess, onError);
    }

    /// <summary>현재 유저 프로필(userRating, 세션 수 등)을 가져옵니다.</summary>
    public IEnumerator GetUserProfile(Action<UserProfile> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<UserProfile>("/api/user/profile", onSuccess, onError);
    }

    /// <summary>전체 단어 목록(레이팅 포함)을 가져옵니다.</summary>
    public IEnumerator GetAllWords(Action<string> onRawJson, Action<string> onError = null)
    {
        // 단어 수가 많아 raw JSON으로 반환 (필요 시 직접 파싱)
        using var req = UnityWebRequest.Get(baseUrl + "/api/words/all");
        req.timeout = Mathf.RoundToInt(timeoutSeconds);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess(req.downloadHandler.text);
        else
            onError?.Invoke(req.error);

        void onSuccess(string json) => onRawJson?.Invoke(json);
    }

    // ──────────────────────────────────────────
    // 내부 헬퍼
    // ──────────────────────────────────────────

    private IEnumerator GetRequest<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
    {
        using var req = UnityWebRequest.Get(baseUrl + endpoint);
        req.timeout = Mathf.RoundToInt(timeoutSeconds);
        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    private IEnumerator PostRequest<T>(string endpoint, object body, Action<T> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(baseUrl + endpoint, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = Mathf.RoundToInt(timeoutSeconds);

        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    private void HandleResponse<T>(UnityWebRequest req, Action<T> onSuccess, Action<string> onError)
    {
        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T result = JsonUtility.FromJson<T>(req.downloadHandler.text);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onError?.Invoke($"JSON 파싱 오류: {e.Message}\n응답: {req.downloadHandler.text}");
            }
        }
        else
        {
            // 서버 에러 메시지 파싱 시도
            string detail = req.downloadHandler?.text ?? req.error;
            try
            {
                var err = JsonUtility.FromJson<ApiError>(detail);
                onError?.Invoke($"[{req.responseCode}] {err.detail}");
            }
            catch
            {
                onError?.Invoke($"[{req.responseCode}] {req.error}");
            }
        }
    }
}