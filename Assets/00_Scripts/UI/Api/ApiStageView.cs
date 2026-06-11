using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StageView의 ApiManager 기반 재구현.
/// - StudyManager.StartToday() 제거 (서버에서 처리)
/// - deck.GetCurrentDay() → UserProfile.DayCount로 대체
/// - futureStage 관련 코드 제거 (leftDays = 0으로 미사용)
/// - 씬 로딩 중 프리로드된 UserProfile을 사용해 씬 진입 즉시 뷰 생성
/// 씬: StudyDungeon_ApiStageSelect
/// </summary>
public class ApiStageView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] ScrollRect scrollView;
    [SerializeField] RectTransform content;
    [SerializeField] RectTransform viewport;
    [SerializeField] ApiStageSelectPanel stageSelectPanel;

    [Header("Setting")]
    [SerializeField] float upperMarginY = 300;
    [SerializeField] float belowMarginY = 300;
    [SerializeField] float spaceY = 100;
    [SerializeField] float minX = 100;
    [SerializeField] float maxX = 100;
    [SerializeField] int pathLineCount = 10;
    [SerializeField] float pathCurvature = 0.5f;
    [SerializeField] int futureStageCount = 3;

    [Header("Prefab")]
    [SerializeField] GameObject currentStagePrefab;
    [SerializeField] GameObject pastStagePrefab;
    [SerializeField] GameObject pathPrefab;
    [SerializeField] GameObject futureStagePrefab; // future 노드 프리팹

    [Header("로딩")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    List<GameObject> stages = new();

    // ── 프리로드 캐시 ──
    private static UserProfile _cachedProfile = null;

    /// <summary>
    /// 이 씬으로 이동할 때 호출합니다.
    /// 로딩 화면 중에 서버에서 UserProfile을 미리 받아두고 씬을 전환합니다.
    /// </summary>
    public static void LoadScene()
    {
        _cachedProfile = null;
        LoadingSceneManager.LoadSceneWithPreload(
            sceneName: "StudyDungeon_ApiStageSelect",
            preloadTask: () => PreloadCoroutine()
        );
    }

    private static IEnumerator PreloadCoroutine()
    {
        yield return ApiManager.Instance.GetUserProfile(
            onSuccess: profile => {
                _cachedProfile = profile;
                PlayerPrefs.SetInt("currentDay", profile.DayCount);
                PlayerPrefs.Save();
                Debug.Log($"[ApiStageView] 프리로드 완료 — DayCount: {profile.DayCount}");
            },
            onError: err => {
                Debug.LogError($"[ApiStageView] 프리로드 실패: {err}");
            }
        );
    }

    // ══════════════════════════════════════════
    // 씬 진입
    // ══════════════════════════════════════════

    private void Start()
    {
        if (_cachedProfile != null)
        {
            // 프리로드 완료 → 즉시 뷰 생성
            CreateView(_cachedProfile.DayCount, _cachedProfile.user_id);
        }
        else
        {
            // 프리로드 없이 직접 진입한 경우 → 씬 내에서 로드
            ShowLoading("프로필 불러오는 중...");
            StartCoroutine(LoadAndCreateView());
        }
    }

    // ── UserProfile 로드 후 뷰 생성 (폴백) ──
    private IEnumerator LoadAndCreateView()
    {
        UserProfile profile = null;

        yield return ApiManager.Instance.GetUserProfile(
            onSuccess: p => { profile = p; },
            onError: err => {
                Debug.LogError($"[ApiStageView] 프로필 로드 실패: {err}");
                ShowLoading($"프로필 로드 실패\n{err}");
            }
        );

        if (profile == null) yield break;

        HideLoading();

        // ApiStageSelectPanel의 titleText용으로 currentDay 저장
        PlayerPrefs.SetInt("currentDay", profile.DayCount);
        PlayerPrefs.Save();

        CreateView(profile.DayCount, profile.user_id);
    }

    // ── 뷰 생성 (기존 CreateView와 동일한 구조, futureStage 제거) ──
    public void CreateView(int currentDay, string userId)
    {
        // currentDay는 1-based DayCount이므로 0-based 인덱스로 변환
        int currentDayIndex = currentDay - 1;

        int totalStageCount = currentDay + futureStageCount;

        float pastHeight = pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
        float currentHeight = currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
        float futureHeight = futureStagePrefab.GetComponent<RectTransform>().sizeDelta.y;

        float totalSizeY = pastHeight * currentDayIndex
                         + currentHeight
                         + futureHeight * futureStageCount
                         + spaceY * (totalStageCount - 1)
                         + upperMarginY + belowMarginY;
        totalSizeY = Mathf.Max(totalSizeY, viewport.rect.height);

        float accumulatedY = 0f;
        for (int i = 0; i < totalStageCount; i++)
        {
            bool isPast = i < currentDayIndex;
            bool isCurrent = i == currentDayIndex;
            bool isFuture = i > currentDayIndex;

            GameObject stagePrefab = isPast ? pastStagePrefab
                                   : isCurrent ? currentStagePrefab
                                               : futureStagePrefab;

            float stageHeight = stagePrefab.GetComponent<RectTransform>().sizeDelta.y;

            float x = GetRandomPosX(i, userId, minX, maxX);
            float y = accumulatedY + stageHeight * 0.5f;
            y -= totalSizeY * 0.5f - belowMarginY;

            accumulatedY += stageHeight + spaceY;

            GameObject stage = Instantiate(stagePrefab, content);
            RectTransform stageTr = stage.GetComponent<RectTransform>();
            stageTr.anchoredPosition = new Vector2(x, y);

            // dayText: future 마지막 노드는 "...", 그 외는 "N일차"
            bool isLastFuture = isFuture && (i == totalStageCount - 1);
            stage.GetComponent<StageButton>().dayText.text = isLastFuture ? "..." : $"{i + 1}일차";

            // 현재 Day 노드에만 클릭 이벤트 연결
            if (isCurrent)
            {
                stage.GetComponent<Button>().onClick.AddListener(() => {
                    stageSelectPanel.ShowStageSelectPanel();
                });
            }

            stages.Add(stage);
        }

        // 노드 간 베지어 곡선 path 생성 (기존과 동일)
        for (int i = 0; i < stages.Count - 1; i++)
        {
            RectTransform start = stages[i].transform.Find("Top").GetComponent<RectTransform>();
            RectTransform end = stages[i + 1].transform.Find("Bottom").GetComponent<RectTransform>();

            Vector2 P0 = start.position;
            Vector2 P3 = end.position;
            Vector2 P1 = new Vector2(P0.x, P0.y + spaceY * pathCurvature);
            Vector2 P2 = new Vector2(P3.x, P3.y - spaceY * pathCurvature);

            List<RectTransform> points = new List<RectTransform>();
            points.Add(start);
            for (int j = 0; j < pathLineCount - 1; j++)
            {
                RectTransform inter = (new GameObject("interpoint", typeof(RectTransform))).GetComponent<RectTransform>();
                inter.SetParent(stages[i].transform, false);
                inter.position = MyMath.Bezier(P0, P1, P2, P3, (float)(j + 1) / pathLineCount);
                points.Add(inter);
            }
            points.Add(end);

            var path = Instantiate(pathPrefab, stages[i].transform);
            path.GetComponent<LineDrawer>().points = points.ToArray();
        }

        // content 크기 및 스크롤 위치 (기존과 동일)
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalSizeY);

        Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content);
        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            content, stages[currentDayIndex].GetComponent<RectTransform>());
        float targetY = contentBounds.max.y - targetBounds.center.y + viewport.rect.y * 0.5f;
        float scrollableHeight = content.rect.height - viewport.rect.height;
        scrollView.normalizedPosition = new Vector2(0.5f, Mathf.Clamp01(1 - (targetY / scrollableHeight)));
    }

    private float GetRandomPosX(int index, string seed, float minX, float maxX)
    {
        if (index == 0) return 0;
        float randomRange = DeterministicRandom.RandomFromIndex(index, seed);
        return Mathf.Lerp(minX, maxX, randomRange);
    }

    // ── 로딩 UI 헬퍼 ──
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