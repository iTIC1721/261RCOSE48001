using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] List<GameObject> panels = new List<GameObject>();
    [SerializeField] List<GameObject> panelButtons = new List<GameObject>();

    [Header("Setting")]
    [SerializeField] Color panelButtonEnabled = Color.white;
    [SerializeField] Color panelButtonDisabled = Color.white;

    private int currentPanelIndex = 0;

    private void Start()
    {
        ActivePanel(currentPanelIndex);
    }

    public void ActivePanel(int index)
    {
        if (index >= panels.Count)
        {
            Log.LogError($"패널 인덱스 오류: {index}");
            return;
        }

        currentPanelIndex = index;

        for (int i = 0; i < panels.Count; i++)
        {
            if (i == index)
            {
                panels[i].SetActive(true);
                panelButtons[i].GetComponent<Image>().color = panelButtonEnabled;
            }
            else
            {
                panels[i].SetActive(false);
                panelButtons[i].GetComponent<Image>().color = panelButtonDisabled;
            }
        }
    }

    public void MoveToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
