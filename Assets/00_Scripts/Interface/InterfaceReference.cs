using System;
using UnityEngine;

[Serializable]
public class InterfaceReference<I> where I : class
{
    [SerializeField] private UnityEngine.Object obj;

    public I Value
    {
        get => obj as I;
        set => obj = value as UnityEngine.Object;
    }
}
