using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Asteroid_Belt_Instantiator : MonoBehaviour
{
    public float belt_radius = 10.0f;
    public float belt_scale = 5.0f;
    public float belt_hieght = 2.5f;
    public float belt_density = 1.0f;
    public float orbital_velocity = 10.0f;
    public GameObject[] asteroids;
    private int num_asteroids;
    private float inv_belt_radius;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inv_belt_radius = 1 / belt_radius;
        num_asteroids = (int)(belt_radius * belt_density * belt_scale * 10); // 10 comes from observation
        int steps = num_asteroids;
        float step_angle = 2*Mathf.PI / steps;
        float x;
        float z;
        float a;
        // instantiate asteroids at belt_radius +- random number
        for (int i = 0; i < steps; i++)
        {
            x = belt_radius * Mathf.Cos(i * step_angle);
            z = belt_radius * Mathf.Sin(i * step_angle);
            a = Random.Range(-1.0f, 1.0f);

            // position should be close to the belt radius
            Vector3 postion = new Vector3(x + Random.Range(-1.0f, 1.0f) * belt_scale,
                                          Random.Range(-1.0f, 1.0f) * belt_hieght,
                                          z + Random.Range(-1.0f, 1.0f) * belt_scale);
            // inital rotation is not important
            Quaternion rotation = new Quaternion(a, a, a, 1.0f);
            // add a randome asteroid
            Instantiate(asteroids[Random.Range(0, asteroids.Length - 1)], postion, rotation, this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, -orbital_velocity * inv_belt_radius, 0.0f) * Time.deltaTime);
    }
}
