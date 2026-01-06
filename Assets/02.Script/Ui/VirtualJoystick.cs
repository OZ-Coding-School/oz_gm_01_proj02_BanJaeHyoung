using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform containerRect; // 조이스틱이 돌아다닐 패널
    [SerializeField] private RectTransform bgRect; // 조이스틱 배경 원
    [SerializeField] private RectTransform handleRect; // 조이스틱 손잡이 원

    [Header("설정")]
    [SerializeField] private float joystickRadius = 100f; // 손잡이가 움직이는 반경

    private Vector2 inputVector;

    // 외부 접근용
    public Vector2 InputVector => inputVector;

    // 혹시 모를
    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;

    void Start()
    {
        // 시작할 땐 조이스틱 숨기기
        if (bgRect != null) bgRect.gameObject.SetActive(false);
        inputVector = Vector2.zero;
    }

    // 화면을 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        // 조이스틱 배경 보이게 하기
        bgRect.gameObject.SetActive(true);

        // 터치한 위치로 조이스틱 배경 이동
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            bgRect.anchoredPosition = localPoint;
        }

        // 손잡이는 중앙 정렬
        handleRect.anchoredPosition = Vector2.zero;

        OnDrag(eventData);
    }

    // 드래그 중일 때
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;

        // 배경(bgRect) 기준으로 터치 위치 계산
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // 이동 반경 제한
            localPoint = Vector2.ClampMagnitude(localPoint, joystickRadius);

            // 방향 벡터 계산
            inputVector = localPoint / joystickRadius;

            handleRect.anchoredPosition = localPoint;
        }
    }

    // 손을 뗐을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        // 조이스틱 다시 숨기기
        bgRect.gameObject.SetActive(false);

        inputVector = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;
    }
}