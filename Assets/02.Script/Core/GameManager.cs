using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static GameManager Instance;

    [Header("Player Data")]
    [SerializeField] private long currentGold = 10000; // 초기 자금
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private float currentExp = 0;
    [SerializeField] private float maxExp = 100;

    [Header("Player Stats")]
    [SerializeField] private int luckStat = 0; // 운
    [SerializeField] private int sharpEyesStat = 0; // 눈썰미

    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 파괴X
        }
        else
        {
            Destroy(gameObject); // 중복 매니저 제거
        }
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

        Debug.Log($"[GameManager] 현재 골드: {currentGold}");

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

        Debug.Log($"[GameManager] 레벨 업! 현재 레벨: {playerLevel}");

    }


    public void UpgradeLuck()
    {
        luckStat++;
    }

    public void UpgradeSharpEyes()
    {
        sharpEyesStat++;
    }
}