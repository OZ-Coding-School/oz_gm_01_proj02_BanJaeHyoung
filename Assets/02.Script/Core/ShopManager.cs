using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI 패널 연결")]
    [SerializeField] private GameObject shopPanel;       // 전체 패널
    [SerializeField] private GameObject interactionText;  // 안내문
    [SerializeField] private TextMeshProUGUI myGoldText;  // 보유 골드

    [Header("행운")]
    [SerializeField] private TextMeshProUGUI txtLuckCurLv;   // 현재 LV
    [SerializeField] private TextMeshProUGUI txtLuckNextLv;  // 다음 LV
    [SerializeField] private TextMeshProUGUI txtLuckPrice;   // 가격 텍스트

    [Header("눈썰미")]
    [SerializeField] private TextMeshProUGUI txtEyeCurLv;
    [SerializeField] private TextMeshProUGUI txtEyeNextLv;
    [SerializeField] private TextMeshProUGUI txtEyePrice;

    [Header("수완")]
    [SerializeField] private TextMeshProUGUI txtBizCurLv;
    [SerializeField] private TextMeshProUGUI txtBizNextLv;
    [SerializeField] private TextMeshProUGUI txtBizPrice;

    [Header("통찰")]
    [SerializeField] private TextMeshProUGUI txtInsightCurLv;
    [SerializeField] private TextMeshProUGUI txtInsightNextLv;
    [SerializeField] private TextMeshProUGUI txtInsightPrice;

    [Header("가격 설정")]
    [SerializeField] private int basePrice = 3000;      // 시작 가격
    [SerializeField] private int priceIncrement = 1000; // 1업당 증가할 가격

    private bool isPlayerNearby = false;

    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (interactionText != null) interactionText.SetActive(false);
    }

    void Update()
    {
        // F키로 열고 닫기
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            if (shopPanel.activeSelf) CloseShop();
            else OpenShop();
        }
        // ESC키로 닫기
        else if (shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    // 상점 열고 닫기
    public void OpenShop()
    {
        shopPanel.SetActive(true);
        if (interactionText != null) interactionText.SetActive(false);
        UpdateUI(); // 열릴 때 데이터 갱신
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        if (isPlayerNearby && interactionText != null) interactionText.SetActive(true);
    }

    // UI 갱신
    private void UpdateUI()
    {
        if (GameManager.Instance == null) return;
        var data = GameManager.Instance.currentPlayer;

        // 보유 자금 표시
        myGoldText.text = $"{data.gold:N0}";

        // 각 스탯 정보 갱신
        UpdateStatRow(data.luck, txtLuckCurLv, txtLuckNextLv, txtLuckPrice);
        UpdateStatRow(data.sharpEyes, txtEyeCurLv, txtEyeNextLv, txtEyePrice);
        UpdateStatRow(data.business, txtBizCurLv, txtBizNextLv, txtBizPrice);
        UpdateStatRow(data.insight, txtInsightCurLv, txtInsightNextLv, txtInsightPrice);
    }

    private void UpdateStatRow(int currentLv, TextMeshProUGUI txtCur, TextMeshProUGUI txtNext, TextMeshProUGUI txtPrice)
    {
        // 현재, 다음 레벨 표시
        txtCur.text = currentLv.ToString();
        txtNext.text = (currentLv + 1).ToString();

        // 버튼 텍스트 갱신
        long price = CalculatePrice(currentLv);
        txtPrice.text = $"{price:N0}";
    }

    private long CalculatePrice(int currentLv)
    {
        return basePrice + (currentLv * priceIncrement);
    }

    // 구매 버튼 연결용
    public void OnClickBuyLuck() { BuyStat(0); }
    public void OnClickBuySharpEyes() { BuyStat(1); }
    public void OnClickBuyBusiness() { BuyStat(2); }
    public void OnClickBuyInsight() { BuyStat(3); }

    // 구매 처리 로직
    private void BuyStat(int type)
    {
        var data = GameManager.Instance.currentPlayer;
        int currentLv = 0;

        if (type == 0) currentLv = data.luck;
        else if (type == 1) currentLv = data.sharpEyes;
        else if (type == 2) currentLv = data.business;
        else if (type == 3) currentLv = data.insight;

        long price = CalculatePrice(currentLv);

        if (data.gold >= price)
        {
            GameManager.Instance.ChangeGold(-price); // 돈 차감

            // 스탯 적용
            if (type == 0) GameManager.Instance.UpgradeLuck();
            else if (type == 1) GameManager.Instance.UpgradeSharpEyes();
            else if (type == 2) GameManager.Instance.UpgradeBusiness();
            else if (type == 3) GameManager.Instance.UpgradeInsight();

            UpdateUI(); // 화면 갱신!
        }
    }

    // NPC 접근 감지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactionText != null) interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionText != null) interactionText.SetActive(false);
            CloseShop(); // 멀어지면 강제로 닫기
        }
    }
}