using UnityEngine;

public abstract class PlayerAttackObject : PoolObject
{
    [Min(0)] public float lifeTime = 10;
    [SerializeField] protected HitBox2D hitBox;

    protected bool isInitialized = false;

    public virtual void Initialize(float damage, IAttackable parent)
    {
        //transform.position = parent.Transform.position + Vector3.up * 0.2f;

        hitBox.Initialize(damage, parent);
        StartHitBox();

        Return(lifeTime, CallBack);

        isInitialized = true;
    }

    public abstract void StartHitBox();

    public virtual void CallBack(GameObject _)
    {
        hitBox.StopCheckingCollision();
        isInitialized = false;
    }
}
