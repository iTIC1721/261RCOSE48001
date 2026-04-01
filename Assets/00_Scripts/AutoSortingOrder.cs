using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class AutoSortingOrder : MonoBehaviour
{
    [SerializeField] bool isStatic = false;

    SortingGroup sortingGroup;

    private void Awake()
    {
        sortingGroup = GetComponentInChildren<SortingGroup>();
        if (sortingGroup == null)
        {
            sortingGroup = transform.AddComponent<SortingGroup>();
        }
    }

    private void Start()
    {
        SetSortingOrder();
    }

    private void LateUpdate()
    {
        if (!isStatic)
        {
            SetSortingOrder();
        }
    }

    public void SetSortingOrder()
    {
        sortingGroup.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
