using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;

    private void Start()
    {
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

    public void MoveToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
