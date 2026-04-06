using UnityEngine;

public class GLOBAL_CANVAS : MonoBehaviour
{
    public static GLOBAL_CANVAS Instance { get; private set; }

    public static FadeManager Fade;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Fade = GetComponentInChildren<FadeManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
