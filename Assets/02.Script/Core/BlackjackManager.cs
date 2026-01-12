using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class BlackjackManager : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private DeckManager deckManager;

    [System.Serializable]
    public class SeatUI
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI moneyText;
        public Image faceImage;
    }

    [Header("SeatUi 연결")]
    public List<SeatUI> seatUIs; // 4개의 자리 UI를 여기에 연결

    [Header("참가자 카드 위치")]
    [SerializeField] private List<Transform> playerSeats; // Bot1, Bot2, Player, Bot3

    [Header("딜러 위치")]
    [SerializeField] private Transform dealerSeats;

    [Header("점수 표시용 텍스트")]
    [SerializeField] private List<TextMeshProUGUI> scoreTexts; // Bot1, Bot2, Player, Bot3

    [Header("딜러 텍스트 위치")]
    [SerializeField] private TextMeshProUGUI dealerScoreText;

    [Header("UI")]
    [SerializeField] private Button btnHit;
    [SerializeField] private Button btnStand;
    [SerializeField] private TextMeshProUGUI txtResult;
    [SerializeField] private GameObject btnNextRound;

    [Header("배팅 UI")]
    [SerializeField] private GameObject bettingPanel; // 판넬
    [SerializeField] private TextMeshProUGUI txtCurrentBet; // 현재 배팅액

    [Header("게임 오버 UI")]
    [SerializeField] private GameObject gameOverPanel;

    // 내부 변수
    private int currentStartPlayerIndex = 0; // 누가 먼저 받나?

    // 각 자리의 카드 합계 점수 저장
    private int[] seatScores;
    private int dealerScore = 0;

    // 봇 정보 저장
    private List<GameManager.BotState> currentTableBots = new List<GameManager.BotState>();

    // 배팅, 턴 관리
    private long currentBet = 0;
    private int currentTurnIndex = -1; // 현재 누구 차례? (0~3:참가자, 4:딜러)
    private bool isMyTurn = false;

    void Start()
    {
        btnNextRound.SetActive(false);
        txtResult.text = "";
        SetButtonsActive(false);

        OpenBettingPhase();
    }

    private void SetupTable()
    {
        // 처음 시작시 3명뽑기
        if (currentTableBots.Count == 0)
        {
            currentTableBots = GameManager.Instance.GetRandomBots(3);
        }
        // 이미 자리가 찼다면?
        else
        {
            for (int i = 0; i < currentTableBots.Count; i++)
            {
                // 이 자리에 있는 봇이 파산?
                if (currentTableBots[i].isBankrupt)
                {
                    // 매니저에 1명 호출
                    GameManager.BotState newBot = GameManager.Instance.GetReplacementBot(currentTableBots);

                    if (newBot != null)
                    {
                        GameManager.Instance.ReviveBot(currentTableBots[i]); // 쫒겨나는 놈 리셋

                        currentTableBots[i] = newBot; // 봇 교체
                    }

                    else
                    {
                        // 버그로인한 모든 봇 파산상태시 즉시 리셋
                        GameManager.Instance.ReviveBot(currentTableBots[i]);
                    }
                }
            }
        }
        // Bot 1 (좌석 인덱스 0)
        UpdateSeatUI(0, currentTableBots[0].data.characterName, currentTableBots[0].currentMoney, currentTableBots[0].data.portrait);

        // Bot 2 (좌석 인덱스 1)
        UpdateSeatUI(1, currentTableBots[1].data.characterName, currentTableBots[1].currentMoney, currentTableBots[1].data.portrait);

        // 내 정보는 GameManager에서 직접 가져옴
        UpdateSeatUI(2, "Player", GameManager.Instance.GetGold(), null);

        // Bot 3 (좌석 인덱스 3)
        UpdateSeatUI(3, currentTableBots[2].data.characterName, currentTableBots[2].currentMoney, currentTableBots[2].data.portrait);
    }

    public void OpenBettingPhase()
    {
        // 수정 부분. 삭제 하려다가 혹시 모를 버그를 대비
        if (GameManager.Instance.GetGold() <= 0)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true); // 게임 오버 창 켜기
            if (bettingPanel != null) bettingPanel.SetActive(false);  // 배팅 창 끄기
            return; // 함수 종료
        }

        SetupTable();
        // 테이블 청소
        foreach (Transform seat in playerSeats) ClearHand(seat);
        ClearHand(dealerSeats);

        // 점수 텍스트 초기화
        foreach (var txt in scoreTexts) if (txt != null) txt.text = "";
        if (dealerScoreText != null) dealerScoreText.text = "";
        txtResult.text = "";
        btnNextRound.SetActive(false);

        // 배팅 초기화
        currentBet = 0;
        UpdateBetText();

        // 패널 켜기
        if (bettingPanel != null) bettingPanel.SetActive(true);
    }

    public void StartGame()
    {
        seatScores = new int[playerSeats.Count];

        // 점수 초기화
        for (int i = 0; i < seatScores.Length; i++)
        {
            seatScores[i] = 0;
        }

        dealerScore = 0;
        if (dealerScoreText != null) dealerScoreText.text = "0";

        // 덱 준비
        deckManager.PrepareDeck();

        //  현재 라운드에 맞춰 시작 순서 정하기
        int roundNum = deckManager.RoundCount;
        currentStartPlayerIndex = (roundNum - 1) % playerSeats.Count;

        // 카드 분배
        StartCoroutine(DealInitialCards());
    }

    // 코루틴 & 턴 관리
    IEnumerator DealInitialCards()
    {
        // 2바퀴
        for (int i = 0; i < 2; i++)
        {
            // 순서대로 카드 분배
            for (int j = 0; j < playerSeats.Count; j++)
            {
                // 로테이션 알고리즘: (시작인덱스 + j) % 4 
                int seatIndex = (currentStartPlayerIndex + j) % playerSeats.Count;

                // 플레이어인지 확인 (2번 인덱스가 Player라고 정함)
                bool isPlayer = (seatIndex == 2);

                DrawCardTo(playerSeats[seatIndex], isPlayer, seatIndex);

                yield return new WaitForSeconds(0.2f); // 딜레이 연출
            }

            // 마지막 딜러 카드 분배
            bool isDealerFaceUp = (i == 0); // 첫장만 앞면
            DrawCardToDealer(isDealerFaceUp);

            yield return new WaitForSeconds(0.2f);
        }
        StartTurn(currentStartPlayerIndex);
    }

    private void StartTurn(int seatIndex)
    {
        if (playerSeats.Count == 0) return; // 안전장치

        currentTurnIndex = seatIndex % playerSeats.Count;

        // 내 번호인지 확인 (플레이어는 항상 2번 자리)
        if (currentTurnIndex == 2)
        {
            isMyTurn = true;
            SetButtonsActive(true);
        }
        else
        {
            isMyTurn = false;
            SetButtonsActive(false);
            StartCoroutine(BotProcess(currentTurnIndex));
        }
    }

    public void NextTurn()
    {
        // playerSeats.Count 사용

        if (playerSeats.Count == 0) return;

        int nextIndex = (currentTurnIndex + 1) % playerSeats.Count;

        // 다음 사람이 '시작했던 사람'이라면? -> 한 바퀴 다 돎 -> 딜러 차례
        if (nextIndex == currentStartPlayerIndex)
        {
            StartCoroutine(DealerProcess());
        }
        else
        {
            StartTurn(nextIndex);
        }
    }

    IEnumerator BotProcess(int seatIndex)
    {
        yield return new WaitForSeconds(1.0f); // 딜레이 연출

        // 간단한 AI : 14점 미만이면 Hit, 아니면 Stand
        while (seatScores[seatIndex] < 14)
        {
            DrawCardTo(playerSeats[seatIndex], false, seatIndex);

            yield return new WaitForSeconds(1.0f);

            // 버스트 체크
            if (seatScores[seatIndex] > 21)
            {
                break; // 턴 종료
            }
        }
        NextTurn(); // 다음 사람에게 넘김
    }

    IEnumerator DealerProcess()
    {
        // 딜러 패에 있는 두 번째 카드를 찾아서 뒤집기
        Card hiddenCard = dealerSeats.GetChild(1).GetComponent<Card>();
        if (hiddenCard != null && !hiddenCard.IsFront)
        {
            hiddenCard.ShowFront(); // 뒤집기
        }
        UpdateDealerScore(); // 점수 갱신
        yield return new WaitForSeconds(1.0f);

        // 딜러 룰 : 16 이하면 무조건 Hit, 17 이상이면 Stand
        while (dealerScore <= 16)
        {
            DrawCardToDealer(true);
            yield return new WaitForSeconds(1.0f);
        }

        // 게임 종료 및 정산 (여기서 승패 판정)
        CalculateResult();
    }

    private void CalculateResult()
    {
        // 모든 자리를 순회하며 정산
        for (int i = 0; i < playerSeats.Count; i++)
        {
            int score = seatScores[i];
            long bet = currentBet;

            // 플레이어 (2번 자리)
            if (i == 2)
            {
                string resultMsg = "";

                // 플레이어 승패 판정
                if (score > 21) // 버스트
                {
                    resultMsg = "<color=red>BUST!</color>";
                    GameManager.Instance.ChangeGold(-bet);
                }
                else if (dealerScore > 21) // 딜러 버스트 -> 플레이어 승
                {
                    resultMsg = "<color=yellow>WIN!</color>";
                    GameManager.Instance.ChangeGold(bet);
                }
                else if (score > dealerScore) // 점수 승
                {
                    if (score == 21 && playerSeats[2].childCount == 2) // 블랙잭 (1.5배)
                    {
                        resultMsg = "<color=yellow>BLACKJACK!</color>";
                        GameManager.Instance.ChangeGold((long)(bet * 1.5f));
                    }
                    else
                    {
                        resultMsg = "<color=yellow>WIN!</color>";
                        GameManager.Instance.ChangeGold(bet);
                    }
                }
                else if (score < dealerScore) // 점수 패
                {
                    resultMsg = "<color=red>LOSE...</color>";
                    GameManager.Instance.ChangeGold(-bet);
                }
                else // 무승부
                {
                    resultMsg = "<color=white>PUSH</color>";
                }

                // 결과 텍스트 & 내 돈 UI 갱신
                txtResult.text = resultMsg;
                seatUIs[2].moneyText.text = $"${GameManager.Instance.GetGold()}";

                if (GameManager.Instance.GetGold() <= 0)
                {
                    if (gameOverPanel != null) gameOverPanel.SetActive(true); // 게임 오버 창 켜기
                    btnNextRound.SetActive(false); // 라운드 버튼은 숨김
                }
                else
                {
                    // 살아있을 때만 라운드 버튼 노출
                    btnNextRound.SetActive(true);
                }
            }
            // 봇 (0, 1, 3번 자리)
            else
            {
                // 봇 리스트에서의 인덱스 계산 (자리0->봇0, 자리1->봇1, 자리3->봇2)
                int botIndex = (i < 2) ? i : i - 1;

                // 안전장치
                if (botIndex >= currentTableBots.Count) continue;

                // 해당 봇 데이터 가져오기
                GameManager.BotState bot = currentTableBots[botIndex];

                // 봇 승패 판정
                if (score > 21) // 봇 버스트
                {
                    bot.currentMoney -= bet;
                }
                else if (dealerScore > 21) // 딜러 버스트
                {
                    bot.currentMoney += bet;
                }
                else if (score > dealerScore) // 점수 승리
                {
                    // 일단은 기본만큼 획득
                    bot.currentMoney += bet;
                }
                else if (score < dealerScore) // 점수 패배
                {
                    bot.currentMoney -= bet;
                }
                // 무승부시 변동 없음

                if (bot.currentMoney <= 0)
                {
                    GameManager.Instance.BankruptBot(bot.data);
                    bot.currentMoney = 0;
                }

                // 봇 UI 즉시 갱신
                UpdateSeatUI(i, bot.data.characterName, bot.currentMoney, bot.data.portrait);
            }
        }
    }

    private void UpdateSeatUI(int index, string name, long money, Sprite face)
    {
        if (index >= seatUIs.Count) return;

        seatUIs[index].nameText.text = name;
        seatUIs[index].moneyText.text = $"${money}";

        if (face != null && seatUIs[index].faceImage != null)
        {
            seatUIs[index].faceImage.sprite = face;
        }
    }

    private void UpdateBetText()
    {
        if (txtCurrentBet != null)
            txtCurrentBet.text = $"Bet: {currentBet} G";
    }

    private void ClearHand(Transform hand)
    {
        foreach (Transform child in hand) Destroy(child.gameObject);
    }

    private bool CheckAllCardsOpen(Transform hand)
    {
        foreach (Transform child in hand)
        {
            Card card = child.GetComponent<Card>();
            if (card != null && !card.IsFront) return false; // 하나라도 뒷면이면 false
        }
        return true;
    }

    private void SetButtonsActive(bool isActive)
    {
        // 버튼 오브젝트 자체를 끄고 켜기
        if (btnHit != null) btnHit.gameObject.SetActive(isActive);
        if (btnStand != null) btnStand.gameObject.SetActive(isActive);
    }

    private void DrawCardTo(Transform handPos, bool isPlayer, int seatIndex)
    {
        Card card = deckManager.DrawCard();
        if (card == null) return;

        card.gameObject.SetActive(true);
        card.transform.SetParent(handPos, false);

        // 카드 주인 설정
        card.SetOwner(isPlayer);

        // 플레이어면 뒷면, 봇이면 앞면
        if (isPlayer)
        {
            card.ShowBack();
            // 플레이어가 카드를 뒤집으면 점수 갱신
            card.OnFlipAction = () => UpdateScore(seatIndex);
        }
        else
        {
            card.ShowFront();
        }

        // 점수 갱신
        UpdateScore(seatIndex);
    }

    private void DrawCardToDealer(bool isFaceUp)
    {
        Card card = deckManager.DrawCard();
        if (card == null) return;

        card.gameObject.SetActive(true);
        card.transform.SetParent(dealerSeats, false);
        card.SetOwner(false); // 딜러는 플레이어가 아님

        if (isFaceUp) card.ShowFront();
        else card.ShowBack();

        UpdateDealerScore();
    }

    private void UpdateScore(int seatIndex)
    {
        Transform hand = playerSeats[seatIndex];
        int score = CalculateHandScore(hand);

        seatScores[seatIndex] = score; // 점수 저장
        scoreTexts[seatIndex].text = score.ToString(); // 텍스트 표시

        if (seatIndex == 2 && isMyTurn)
        {
            if (score > 21)
            {
                SetButtonsActive(false); // 버튼 끄기

                CancelInvoke("NextTurn");
                Invoke("NextTurn", 1.5f);
            }
            else if (score == 21)
            {
                SetButtonsActive(false);
                CancelInvoke("NextTurn");
                Invoke("NextTurn", 1.0f);
            }
        }
    }

    private void UpdateDealerScore()
    {
        int score = CalculateHandScore(dealerSeats);
        dealerScore = score;
        dealerScoreText.text = score.ToString();
    }

    // 점수 계산
    private int CalculateHandScore(Transform hand)
    {
        int total = 0;
        int aceCount = 0;

        // 카드들을 하나씩 검사
        foreach (Transform child in hand)
        {
            Card card = child.GetComponent<Card>();

            // 카드가 없거나, 뒷면이면 계산 안 함
            if (card == null || !card.IsFront) continue;

            int rank = card.Rank;

            if (rank >= 10) // J, Q, K는 10점
            {
                total += 10;
            }
            else if (rank == 1) // Ace는 일단 11점
            {
                total += 11;
                aceCount++;
            }
            else // 2~9는 그대로
            {
                total += rank;
            }
        }

        // Ace : 합이 21을 넘으면 Ace를 11 -> 1로 바꿈
        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    // OnClick

    public void OnClickChip(int amount)
    {
        long myMoney = GameManager.Instance.GetGold();

        // 돈이 부족하면 못 검
        if (currentBet + amount > myMoney)
        {
            return;
        }

        currentBet += amount;
        UpdateBetText();
    }

    public void OnClickAllIn()
    {
        long myMoney = GameManager.Instance.GetGold();
        currentBet = myMoney; // 올인
        UpdateBetText();
    }

    public void OnClickResetBet()
    {
        currentBet = 0;
        UpdateBetText();
    }

    public void OnClickDeal()
    {
        if (currentBet <= 0)
        {
            return; // 0원 배팅 방지
        }

        // 배팅 패널 끄고 게임 시작
        if (bettingPanel != null) bettingPanel.SetActive(false);
        StartGame();
    }

    public void OnClickHit()
    {
        if (!isMyTurn) return;

        // 카드 확인 -> 카드 받기
        if (!CheckAllCardsOpen(playerSeats[2]))
        {
            return;
        }

        DrawCardTo(playerSeats[2], true, 2);
    }

    public void OnClickStand()
    {
        if (!isMyTurn) return;

        SetButtonsActive(false);
        NextTurn();
    }

    public void OnClickNextRound()
    {
        OpenBettingPhase();
    }

    public void OnClickRetry()
    {
        GameManager.Instance.ChangeGold(10000); // 임시

        // 게임 오버 창 끄고 배팅 화면 열기
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        OpenBettingPhase();
    }
}