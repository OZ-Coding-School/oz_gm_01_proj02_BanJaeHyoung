using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private GameObject settingsPanel; // 설정 창

    [Header("씬 설정")]
    [SerializeField] private string gameSceneName = "SampleScene"; // 씬 이름

    void Start()
    {
        // 이어하기 버튼 활성화/비활성화
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.HasSaveData())
            {
                btnContinue.interactable = true; // 세이브 파일 있으면 켜기
            }
            else
            {
                btnContinue.interactable = false; // 없으면 끄기
            }
        }

        // 시작할 때 설정 창 끄기
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnClickNewGame()
    {
        // 새 게임
        GameManager.Instance.NewGame();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickContinue()
    {
        // 이어하기
        GameManager.Instance.LoadGame();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickSettings()
    {
        // 설정 창 켜기
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnClickCloseSettings()
    {
        // 설정 창 끄기
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OnClickQuit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}