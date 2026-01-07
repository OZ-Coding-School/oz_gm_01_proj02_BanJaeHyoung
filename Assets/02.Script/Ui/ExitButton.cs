using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();

        // 버튼에 기능 연결
        btn.onClick.AddListener(OnClickExit);
    }

    private void OnClickExit()
    {
        // 살아있는 게임 매니저를 찾아서 씬 이동 요청
        if (GameManager.Instance != null)
        {
            // "WorldMap"으로 돌아가기
            GameManager.Instance.ChangeScene("WorldMap");
        }
    }
}