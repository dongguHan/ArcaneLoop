using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button resumeButton;
    public Button quitButton;

    void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnDisable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // public으로 선언 (Inspector의 Button → On Click 에 연결 가능)
    public void OnResumeClicked()
    {
        GamePauseManager.Instance.ResumeGame();
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit button pressed. Implement scene change if needed.");
        Application.Quit();
    }
}
