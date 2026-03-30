using System.Collections;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    private Coroutine returnCoroutine = null;

    protected void Return()
    {
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);

        if (MANAGER.Pool.m_poolDictionary.ContainsKey(gameObject.name))
        {
            MANAGER.Pool.m_poolDictionary[gameObject.name].Return(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void Return(float lifeTime)
    {
        returnCoroutine = StartCoroutine(ReturnCoroutine(lifeTime));
    }

    private IEnumerator ReturnCoroutine(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Return();
    }
}
