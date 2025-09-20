using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AttackPool : MonoBehaviour
{
    [SerializeField] private GameObject trianglePrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private int initialPoolSize = 10;

    private Queue<GameObject> trianglePool = new Queue<GameObject>();
    private Queue<GameObject> squarePool = new Queue<GameObject>();

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

    public GameObject GetObject(bool isTriangle)
    {
        Queue<GameObject> pool = isTriangle ? trianglePool : squarePool;
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀에 없으면 새로 생성 (필요하다면 제한 가능)
            var prefab = isTriangle ? trianglePrefab : squarePrefab;
            var obj = Instantiate(prefab, transform);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj, bool isTriangle)
    {
        obj.SetActive(false);
        (isTriangle ? trianglePool : squarePool).Enqueue(obj);
    }
}
