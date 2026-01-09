using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    // 싱글톤
    public static GameManager Instance;

    [Header("Player Data")]
    [SerializeField] private long currentGold = 10000; // 초기 자금
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private float currentExp = 0;
    [SerializeField] private float maxExp = 100;

    [Header("Player Stats")]
    [SerializeField] private int luckStat = 0; // 운
    [SerializeField] private int sharpEyesStat = 0; // 눈썰미

    [Header("Bot")]
    public List<CharacterData> allCharacterPool; // 봇 데이터

    [System.Serializable]
    public class BotState
    {
        public CharacterData data; // 봇 정보
        public long currentMoney;  // 봇 재산
        public bool isBankrupt;    // 파산 여부
    }

    public List<BotState> activeBots = new List<BotState>();

    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 파괴X

            InitializeBots();
        }
        else
        {
            Destroy(gameObject); // 중복 매니저 제거
        }
    }

    // 봇 초기화
    private void InitializeBots()
    {
        activeBots.Clear();
        // 데이터 풀이 비어있으면 에러 방지
        if (allCharacterPool == null) return;

        foreach (var charData in allCharacterPool)
        {
            BotState newState = new BotState();
            newState.data = charData;
            newState.currentMoney = charData.initialMoney;
            newState.isBankrupt = false;

            activeBots.Add(newState);
        }
    }

    // 봇 셔플 로직
    public List<BotState> GetRandomBots(int count)
    {
        List<BotState> candidates = new List<BotState>();

        // 파산하지 않은 봇들만 추리기
        foreach (var bot in activeBots)
        {
            if (!bot.isBankrupt) candidates.Add(bot);
        }

        // 셔플 (섞기)
        for (int i = 0; i < candidates.Count; i++)
        {
            BotState temp = candidates[i];
            int randomIndex = Random.Range(i, candidates.Count);
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        // 앞에서부터 필요한 만큼 자르기
        List<BotState> selectedBots = new List<BotState>();
        for (int i = 0; i < count; i++)
        {
            if (i < candidates.Count) selectedBots.Add(candidates[i]);
        }

        return selectedBots;
    }

    // 특정 봇 파산 처리
    public void BankruptBot(CharacterData botData)
    {
        // 리스트에서 조건 해당 봇을 찾아 파산딱지
        foreach (var bot in activeBots)
        {
            if (bot.data == botData)
            {
                bot.isBankrupt = true;
                bot.currentMoney = 0;
                break;
            }
        }
    }

    // 파산안한 봇 찾기
    public BotState GetReplacementBot(List<BotState> currentServingBots)
    {
        List<BotState> candidates = new List<BotState>();

        foreach (var bot in activeBots)
        {
            // 이미 파산한 애는 제외
            if (bot.isBankrupt) continue;

            // 지금 플레이중인 봇 제외
            if (currentServingBots.Contains(bot)) continue;

            candidates.Add(bot);
        }

        // 후보가 없으면(전원 파산 or 남은 봇 없음) null 반환
        if (candidates.Count == 0) return null;

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    // 골드 확인
    public long GetGold()
    {
        return currentGold;
    }

    // 골드 추가/사용
    public void ChangeGold(long amount)
    {
        currentGold += amount;

        // 골드 0보다 떨어지지 않게
        if (currentGold < 0) currentGold = 0;


    }

    public void AddExp(float amount)
    {
        currentExp += amount;
        // 레벨업
        if (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        playerLevel++;
        currentExp = currentExp - maxExp; // 남은 경험치는 다음 레벨로 이월
        maxExp *= 1.2f; // 다음 레벨 필요 경험치 % 증가


    }


    public void UpgradeLuck()
    {
        luckStat++;
    }

    public void UpgradeSharpEyes()
    {
        sharpEyesStat++;
    }

    public void ChangeScene(string sceneName)
    {
        PrevSceneName = SceneManager.GetActiveScene().name;


        SceneManager.LoadScene(sceneName);
    }

    // 외부 읽기용
    public string PrevSceneName { get; private set; } = "";
}