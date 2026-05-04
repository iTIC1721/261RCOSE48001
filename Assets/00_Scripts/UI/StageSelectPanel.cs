using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    [SerializeField] GameObject stageSelectPanel;
    [SerializeField] Button learnStageButton;
    [SerializeField] Button[] stageSelectButtons;   // 0: Easy, 1: Hard
    [SerializeField] GameObject[] rewardDecoration;

    public void ShowStageSelectPanel()
    {
        if (MANAGER.StudyManager.deck.lastLearnDate.Date != CustomTime.GetTimeNow().Date)   // ¿À´Ã °øºÎ ¾È³¡³ÂÀ¸¸é
        {
            rewardDecoration[0].SetActive(true);
            learnStageButton.interactable = true;

            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                rewardDecoration[d + 1].SetActive(true);
                stageSelectButtons[d].interactable = false;
            }
        }
        else
        {
            rewardDecoration[0].SetActive(false);
            learnStageButton.interactable = false;

            bool isCleared = false;
            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                if (isCleared || MANAGER.StudyManager.deck.quizCompleted[d])
                {
                    isCleared = true;
                    rewardDecoration[d + 1].SetActive(false);
                    stageSelectButtons[d].interactable = false;
                }
                else
                {
                    rewardDecoration[d + 1].SetActive(true);
                    stageSelectButtons[d].interactable = true;
                }
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
}
