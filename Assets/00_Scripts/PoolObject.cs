using System;
using System.Collections;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    private Coroutine returnCoroutine = null;

    protected void Return(Action<GameObject> callback = null)
    {
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);

        if (MANAGER.Pool.m_poolDictionary.ContainsKey(gameObject.name))
        {
            MANAGER.Pool.m_poolDictionary[gameObject.name].Return(this.gameObject, callback);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void Return(float lifeTime, Action<GameObject> callback = null)
    {
        returnCoroutine = StartCoroutine(ReturnCoroutine(lifeTime));
    }

    private IEnumerator ReturnCoroutine(float lifeTime, Action<GameObject> callback = null)
    {
        yield return new WaitForSeconds(lifeTime);
        Return(callback);
    }
}
