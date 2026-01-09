using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform cardSpawnPos; // 카드가 생성될 위치

    [Header("이미지")]
    [SerializeField] private Sprite cardBackSprite;  // 카드 뒷면 이미지
    [SerializeField] private List<Sprite> allCardSprites; // 카드 앞면 이미지 52장

    // 카드들의 목록
    public List<Card> MyDeck { get; private set; } = new List<Card>();

    public int RoundCount { get; private set; } = 0;

    void Awake()
    {
        CreateDeck();
    }

    // 덱 생성 함수
    private void CreateDeck()
    {
        // 카드 생성
        foreach (Sprite sprite in allCardSprites)
        {
            // 파일 이름
            string fileName = sprite.name;

            // "card" 라는 글자 제거
            string tempName = fileName.Replace("card", "");

            // 문양 찾기
            string suitStr = "";
            if (tempName.StartsWith("Clubs")) suitStr = "Clubs";
            else if (tempName.StartsWith("Diamonds")) suitStr = "Diamonds";
            else if (tempName.StartsWith("Hearts")) suitStr = "Hearts";
            else if (tempName.StartsWith("Spades")) suitStr = "Spades";

            // 문양이 없으면 건너뜀
            if (suitStr == "") continue;

            // 숫자 찾기
            string rankStr = tempName.Replace(suitStr, "");

            // 숫자로 변환 (A->1, K->13 등등)
            int rank = GetRankFromString(rankStr);

            // 카드 생성
            Card newCard = Instantiate(cardPrefab, cardSpawnPos);
            newCard.name = $"Card_{suitStr}_{rankStr}"; // 하이어라키 언더바 넣어서 정리

            newCard.Setup(sprite, rank, suitStr, cardBackSprite);
            newCard.gameObject.SetActive(false);

            MyDeck.Add(newCard);
        }
    }

    // 영문자에서 숫자로 체인지
    private int GetRankFromString(string rankStr)
    {
        switch (rankStr)
        {
            case "A": return 1;  // 블랙잭 점수 계산은 나중에
            case "J": return 11; 
            case "Q": return 12;
            case "K": return 13;
            default:
                // 숫자 문자열을 실제 숫자로 변환
                return int.Parse(rankStr);
        }
    }

    public void PrepareDeck()
    {
        // 3판후 리셋
        RoundCount++;

        bool needReset = (RoundCount > 1 && (RoundCount - 1) % 3 == 0);

        if (needReset || RoundCount == 1)
        {
            if (needReset)
            {
                ResetDeck();
            }

            // 1라운드이거나 리셋 직후에는 셔플
            Shuffle();
        }
    }

    // 셔플
    public void Shuffle()
    {
        // Fisher-Yates
        for (int i = MyDeck.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);

            Card temp = MyDeck[i];
            MyDeck[i] = MyDeck[rnd];
            MyDeck[rnd] = temp;
        }

    }

    // 카드 한 장 뽑기
    public Card DrawCard()
    {
        if (MyDeck.Count == 0)
        {
            return null;
        }

        Card card = MyDeck[0];
        MyDeck.RemoveAt(0);

        return card;
    }

    // 덱 리셋
    public void ResetDeck()
    {
        foreach (Transform child in cardSpawnPos)
        {
            Destroy(child.gameObject);
        }
        MyDeck.Clear();
        CreateDeck(); // 다시 생성
    }
}