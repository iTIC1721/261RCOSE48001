using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    [SerializeField] GameObject slotMachine;
    [SerializeField] List<Slot> slots;
    [SerializeField] int baseRollCount = 4;

    private int slotDummyCount = 2;

    private SkillData[] targetSkills;

    private void Awake()
    {
        targetSkills = new SkillData[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;

            slots[i].GetComponent<Button>().onClick.RemoveAllListeners();
            slots[i].GetComponent<Button>().onClick.AddListener(() => SelectSlot(index));
        }
    }

    public void StartSlotMachine()
    {
        // 일시정지
        Time.timeScale = 0;
        slotMachine.SetActive(true);

        // 등장 가능 스킬 풀 지정
        List<SkillData> skillPool = GetSkillPool();

        // 풀 내에서 슬롯 개수만큼 정답 스킬 지정
        RandomQueue<SkillData> rq = new RandomQueue<SkillData>(skillPool);
        if (skillPool.Count >= slots.Count)
        {
            // 스킬 풀의 스킬 개수가 충분할 경우 중복 없이 랜덤으로 뽑음
            for (int i = 0; i < slots.Count; i++)
            {
                targetSkills[i] = rq.Dequeue();
            }
        }
        else {
            // 스킬 풀의 스킬 개수가 부족할 경우 중복 포함하여 랜덤으로 뽑음
            for (int i = 0; i < slots.Count; i++)
            {
                targetSkills[i] = rq.Peek();
            }
        }

        // 슬롯별로 더미 스킬들 지정 (정답 스킬과 중복도 가능)
        rq = new RandomQueue<SkillData>(skillPool);
        SkillData[][] dummySkills = new SkillData[slots.Count][];
        for (int i = 0; i < slots.Count; i++)
        {
            dummySkills[i] = new SkillData[slotDummyCount];
            for (int j = 0; j < slotDummyCount; j++)
            {
                dummySkills[i][j] = rq.Peek();
            }
        }

        // 랜덤 인덱스로 정답 위치 설정
        int[] indexes = new int[slots.Count];
        for (int i = 0; i < slots.Count; i++)
        {
            indexes[i] = Random.Range(0, 3);
        }

        // 슬롯에 정답 스킬과 더미 스킬 지정
        for (int i = 0; i < slots.Count; i++)
        {
            List<Sprite> sprites = new List<Sprite>();
            for (int j = 0, dummy = 0; j < slotDummyCount + 1; j++)
            {
                if (j == indexes[i])
                {
                    sprites.Add(targetSkills[i].skillSprite);
                }
                else
                {
                    sprites.Add(dummySkills[i][dummy++].skillSprite);
                }
            }

            slots[i].SetItem(sprites);
        }

        // Roll
        Roll(indexes, baseRollCount);
    }

    public void ExitSlotMachine()
    {
        slotMachine.SetActive(false);
        Time.timeScale = 1;
    }

    public void SelectSlot(int index)
    {
        SkillData selectedSkill = targetSkills[index];
        Player.Instance.skillManager.AddSkill(selectedSkill);

        ExitSlotMachine();
    }

    private List<SkillData> GetSkillPool()
    {
        // TODO: 플레이어가 가지고 있는 스킬은 제외
        return new List<SkillData>(MANAGER.DB.skillDB.skillDatas);
    }

    private void Roll(int[] indexes, int baseRollCount = 4)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].RollStart(indexes[i], baseRollCount + i);
        }
    }
}
