using UnityEngine;
using System.Collections.Generic;

public class WorldMapSpawner : MonoBehaviour
{
    [Header("캐릭터 프리팹")]
    public GameObject[] charPrefabs;

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
        GameObject myPlayer = CreatePlayer();

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
                myPlayer.transform.position = data.spawnPoint.position;
                return;
            }
        }
    }
    GameObject CreatePlayer()
    {
        int index = 0;
        // 게임매니저에서 가져옴
        if (GameManager.Instance != null)
        {
            index = GameManager.Instance.myCharIndex;
        }

        // 안전장치
        if (index < 0 || index >= charPrefabs.Length) index = 0;

        return Instantiate(charPrefabs[index], transform.position, Quaternion.identity);
    }
}