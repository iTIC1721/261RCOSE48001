using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMapManager : MonoBehaviour
{
    [Header("Setting")]
    public int lastStageIndex = 5;

    [Header("Map")]
    public GameObject startMap;
    public List<GameObject> combatMaps;
    public GameObject bossMap;

    private RandomQueue<GameObject> combatMapRandomQueue;

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
        combatMapRandomQueue = new RandomQueue<GameObject>(combatMaps);

        NextStage();
    }

    public void NextStage()
    {
        currentStage++;
        isClearStage = false;
        Log.LogMessage($"Stage {currentStage}");

        if (currentStage > lastStageIndex)
            return;

        if (combatMapRandomQueue.Count <= 0)
        {
            Log.LogError("배정된 맵이 더 이상 없습니다!");
            return;
        }

        if (currentStage == 0)      // 시작 맵
        {
            currentMap = startMap;
        }
        else if (currentStage == lastStageIndex)     // 보스 맵
        {
            currentMap = bossMap;
        }
        else
        {
            // 일반 맵 중에서 중복 없이 뽑음
            currentMap = combatMapRandomQueue.Dequeue();            
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
