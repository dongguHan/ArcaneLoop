using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public PlayerManager playerManager;

    [Header("Camera Move Settings")]
    public float followSmoothTime = 0.3f;    // ī�޶� �̵� �ε巯��
    public float grayFollowDelay = 0.2f;     // Gray ������ �� �߰� ������

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 5f;           // �Ÿ� ��� �� ��ȭ �ΰ���
    public float zoomSmoothSpeed = 5f;

    [Header("Camera Boundaries")]
    public Vector2 minBounds;                // �� ���� �Ʒ� ���
    public Vector2 maxBounds;                // �� ������ �� ���

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
            // Gray ����: �ణ�� �����̷� �ε巴�� ����
            smoothPos = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y, transform.position.z),
                                           ref currentVelocity, followSmoothTime + grayFollowDelay);
        }
        else
        {
            // Black/White ����: ������ �߾� ����
            smoothPos = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y, transform.position.z),
                                           ref currentVelocity, followSmoothTime);
        }

        // ī�޶� ��ġ ���� (�� ������ �� ������)
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampedX = Mathf.Clamp(smoothPos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(smoothPos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        AdjustZoom();
    }

    void AdjustZoom()
    {
        // Gray ������ �� �� ����
        if (playerManager.IsGrayActive)
        {
            float targetZoom = Mathf.Lerp(cam.orthographicSize, (minZoom + maxZoom) * 0.5f, Time.deltaTime * zoomSmoothSpeed);
            cam.orthographicSize = targetZoom;
            return;
        }

        else
        {
            // Black / White �Ÿ� ��� ��
            float distance = Vector3.Distance(playerManager.playerBlack.transform.position,
                                              playerManager.playerWhite.transform.position);

            float targetZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothSpeed);
        }
    }
}
