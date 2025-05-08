using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways] // <- Important!
public class CelestialBody : MonoBehaviour
{
    public float Mass = 1f;
    public float Radius = 1f;

    [Header("Sphere of Influence")]
    public float SoiRadius = 2f;
    public GameObject SoiPrefab;
    private GameObject _soiVis;

    private static List<CelestialBody> _celestialBodies = new List<CelestialBody>();

    void OnEnable()
    {
        _celestialBodies.Add(this);
        _celestialBodies = _celestialBodies.OrderBy(body => body.SoiRadius).ToList();
        UpdateScale();
        UpdateSOI();
    }

    void OnDisable()
    {
        _celestialBodies.Remove(this);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateScale();
            UpdateSOI();
        }
#endif
    }

    private void UpdateScale()
    {
        transform.localScale = Vector3.one * Radius * 2f;
    }

    private void UpdateSOI()
    {
        if (SoiPrefab != null && _soiVis == null)
        {
            _soiVis = Instantiate(SoiPrefab);
            _soiVis.transform.SetParent(this.transform);
        }

        if (_soiVis != null)
        {
            _soiVis.transform.localScale = Vector3.one * SoiRadius * 2f / transform.localScale.x;
            _soiVis.transform.localPosition = Vector3.zero;
        }
    }

    public static List<CelestialBody> GetAllCelestialBodies() => _celestialBodies;

    public static CelestialBody FindBodyWithSOIContaining(Vector3 point)
    {
        return _celestialBodies.FirstOrDefault(body => body.IsPointInsideSOI(point));
    }

    public bool IsPointInsideSOI(Vector3 point)
    {
        float distanceSqr = (point - transform.position).sqrMagnitude;
        return distanceSqr < SoiRadius * SoiRadius;
    }

    public void SOIVisEnabled(bool enabled)
    {
        if (_soiVis != null)
            _soiVis.SetActive(enabled);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, SoiRadius);
    }
}
