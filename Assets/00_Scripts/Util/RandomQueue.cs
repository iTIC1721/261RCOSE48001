using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomQueue<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ICollection
{
    private readonly List<T> _list = new List<T>();
    private readonly System.Random _rng = new System.Random();

    private readonly object _syncRoot = new object();

    public void Enqueue(T item)
    {
        _list.Add(item);
    }

    public T Dequeue()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("RandomQueue is empty");

        int index = UnityEngine.Random.Range(0, _list.Count);

        T item = _list[index];

        int lastIndex = _list.Count - 1;
        _list[index] = _list[lastIndex];
        _list.RemoveAt(lastIndex);

        return item;
    }

    public T Peek()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("RandomQueue is empty");

        int index = UnityEngine.Random.Range(0, _list.Count);
        return _list[index];
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    // ------------------------------------

    public int Count => _list.Count;

    int IReadOnlyCollection<T>.Count => Count;

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (array.Rank != 1)
            throw new ArgumentException("Array must be one-dimensional");

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (array.Length - index < _list.Count)
            throw new ArgumentException("Not enough space");

        try
        {
            for (int i = 0; i < _list.Count; i++)
            {
                array.SetValue(_list[i], index + i);
            }
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("Invalid array type");
        }
    }

    public bool IsSynchronized => false;

    public object SyncRoot => _syncRoot;
}
