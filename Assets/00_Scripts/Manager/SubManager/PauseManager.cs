using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] RectTransform skillViewContent;
    [SerializeField] SkillIcon skillIconPrefab;

    List<SkillIcon> skillIcons = new List<SkillIcon>();

    public void ShowPausePanel()
    {
        SetSkillIcons();

        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void HidePausePanel()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void Quit()
    {
        // TODO: 게임 진행도 저장되지 않는다는 경고창 띄우기
    }

    private void SetSkillIcons()
    {
        var skills = Player.Instance.skillManager.GetActiveSkills().ToList();

        int index = 0;
        for (; index < skills.Count; index++)
        {
            if (skillIcons.Count <= index)
            {
                skillIcons.Add(Instantiate(skillIconPrefab, skillViewContent));
            }

            if (!skillIcons[index].gameObject.activeSelf) skillIcons[index].gameObject.SetActive(true);
            skillIcons[index].SetIcon(skills[index].data, skills[index].stack);
        }
        for (; index < skillIcons.Count; index++)
        {
            skillIcons[index].gameObject.SetActive(false);
        }
    }
}
