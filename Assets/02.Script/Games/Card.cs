using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler
{
    [Header("설정")]
    [SerializeField] private Image cardImage;

    private Sprite frontSprite; // 앞면 그림
    private Sprite backSprite;  // 뒷면 그림
    private bool isPlayerCard = false; // 플레이어?

    public System.Action OnFlipAction;

    // 초기 카드 정보
    public void Setup(Sprite newSprite, int newRank, string newSuit, Sprite newBackSprite)
    {
        this.frontSprite = newSprite;
        this.Rank = newRank;
        this.Suit = newSuit;
        this.backSprite = newBackSprite;

        // 기본
        ShowFront();
    }

    // 플레이어?
    public void SetOwner(bool isPlayer)
    {
        this.isPlayerCard = isPlayer;
    }

    // 앞면 보여주기
    public void ShowFront()
    {
        IsFront = true;
        cardImage.sprite = frontSprite;
    }

    // 뒷면 보여주기
    public void ShowBack()
    {
        IsFront = false;
        cardImage.sprite = backSprite;
    }

    // 뒤집기 (앞면이면 뒷면으로, 뒷면이면 앞면으로)
    public void Flip()
    {
        if (IsFront) ShowBack();
        else ShowFront();

        if (OnFlipAction != null) OnFlipAction.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 플레이어 카드, 현재 뒷면일 때만 뒤집기 허용
        if (isPlayerCard && !IsFront)
        {
            Flip();
        }
    }

    // 외부 읽기용
    public int Rank { get; private set; } // 숫자 (1~13)

    public string Suit { get; private set; } // 문양

    public bool IsFront { get; private set; } = true;

}