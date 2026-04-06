using UnityEngine;

public class SpinningSprite : MonoBehaviour
{
    public float anglularSpeed;

    private void Update()
    {
        Vector3 angle = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(angle.x, angle.y, angle.z + anglularSpeed * Time.deltaTime);
    }
}
