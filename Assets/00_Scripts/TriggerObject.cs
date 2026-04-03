using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TriggerObject : MonoBehaviour
{
    [SerializeField] private bool isOneTime = true;
    public UnityEvent triggerEvent;

    private bool isTriggered = false;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();

        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerEvent?.Invoke();

            if (isOneTime) isTriggered = true;
        }
    }

    public void SetEvent(UnityAction action)
    {
        triggerEvent?.RemoveAllListeners();
        triggerEvent.AddListener(action);
    }
}
