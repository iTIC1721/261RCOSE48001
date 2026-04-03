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

    private int currentStage = 0;
    private GameObject currentMap;
    private List<Monster> currentMapMonsters;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        combatMapRandomQueue = new RandomQueue<GameObject>(combatMaps);
    }

    public void NextStage()
    {
        currentStage++;

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
        // TODO: 플레이어 startPos로 텔레포트
        // TODO: 카메라 다음 맵 위치로 이동
    }

    public bool CheckClearMap()
    {
        for (int i = 0; i < currentMapMonsters.Count; i++)
        {
            if (currentMapMonsters[i] != null && currentMapMonsters[i].gameObject.activeSelf == true)
            {
                return false;
            }
        }

        return true;
    }

    public void OpenNextStage()
    {
        // TODO: 다음 맵 포탈로 가는 길 열기
    }
}
