using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : IPool
{
    public Transform ParentTransform { get; set; }
    public Queue<GameObject> Pool { get; set; } = new Queue<GameObject>();

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다.
    /// </summary>
    /// <param name="action">오브젝트 초기화 메서드</param>
    /// <returns>GameObject</returns>
    public GameObject Get(Action<GameObject> action = null)
    {
        GameObject obj = Pool.Dequeue();
        obj.SetActive(true);

        if (action != null)
        {
            action?.Invoke(obj);
        }

        return obj;
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다.
    /// </summary>
    /// <param name="position">오브젝트 초기 위치</param>
    /// <param name="action">오브젝트 초기화 메서드</param>
    /// <returns>GameObject</returns>
    public GameObject Get(Vector3 position, Action<GameObject> action = null)
    {
        GameObject obj = Pool.Dequeue();
        obj.transform.position = position;
        obj.SetActive(true);

        if (action != null)
        {
            action?.Invoke(obj);
        }

        return obj;
    }

    /// <summary>
    /// 풀에 오브젝트를 반환합니다.
    /// </summary>
    /// <param name="obj">반환할 오브젝트</param>
    /// <param name="action">반환 후 실행할 콜백</param>
    public void Return(GameObject obj, Action<GameObject> action = null)
    {
        Pool.Enqueue(obj);
        obj.transform.SetParent(ParentTransform);
        obj.SetActive(false);
        if (action != null)
        {
            action?.Invoke(obj);
        }
    }
}

public class PoolManager : MonoBehaviour
{
    [SerializeField] private Vector3 initialPosition = new Vector3(0, -50, 0);

    public Dictionary<string, IPool> m_poolDictionary = new Dictionary<string, IPool>();
    private Transform base_Obj = null;

    private void Start()
    {
        base_Obj = this.transform;
    }

    public IPool PoolingObj(string path)
    {
        if (!m_poolDictionary.ContainsKey(path))
        {
            AddPool(path);
        }

        if (m_poolDictionary[path].Pool.Count <= 0)
        {
            AddQueue(path);
        }

        return m_poolDictionary[path];
    }

    private GameObject AddPool(string key)
    {
        GameObject obj = new GameObject($"Pool@{key}");
        obj.transform.SetParent(base_Obj);

        ObjectPool T_Pool = new ObjectPool();
        m_poolDictionary.Add(key, T_Pool);

        T_Pool.ParentTransform = obj.transform;

        return obj;
    }

    private void AddQueue(string key)
    {
        var obj = Instantiate(Resources.Load<GameObject>($"Pool/{key}"), initialPosition, Quaternion.identity);
        obj.name = key;
        m_poolDictionary[key].Return(obj);
    }
}
