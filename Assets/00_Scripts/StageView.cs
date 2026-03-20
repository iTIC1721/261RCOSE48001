using System.Collections.Generic;
using UnityEngine;

public class StageView : MonoBehaviour
{
    [SerializeField] float marginY = 300;
    [SerializeField] float spaceY = 100;
    [SerializeField] float minX = 100;
    [SerializeField] float maxX = 100;

    [SerializeField] private GameObject currentStagePrefab;
    [SerializeField] private GameObject pastStagePrefab;
    [SerializeField] private GameObject futureStagePrefab;
    [SerializeField] private GameObject lastStagePrefab;

    List<GameObject> stages = new();

    public void CreateView()
    {
        MANAGER.StudyManager.StartToday();

        int leftDays = MANAGER.StudyManager.PredictLeftDays();
        int currentDay = MANAGER.StudyManager.GetCurrentDay();

        int totalStageCount = leftDays + currentDay;

        for (int i = 0; i < totalStageCount; i++)
        {
            GameObject stagePrefab;
            if (i == totalStageCount - 1) stagePrefab = lastStagePrefab;
            else if (i < currentDay) stagePrefab = pastStagePrefab;
            else if (i > currentDay) stagePrefab = futureStagePrefab;
            else stagePrefab = currentStagePrefab;

            float x = GetPosX(i, minX, maxX);
            float y = 0;
            // TODO: y°ª °è»ê
        }
    }

    private void SetContainerSize(int totalStageCount)
    {

    }

    private float GetPosX(int index, float minX, float maxX)
    {
        float randomRange = Mathf.Abs(Mathf.Sin(index * 12.9898f) * 43758.5453f) % 1f;
        return Mathf.Lerp(minX, maxX, randomRange);
    }
}
