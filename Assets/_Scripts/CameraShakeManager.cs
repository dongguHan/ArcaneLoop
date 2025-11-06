using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    Vector3 basePos;
    Vector3 shakeOffset;

    void Awake()
    {
        Instance = this;
    }

    public void SetBasePos(Vector3 pos)
    {
        basePos = pos;
        Apply();
    }

    void Apply()
    {
        transform.position = basePos + shakeOffset;
    }

    public void Shake(float magnitude, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            shakeOffset = (Vector2)Random.insideUnitCircle * magnitude;
            Apply();
            time += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        Apply();
    }
}
