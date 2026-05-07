using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DungeonSelectButton : MonoBehaviour
{
    public bool locked = false;
    [SerializeField] string lockMessage;

    [Space]
    [SerializeField] Image buttonImage;
    [SerializeField] Color lockColor;
    [SerializeField] GameObject lockPanel;
    [SerializeField] TextMeshProUGUI lockMessageText;

    private Button button;
    private UIPulse uiPulse;

    private void Awake()
    {
        button = GetComponent<Button>();
        uiPulse = GetComponentInChildren<UIPulse>();
    }

    private void Start()
    {
        lockMessageText.text = lockMessage;

        if (locked)
        {
            Lock();
        }
        else
        {
            UnLock();
        }
    }

    public void Lock()
    {
        locked = true;

        uiPulse.Stop();
        buttonImage.color = lockColor;
        lockPanel.SetActive(true);

        button.interactable = false;
    }

    public void UnLock()
    {
        locked = false;

        uiPulse.Resume();
        buttonImage.color = Color.white;
        lockPanel.SetActive(false);

        button.interactable = true;
    }
}
