using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    [SerializeField] GameObject stageSelectPanel;
    [SerializeField] Button[] stageSelectButtons;
    [SerializeField] GameObject[] rewardDecoration;

    public void ShowStageSelectPanel()
    {
        for (int d = 0; d < Enum.GetValues(typeof(StageDifficulty)).Length; d++)
        {
            StageDifficulty stageDifficulty = (StageDifficulty)d;

            if (!MANAGER.StudyManager.GetStageProgress(stageDifficulty).isCompleted)
            {
                rewardDecoration[d].SetActive(true);
                stageSelectButtons[d].interactable = true;
            }
            else
            {
                rewardDecoration[d].SetActive(false);
                stageSelectButtons[d].interactable = false;
            }
        }

        stageSelectPanel.SetActive(true);
    }

    public void HideStageSelectPanel()
    {
        stageSelectPanel.SetActive(false);
    }

    public void SetDifficulty(int diff)
    {
        MANAGER.StudyManager.currentStageDifficulty = (StageDifficulty)diff;
    }

    public void MoveToQuiz()
    {
        SceneManager.LoadScene("StudyDungeon_Quiz");
    }
}
