using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance;

    [Header("References")]
    public GameObject inventoryUI;          // InventoryUI Canvas (비활성 상태)
    public PlayerMove[] playerMoves;        // 여러 플레이어의 PlayerMove 컴포넌트들을 관리
    public PlayerManager playerManager;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;

        // === 세 플레이어 모두 입력 비활성화 ===
        foreach (var move in playerMoves)
        {
            if (move != null)
                move.enabled = false;
        }
        if(playerManager != null)
            playerManager.enabled = false;

        if (inventoryUI != null)
            inventoryUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;

        // === 세 플레이어 모두 입력 활성화 ===
        foreach (var move in playerMoves)
        {
            if (move != null)
                move.enabled = true;
        }
        if (playerManager != null)
            playerManager.enabled = true;

        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool IsPaused => isPaused;
}
