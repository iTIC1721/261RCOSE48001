using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public void SpawnAttackObject()
    {
        player.SpawnAttackObject();
    }
}
