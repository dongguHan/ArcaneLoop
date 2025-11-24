using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private AttackPool attackPool;
    [SerializeField] private PlayerManager playerManager;

    [SerializeField] private float triangleOffset = 1.1f;
    [SerializeField] private float squareOffset = 2.2f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float comboResetTime = 0.8f;

    [SerializeField] private Color[] comboColors;

    private float lastAttackTime;
    private int comboStep = 0;
    public bool isAttacking = false;

    void Update()
    {
        if (playerManager.isTransform && !isAttacking)
        {
            Vector2 dir = Vector2.zero;
            if (playerManager.isBlack)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2.up;
                if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2.down;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2.left;
                if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2.right;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.W)) dir = Vector2.up;
                if (Input.GetKeyDown(KeyCode.S)) dir = Vector2.down;
                if (Input.GetKeyDown(KeyCode.A)) dir = Vector2.left;
                if (Input.GetKeyDown(KeyCode.D)) dir = Vector2.right;
            }

            if (dir != Vector2.zero)
                StartCoroutine(DoAttack(dir));

            if (Time.time - lastAttackTime > comboResetTime)
                comboStep = 0;
        }
    }

    IEnumerator DoAttack(Vector2 dir)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (comboColors.Length > 0)
            comboStep = (comboStep + 1) % comboColors.Length;
        else
            comboStep = 0;

        bool isTriangle = playerManager.isBlack;
        GameObject attackObj = attackPool.GetObject(isTriangle);

        var sr = attackObj.GetComponent<SpriteRenderer>();
        if (sr != null && comboColors.Length > 0)
            sr.color = comboColors[comboStep];

        Vector2 normalizedDir = dir.normalized;
        float offset = isTriangle ? triangleOffset : squareOffset;

        if(playerManager.isBlack && attackObj.GetComponent<AttackObject>() != null)
        {
            // 공격 위치(실제 타격지점) 계산
            Vector2 attackPos = (Vector2)transform.position + normalizedDir * offset;

            attackObj.GetComponent<AttackObject>().BreakTiles(attackPos, normalizedDir);
        }

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            Vector2 spawnPos = (Vector2)transform.position + normalizedDir * offset;
            attackObj.transform.position = spawnPos;

            float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
            attackObj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        attackPool.ReturnObject(attackObj, isTriangle);
        isAttacking = false;
    }
}
