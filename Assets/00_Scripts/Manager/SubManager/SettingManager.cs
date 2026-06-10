using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정 씬에서 유저 데이터를 삭제하고 새 프로필을 생성합니다.
/// Button의 onClick에 ResetAndReinitialize()를 연결하세요.
/// </summary>
public class SettingManager : MonoBehaviour
{
    [Header("UI 연결 (선택)")]
    [Tooltip("초기화 진행 중 비활성화할 버튼")]
    [SerializeField] private Button resetButton;

    [Tooltip("상태 메시지를 표시할 TMP 텍스트")]
    [SerializeField] private TMP_Text statusText;

    public event Action OnResetSuccess;
    public event Action<string> OnResetFailed;

    // ──────────────────────────────────────────
    // Public
    // ──────────────────────────────────────────

    public void ResetAndReinitialize()
    {
        StartCoroutine(ResetCoroutine());
    }

    // ──────────────────────────────────────────
    // Internal
    // ──────────────────────────────────────────

    private IEnumerator ResetCoroutine()
    {
        SetButtonInteractable(false);

        // 1. 이전 유저의 PlayerPrefs 키 전체 삭제
        SetStatus("기존 유저 데이터 삭제 중…");
        DeleteAllUserPrefs();

        // 2. 새 계정 생성
        SetStatus("새 계정 생성 중…");

        bool createSuccess = false;
        string errorMsg = null;

        yield return ApiManager.Instance.CreateUser(
            onSuccess: profile => {
                Debug.Log($"[UserResetManager] 새 유저 생성: {profile.user_id}");
                createSuccess = true;
            },
            onError: err => { errorMsg = err; }
        );

        if (!createSuccess)
        {
            HandleError($"계정 생성 실패: {errorMsg}");
            yield break;
        }

        // 3. 기본 단어 초기화
        SetStatus("단어 데이터 초기화 중…");

        bool initSuccess = false;

        yield return ApiManager.Instance.InitDefault(
            onSuccess: result => {
                Debug.Log($"[UserResetManager] 초기화 완료 — 총 단어: {result.total_words}");
                initSuccess = true;
            },
            onError: err => { errorMsg = err; }
        );

        if (!initSuccess)
        {
            HandleError($"단어 초기화 실패: {errorMsg}");
            yield break;
        }

        SetStatus("초기화 완료!");
        Debug.Log("[UserResetManager] 유저 재생성 완료");
        OnResetSuccess?.Invoke();
        SetButtonInteractable(true);
    }

    /// <summary>
    /// 현재 저장된 user_id를 기준으로 관련 PlayerPrefs 키를 모두 삭제합니다.
    /// 삭제 대상:
    ///   "user_id"
    ///   "studied_words_{userId}_{date}"             — 오늘 포함 과거 30일치
    ///   "learnCompleted_{userId}_{date}"            — 오늘 포함 과거 30일치
    ///   "quizCompleted_{diffIndex}_{userId}_{date}" — 난이도 0~9, 오늘 포함 과거 30일치
    /// </summary>
    private void DeleteAllUserPrefs()
    {
        string userId = PlayerPrefs.GetString("user_id", "");

        PlayerPrefs.DeleteKey("user_id");

        if (string.IsNullOrEmpty(userId))
        {
            PlayerPrefs.Save();
            Debug.Log("[UserResetManager] user_id 없음 — 기본 키만 삭제");
            return;
        }

        for (int i = 0; i <= 30; i++)
        {
            string date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");

            PlayerPrefs.DeleteKey($"studied_words_{userId}_{date}");
            PlayerPrefs.DeleteKey($"learnCompleted_{userId}_{date}");

            for (int diff = 0; diff < 10; diff++)
                PlayerPrefs.DeleteKey($"quizCompleted_{diff}_{userId}_{date}");
        }

        PlayerPrefs.Save();
        Debug.Log($"[UserResetManager] user_id({userId}) 관련 PlayerPrefs 삭제 완료");
    }

    private void HandleError(string message)
    {
        Debug.LogError($"[UserResetManager] {message}");
        SetStatus(message);
        OnResetFailed?.Invoke(message);
        SetButtonInteractable(true);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (resetButton != null)
            resetButton.interactable = interactable;
    }
}