using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackjackManager : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private DeckManager deckManager;

    [Header("참가자 카드 위치")]
    // 0:Bot1, 1:Bot2, 2:Player, 3:Bot3
    [SerializeField] private List<Transform> playerSeats;

    [Header("딜러 위치")]
    [SerializeField] private Transform dealerHandPos;

    // 내부 변수
    private int currentStartPlayerIndex = 0; // 누가 먼저 받나?

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        // 덱 준비 (3판마다 셔플 체크)
        deckManager.PrepareDeck();

        //  현재 라운드에 맞춰 시작 순서 정하기
        int roundNum = deckManager.RoundCount;
        currentStartPlayerIndex = (roundNum - 1) % playerSeats.Count;


        // 카드 분배
        StartCoroutine(DealInitialCards());
    }

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

                Transform targetHand = playerSeats[seatIndex];

                // 플레이어인지 확인 (2번 인덱스가 Player라고 정함)
                DrawCardTo(targetHand);

                yield return new WaitForSeconds(0.2f); // 딜레이 연출
            }

            // 마지막 딜러 카드 분배
            DrawCardTo(dealerHandPos);
            yield return new WaitForSeconds(0.2f);
        }
    }

    // 카드를 생성해서 위치로 보내는 함수
    private void DrawCardTo(Transform handPos)
    {
        Card card = deckManager.DrawCard();

        if (card != null)
        {
            card.gameObject.SetActive(true);
            card.transform.SetParent(handPos, false);
            card.ShowFront();
        }
    }
}