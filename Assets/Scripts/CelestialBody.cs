using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public float Mass = 1f;
    public float Radius = 1f;

    [Header("Sphere of Influence")]
    public float SoiRadius = 2f;

    [Header("Initial Conditions")]
    public Vector3 InitialVelocity = new Vector3(0f, 0f, 0f);

    void Start()
    {
        transform.localScale = new Vector3(Radius * 2f, Radius * 2f, Radius * 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, SoiRadius);
    }
}
