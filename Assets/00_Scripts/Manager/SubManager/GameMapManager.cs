using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMapManager : MonoBehaviour
{
    [Serializable]
    public class StageData
    {
        public int stageCount;
        public List<GameObject> mapPool;

        private RandomQueue<GameObject> randomQueue;
        
        public void Initialize()
        {
            if (mapPool.Count < stageCount)
            {
                throw new Exception("배정된 맵이 부족합니다");
            }

            randomQueue = new RandomQueue<GameObject>(mapPool);
        }

        public GameObject GetMap()
        {
            return randomQueue.Dequeue();
        }
    }

    [Header("Map")]
    public List<StageData> stageDatas;

    private int lastStageIndex = 0;

    private int currentStage = -1;
    private GameObject currentMap = null;
    private List<Monster> currentMapMonsters;
    private Gate currentMapGate;
    private bool isClearStage = false;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        // TODO: 임시 - 맵 클리어 여부 체크
        if (currentMap != null && !isClearStage && CheckClearMap())
        {
            OpenNextStage();
            isClearStage = true;
        }
    }

    public void Initialize()
    {
        foreach (StageData data in stageDatas)
        {
            lastStageIndex += data.stageCount;

            data.Initialize();
        }

        NextStage();
    }

    public void NextStage()
    {
        currentStage++;
        isClearStage = false;
        Log.LogMessage($"Stage {currentStage}");

        if (currentStage > lastStageIndex)
            return;

        // 맵 결정
        int currentStageTmp = currentStage;
        for (int i = 0; i < stageDatas.Count; i++)
        {
            if (currentStageTmp < stageDatas[i].stageCount)
            {
                currentMap = stageDatas[i].GetMap();
                break;
            }

            currentStageTmp -= stageDatas[i].stageCount;
        }
        currentMapMonsters = currentMap.GetComponentsInChildren<Monster>().ToList();
        currentMapGate = currentMap.GetComponentInChildren<Gate>();

        Transform startPosObj = currentMap.transform.Find("StartPos");
        Vector3 startPos = Vector2.zero;
        if (startPosObj != null)
        {
            startPos = startPosObj.position;
        }
        else
        {
            Log.LogWarning("현재 맵에 \"StartPos\" 이름을 가진 오브젝트가 없습니다.");
            startPos = currentMap.transform.position;
        }

        if (currentStage > 0)
        {
            StartCoroutine(MoveMapCoroutine(startPos));
        }
        else
        {
            MoveMap(startPos);
        }
    }

    private IEnumerator MoveMapCoroutine(Vector3 startPos)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.3f);

        yield return new WaitForSeconds(0.5f);

        MoveMap(startPos);

        GLOBAL_CANVAS.Fade.FadeOut(0.1f);
    }

    private void MoveMap(Vector3 startPos)
    {
        // 플레이어 startPos로 텔레포트
        Player.Instance.transform.position = startPos;

        // 카메라 다음 맵 위치로 이동
        Camera.main.transform.position = new Vector3(currentMap.transform.position.x, currentMap.transform.position.y, Camera.main.transform.position.z);
    }

    public bool CheckClearMap()
    {
        for (int i = 0; i < currentMapMonsters.Count; i++)
        {
            if (currentMapMonsters[i] != null && 
                !(currentMapMonsters[i].IsDied || currentMapMonsters[i].gameObject.activeSelf == false))    // 살아있으면
            {
                return false;
            }
        }

        return true;
    }

    public void OpenNextStage()
    {
        // 다음 맵 포탈로 가는 길 열기
        if (currentMapGate != null)
        {
            currentMapGate.OpenGate();
        }
    }
}
