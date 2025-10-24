using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // 싱글턴 안전 초기화
        if (Instance == null)
        {
            Instance = this;
            // 선택: 씬 전환 시에도 유지하려면 아래 주석 해제
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("BulletPool: Duplicate instance found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPool: bulletPrefab is not assigned in inspector.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetBullet()
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(bulletPrefab, transform);
            obj.SetActive(true);
        }
        return obj;
    }

    public void ReturnBullet(GameObject go)
    {
        if (go == null) return;

        // 안전하게 리셋
        go.SetActive(false);
        go.transform.SetParent(transform, true);
        // 위치 초기화(선택)
        // go.transform.position = transform.position;

        pool.Enqueue(go);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
