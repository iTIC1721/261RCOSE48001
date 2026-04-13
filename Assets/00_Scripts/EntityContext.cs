using UnityEngine;

public class EntityContext
{
    public Entity source;           // 스킬을 사용하는 플레이어
    public float damage;            // 기본 데미지

    public Entity target;           // 피격 대상 (없을 수도 있음)
    public Vector2 targetPosition;  // 피격 위치

    public Vector2 direction;       // 공격 방향
}
