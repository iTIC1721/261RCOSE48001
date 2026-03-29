using UnityEngine;

public class BaseCanvas : MonoBehaviour
{
    public static BaseCanvas Instance {  get; private set; }

    public Transform damageLayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
