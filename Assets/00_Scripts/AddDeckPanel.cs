using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddDeckPanel : MonoBehaviour
{
    [SerializeField] private DeckView deckView;

    [SerializeField] private GameObject addDeckPanel;
    [SerializeField] private TMP_InputField pathInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField dailyCountInput;
    [SerializeField] private Button addButton;
    [SerializeField] private Button cancelButton;

    public void ShowPanel()
    {
        pathInput.text = string.Empty;
        nameInput.text = string.Empty;
        dailyCountInput.text = string.Empty;

        addDeckPanel.SetActive(true);
    }

    public void HidePanel()
    {
        addDeckPanel.SetActive(false);
    }

    public void Add()
    {
        string deckId = string.Empty;

        string path = pathInput.text;
        string name = nameInput.text;
        // TODO: path ¼öÁ¤
        if (int.TryParse(dailyCountInput.text, out int dailyCount))
        {
            deckId = DeckSystem.CreateDeckFromCSV(deckView.testCSVPath, name, dailyCount);
        }
        else
        {
            deckId = DeckSystem.CreateDeckFromCSV(deckView.testCSVPath, name);
        }

        deckView.CreateDeckButton(SaveSystem.LoadDeck(deckId));

        HidePanel();
    }

    public void Cancel()
    {
        HidePanel();
    }
}
