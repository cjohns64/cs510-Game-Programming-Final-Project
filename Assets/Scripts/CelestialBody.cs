using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CelestialBody : MonoBehaviour
{
    public float Mass = 1f;
    public float Radius = 1f;

    [Header("Sphere of Influence")]
    public float SoiRadius = 2f;
    public GameObject SoiPrefab;
    private GameObject _soiVis;

    // Register all of the celestialBody instances
    private static List<CelestialBody> _celestialBodies = new List<CelestialBody>();

    void Start()
    {
        transform.localScale = Vector3.one * Radius * 2f;

        if (SoiPrefab != null)
        {
            _soiVis = Instantiate(SoiPrefab);
            _soiVis.transform.SetParent(this.transform);
            _soiVis.transform.localScale = Vector3.one * SoiRadius * 2f / transform.localScale.x;
            _soiVis.transform.localPosition = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        _celestialBodies.Add(this);
        _celestialBodies = _celestialBodies
            .OrderBy(body => body.SoiRadius)
            .ToList();
    }

    private void OnDisable()
    {
        _celestialBodies.Remove(this);
    }

    public static List<CelestialBody> GetAllCelestialBodies()
    {
        return _celestialBodies;
    }

    public static CelestialBody FindBodyWithSOIContaining(Vector3 point)
    {
        foreach (var body in _celestialBodies)
        {
            if (body.IsPointInsideSOI(point))
                return body;
        }
        return null;
    }

    public bool IsPointInsideSOI(Vector3 point)
    {
        float distanceSqr = (point - transform.position).sqrMagnitude;
       
        return distanceSqr < SoiRadius * SoiRadius;
    }

    public void SOIVisEnabled(bool enabled)
    {
        _soiVis.SetActive(enabled);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, SoiRadius);
    }
}
