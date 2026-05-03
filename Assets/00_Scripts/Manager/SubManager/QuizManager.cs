using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] ShakeUI shakeLayer;
    [SerializeField] DamageEffect damageEffect;
    [SerializeField] QuizResultPanel resultPanel;
    [SerializeField] GameObject diePanel;
    [SerializeField] TextMeshProUGUI wordText;
    [SerializeField] TextMeshProUGUI meaningText;
    [SerializeField] TextMeshProUGUI leftHpText;
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] Image timeBarFill;
    [SerializeField] Button nextButton; 
    [SerializeField] Button[] choices = new Button[4];

    [Header("Entity")]
    [SerializeField] Transform playerPos;
    [SerializeField] Transform monsterPos;
    [SerializeField] Transform damageLayer;
    //[SerializeField] Transform enemy;

    [Header("DB")]
    [SerializeField] QuizSetting easySetting;
    [SerializeField] QuizSetting hardSetting;

    [Header("Setting")]
    [SerializeField] float baseDamage = 1000;

    private Dictionary<StageDifficulty, QuizSetting> quizSettingDict = null;
    private float timeLimit = 5;
    private int hp = 5;
    private int combo = 0;

    private Entity player;
    private Entity monster;

    private float totalDamage = 0;
    private int progressCount = 0;
    private int correctCount = 0;
    private int todayCount = 0;

    private RandomQueue<Card> todayCards;

    private Card currentCard = null;
    private int currentAnswer = -1;
    private List<(Card card, bool isCorrect)> quizLogs = new();

    private float questionStartTime = 0;
    private float questionResponseTime = 0;

    private bool isSolvingQuiz = false;
    private bool isDied = false;

    private void Awake()
    {
        quizSettingDict = new()
        {
            {StageDifficulty.Easy, easySetting},
            {StageDifficulty.Hard, hardSetting},
        };
    }

    private void Start()
    {
        timeLimit = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].timeLimit;
        SetHp(quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].maxHp);
        SetCombo(0);

        // 캐릭터 생성
        PlayerSaveData data = SaveSystem.LoadPlayerData();
        if (data == null) data = new PlayerSaveData();

        int characterId = data.characterId;
        var playerObj = Instantiate(MANAGER.DB.characterDB.GetCharacterData(characterId).character, playerPos);
        playerObj.transform.localPosition = new Vector3(0, 0, playerObj.transform.localPosition.z);
        playerObj.transform.localScale = new Vector3(-1, 1, 1);
        playerObj.GetComponent<Player>().enableMove = false;
        playerObj.GetComponent<Player>().enableAttack = false;
        playerObj.GetComponent<Player>().invulnerable = true;
        player = playerObj.GetComponent<Entity>();

        // 몬스터 생성
        int monsterId = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].monsterId;
        var monsterObj = Instantiate(MANAGER.DB.monsterDB.GetMonsterData(monsterId).monster, monsterPos);
        monsterObj.transform.localPosition = new Vector3(0, 0, monsterObj.transform.localPosition.z);
        monsterObj.transform.localScale = new Vector3(1, 1, 1);
        monster = monsterObj.GetComponent<Entity>();

        StartQuiz();
    }

    private void Update()
    {
        if (isSolvingQuiz)
        {
            float time = Time.time - questionStartTime;
            timeBarFill.fillAmount = Mathf.Clamp01((timeLimit - time) / timeLimit);

            if (timeLimit - time <= 0)
            {
                // 오답 처리
                SelectAnswer(-1, currentAnswer);
            }
        }
    }
    
    public void StartQuiz()
    {
        Deck currentDeck = MANAGER.StudyManager.deck;
        var cards = MainScheduler.GetCardsById(currentDeck, currentDeck.todayCardIds);

        todayCount = cards.Count;
        todayCards = new RandomQueue<Card>(cards);

        StartCoroutine(CountDownCoroutine());
    }

    private IEnumerator CountDownCoroutine()
    {
        wordText.text = "준비";
        yield return new WaitForSeconds(1);

        for (int i = 3; i >= 1; i--)
        {
            wordText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        wordText.text = "시작!";
        yield return new WaitForSeconds(1);

        ShowNextWord();
    }

    public void ShowNextWord()
    {
        if (isDied) return;

        currentCard = null;
        if (todayCards.Count > 0) 
            currentCard = todayCards.Dequeue();

        if (currentCard != null)
        {
            progressCount++;

            wordText.text = currentCard.front;
            meaningText.text = currentCard.back;
            progressText.text = $"진행도: {progressCount} / {todayCount}";

            // 선택지
            string[] wrongMeanings = MANAGER.StudyManager.GetRandomMeanings(3, currentCard.back);
            currentAnswer = UnityEngine.Random.Range(0, 4);
            string[] meanings = new string[4];
            for (int i = 0, j = 0; i < 4; i++)
            {
                if (i == currentAnswer) meanings[i] = currentCard.back;
                else meanings[i] = wrongMeanings[j++];
            }
            SetChoices(meanings, currentAnswer);

            questionStartTime = Time.time;
            isSolvingQuiz = true;
        }
        else
        {
            // 끝내기
            CompleteStage();
        }
    }

    public void SetChoices(string[] meanings, int answerIndex)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].GetComponentInChildren<TextMeshProUGUI>().text = meanings[i];
            choices[i].interactable = true;

            int tmp = i;
            choices[i].onClick.RemoveAllListeners();
            choices[i].onClick.AddListener(() => SelectAnswer(tmp, answerIndex));
        }
    }

    private void SelectAnswer(int selectIndex, int answerIndex)
    {
        questionResponseTime = Time.time - questionStartTime;
        isSolvingQuiz = false;

        float stayTime = 0;
        if (selectIndex == answerIndex)
        {
            Log.LogMessage("정답!");
            correctCount++;
            SetCombo(combo + 1);
            stayTime = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].correctStayTime;

            // 적 데미지 입음
            float damage = baseDamage + GetAdditionalDamage(questionResponseTime) * baseDamage * 0.8f;
            player.Attack();
            StartCoroutine(EntityAttackCoroutine(() => EnemyHurt(damage)));
        }
        else
        {
            Log.LogMessage($"오답");
            SetCombo(0);
            stayTime = quizSettingDict[MANAGER.StudyManager.currentStageDifficulty].incorrectStayTime;

            // 플레이어 데미지 입음
            monster.Attack();
            StartCoroutine(EntityAttackCoroutine(() => PlayerHurt()));
        }

        // 퀴즈 기록을 로그로 남김
        quizLogs.Add((currentCard, selectIndex == answerIndex));

        // 잠시동안 정답 제외 버튼들을 비활성화하여 정답을 표시
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].onClick.RemoveAllListeners();

            if (i == answerIndex) continue;
            choices[i].interactable = false;
        }

        StartCoroutine(ShowNextWordCoroutine(stayTime));
    }

    private IEnumerator ShowNextWordCoroutine(float stayTime)
    {
        yield return new WaitForSeconds(stayTime);

        ShowNextWord();
    }

    private IEnumerator EntityAttackCoroutine(Action action)
    {
        yield return new WaitForSeconds(0.25f);
        action?.Invoke();
    }

    private void EnemyHurt(float damage)
    {
        // 적 데미지 입음
        Log.LogMessage("적 데미지 입음");
        shakeLayer.Shake(0.5f + combo * 0.02f, true);

        // 기본 데미지 + (콤보 / 5)번의 추가 데미지
        List<DamageInfo> damageList = new List<DamageInfo>();
        damageList.Add(new DamageInfo(damage, player.Transform));
        for (int i = 1; i <= combo / 5; i++)
        {
            damageList.Add(new DamageInfo(damage * (float)i * 0.125f, player.Transform));
        }

        // 데미지 입힘
        monster.GetDamaged(damageList.ToArray());
        for (int i = 0; i < damageList.Count; i++) 
            totalDamage += damageList[i].damage;
    }

    private void PlayerHurt()
    {
        // 플레이어 데미지 입음
        SetHp(hp - 1);
        shakeLayer.Shake(2, true);
        damageEffect.OnDamage();

        if (hp <= 0)
        {
            PlayerDie();
        }
        else
        {
            player.GetDamaged(new DamageInfo(1, monster.Transform));
        }
    }

    private void PlayerDie()
    {
        // 플레이어 사망 - 게임 오버
        Log.LogMessage("플레이어 사망");
        isDied = true;

        player.Die();

        diePanel.SetActive(true);
    }

    private void CompleteStage()
    {
        Log.LogMessage("학습이 종료되었습니다.");

        ReviewCards();

        GetReward();

        MANAGER.StudyManager.deck.quizCompleted[(int)MANAGER.StudyManager.currentStageDifficulty] = true;
        SaveSystem.SaveDeck(MANAGER.StudyManager.deck);

        DisplayResult();
    }

    private void ReviewCards()
    {
        foreach (var item in quizLogs)
        {
            // Rating 결정 후 Review하기
            int rating = GetRating(item.card, item.isCorrect);
            FSRSScheduler.Review(item.card, MANAGER.StudyManager.deck, rating);

            // 다음 학습일 갱신
            Log.LogMessage($"stability: {item.card.stability}");
            item.card.due = CustomTime.GetTimeNow().AddDays(item.card.stability);
            item.card.state = CardState.Review;
        }
    }

    private int GetRating(Card card, bool isCorrect)
    {
        if (!isCorrect) return 1;

        // 맞춘 경우 Card Difficulty에 따라 rating 정하기
        float a = 10;
        float b = 0.5f;

        float Sa = isCorrect ? 1 : 0;
        float diff = card.difficulty / 10f;
        float ma = 1f / (1f + Mathf.Exp(-a * (Mathf.Abs(Sa - diff) - b)));

        float value = ma * (Sa - diff);
        int rating = Mathf.Clamp(Mathf.RoundToInt(value), -1, 1) + 3;

        return rating;
    }

    private void DisplayResult()
    {
        // 정답률
        float correctRate = (float)correctCount / todayCount;
        resultPanel.descTexts[0].text = $"정답률: {(correctRate * 100f).ToString("F0")}%";

        // 총 데미지
        resultPanel.descTexts[1].text = $"총 데미지: {Mathf.FloorToInt(totalDamage)}";

        // 총 진행도
        int totalCount = MANAGER.StudyManager.deck.cards.Count;
        int newCount = MANAGER.StudyManager.deck.cards.Where(w => w.state == CardState.New).Count();
        int studiedCount = totalCount - newCount;
        resultPanel.descTexts[2].text = $"학습 진행도: {studiedCount}/{totalCount}";

        resultPanel.resultPanel.SetActive(true);
    }

    private void GetReward()
    {
        int reward = 0;

        int diff = (int)MANAGER.StudyManager.currentStageDifficulty;
        for (int i = diff; i >= 0; i--)
        {
            if (MANAGER.StudyManager.deck.quizCompleted[i]) continue;

            reward += RewardSystem.CalculateQuizReward((StageDifficulty)i);
        }

        MANAGER.Inventory.AddMoney(reward);
    }

    public void Back()
    {
        StartCoroutine(MoveToSceneCoroutine("StudyDungeon_DeckSelect"));
    }

    private IEnumerator MoveToSceneCoroutine(string sceneName)
    {
        GLOBAL_CANVAS.Fade.FadeIn(0.1f);

        yield return new WaitForSecondsRealtime(0.2f);

        LoadingSceneManager.LoadScene(sceneName);
    }


    private void SetCombo(int combo)
    {
        this.combo = combo;
        comboText.text = $"콤보: {combo}";
    }

    private void SetHp(int hp)
    {
        this.hp = hp;
        leftHpText.text = $"남은 체력: {hp}";
    }


    private float GetAdditionalDamage(float t)
    {
        if (t <= 1f)
            return 1f;

        float x = Mathf.Clamp01((t - 1f) / 4f);
        return Mathf.Exp(-6f * x * x);
    }
}
