using UnityEngine;

public class Random_Rotate : MonoBehaviour
{

    private float x;
    private float y;
    private float z;
    // intensity of the rotation
    public float spin_intensity = 15.0f;
    void Start()
    {
        // random value between 0 and 1, scaled by the spin_intensity
        x = Random.value * spin_intensity;
        y = Random.value * spin_intensity;
        z = Random.value * spin_intensity;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(x, y, z) * Time.deltaTime);
    }
}
