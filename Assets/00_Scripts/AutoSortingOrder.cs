using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class AutoSortingOrder : MonoBehaviour
{
    [SerializeField] bool isStatic = false;

    [SerializeField] bool useSortingGroup = false;
    [SerializeField] int sortingLayerID = -1;

    SortingGroup sortingGroup;
    SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        SetReference(gameObject);
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

    public void SetReference(GameObject parent)
    {
        if (useSortingGroup)
        {
            sortingGroup = parent.GetComponentInChildren<SortingGroup>();
            if (sortingGroup == null)
            {
                sortingGroup = parent.transform.AddComponent<SortingGroup>();
            }
            if (UsingSortingLayerID()) sortingGroup.sortingLayerID = sortingLayerID;
        }
        else
        {
            spriteRenderers = parent.GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void SetSortingOrder()
    {
        int order = Mathf.RoundToInt(-transform.position.y * 100);

        if (useSortingGroup)
        {
            sortingGroup.sortingOrder = order;
        }
        else
        {
            foreach (var renderer in spriteRenderers)
            {
                if (UsingSortingLayerID() && renderer.sortingLayerID != sortingLayerID) continue;

                renderer.sortingOrder = order;
            }
        }
    }

    private bool UsingSortingLayerID()
    {
        if (sortingLayerID >= 0) return true;
        return false;
    }
}
