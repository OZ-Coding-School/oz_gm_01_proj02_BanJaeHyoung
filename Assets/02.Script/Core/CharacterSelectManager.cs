using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("데이터 연결")]
    public GameObject[] charPrefabs; // 캐릭터 프리팹들
    public Sprite[] faceIcons;       // 얼굴 아이콘들

    [Header("화면 배치 위치, 캐릭터 크기")]
    public Transform spawnPoint;     // 캐릭터가 서 있을 위치
    public Image displayFace;        // 얼굴 보여줄 이미지
    public TMP_InputField inputName; // 이름 입력칸
    public float characterScale = 100f;

    private int currentIndex = 0;    // 현재 몇 번 캐릭터인지 기억
    private GameObject currentModel; // 지금 화면에 나와있는 캐릭터

    void Start()
    {
        // 시작하면 0번 캐릭터 보여주기
        UpdateCharacter(0);
    }

    // 다음 버튼
    public void OnClickNext()
    {
        currentIndex++;
        if (currentIndex >= charPrefabs.Length) currentIndex = 0; // 마지막 다음은 처음으로
        UpdateCharacter(currentIndex);
    }

    // 이전 버튼
    public void OnClickPrev()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = charPrefabs.Length - 1; // 처음 이전은 마지막으로
        UpdateCharacter(currentIndex);
    }

    // 화면 갱신
    void UpdateCharacter(int index)
    {
        // 기존 삭제
        if (currentModel != null) Destroy(currentModel);

        // 생성
        if (index < charPrefabs.Length)
        {
            currentModel = Instantiate(charPrefabs[index], spawnPoint.position, spawnPoint.rotation);
            currentModel.transform.localScale = Vector3.one * characterScale;

            // 움직임 끄기
            PlayerController controller = currentModel.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;

            Rigidbody2D rb = currentModel.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            // 이름표 끄기
            Canvas nameCanvas = currentModel.GetComponentInChildren<Canvas>();
            if (nameCanvas != null)
            {
                nameCanvas.gameObject.SetActive(false); // 캔버스 끄기
            }
        }

        // 얼굴 아이콘 변경
        if (index < faceIcons.Length && displayFace != null)
        {
            displayFace.sprite = faceIcons[index];
        }
    }

    public void OnClickStartGame()
    {
        // 이름 저장
        if (inputName != null && inputName.text.Length > 0)
        {
            GameManager.Instance.myName = inputName.text;
        }
        else
        {
            GameManager.Instance.myName = "Guest";
        }

        // 캐릭터 번호 저장
        GameManager.Instance.myCharIndex = currentIndex;

        // 월드맵 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
    }
}