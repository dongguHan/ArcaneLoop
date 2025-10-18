using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public PlayerManager playerManager;

    [Header("Camera Move Settings")]
    public float followSmoothTime = 0.3f;    // 카메라 이동 부드러움
    public float grayFollowDelay = 0.2f;     // Gray 상태일 때 추가 딜레이

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 5f;           // 거리 대비 줌 변화 민감도
    public float zoomSmoothSpeed = 5f;

    [Header("Camera Boundaries")]
    public Vector2 minBounds;                // 맵 왼쪽 아래 경계
    public Vector2 maxBounds;                // 맵 오른쪽 위 경계

    private Camera cam;
    private Vector3 currentVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (playerManager == null)
        {
            Debug.LogError("CameraFollow: PlayerManager reference is missing!");
        }
    }

    void LateUpdate()
    {
        if (playerManager == null)
            return;

        Vector3 targetPos = playerManager.GetCameraTargetPosition();
        Vector3 smoothPos;

        if (playerManager.IsGrayActive)
        {
            // Gray 상태: 약간의 딜레이로 부드럽게 따라감
            smoothPos = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y, transform.position.z),
                                           ref currentVelocity, followSmoothTime + grayFollowDelay);
        }
        else
        {
            // Black/White 상태: 빠르게 중앙 추적
            smoothPos = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y, transform.position.z),
                                           ref currentVelocity, followSmoothTime);
        }

        // 카메라 위치 제한 (맵 밖으로 안 나가게)
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampedX = Mathf.Clamp(smoothPos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(smoothPos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        AdjustZoom();
    }

    void AdjustZoom()
    {
        // Gray 상태일 땐 줌 고정
        if (playerManager.IsGrayActive)
        {
            float targetZoom = Mathf.Lerp(cam.orthographicSize, (minZoom + maxZoom) * 0.5f, Time.deltaTime * zoomSmoothSpeed);
            cam.orthographicSize = targetZoom;
            return;
        }

        else
        {
            // Black / White 거리 기반 줌
            float distance = Vector3.Distance(playerManager.playerBlack.transform.position,
                                              playerManager.playerWhite.transform.position);

            float targetZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothSpeed);
        }
    }
}
