using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    public int defaultCapacity = 10;
    public int maxPoolSize = 15;
    public GameObject arrowPrefab;

    public IObjectPool<GameObject> Pool { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        Init();
    }

    private void Init()
    {
        Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, defaultCapacity, maxPoolSize);

        //// �̸� ������Ʈ ���� �س���
        //for (int i = 0; i < defaultCapacity; i++)
        //{
        //    Arrow arrow = CreatePooledItem().GetComponent<Arrow>();
        //    arrow.Pool.Release(arrow.gameObject);
        //}
    }

    // ����
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(arrowPrefab);
        poolGo.GetComponent<Arrow>().Pool = this.Pool;
        return poolGo;
    }

    // ���
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // ��ȯ
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // ����
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
}
