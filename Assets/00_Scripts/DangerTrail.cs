using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class DangerTrail : PoolObject
{
    private Vector2 direction;
    private float lifeTime;

    private TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Initialize(Vector2 startPosition, Vector2 direction, float lifeTime)
    {
        transform.position = startPosition;
        this.direction = direction;
        this.lifeTime = lifeTime;

        Draw();
    }

    public void Draw()
    {
        trailRenderer.Clear();
        StartCoroutine(DrawCoroutine());
    }

    private IEnumerator DrawCoroutine()
    {
        Vector2 start = transform.position;

        var raycast = Physics2D.Raycast(start, direction, 50f, LayerMask.GetMask("Wall"));
        Vector2 end = raycast.point;

        float time = 0;
        while (time < lifeTime)
        {
            yield return null;
            time += Time.deltaTime;

            transform.position = Vector2.Lerp(start, end, time / lifeTime);
        }

        Return();
    }
}
