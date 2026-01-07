using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("¿Ãµø«“ æ¿")]
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeScene(sceneName);
            }
        }
    }
}