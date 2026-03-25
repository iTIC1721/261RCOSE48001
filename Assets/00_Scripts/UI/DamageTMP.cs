using System.Collections;
using TMPro;
using UnityEngine;

public class DamageTMP : PoolObject
{
    private TextMeshProUGUI m_Text;
    private RectTransform rectTransform;

    private Transform target;
    private Vector3 targetPosition;
    private Vector3 offset;

    [SerializeField] private float gravity = 200f;
    [SerializeField] private float lifeTime = 1.0f;
    [SerializeField] private Vector2 randomRangeX = new Vector2(-25, 25);
    [SerializeField] private Vector2 randomRangeY = new Vector2(-25, -50);

    private Color textColor;

    private Vector2 velocity;   // 초기 속도 (포물선 운동)
    private Vector2 displacement;

    private void Awake()
    {
        m_Text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform parent, Transform target, Vector3 offset, float value, Color color)
    {
        transform.SetParent(parent);
        this.target = target;
        this.offset = offset;

        // 데미지 소수점 아래는 버림하여 표기
        m_Text.text = Mathf.FloorToInt(value).ToString();

        // 초기화
        velocity = new Vector2(Random.Range(randomRangeX.x, randomRangeX.y), Random.Range(randomRangeY.x, randomRangeY.y));
        displacement = Vector2.zero;
        textColor = color;
        StartCoroutine(MoveAndFade());
    }

    IEnumerator MoveAndFade()
    {
        float elapsedTime = 0f;

        while (elapsedTime < lifeTime)
        {
            velocity.y += gravity * Time.deltaTime;
            displacement += velocity * Time.deltaTime;

            MovePosition();

            float t = elapsedTime / lifeTime;
            textColor.a = Mathf.Lerp(1.0f, 0.0f, t);
            m_Text.color = textColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy();
    }

    private void MovePosition()
    {
        if (target != null) targetPosition = target.position;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(targetPosition + offset);
        rectTransform.position = screenPosition + displacement;
    }
}
