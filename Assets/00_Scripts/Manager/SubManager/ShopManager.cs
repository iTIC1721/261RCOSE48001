using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    enum PurchaseButtonType
    {
        Select, Selected, Purchase, MoveToNext
    }

    [Header("UI")]
    [SerializeField] List<GameObject> panels = new List<GameObject>();
    [SerializeField] List<GameObject> panelButtons = new List<GameObject>();
    [SerializeField] TextMeshProUGUI moneyText;

    [SerializeField] RawImage characterImage;
    [SerializeField] RectTransform itemView;
    [SerializeField] GameObject characterItemPrefab;
    [SerializeField] Transform characterPos;

    [Header("Setting")]
    [SerializeField] Color panelButtonEnabled = Color.white;
    [SerializeField] Color panelButtonDisabled = Color.white;
    [SerializeField] string selectedText = "선택됨";
    [SerializeField] string selectText = "선택하기";
    [SerializeField] string purchaseText = "C";
    [SerializeField] string moveToNextText = "다음";

    private int currentPanelIndex = 0;

    // -----------------------------

    // 캐릭터 샵
    private int currentCharacterIndex = 0;

    private GameObject currentCharacterObj;
    private List<GameObject> characterItems = new List<GameObject>();

    private Coroutine characterMoveCoroutine = null;
    
    // -----------------------------

    // 업그레이드 샵



    private void Start()
    {
        ActivePanel(currentPanelIndex);
        CharacterPanelInitialize();
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
                if (panelButtons[i] != null) panelButtons[i].GetComponent<Image>().color = panelButtonEnabled;
            }
            else
            {
                panels[i].SetActive(false);
                if (panelButtons[i] != null) panelButtons[i].GetComponent<Image>().color = panelButtonDisabled;
            }
        }

        // Money Text 갱신
        var data = SaveSystem.LoadPlayerData();
        SetMoneyText(data.money);
    }

    private void SetMoneyText(int money)
    {
        moneyText.text = $"{money} C";
    }

    // ----------------------

    // 캐릭터 샵

    private void CharacterPanelInitialize()
    {
        List<ShopItem> shopItems = MANAGER.DB.shopDB.items;
        PlayerSaveData saveData = SaveSystem.LoadPlayerData();

        for (int i = 0; i < shopItems.Count; i++)
        {
            var item = Instantiate(characterItemPrefab, itemView);

            RectTransform itemTr = item.GetComponent<RectTransform>();
            itemTr.anchoredPosition = new Vector2(i * itemTr.sizeDelta.x, 0);
            itemTr.Find("Name").GetComponent<TextMeshProUGUI>().text = shopItems[i].name;
            itemTr.Find("Desc").GetComponent<TextMeshProUGUI>().text = shopItems[i].desc;
            if (MANAGER.DB.shopDB.HasItem(shopItems[i].id, saveData))
            {
                if (saveData.characterId == shopItems[i].characterId)
                {
                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.MoveToNext, null);
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.Select, shopItem);
                }
            }
            else
            {
                ShopItem shopItem = shopItems[i];

                Transform purchaseButton = itemTr.Find("Purchase");
                SetPurchaseButton(purchaseButton, PurchaseButtonType.Purchase, shopItem);
            }

            characterItems.Add(item);
        }

        int characterId = shopItems[currentCharacterIndex].characterId;
        GameObject characterPrefab = MANAGER.DB.characterDB.GetCharacterData(characterId).character;
        currentCharacterObj = Instantiate(characterPrefab, characterPos);
        currentCharacterObj.transform.localPosition = Vector3.zero;
    }

    public void SelectCharacter(int shopItemId)
    {
        List<ShopItem> shopItems = MANAGER.DB.shopDB.items;
        PlayerSaveData saveData = SaveSystem.LoadPlayerData();

        int characterId = MANAGER.DB.shopDB.items.Find(c => c.id ==  shopItemId).characterId;
        saveData.characterId = characterId;

        SaveSystem.SavePlayerData(saveData);

        for (int i = 0; i < characterItems.Count; i++)
        {
            RectTransform itemTr = characterItems[i].GetComponent<RectTransform>();
            if (MANAGER.DB.shopDB.HasItem(shopItems[i].id, saveData))
            {
                if (saveData.characterId == shopItems[i].characterId)
                {
                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.MoveToNext, null);
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.Select, shopItem);
                }
            }
        }
    }

    public void PurchaseCharacter(int shopItemId)
    {
        List<ShopItem> shopItems = MANAGER.DB.shopDB.items;

        bool tryPurchase = MANAGER.Inventory.SpendMoney(shopItems.Find(s => s.id == shopItemId).price);
        if (!tryPurchase) return;

        PlayerSaveData saveData = SaveSystem.LoadPlayerData();

        Log.LogMessage($"{shopItemId}, {saveData.purchaseList}");
        saveData.purchaseList |= (uint)(1 << shopItemId);

        SetMoneyText(saveData.money);

        SaveSystem.SavePlayerData(saveData);


        for (int i = 0; i < characterItems.Count; i++)
        {
            RectTransform itemTr = characterItems[i].GetComponent<RectTransform>();
            if (MANAGER.DB.shopDB.HasItem(shopItems[i].id, saveData))
            {
                if (saveData.characterId == shopItems[i].characterId)
                {
                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.MoveToNext, null);
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    SetPurchaseButton(purchaseButton, PurchaseButtonType.Select, shopItem);
                }
            }
        }
    }

    private void SetPurchaseButton(Transform purchaseButton, PurchaseButtonType type, ShopItem shopItem)
    {
        switch (type)
        {
            case PurchaseButtonType.Select:
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = selectText;
                purchaseButton.GetComponent<Outline>().enabled = false;
                purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                    if (shopItem != null) SelectCharacter(shopItem.id);
                });
                break;
            case PurchaseButtonType.Selected:
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = selectedText;
                purchaseButton.GetComponent<Outline>().enabled = false;
                purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                break;
            case PurchaseButtonType.Purchase:
                int price = (shopItem != null) ? shopItem.price : -1;
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{shopItem.price} {purchaseText}";
                purchaseButton.GetComponent<Outline>().enabled = false;
                purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                    if (shopItem != null) PurchaseCharacter(shopItem.id);
                });
                break;
            case PurchaseButtonType.MoveToNext:
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = moveToNextText;
                purchaseButton.GetComponent<Outline>().enabled = true;
                purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                    ActivePanel(1);
                });
                break;
        }
    }

    public void NextCharacter()
    {
        if (characterMoveCoroutine != null) return;
        if (currentCharacterIndex >= characterItems.Count - 1) return;
        
        currentCharacterIndex++;

        characterMoveCoroutine = StartCoroutine(NextCharacterCoroutine(0.2f));
    }

    private IEnumerator NextCharacterCoroutine(float duration)
    {
        float t = 0;
        bool isChangedCharacter = false;
        Vector2 itemViewOriginalPos = itemView.anchoredPosition;
        float sizeX = characterItemPrefab.GetComponent<RectTransform>().sizeDelta.x;

        while (t < duration)
        {
            yield return null;
            t += Time.deltaTime;

            if (t < duration * 0.5f)
            {
                float alpha = Mathf.Lerp(1, 0, t / (duration * 0.5f));
                characterImage.color = new Color(1, 1, 1, alpha);
            }
            else
            {
                if (!isChangedCharacter)
                {
                    // 캐릭터 교체
                    Destroy(currentCharacterObj);

                    int characterId = MANAGER.DB.shopDB.items[currentCharacterIndex].characterId;
                    GameObject characterPrefab = MANAGER.DB.characterDB.GetCharacterData(characterId).character;
                    currentCharacterObj = Instantiate(characterPrefab, characterPos);
                    currentCharacterObj.transform.localPosition = Vector3.zero;

                    isChangedCharacter = true;
                }

                float alpha = Mathf.Lerp(0, 1, (t - duration * 0.5f) / (duration * 0.5f));
                characterImage.color = new Color(1, 1, 1, alpha);
            }

            // Item 옆으로 넘기기
            Vector2 offset = Vector2.Lerp(Vector2.zero, Vector2.right * sizeX, MyMath.EaseOut(t / duration));
            itemView.anchoredPosition = itemViewOriginalPos - offset;
        }

        characterImage.color = new Color(1, 1, 1, 1);
        itemView.anchoredPosition = itemViewOriginalPos - Vector2.right * sizeX;

        characterMoveCoroutine = null;
    }

    public void PrevCharacter()
    {
        if (characterMoveCoroutine != null) return;
        if (currentCharacterIndex <= 0) return;

        currentCharacterIndex--;

        characterMoveCoroutine = StartCoroutine(PrevCharacterCoroutine(0.2f));
    }

    private IEnumerator PrevCharacterCoroutine(float duration)
    {
        float t = 0;
        bool isChangedCharacter = false;
        Vector2 itemViewOriginalPos = itemView.anchoredPosition;
        float sizeX = characterItemPrefab.GetComponent<RectTransform>().sizeDelta.x;

        while (t < duration)
        {
            yield return null;
            t += Time.deltaTime;

            float time = MyMath.EaseOut(t / duration);
            if (time < 0.5f)
            {
                float alpha = Mathf.Lerp(1, 0, time / 0.5f);
                characterImage.color = new Color(1, 1, 1, alpha);
            }
            else
            {
                if (!isChangedCharacter)
                {
                    // 캐릭터 교체
                    Destroy(currentCharacterObj);

                    int characterId = MANAGER.DB.shopDB.items[currentCharacterIndex].characterId;
                    GameObject characterPrefab = MANAGER.DB.characterDB.GetCharacterData(characterId).character;
                    currentCharacterObj = Instantiate(characterPrefab, characterPos);
                    currentCharacterObj.transform.localPosition = Vector3.zero;

                    isChangedCharacter = true;
                }

                float alpha = Mathf.Lerp(0, 1, (time - 0.5f) / 0.5f);
                characterImage.color = new Color(1, 1, 1, alpha);
            }

            // Item 옆으로 넘기기
            Vector2 offset = Vector2.Lerp(Vector2.zero, Vector2.right * sizeX, MyMath.EaseOut(t / duration));
            itemView.anchoredPosition = itemViewOriginalPos + offset;
        }

        characterImage.color = new Color(1, 1, 1, 1);
        itemView.anchoredPosition = itemViewOriginalPos + Vector2.right * sizeX;

        characterMoveCoroutine = null;
    }


    // ----------------------




}
