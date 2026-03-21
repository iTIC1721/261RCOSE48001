using System.Collections;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    private Coroutine destroyCoroutine = null;

    protected void Destroy()
    {
        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);

        if (MANAGER.Pool.m_poolDictionary.ContainsKey(gameObject.name))
        {
            MANAGER.Pool.m_poolDictionary[gameObject.name].Return(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void Destroy(float lifeTime)
    {
        destroyCoroutine = StartCoroutine(DestroyCoroutine(lifeTime));
    }

    private IEnumerator DestroyCoroutine(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy();
    }
}
