using System.Collections;
using UnityEngine;

public class HandPunch : MonoBehaviour
{
    [Header("Refs")]
    public Transform shoulder;               // 기준(없으면 parent 사용)
    public SpriteRenderer parentSprite;      // 플레이어의 SpriteRenderer (flipX 읽음)
    public SpriteRenderer handSprite;        // 손 SpriteRenderer
    public Collider2D hitbox;                // (선택) 판정용 Collider2D (Trigger 권장)

    [Header("Idle (항상 보임)")]
    public Vector2 restLocalPos = new Vector2(0.30f, 0.0f); // 오른쪽에 살짝
    public bool idleBob = true;
    public float bobAmp = 0.02f;   // 상하 미세 떨림
    public float bobSpeed = 6f;

    [Header("Punch Motion (Local X축)")]
    public float extendDistance = 1.10f;   // 앞으로 뻗는 거리
    public float extendTime = 0.08f;       // 나가는 속도(빠르게)
    public float holdTime = 0.05f;         // 끝에서 잠깐 유지
    public float returnTime = 0.10f;       // 돌아오는 속도

    [Header("Curves")]
    public AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve easeIn = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Hit Window (progress)")]
    [Range(0, 1f)] public float hitStart = 0.35f;
    [Range(0, 1f)] public float hitEnd = 0.75f;

    bool busy;
    float bobT;

    void Awake()
    {
        // 참조 자동 보정
        if (!handSprite) handSprite = GetComponentInChildren<SpriteRenderer>();
        if (!hitbox) hitbox = GetComponent<Collider2D>();
        if (!parentSprite && transform.parent)
            parentSprite = transform.parent.GetComponentInChildren<SpriteRenderer>();
        if (!shoulder) shoulder = transform.parent;

        // 항상 보이게 시작
        if (handSprite) handSprite.enabled = true;
        if (hitbox) hitbox.enabled = false;

        // 첫 위치 정렬
        transform.localPosition = GetIdleLocalPos(0f);
    }

    void Update()
    {
        // 공격 중이 아닐 때는 플레이어 좌/우 방향에 맞춰
        // 항상 옆자리에 "자연스럽게" 붙어 있게 유지
        if (!busy)
        {
            bobT += Time.deltaTime * bobSpeed;
            transform.localPosition = GetIdleLocalPos(idleBob ? Mathf.Sin(bobT) : 0f);
        }
    }

    Vector2 GetIdleLocalPos(float bobSin)
    {
        // 부모의 바라보는 방향(오른쪽=+1, 왼쪽=-1)
        int face = (parentSprite && parentSprite.flipX) ? -1 : 1;

        // 좌우는 sign으로 반전, 위아래는 약간의 bob 부여
        var basePos = restLocalPos;
        basePos.x *= face;
        basePos.y += bobSin * bobAmp; // 미세 떨림

        return basePos;
    }

    public void Punch()
    {
        if (!busy) StartCoroutine(PunchRoutine());
    }

    public void PunchDir(Vector2 worldDir)
    {
        if (!gameObject.activeInHierarchy) return;
        if (!enabled) return;
        if (!busy) StartCoroutine(PunchDirRoutine(worldDir));
    }

    IEnumerator PunchRoutine()
    {
        busy = true;
        // 시작점/끝점 계산 (항상 현재 idle 위치 기준)
        Vector2 start = GetIdleLocalPos(idleBob ? Mathf.Sin(bobT) : 0f);
        int face = (parentSprite && parentSprite.flipX) ? -1 : 1;
        Vector2 end = start + new Vector2(face * extendDistance, 0f);

        // 나가기
        float t = 0f;
        if (hitbox) hitbox.enabled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, extendTime);
            float et = easeOut.Evaluate(Mathf.Clamp01(t));
            transform.localPosition = Vector2.Lerp(start, end, et);

            if (hitbox) hitbox.enabled = (et >= hitStart && et <= hitEnd);
            yield return null;
        }

        if (holdTime > 0f) yield return new WaitForSeconds(holdTime);

        // 돌아오기
        t = 0f;
        if (hitbox) hitbox.enabled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, returnTime);
            float et = easeIn.Evaluate(Mathf.Clamp01(t));
            transform.localPosition = Vector2.Lerp(end, start, et);
            yield return null;
        }

        // 복귀 후 idle 자리 고정
        transform.localPosition = GetIdleLocalPos(idleBob ? Mathf.Sin(bobT) : 0f);
        busy = false;
    }

    IEnumerator PunchDirRoutine(Vector2 worldDir)
    {
        busy = true;

        // 현재 idle 기준점
        Vector2 start = GetIdleLocalPos(idleBob ? Mathf.Sin(bobT) : 0f);

        // worldDir -> localDir (부모 공간 기준으로 변환)
        Vector2 localDir = worldDir.normalized;
        if (transform.parent != null)
            localDir = (Vector2)transform.parent.InverseTransformDirection(worldDir).normalized;

        Vector2 end = start + localDir * extendDistance;

        // (선택) 주먹 방향으로 회전시키고 싶다면:
        float ang = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, ang - 90f);

        // 나가기
        float t = 0f;
        if (hitbox) hitbox.enabled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, extendTime);
            float et = easeOut.Evaluate(Mathf.Clamp01(t));
            transform.localPosition = Vector2.Lerp(start, end, et);
            if (hitbox) hitbox.enabled = (et >= hitStart && et <= hitEnd);
            yield return null;
        }

        if (holdTime > 0f) yield return new WaitForSeconds(holdTime);

        // 돌아오기
        t = 0f;
        if (hitbox) hitbox.enabled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, returnTime);
            float et = easeIn.Evaluate(Mathf.Clamp01(t));
            transform.localPosition = Vector2.Lerp(end, start, et);
            yield return null;
        }

        // 복귀 후 idle 자리/회전으로
        transform.localPosition = GetIdleLocalPos(idleBob ? Mathf.Sin(bobT) : 0f);
        transform.localRotation = Quaternion.identity;
        busy = false;
    }
}
