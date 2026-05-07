using System;
using UnityEngine;
using UnityEngine.UI;

public class AlphaButton : MonoBehaviour
{
    private void Start()
    {
        var images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            try
            {
                if (image.sprite != null)
                    image.alphaHitTestMinimumThreshold = 0.001f;
            }
            catch (InvalidOperationException) { }
        }
    }
}
