using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class StageView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] ScrollRect scrollView;
    [SerializeField] RectTransform content;
    [SerializeField] RectTransform viewport;
    [SerializeField] StageSelectPanel stageSelectPanel;

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
    [SerializeField] GameObject futureStagePrefab;
    [SerializeField] GameObject pathPrefab;

    List<GameObject> stages = new();

    private void Start()
    {
        CreateView();
    }

    public void CreateView()
    {
        MANAGER.StudyManager.StartToday();

        int leftDays = 0;
        int currentDay = MANAGER.StudyManager.GetCurrentDay();

        // 스테이지 노드 생성
        int totalStageCount = leftDays + currentDay + 1;
        float totalSizeY =
            pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y * currentDay +
            currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y +
            futureStagePrefab.GetComponent<RectTransform>().sizeDelta.y * leftDays +
            spaceY * (totalStageCount - 1) +
            upperMarginY + belowMarginY;
        totalSizeY = Mathf.Max(totalSizeY, viewport.rect.height);

        for (int i = 0; i < totalStageCount; i++)
        {
            GameObject stagePrefab;
            if (i < currentDay) stagePrefab = pastStagePrefab;
            else if (i > currentDay) stagePrefab = futureStagePrefab;
            else stagePrefab = currentStagePrefab;

            float x = GetRandomPosX(i, MANAGER.StudyManager.deck.id, minX, maxX);
            float y = stagePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.5f;
            for (int j = 0; j < i; j++)
            {
                float tmpY = 0;
                if (j < currentDay) tmpY = pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                else if (j > currentDay) tmpY = futureStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                else tmpY = currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y;

                y += tmpY + spaceY;
            }
            y -= totalSizeY * 0.5f - belowMarginY;

            GameObject stage = Instantiate(stagePrefab, content);
            RectTransform stageTr = stage.GetComponent<RectTransform>();
            stageTr.anchoredPosition = new Vector2(x, y);
            stage.GetComponent<StageButton>().dayText.text = $"{i + 1}일차";
            if (i == currentDay)
            {
                stage.GetComponent<Button>().onClick.AddListener(() => {
                    stageSelectPanel.ShowStageSelectPanel();
                });
            }

            stages.Add(stage);
        }

        // 노드끼리 연결하는 곡선 path 생성
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
            LineDrawer ld = path.GetComponent<LineDrawer>();
            ld.points = points.ToArray();
        }

        // content 크기 조정
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalSizeY);

        // 스크롤 이동
        Bounds contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content);
        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, stages[currentDay].GetComponent<RectTransform>());
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
}
