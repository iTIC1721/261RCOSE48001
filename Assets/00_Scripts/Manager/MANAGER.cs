using UnityEngine;

public class MANAGER : MonoBehaviour
{
    public static MANAGER Instance { get; private set; }

    public static StudyManager StudyManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            StudyManager = GetComponentInChildren<StudyManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
