using UnityEngine;
using System.Collections.Generic;

public class WorldMapSpawner : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Transform playerTransform;

    // 어떤 씬 -> 어디로 보낼지 짝을 지어주는 데이터
    [System.Serializable]
    public struct SpawnData
    {
        public string fromSceneName; // 예: Game_Blackjack
        public Transform spawnPoint; // 예: Point_Blackjack
    }

    [Header("스폰 위치 설정")]
    [SerializeField] private List<SpawnData> spawnList;

    private void Start()
    {
        // 게임 매니저가 없으면 무시
        if (GameManager.Instance == null) return;

        // 매니저 호ㅓ출
        string prevScene = GameManager.Instance.PrevSceneName;

        // 리스트안에서 맞는 장소 찾기
        foreach (SpawnData data in spawnList)
        {
            if (data.fromSceneName == prevScene)
            {
                // 위치 이동
                playerTransform.position = data.spawnPoint.position;
                return;
            }
        }
    }
}