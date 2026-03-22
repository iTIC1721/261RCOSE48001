using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class StageView : MonoBehaviour
{
    [SerializeField] ScrollRect scrollView;
    [SerializeField] RectTransform content;
    [SerializeField] RectTransform viewport;
    [SerializeField] StageSelectPanel stageSelectPanel;

    [SerializeField] float upperMarginY = 300;
    [SerializeField] float belowMarginY = 300;
    [SerializeField] float spaceY = 100;
    [SerializeField] float minX = 100;
    [SerializeField] float maxX = 100;

    [SerializeField] GameObject currentStagePrefab;
    [SerializeField] GameObject pastStagePrefab;
    [SerializeField] GameObject futureStagePrefab;

    List<GameObject> stages = new();

    private void Start()
    {
        CreateView();
    }

    public void CreateView()
    {
        MANAGER.StudyManager.StartToday();

        int leftDays = /*MANAGER.StudyManager.PredictLeftDays()*/ 0;
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

            float x = GetRandomPosX(i, MANAGER.StudyManager.deckId, minX, maxX);
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
