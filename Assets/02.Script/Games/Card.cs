using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Image cardImage;

    private Sprite frontSprite; // 앞면 그림
    private Sprite backSprite;  // 뒷면 그림
    private bool isFront = true; // 현재 앞면인지 확인

    // 초기 카드 정보
    public void Setup(Sprite newSprite, int newRank, string newSuit, Sprite newBackSprite)
    {
        this.frontSprite = newSprite;
        this.Rank = newRank;
        this.Suit = newSuit;
        this.backSprite = newBackSprite;

        // 일단 앞면으로 시작
        ShowFront();
    }

    // 앞면 보여주기
    public void ShowFront()
    {
        isFront = true;
        cardImage.sprite = frontSprite;
    }

    // 뒷면 보여주기
    public void ShowBack()
    {
        isFront = false;
        cardImage.sprite = backSprite;
    }

    // 뒤집기 (앞면이면 뒷면으로, 뒷면이면 앞면으로)
    public void Flip()
    {
        if (isFront) ShowBack();
        else ShowFront();
    }

    // 외부 읽기용
    public int Rank { get; private set; } // 숫자 (1~13)
    public string Suit { get; private set; } // 문양

}