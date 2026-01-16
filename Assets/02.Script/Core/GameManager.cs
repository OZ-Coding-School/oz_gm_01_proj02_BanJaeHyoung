using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    // 싱글톤
    public static GameManager Instance;

    [System.Serializable]
    public class PlayerData
    {
        public long gold = 10000;
        public int level = 1;
        public float exp = 0;
        public float maxExp = 100;
        public int luck = 0;
        public int sharpEyes = 0;
        public string prevSceneName = ""; // 이전 씬 이름 저장

        // 초기화 (생성자)
        public PlayerData()
        {
            gold = 10000;
            level = 1;
            exp = 0;
            maxExp = 100;
            luck = 0;
            sharpEyes = 0;
            prevSceneName = "";
        }
    }

    [Header("Current Player Data")]
    public PlayerData currentPlayer;

    [Header("플레이어 정보")]
    public int myCharIndex = 0;
    public string myName = "Player";

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

    string SavePath => Application.persistentDataPath + "/SaveData.json";

    public void SaveGame()
    {
        if (currentPlayer == null) return;

        string json = JsonUtility.ToJson(currentPlayer, true);
        File.WriteAllText(SavePath, json);

    }

    public void LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            currentPlayer = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        currentPlayer = new PlayerData(); // 데이터 초기화
        SaveGame(); // 덮어쓰기
    }

    // 데이터 존재 여부 확인 (이어하기 버튼 활성화용)
    public bool HasSaveData()
    {
        return File.Exists(SavePath);
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

    // 파산한 봇 리셋
    public void ReviveBot(BotState bot)
    {
        bot.currentMoney = bot.data.initialMoney; // 초기 자금 복구
        bot.isBankrupt = false; // 파산 딱지 떼기
    }

    // 골드 확인
    public long GetGold()
    {
        if (currentPlayer == null) return 0;
        return currentPlayer.gold;
    }

    // 골드 추가/사용
    public void ChangeGold(long amount)
    {
        if (currentPlayer == null) return;

        currentPlayer.gold += amount;
        if (currentPlayer.gold < 0) currentPlayer.gold = 0; // 0보다 떨어지지 않게

        SaveGame();
    }

    public void AddExp(float amount)
    {
        if (currentPlayer == null) return;

        currentPlayer.exp += amount;
        // 레벨업
        if (currentPlayer.exp >= currentPlayer.maxExp)
        {
            LevelUp();
        }
        SaveGame();
    }

    private void LevelUp()
    {
        currentPlayer.level++;
        currentPlayer.exp = currentPlayer.exp - currentPlayer.maxExp; // 이월
        currentPlayer.maxExp *= 1.2f; // 필요 경험치 증가
    }

    public void UpgradeLuck()
    {
        if (currentPlayer != null) currentPlayer.luck++;
        SaveGame();
    }

    public void UpgradeSharpEyes()
    {
        if (currentPlayer != null) currentPlayer.sharpEyes++;
        SaveGame();
    }

    public void ChangeScene(string sceneName)
    {
        if (currentPlayer != null)
        {
            currentPlayer.prevSceneName = SceneManager.GetActiveScene().name;
            SaveGame(); // 씬 이동 전 저장
        }
        SceneManager.LoadScene(sceneName);
    }

    public string PrevSceneName
    {
        get { return currentPlayer != null ? currentPlayer.prevSceneName : ""; }
    }
}