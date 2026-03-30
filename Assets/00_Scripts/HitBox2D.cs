using System.Collections.Generic;
using UnityEngine;

public class HitBox2D : MonoBehaviour
{
    public enum ColliderState
    {
        Closed,
        Open,
        Colliding
    }

    public enum HitBoxType
    {
        Square,
        Circle
    }

    [Header("Setting")]
    public LayerMask mask;
    public HitBoxType type;
    [ShowIf("type", HitBoxType.Square)] public Vector2 hitboxSize = Vector2.one;
    [ShowIf("type", HitBoxType.Circle)] public float radius = 1;
    public Vector3 offset = Vector3.zero;
    public InterfaceReference<IEntity> parent;
    public float damage = 5;

    [Header("Colors")]
    public Color inactiveColor = Color.grey;
    public Color collisionOpenColor = Color.green;
    public Color collidingColor = Color.magenta;

    private DamageInfo damageInfo = new DamageInfo
    {
        damage = 0,
        damageSource = null
    };

    private ColliderState _state = ColliderState.Closed;

    private List<Collider2D> lastHitCollidersList;

    private void Awake()
    {
        Initialize(parent.Value);

        lastHitCollidersList = new List<Collider2D>();
    }

    public void Initialize(IEntity parent)
    {
        if (parent == null) return;

        this.parent.Value = parent;
        damageInfo.damage = damage;
        damageInfo.damageSource = this.parent.Value.Transform;
    }

    private void CheckGizmoColor()
    {
        switch (_state)
        {
            case ColliderState.Closed:
                Gizmos.color = inactiveColor;
                break;
            case ColliderState.Open:
                Gizmos.color = collisionOpenColor;
                break;
            case ColliderState.Colliding:
                Gizmos.color = collidingColor;
                break;
        }
    }

    public void StartCheckingCollision()
    {
        lastHitCollidersList.Clear();
        _state = ColliderState.Open;
    }

    public void StopCheckingCollision()
    {
        _state = ColliderState.Closed;
    }

    private void FixedUpdate()
    {
        if (_state == ColliderState.Closed)
            return;

        Vector2 size = new Vector2(
          hitboxSize.x * transform.lossyScale.x,
          hitboxSize.y * transform.lossyScale.y);

        Collider2D[] colls;
        switch (type)
        {
            case HitBoxType.Square:
                colls = Physics2D.OverlapBoxAll(transform.TransformPoint(offset), size * 0.5f, transform.rotation.eulerAngles.z, mask);
                break;
            case HitBoxType.Circle:
                colls = Physics2D.OverlapCircleAll(transform.TransformPoint(offset), radius, mask);
                break;
            default:
                return;
        }

        for (int i = 0; i < colls.Length; i++)
        {
            if (lastHitCollidersList.Contains(colls[i]))
                continue;

            colls[i].GetComponent<IEntity>().GetDamaged(damageInfo);
            Log.LogMessage($"Got Damage: {colls[i].name}");

            lastHitCollidersList.Add(colls[i]);
        }

        _state = colls.Length > 0 ? ColliderState.Colliding : ColliderState.Open;
    }

    private void OnDrawGizmos()
    {
        CheckGizmoColor();

        Gizmos.matrix = transform.localToWorldMatrix;
        switch (type)
        {
            case HitBoxType.Square:
                Gizmos.DrawCube(offset, hitboxSize);
                break;
            case HitBoxType.Circle:
                Gizmos.DrawSphere(offset, radius);
                break;
        }
    }
}
