using UnityEngine;
using System.Collections.Generic;

public class AttackPool : MonoBehaviour
{
    [SerializeField] private GameObject trianglePrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private int initialPoolSize = 2;

    private Queue<GameObject> trianglePool = new Queue<GameObject>();
    private Queue<GameObject> squarePool = new Queue<GameObject>();

    // === 추가: 공격 ID 생성기 ===
    private int nextAttackId = 0;

    void Awake()
    {
        InitPool(trianglePrefab, trianglePool);
        InitPool(squarePrefab, squarePool);
    }

    private void InitPool(GameObject prefab, Queue<GameObject> pool)
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 공격 오브젝트를 풀에서 가져와 활성화하고 고유 attackId 부여
    /// </summary>
    public GameObject GetObject(bool isTriangle)
    {
        Queue<GameObject> pool = isTriangle ? trianglePool : squarePool;
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            var prefab = isTriangle ? trianglePrefab : squarePrefab;
            obj = Instantiate(prefab, transform);
        }

        obj.SetActive(true);

        // === 공격 ID 부여 ===
        AttackObject attack = obj.GetComponent<AttackObject>();
        if (attack != null)
        {
            attack.attackId = nextAttackId++;
        }

        return obj;
    }

    public void ReturnObject(GameObject obj, bool isTriangle)
    {
        obj.SetActive(false);
        (isTriangle ? trianglePool : squarePool).Enqueue(obj);
    }
}
