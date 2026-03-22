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

    public string testCSVPath = "D:/Download/eng_test.csv";

    private void Start()
    {
        List<DeckInfo> decks = DeckSystem.GetAllDecks();

        for (int i = 0; i < decks.Count; i++)
        {
            CreateDeckButton(decks[i]);
        }
    }

    public GameObject CreateDeckButton(DeckInfo deckInfo)
    {
        var deckBtn = Instantiate(deckButtonPrefab, content);

        deckBtn.GetComponent<DeckButton>().deckNameText.text = deckInfo.deckName;
        deckBtn.GetComponent<DeckButton>().deckId = deckInfo.deckId;

        deckBtn.GetComponent<DeckButton>().AddEvent(() => {
            Log.LogMessage(SaveSystem.Load(deckInfo.deckId).ToString());
            MANAGER.StudyManager.Load(deckInfo.deckId);

            // TODO: 오늘 스테이지 선택으로 이동
            SceneManager.LoadScene("StudyDungeon_StageSelect");
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
