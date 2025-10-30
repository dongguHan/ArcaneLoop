using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private AttackPool attackPool;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private HandPunch handPunch;



    [Header("Dir-attack (pool)")]
    [SerializeField] private float triangleOffset = 1.1f;
    [SerializeField] private float squareOffset = 2.2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float comboResetTime = 0.8f;
    [SerializeField] private Color[] comboColors;

    private float lastAttackTime;
    private int comboStep = 0;
    public bool isAttacking = false;

    void Awake()
    {
        // 누락 대비 자동 연결
        if (!handPunch) handPunch = GetComponentInChildren<HandPunch>(true);
        if (!playerManager) playerManager = FindObjectOfType<PlayerManager>();
    }

    void Update()
    {


        if (playerManager.isTransform && !isAttacking)
        {
            Vector2 dir = Vector2.zero;

            if (playerManager.isBlack)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2.up;
                else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2.down;
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2.left;
                else if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2.right;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.W)) dir = Vector2.up;
                else if (Input.GetKeyDown(KeyCode.S)) dir = Vector2.down;
                else if (Input.GetKeyDown(KeyCode.A)) dir = Vector2.left;
                else if (Input.GetKeyDown(KeyCode.D)) dir = Vector2.right;
            }

            if (dir != Vector2.zero)
            {
                isAttacking = true;

                // 손을 그 방향으로 뻗기
                handPunch.PunchDir(dir);

                // 공격 중 플래그 해제 타이밍
                StartCoroutine(ResetFlagAfter(
                    handPunch.extendTime + handPunch.holdTime + handPunch.returnTime + 0.02f
                ));
            }
        }
    }


    private IEnumerator ResetFlagAfter(float t)
    {
        yield return new WaitForSeconds(t);
        isAttacking = false;
    }

    IEnumerator DoPunch()
    {
        isAttacking = true;

        if (!handPunch)
        {
            Debug.LogError("[PlayerAttack] HandPunch 참조가 비었습니다. Hand 오브젝트를 연결하세요.");
            yield return new WaitForSeconds(0.2f);
            isAttacking = false;
            yield break;
        }

        handPunch.Punch(); // HandPunch가 손 스프라이트를 켜고, localPosition으로 뻗었다 복귀합니다. :contentReference[oaicite:3]{index=3}
        float wait = handPunch.extendTime + handPunch.holdTime + handPunch.returnTime + 0.02f;
        yield return new WaitForSeconds(wait);

        isAttacking = false;
    }

    IEnumerator DoAttack(Vector2 dir)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (comboColors != null && comboColors.Length > 0)
            comboStep = (comboStep + 1) % comboColors.Length;
        else
            comboStep = 0;

        bool isTriangle = playerManager.isBlack;
        GameObject attackObj = attackPool.GetObject(isTriangle); // 풀에서 꺼냄 :contentReference[oaicite:4]{index=4}

        var sr = attackObj.GetComponent<SpriteRenderer>();
        if (sr && comboColors != null && comboColors.Length > 0)
            sr.color = comboColors[comboStep];

        Vector2 n = dir.normalized;
        float offset = isTriangle ? triangleOffset : squareOffset;

        float t = 0f;
        while (t < attackDuration)
        {
            Vector2 pos = (Vector2)transform.position + n * offset;
            attackObj.transform.position = pos;

            float angle = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg;
            attackObj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            t += Time.deltaTime;
            yield return null;
        }

        attackPool.ReturnObject(attackObj, isTriangle); // 풀에 반환 :contentReference[oaicite:5]{index=5}
        isAttacking = false;
    }
}
