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

// ── CAT 온보딩 모델 ──

[Serializable]
public class CatStartPayload
{
    public string user_id;
}

[Serializable]
public class CatQuestion
{
    public bool done;
    public int question_num;
    public int max_questions;
    public int theta;          // 현재 추정 실력
    public string word;
    public int rating;
}

[Serializable]
public class CatAnswerPayload
{
    public string user_id;
    public string word;
    public bool correct;
    public int response_time_ms;
}

[Serializable]
public class CatResult
{
    public bool done;
    public int question_num;
    public UserProfile user_profile;
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
    public int rating;
    public string type;
    public string fsrs_due;
    public string pos;
    public string meaning;
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
    public int rating_given; // 1(Again) 2(Hard) 3(Good) 4(Easy)
}

[Serializable]
public class SessionResultPayload
{
    public string user_id;
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
public class InitDefaultPayload
{
    public string user_id;
}

[Serializable]
public class InitDefaultResponse
{
    public string status;
    public int total_words;
    public int oxford_matched;
    public int predicted;
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
/// Memorix 백엔드 서버와의 모든 HTTP 통신을 담당합니다.
/// MonoBehaviour에서 StartCoroutine()으로 호출하세요.
/// </summary>
public class ApiManager : MonoBehaviour
{
    // ── 싱글톤 ──
    public static ApiManager Instance { get; private set; }

    [Header("서버 설정")]
    [SerializeField] private string baseUrl = "https://memorix-api-production.up.railway.app";
    [SerializeField] private float timeoutSeconds = 30f;

    /// <summary>현재 로그인된 유저 ID. PlayerPrefs에서 로드합니다.</summary>
    public string UserId => PlayerPrefs.GetString("user_id", "");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ──────────────────────────────────────────
    // 공개 API 메서드 (StartCoroutine으로 호출)
    // ──────────────────────────────────────────

    /// <summary>서버 상태 확인. 앱 시작 시 호출합니다.</summary>
    public IEnumerator CheckHealth(Action<HealthResponse> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<HealthResponse>("/api/health", onSuccess, onError);
    }

    /// <summary>
    /// 유저 ID 발급. 최초 실행 시 1회 호출하며, 발급된 ID를 PlayerPrefs에 저장합니다.
    /// </summary>
    public IEnumerator CreateUser(Action<UserProfile> onSuccess, Action<string> onError = null)
    {
        yield return PostRequest<UserProfile>("/api/users/create", new object(), onSuccess, onError,
            onAfterSuccess: profile => {
                PlayerPrefs.SetString("user_id", profile.user_id);
                PlayerPrefs.Save();
                Debug.Log($"[ApiManager] user_id 발급 및 저장: {profile.user_id}");
            });
    }

    /// <summary>CSV 없이 Oxford 기본 단어만으로 초기화합니다.</summary>
    public IEnumerator InitDefault(Action<InitDefaultResponse> onSuccess, Action<string> onError = null)
    {
        var payload = new InitDefaultPayload { user_id = UserId };
        yield return PostRequest<InitDefaultResponse>("/api/init-default", payload, onSuccess, onError);
    }

    /// <summary>CSV 파일을 업로드하여 단어 레이팅을 계산합니다.</summary>
    public IEnumerator UploadCsv(string filePath, Action<UploadCsvResponse> onSuccess, Action<string> onError = null)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        string fileName = System.IO.Path.GetFileName(filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, fileName, "text/csv");
        form.AddField("user_id", UserId);

        using var req = UnityWebRequest.Post(baseUrl + "/api/upload-csv", form);
        req.timeout = Mathf.RoundToInt(timeoutSeconds * 3);

        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError);
    }

    /// <summary>CAT 온보딩 시작 → 첫 문항 반환.</summary>
    public IEnumerator CatStart(Action<CatQuestion> onSuccess, Action<string> onError = null)
    {
        var payload = new CatStartPayload { user_id = UserId };
        yield return PostRequest<CatQuestion>("/api/onboarding/cat/start", payload, onSuccess, onError);
    }

    /// <summary>CAT 한 문항 답변 제출 → 다음 문항 or 완료 반환.</summary>
    public IEnumerator CatAnswer(string word, bool correct, int responseTimeMs,
        Action<CatQuestion> onQuestion, Action<CatResult> onDone, Action<string> onError = null)
    {
        var payload = new CatAnswerPayload
        {
            user_id = UserId,
            word = word,
            correct = correct,
            response_time_ms = responseTimeMs,
        };

        // 응답이 done=true(완료)이면 CatResult로, false(진행중)이면 CatQuestion으로 파싱
        using var req = new UnityWebRequest(baseUrl + "/api/onboarding/cat/answer", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = Mathf.RoundToInt(timeoutSeconds);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"[{req.responseCode}] {req.error}");
            yield break;
        }

        try
        {
            string raw = req.downloadHandler.text;
            Debug.Log($"[CAT] 서버 응답: {raw}");

            // done 필드 먼저 확인
            var peek = JsonUtility.FromJson<CatResult>(raw);
            if (peek.done)
            {
                Debug.Log($"[CAT] 완료 — question_num: {peek.question_num}");
                onDone?.Invoke(peek);
            }
            else
            {
                var q = JsonUtility.FromJson<CatQuestion>(raw);
                Debug.Log($"[CAT] 다음 문항 — question_num: {q.question_num}, word: {q.word}");
                onQuestion?.Invoke(q);
            }
        }
        catch (Exception e)
        {
            onError?.Invoke($"JSON 파싱 오류: {e.Message}\n응답: {req.downloadHandler.text}");
        }
    }

    /// <summary>오늘의 학습 스케줄을 가져옵니다.</summary>
    public IEnumerator GetTodaySchedule(int dailyLimit, Action<DailySchedule> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<DailySchedule>(
            $"/api/schedule/today?user_id={UserId}&daily_limit={dailyLimit}",
            onSuccess, onError);
    }

    /// <summary>학습 세션 결과를 제출합니다.</summary>
    public IEnumerator SubmitSessionResult(SessionAnswer[] answers, Action<SessionResultResponse> onSuccess, Action<string> onError = null)
    {
        var payload = new SessionResultPayload { user_id = UserId, answers = answers };
        yield return PostRequest<SessionResultResponse>("/api/session/result", payload, onSuccess, onError);
    }

    /// <summary>현재 유저 프로필을 가져옵니다.</summary>
    public IEnumerator GetUserProfile(Action<UserProfile> onSuccess, Action<string> onError = null)
    {
        yield return GetRequest<UserProfile>($"/api/user/profile?user_id={UserId}", onSuccess, onError);
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

    private IEnumerator PostRequest<T>(string endpoint, object body, Action<T> onSuccess,
        Action<string> onError, Action<T> onAfterSuccess = null)
    {
        string json = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(baseUrl + endpoint, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = Mathf.RoundToInt(timeoutSeconds);

        yield return req.SendWebRequest();
        HandleResponse(req, onSuccess, onError, onAfterSuccess);
    }

    private void HandleResponse<T>(UnityWebRequest req, Action<T> onSuccess,
        Action<string> onError, Action<T> onAfterSuccess = null)
    {
        if (req.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T result = JsonUtility.FromJson<T>(req.downloadHandler.text);
                onAfterSuccess?.Invoke(result);
                onSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                onError?.Invoke($"JSON 파싱 오류: {e.Message}\n응답: {req.downloadHandler.text}");
            }
        }
        else
        {
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