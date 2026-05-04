using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;

    private void Start()
    {
        Time.timeScale = 1.0f;

        bool isCleared = MANAGER.Game.isCleared;
        if (isCleared)
        {
            titleText.text = "게임 클리어!";
        }
        else
        {
            titleText.text = "게임 오버...";
        }
    }
}
