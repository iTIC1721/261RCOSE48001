using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageView : MonoBehaviour
{
    [SerializeField] ScrollRect scrollView;
    [SerializeField] RectTransform content;

    [SerializeField] float marginY = 300;
    [SerializeField] float spaceY = 100;
    [SerializeField] float minX = 100;
    [SerializeField] float maxX = 100;

    [SerializeField] GameObject currentStagePrefab;
    [SerializeField] GameObject pastStagePrefab;
    [SerializeField] GameObject futureStagePrefab;
    [SerializeField] GameObject lastStagePrefab;

    List<GameObject> stages = new();

    private void Start()
    {
        CreateView();
    }

    public void CreateView()
    {
        MANAGER.StudyManager.StartToday();

        int leftDays = MANAGER.StudyManager.PredictLeftDays();
        int currentDay = MANAGER.StudyManager.GetCurrentDay();

        int totalStageCount = leftDays + currentDay;
        float totalSizeY =
            pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y * (currentDay - 1) +
            currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y +
            futureStagePrefab.GetComponent<RectTransform>().sizeDelta.y * leftDays +
            lastStagePrefab.GetComponent<RectTransform>().sizeDelta.y +
            spaceY * (totalStageCount - 1) +
            marginY * 2;

        for (int i = 0; i < totalStageCount; i++)
        {
            GameObject stagePrefab;
            if (i == totalStageCount - 1) stagePrefab = lastStagePrefab;
            else if (i < currentDay) stagePrefab = pastStagePrefab;
            else if (i > currentDay) stagePrefab = futureStagePrefab;
            else stagePrefab = currentStagePrefab;

            float x = GetRandomPosX(i, MANAGER.StudyManager.deckId, minX, maxX);
            float y = stagePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.5f;
            for (int j = 0; j < i; j++)
            {
                float tmpY = 0;
                if (j == totalStageCount - 1) tmpY = lastStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                else if (j < currentDay) tmpY = pastStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                else if (j > currentDay) tmpY = futureStagePrefab.GetComponent<RectTransform>().sizeDelta.y;
                else tmpY = currentStagePrefab.GetComponent<RectTransform>().sizeDelta.y;

                y += tmpY + spaceY;
            }
            y -= totalSizeY * 0.5f - marginY;

            GameObject stage = Instantiate(stagePrefab, content);
            RectTransform stageTr = stage.GetComponent<RectTransform>();
            stageTr.anchoredPosition = new Vector2(x, y);

            stages.Add(stage);
        }

        // content 크기 조정
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalSizeY);

        // 스크롤 이동
        scrollView.normalizedPosition = new Vector2(0, stages[currentDay].GetComponent<RectTransform>().anchoredPosition.y / totalSizeY);
    }

    private float GetRandomPosX(int index, string seed, float minX, float maxX)
    {
        if (index == 0) return 0;

        float randomRange = DeterministicRandom.RandomFromIndex(index, seed);
        return Mathf.Lerp(minX, maxX, randomRange);
    }
}
