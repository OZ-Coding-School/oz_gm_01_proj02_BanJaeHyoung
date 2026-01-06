using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] private float moveSpeed = 5f;

    public float Speed => moveSpeed;

}