using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("X/Y축 최대 이동 범위 (픽셀)")]
    public Vector2 moveRange = new Vector2(20f, 15f);

    [Tooltip("이동 속도 (값이 클수록 빠르게 움직임)")]
    public float moveSpeed = 0.8f;

    [Header("랜덤 오프셋")]
    [Tooltip("활성화 시 각 인스턴스마다 다른 위상으로 시작")]
    public bool randomizeOffset = true;

    private RectTransform _rect;
    private Vector2 _originPos;

    // Perlin Noise 샘플링을 위한 랜덤 오프셋 (인스턴스마다 다른 움직임)
    private float _seedX, _seedY, _seedRot, _seedScale;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect == null)
        {
            Log.LogWarning("[FloatingUI] RectTransform을 찾을 수 없습니다. UI 오브젝트에만 사용하세요.");
            enabled = false;
            return;
        }

        // 초기 Transform 저장
        _originPos = _rect.anchoredPosition;

        // 각 인스턴스가 다른 위상으로 시작하도록 랜덤 시드 설정
        if (randomizeOffset)
        {
            _seedX = Random.Range(0f, 100f);
            _seedY = Random.Range(0f, 100f);
            _seedRot = Random.Range(0f, 100f);
            _seedScale = Random.Range(0f, 100f);
        }
    }

    private void Update()
    {
        float t = Time.time;

        // Perlin Noise - [-1, 1] 범위로 정규화
        float nx = (Mathf.PerlinNoise(_seedX + t * moveSpeed, 0.5f) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(0.5f, _seedY + t * moveSpeed) - 0.5f) * 2f;

        _rect.anchoredPosition = _originPos + new Vector2(nx * moveRange.x, ny * moveRange.y);
    }
}
