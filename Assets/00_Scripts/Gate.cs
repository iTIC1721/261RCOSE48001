using UnityEngine;

public class Gate : MonoBehaviour
{
    public GameObject gateClosed;
    public GameObject gateOpened;
    public bool isOpened = false;

    private void Awake()
    {
        if (isOpened) OpenGate();
        else CloseGate();
    }

    public void CloseGate()
    {
        gateClosed.SetActive(true);
        gateOpened.SetActive(false);
        isOpened = false;
    }

    public void OpenGate()
    {
        gateClosed.SetActive(false);
        gateOpened.SetActive(true);
        isOpened = true;
    }
}
