using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        for (int i = 0; i < shopItems.Count; i++)
        {
            var item = Instantiate(characterItemPrefab, itemView);

            RectTransform itemTr = item.GetComponent<RectTransform>();
            itemTr.anchoredPosition = new Vector2(i * itemTr.sizeDelta.x, 0);
            itemTr.Find("Name").GetComponent<TextMeshProUGUI>().text = shopItems[i].name;
            itemTr.Find("Desc").GetComponent<TextMeshProUGUI>().text = shopItems[i].desc;
            itemTr.Find("Purchase").GetComponentInChildren<TextMeshProUGUI>().text = $"{shopItems[i].price} C";
        }

        int characterId = shopItems[currentCharacterIndex].characterId;
        GameObject characterPrefab = MANAGER.DB.characterDB.GetCharacterData(characterId).character;
        currentCharacterObj = Instantiate(characterPrefab, characterPos);
        currentCharacterObj.transform.localPosition = Vector3.zero;
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
            Vector2 offset = Vector2.Lerp(Vector2.zero, Vector2.right * sizeX, t / duration);
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
            Vector2 offset = Vector2.Lerp(Vector2.zero, Vector2.right * sizeX, t / duration);
            itemView.anchoredPosition = itemViewOriginalPos + offset;
        }

        characterImage.color = new Color(1, 1, 1, 1);
        itemView.anchoredPosition = itemViewOriginalPos + Vector2.right * sizeX;

        characterMoveCoroutine = null;
    }


    // ----------------------




}
