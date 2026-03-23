using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] List<GameObject> panels = new List<GameObject>();
    [SerializeField] List<GameObject> panelButtons = new List<GameObject>();

    [SerializeField] RawImage characterImage;
    [SerializeField] RectTransform itemView;
    [SerializeField] GameObject characterItemPrefab;
    [SerializeField] Transform characterPos;

    [Header("Setting")]
    [SerializeField] Color panelButtonEnabled = Color.white;
    [SerializeField] Color panelButtonDisabled = Color.white;

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
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택됨";
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택하기";
                    purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                        SelectCharacter(shopItem.id);
                    });
                }
            }
            else
            {
                ShopItem shopItem = shopItems[i];

                Transform purchaseButton = itemTr.Find("Purchase");
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{shopItems[i].price} C";
                purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                    PurchaseCharacter(shopItem.id);
                });
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
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택됨";
                    purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택하기";
                    purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                        SelectCharacter(shopItem.id);
                    });
                }
            }
        }
    }

    public void PurchaseCharacter(int shopItemId)
    {
        List<ShopItem> shopItems = MANAGER.DB.shopDB.items;
        PlayerSaveData saveData = SaveSystem.LoadPlayerData();

        bool tryPurchase = MANAGER.Inventory.SpendMoney(shopItems.Find(s => s.id == shopItemId).price);
        if (!tryPurchase) return;

        Log.LogMessage($"{shopItemId}, {saveData.purchaseList}");
        saveData.purchaseList |= 1 << shopItemId;

        SaveSystem.SavePlayerData(saveData);

        for (int i = 0; i < characterItems.Count; i++)
        {
            RectTransform itemTr = characterItems[i].GetComponent<RectTransform>();
            if (MANAGER.DB.shopDB.HasItem(shopItems[i].id, saveData))
            {
                if (saveData.characterId == shopItems[i].characterId)
                {
                    Transform purchaseButton = itemTr.Find("Purchase");
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택됨";
                    purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                }
                else
                {
                    ShopItem shopItem = shopItems[i];

                    Transform purchaseButton = itemTr.Find("Purchase");
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = $"선택하기";
                    purchaseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    purchaseButton.GetComponent<Button>().onClick.AddListener(() => {
                        SelectCharacter(shopItem.id);
                    });
                }
            }
        }
    }

    public void NextCharacter()
    {
        if (characterMoveCoroutine != null) return;
        if (currentCharacterIndex >= panels.Count - 1) return;
        
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
