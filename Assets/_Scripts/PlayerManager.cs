using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerBlack;
    public GameObject playerWhite;
    public GameObject playerGray;

    public float minMoveTime = 0.1f;
    public float maxMoveTime = 0.7f;

    public bool isBlack = false;
    public bool isTransform = false;

    private void Start()
    {
        playerBlack.SetActive(true);
        playerWhite.SetActive(true);
        playerGray.SetActive(false);
    }

    private void Update()
    {
        HandleTransformInput();
    }

    private void HandleTransformInput()
    {
        if (!playerGray.activeSelf)
        {
            if (Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                StartCoroutine(MoveAndActivateGray(playerWhite, playerBlack.transform.position));
                isBlack = true;
            }
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                StartCoroutine(MoveAndActivateGray(playerBlack, playerWhite.transform.position));
                isBlack = false;
            }
        }
        else
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                DeactivateGray();
            }
        }
    }

    private IEnumerator MoveAndActivateGray(GameObject mover, Vector3 targetPosition)
    {
        isTransform = true;
        mover.SetActive(true);

        Vector3 start = mover.transform.position;
        float distance = Vector3.Distance(start, targetPosition);
        float maxDistance = Vector3.Distance(Vector3.zero, new Vector3(1920f, 1080f, 0f)); // 최대 화면 대각 거리
        float moveTime = Mathf.Lerp(minMoveTime, maxMoveTime, distance / maxDistance);

        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            mover.transform.position = Vector3.Lerp(start, targetPosition, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mover.transform.position = targetPosition;

        // 위치가 같아지면 Gray 활성화
        playerGray.transform.position = targetPosition;
        playerGray.SetActive(true);

        // Black/White 비활성화
        playerBlack.SetActive(false);
        playerWhite.SetActive(false);

        // 이동 역할만 한 오브젝트는 비활성화
        mover.SetActive(false);
    }

    private void DeactivateGray()
    {
        isTransform = false;
        Vector3 pos = playerGray.transform.position;

        playerGray.SetActive(false);

        playerBlack.transform.position = pos;
        playerWhite.transform.position = pos;

        playerBlack.SetActive(true);
        playerWhite.SetActive(true);
    }
}
