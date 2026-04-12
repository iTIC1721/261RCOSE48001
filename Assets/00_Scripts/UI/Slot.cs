using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] RectTransform slotObject;
    [SerializeField] List<Image> images;
    [SerializeField] float rollSpeed = 50f;
    [SerializeField] float decelerationThreshold = 0.9f;
    [SerializeField] float decelerationAmount = 0.5f;

    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetItem(List<Sprite> sprites)
    {
        images[3].sprite = sprites[0];
        images[2].sprite = sprites[1];
        images[1].sprite = sprites[2];
        images[0].sprite = sprites[0];
    }

    public void RollStart(int index, int rollCount)
    {
        button.interactable = false;
        slotObject.anchoredPosition = new Vector2(0, -100);

        StartCoroutine(RollCoroutine(index, rollCount));
    }

    private IEnumerator RollCoroutine(int index, int rollCount)
    {
        float imageSizeY = 200f;
        float finalDist = imageSizeY * (images.Count - 1) * rollCount + imageSizeY * index;

        float rollTime = (float)rollCount / rollSpeed;
        float scaledRollSpeed = (rollCount + ((float)index / (images.Count - 1))) / rollTime;

        float moveDist = 0;
        while (moveDist < finalDist)
        {
            yield return null;

            float speed = (moveDist > finalDist * decelerationThreshold) ? 
                Mathf.Lerp(scaledRollSpeed, scaledRollSpeed * decelerationAmount, (moveDist - finalDist * decelerationThreshold) / (finalDist * (1 - decelerationThreshold))) :
                scaledRollSpeed;
            moveDist += speed * Time.unscaledDeltaTime;
            slotObject.anchoredPosition = Vector2.down * ((moveDist % 600) + 100);
        }

        slotObject.anchoredPosition = Vector2.down * (200 * index + 100);
        button.interactable = true;
    }
}
