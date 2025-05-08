using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CelestialBody : MonoBehaviour
{
    [Header("Physical Properties")]
    public float Mass = 1f;
    public float Radius = 1f;

    [Header("Sphere of Influence")]
    public float SoiRadius = 2f;
    public GameObject SoiPrefab;
    private GameObject _soiVis;

    private static readonly List<CelestialBody> _bodies = new List<CelestialBody>();

#if UNITY_EDITOR 
    private void OnValidate() 
    {
        if (!Application.isPlaying)
            transform.localScale = Vector3.one * Radius * 2f;
    }
#endif 

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, SoiRadius);
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;

        // Maintain sorted list
        int idx = _bodies.FindIndex(b => b.SoiRadius > SoiRadius);
        if (idx < 0) _bodies.Add(this);
        else _bodies.Insert(idx, this);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        _bodies.Remove(this);
    }

    private void Start()
    {
        if (SoiPrefab == null) return;

        var old = transform.Find("SOI_Vis");
        if (old != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(old.gameObject);
#else 
            Destroy(old.gameObject);
#endif 
        }

        _soiVis = Application.isPlaying
            ? Instantiate(SoiPrefab, transform)
            : null;

        if (_soiVis != null)
        {
            _soiVis.name = "SOI_Vis";
            _soiVis.transform.localPosition = Vector3.zero;

            float parentScale = transform.lossyScale.x;
            _soiVis.transform.localScale = Vector3.one * SoiRadius * 2f / parentScale;
        }
    }

    public static IReadOnlyList<CelestialBody> GetAllCelestialBodies() => _bodies;

    public bool IsPointInsideSOI(Vector3 pt)
    {
        return (pt - transform.position).sqrMagnitude < SoiRadius * SoiRadius;
    }

    public static CelestialBody FindBodyWithSOIContaining(Vector3 point)
    {
        foreach (var body in _bodies)
        {
            if (body.IsPointInsideSOI(point))
                return body;
        }
        return null;
    }

    public void SOIVisEnabled(bool enabled)
    {
        if (_soiVis != null)
            _soiVis.SetActive(enabled);
    }

}
