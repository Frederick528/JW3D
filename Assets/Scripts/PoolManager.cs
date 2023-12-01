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

        //// 미리 오브젝트 생성 해놓기
        //for (int i = 0; i < defaultCapacity; i++)
        //{
        //    Arrow arrow = CreatePooledItem().GetComponent<Arrow>();
        //    arrow.Pool.Release(arrow.gameObject);
        //}
    }

    // 생성
    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(arrowPrefab);
        poolGo.GetComponent<Arrow>().Pool = this.Pool;
        return poolGo;
    }

    // 사용
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
}
