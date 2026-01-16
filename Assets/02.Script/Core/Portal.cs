using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("이동할 씬")]
    [SerializeField] private string sceneName;

    [Header("문 연출 설정")]
    [SerializeField] private Sprite openDoorSprite; // 열린 문
    [SerializeField] private float delayTime = 0.5f; // 지연시간

    private bool isEntering = false; // 중복 실행 방지용
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // 이미지 컴포넌트를 가져옴
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player
        if (collision.CompareTag("Player") && !isEntering)
        {
            StartCoroutine(ProcessEnterPortal(collision.gameObject));
        }
    }

    IEnumerator ProcessEnterPortal(GameObject player)
    {
        isEntering = true;

        // 키보드 입력 끄기
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        // 미끄러짐 방지
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; // 물리 연산 끄기
        }

        // 문 이미지 바꾸기
        if (spriteRenderer != null && openDoorSprite != null)
        {
            spriteRenderer.sprite = openDoorSprite;
        }

        // 대기
        yield return new WaitForSeconds(delayTime);

        // 씬 이동
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScene(sceneName);
        }
    }
}