using UnityEngine;
using UnityEngine.UI;

public class LoadingImage : MonoBehaviour
{
    public float flickeringTime = 0.8f;

    Image image;
    Color col;
    float time = 0;

    private void Awake()
    {
        image = GetComponent<Image>();
        col = image.color;

        image.color = new Color(col.r, col.g, col.b, 0);
    }

    void Update()
    {
        float a = 0;
        if (time < flickeringTime * 0.5f)
        {
            a = time / (flickeringTime * 0.5f);
        }
        else if (time < flickeringTime)
        {
            a = 1 - (time - flickeringTime * 0.5f) / (flickeringTime * 0.5f);
        }
        image.color = new Color(col.r, col.g, col.b, col.a * a);

        time += Time.unscaledDeltaTime;
        time %= flickeringTime;
    }
}
