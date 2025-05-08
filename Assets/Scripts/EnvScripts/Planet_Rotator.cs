using UnityEngine;

public class Planet_Rotator : MonoBehaviour
{

    // intensity of the rotation
    public float speed = 15.0f;

    private void Update()
    {
        transform.Rotate(new Vector3(0.0f, speed, 0.0f) * Time.deltaTime);
    }
}
