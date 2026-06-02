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
/// 씬: StudyDungeon_StageSelect
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

    [Header("Prefab")]
    [SerializeField] GameObject currentStagePrefab;
    [SerializeField] GameObject pastStagePrefab;
    [SerializeField] GameObject pathPrefab;
    // futureStagePrefab 제거 (leftDays = 0으로 실제 생성되지 않음)

    [Header("로딩")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] TextMeshProUGUI loadingText;

    List<GameObject> stages = new();

    private void Start()
    {
        ShowLoading("프로필 불러오는 중...");
        StartCoroutine(LoadAndCreateView());
    }

    // ── UserProfile 로드 후 뷰 생성 ──
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

        int totalStageCount = currentDay; // 과거(0 ~ currentDayIndex-1) + 현재(currentDayIndex)
        float totalSizeY =
            pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y * currentDayIndex +
            currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y +
            spaceY * (totalStageCount - 1) +
            upperMarginY + belowMarginY;
        totalSizeY = Mathf.Max(totalSizeY, viewport.rect.height);

        for (int i = 0; i < totalStageCount; i++)
        {
            GameObject stagePrefab = (i < currentDayIndex) ? pastStagePrefab : currentStagePrefab;

            float x = GetRandomPosX(i, userId, minX, maxX);
            float y = stagePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.5f;
            for (int j = 0; j < i; j++)
            {
                float tmpY = (j < currentDayIndex)
                    ? pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y
                    : currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                y += tmpY + spaceY;
            }
            y -= totalSizeY * 0.5f - belowMarginY;

            GameObject stage = Instantiate(stagePrefab, content);
            RectTransform stageTr = stage.GetComponent<RectTransform>();
            stageTr.anchoredPosition = new Vector2(x, y);
            stage.GetComponent<StageButton>().dayText.text = $"{i + 1}일차";

            // 현재 Day 노드에만 클릭 이벤트 연결 (기존과 동일)
            if (i == currentDayIndex)
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