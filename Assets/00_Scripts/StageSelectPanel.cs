using System;
using TMPro;
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
        bool isCleared = false;
        for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
        {
            StageDifficulty stageDifficulty = (StageDifficulty)d;

            if (isCleared/* || MANAGER.StudyManager.GetStageProgress(stageDifficulty).isCompleted*/)
            {
                isCleared = true;
                rewardDecoration[d].SetActive(false);
                stageSelectButtons[d].interactable = false;
            }
            else
            {
                rewardDecoration[d].SetActive(true);
                stageSelectButtons[d].interactable = true;
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
