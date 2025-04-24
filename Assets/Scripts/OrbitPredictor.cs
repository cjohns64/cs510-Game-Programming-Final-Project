using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class OrbitPredictor : MonoBehaviour
{
    public int segments = 180;
    public float maxRenderDistance = 100f;

    [Header("Orbital Markers")]
    public GameObject periapsisMarker;
    public GameObject apoapsisMarker;

    private OrbitMoverAnalytic mover;
    private Transform centralBody;
    private LineRenderer lineRenderer;

    void Start()
    {
        mover = GetComponent<OrbitMoverAnalytic>();
        centralBody = mover.CentralBody;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
    }

    void Update()
    {
        if (mover != null && mover.enabled)
        {
            DrawFromAnalyticMover(mover);
        }
    }

    void DrawFromAnalyticMover(OrbitMoverAnalytic mover)
    {
        float a = mover.SemiMajorAxis;
        float e = mover.Eccentricity;
        Vector3 normal = mover.AngularMomentumVec.normalized;
        Vector3 perigee = mover.EccentricityVec.normalized;

        if (e < 1f)
        {
            DrawEllipse(a, e, normal, perigee);
        }
        else
        {
            DrawHyperbola(a, e, normal, perigee);
        }
    }

    void DrawEllipse(float a, float e, Vector3 normal, Vector3 perigee)
    {
        Quaternion rotation = Quaternion.LookRotation(perigee, normal);
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float theta = 2 * Mathf.PI * i / segments;
            float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(theta));
            points[i] = centralBody.position + rotation * new Vector3(r * Mathf.Sin(theta), 0, r * Mathf.Cos(theta));
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);

        UpdateMarkers(a, e, rotation);
    }

    void DrawHyperbola(float a, float e, Vector3 normal, Vector3 perigee)
    {
        Quaternion rotation = Quaternion.LookRotation(perigee, normal);
        float thetaMax = Mathf.Acos(-1 / e);
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float theta = thetaMax * Mathf.Tan(t * Mathf.PI - Mathf.PI / 2) * 0.9f;
            theta = Mathf.Clamp(theta, -thetaMax, thetaMax);

            float denominator = 1 + e * Mathf.Cos(theta);
            if (Mathf.Abs(denominator) < 0.001f) continue;

            float r = (a * (e * e - 1)) / denominator;
            if (r > maxRenderDistance || float.IsNaN(r)) continue;

            Vector3 point = rotation * new Vector3(
               -r * Mathf.Sin(theta),
                0,
               -r * Mathf.Cos(theta)
            ) + centralBody.position;

            points.Add(point);
        }

        // Clean up artifacts that may appear at the end of the hyperbolic trajectories.
        if (points.Count > 1 &&
            Vector3.Distance(points[points.Count - 2], points[points.Count - 1]) > maxRenderDistance / 2)
        {
            points.RemoveAt(points.Count - 1);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        UpdateMarkers(a, e, rotation);
    }

    void UpdateMarkers(float a, float e, Quaternion rotation)
    {
        if (periapsisMarker != null)
        {
            // Periapsis: closest point to central body
            float r_peri = a * (1 - e);
            Vector3 periPos = rotation * new Vector3(0, 0, r_peri) + centralBody.position;
            periapsisMarker.transform.position = periPos;
        }

        if (apoapsisMarker != null && e < 1f) // Apoapsis only exists for closed orbits (e < 1)
        {
            // Apoapsis: farthest point from central body
            float r_apo = a * (1 + e);
            Vector3 apoPos = rotation * new Vector3(0, 0, -r_apo) + centralBody.position;
            apoapsisMarker.transform.position = apoPos;
            apoapsisMarker.SetActive(true);
        }
        else if (apoapsisMarker != null)
        {
            apoapsisMarker.SetActive(false); // Hide for hyperbolic orbits
        }
    }
}

public static class GravitationalConstants
{
    public static float G = 1f;
}