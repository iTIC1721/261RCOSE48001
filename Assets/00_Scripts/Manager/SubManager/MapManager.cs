using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
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

    [Header("Skill Slot Machine")]
    public SlotMachine skillSlotMachine;
    [Tooltip("몇 스테이지마다 스킬을 획득할지 설정합니다.")]
    public int skillEveryNStages = 2;

    [Header("Map")]
    public List<StageData> stageDatas;

    // ── 스탯 스케일링 ──────────────────────────────────────────────
    [Header("Stat Scaling")]
    [Tooltip("StageStatScaler 컴포넌트를 연결하세요. 비워두면 스케일링을 건너뜁니다.")]
    public StageStatScaler statScaler;

    public int LastStageIndex { get; private set; } = 0;

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
        MANAGER.Game.InitializeGame();
        Player.Instance.SetCharacter();

        foreach (StageData data in stageDatas)
        {
            LastStageIndex += data.stageCount;

            data.Initialize();
        }

        NextStage();
    }

    public void NextStage()
    {
        currentStage++;
        isClearStage = false;
        Log.LogMessage($"Stage {currentStage}");

        if (currentStage > LastStageIndex)
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

        // 스탯 스케일링 적용
        // 몬스터 리스트를 가져온 직후, 맵 이동 전에 적용합니다.
        statScaler?.ApplyStats(currentStage, LastStageIndex, currentMapMonsters);

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

        if ((currentStage - 1) % skillEveryNStages == 0)
        {
            skillSlotMachine.StartSlotMachine();
        }
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

    public void ClearGame()
    {
        MANAGER.Game.isCleared = true;
        StartCoroutine(ClearGameCoroutine());
    }

    private IEnumerator ClearGameCoroutine()
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.5f);

        yield return new WaitForSecondsRealtime(0.7f);

        LoadingSceneManager.LoadScene("Main_Result", 0.7f);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        Time.timeScale = 0.1f;

        float camSize = Camera.main.orthographicSize;
        float zoomSize = 5f;

        Vector3 camPos = Camera.main.transform.position;
        Vector3 playerPos = Player.Instance.transform.position;
        Vector3 camDest = GetClampedPosition(camPos, camSize, playerPos, zoomSize);

        float zoomTime = 2f;
        float t = 0;
        while (t < zoomTime)
        {
            yield return null;
            t += Time.unscaledDeltaTime;

            Camera.main.orthographicSize = Mathf.Lerp(camSize, zoomSize, MyMath.EaseInOut(t / zoomTime));
            Camera.main.transform.position = Vector3.Lerp(camPos, camDest, MyMath.EaseInOut(t / zoomTime));
        }

        GLOBAL_CANVAS.Fade.FadeIn(0.5f);

        yield return new WaitForSecondsRealtime(0.7f);

        LoadingSceneManager.LoadScene("Main_Result", 0.7f);
    }

    private Vector3 GetClampedPosition(Vector3 originalPosition, float originalSize, Vector2 targetPos, float newSize)
    {
        float originalHalfH = originalSize;         // orthographic size = 세로 절반
        float originalHalfW = originalSize * Camera.main.aspect;

        float zoomedHalfH = newSize;
        float zoomedHalfW = newSize * Camera.main.aspect;

        // 기존 카메라 범위의 경계 (originalPosition 기준)
        float minX = originalPosition.x - originalHalfW + zoomedHalfW;
        float maxX = originalPosition.x + originalHalfW - zoomedHalfW;
        float minY = originalPosition.y - originalHalfH + zoomedHalfH;
        float maxY = originalPosition.y + originalHalfH - zoomedHalfH;

        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(clampedX, clampedY, originalPosition.z);
    }
}
