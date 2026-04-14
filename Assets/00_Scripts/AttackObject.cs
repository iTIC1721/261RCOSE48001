using UnityEngine;

public abstract class AttackObject : PoolObject
{
    public float lifeTime = 10;
    [SerializeField] protected HitBox2D hitBox;

    protected bool isInitialized = false;

    public virtual void Initialize(float damage, IAttackable parent)
    {
        hitBox.Initialize(damage, parent);
        StartHitBox();

        if (lifeTime > 0) Return(lifeTime, CallBack);

        isInitialized = true;
    }

    public abstract void StartHitBox();

    public virtual void CallBack(GameObject _)
    {
        hitBox.StopCheckingCollision();
        isInitialized = false;
    }
}
