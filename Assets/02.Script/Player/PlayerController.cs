using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private VirtualJoystick joyStick;

    [Header("컴포넌트")]
    private Rigidbody2D rb;
    private SpriteRenderer spriter;
    private Animator anim;

    // 이동 입력값 저장
    private Vector2 inputVec;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (stats == null) stats = GetComponent<PlayerStats>();
        if (joyStick == null) joyStick = FindObjectOfType<VirtualJoystick>();
    }

    void Update()
    {
        // 키보드 입력 대각선 이동 시 속도가 2배가 되지 않도록 normalized 처리
        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        inputVec = keyboardInput.normalized;

        // 키보드 입력이 없을 때만 조이스틱 입력
        if (joyStick != null && inputVec == Vector2.zero)
        {
            if (joyStick.InputVector != Vector2.zero)
            {
                inputVec = joyStick.InputVector;
            }
        }

        // 캐릭터 좌우 반전
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }

        // 애니메이션
    }

    void FixedUpdate()
    {
        float currentSpeed = (stats != null) ? stats.Speed : 5f;

        Vector2 nextVec = inputVec * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + nextVec);
    }
}