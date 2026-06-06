using UnityEngine;

public class CameraResolutionFitter : MonoBehaviour
{
    [SerializeField] private float referenceHeight = 1920f; // 기준 세로 해상도
    [SerializeField] private float referenceOrthographicSize = 5f; // 기준 ortho size

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        float currentHeight = Screen.height;
        float scaleFactor = currentHeight / referenceHeight;
        cam.orthographicSize = referenceOrthographicSize * scaleFactor;
    }
}