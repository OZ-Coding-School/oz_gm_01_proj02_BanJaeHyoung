using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] private float moveSpeed = 5f;

    public float Speed => moveSpeed;

    void Start()
    {
        // 텍스트 찾기
        TextMeshProUGUI nameText = GetComponentInChildren<TextMeshProUGUI>();

        // 게임매니저에 저장된 이름 표ㅕ시
        if (nameText != null && GameManager.Instance != null)
        {
            nameText.text = GameManager.Instance.myName;
        }
    }

}