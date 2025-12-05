using UnityEngine;

public class SpriteBobLocal : MonoBehaviour
{
    public float amplitude = 0.1f;   // Vertical movement range (local space)
    public float speed = 2.0f;       // Bobbing speed

    private Vector3 _startLocalPos;

    void Start()
    {
        // Save initial local position relative to parent
        _startLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        // Sine wave based offset
        float offsetY = Mathf.Sin(Time.time * speed) * amplitude;

        // Apply local offset only on Y
        transform.localPosition = _startLocalPos + new Vector3(0f, offsetY, 0f);
    }
}
