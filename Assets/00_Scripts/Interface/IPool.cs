using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    Transform ParentTransform { get; set; }
    Queue<GameObject> Pool { get; set; }

    GameObject Get(Action<GameObject> action = null);
    GameObject Get(Vector3 position, Action<GameObject> action = null);

    void Return(GameObject obj, Action<GameObject> action = null);
}
