using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckView : MonoBehaviour
{
    public int maxDeckCount = 3;
    [SerializeField] private GameObject deckButtonPrefab;
    [SerializeField] private RectTransform content;

    [SerializeField] private GameObject addDeckButton;

    private List<GameObject> deckButtonList = new();

    private void Start()
    {
        List<Deck> decks = DeckSystem.GetAllDecks();

        for (int i = 0; i < decks.Count; i++)
        {
            CreateDeckButton(decks[i]);
        }
    }

    public GameObject CreateDeckButton(Deck deck)
    {
        var deckBtn = Instantiate(deckButtonPrefab, content);

        deckBtn.GetComponent<DeckButton>().deckNameText.text = deck.name;
        deckBtn.GetComponent<DeckButton>().deckId = deck.id;

        deckBtn.GetComponent<DeckButton>().AddEvent(() => {
            //Log.LogMessage(SaveSystem.LoadDeck(deck.id).ToString());
            MANAGER.StudyManager.Load(deck.id);
        });

        // 덱 삭제 버튼 이벤트 추가
        deckBtn.GetComponent<DeckButton>().deleteButton.onClick.AddListener(() => {
            DeckSystem.DeleteDeck(deckBtn.GetComponent<DeckButton>().deckId);
            deckButtonList.Remove(deckBtn);
            deckBtn.transform.SetParent(null, false);
            SetAddDeckButton();
            Destroy(deckBtn);
        });

        deckButtonList.Add(deckBtn);

        SetAddDeckButton();
        return deckBtn;
    }

    private void SetAddDeckButton()
    {
        addDeckButton.transform.SetAsLastSibling();
        if (deckButtonList.Count >= maxDeckCount)
            addDeckButton.SetActive(false);
        else
            addDeckButton.SetActive(true);
    }
}
