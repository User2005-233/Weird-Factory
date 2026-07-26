using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> _pool = new Queue<T>();
    private readonly T _prefab;
    private readonly Transform _parent;

    public int ActiveCount { get; private set; }
    public int InactiveCount => _pool.Count;
    public int TotalCount => ActiveCount + InactiveCount;

    public ObjectPool(T prefab, Transform parent = null, int prewarm = 0)
    {
        _prefab = prefab;
        _parent = parent;
        Prewarm(prewarm);
    }

    public T Get()
    {
        T obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _parent);
        }

        obj.gameObject.SetActive(true);
        ActiveCount++;
        return obj;
    }

    public T Get(Transform parent, bool worldPositionStays = true)
    {
        var obj = Get();
        obj.transform.SetParent(parent, worldPositionStays);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        var obj = Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation, Transform parent)
    {
        var obj = Get();
        obj.transform.SetParent(parent, false);
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null)
            return;

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_parent);
        _pool.Enqueue(obj);
        ActiveCount--;
    }

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public void ClearPooled()
    {
        while (_pool.Count > 0)
        {
            Object.Destroy(_pool.Dequeue().gameObject);
        }
    }
}
