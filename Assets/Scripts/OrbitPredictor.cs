using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class OrbitPredictor : MonoBehaviour
{
    public float lineScreenWidth = 0.2f;
    private Camera mainCamera;

    public int segments = 180;
    public float maxRenderDistance = 100f;

    [Header("Orbital Markers")]
    public GameObject periapsisMarker;
    public GameObject apoapsisMarker;

    private OrbitMoverAnalytic mover;
    private Transform centralBody;
    private LineRenderer lineRenderer;

    [Header("Orbit Materials")]
    public Material StableMaterial;
    public Material CollisionMaterial;
    public Material EscapeMaterial;
    public Material EncounterMaterial;

    [Header("SOI Exit")]
    public GameObject soiExitMarkerPrefab;
    private GameObject soiExitMarker;
    private Vector3 soiExitPoint;
    private bool hasSOIExit;
    private float exitTheta;

    void Start()
    {
        mover = GetComponent<OrbitMoverAnalytic>();
        centralBody = mover.CentralBody;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        mainCamera = Camera.main;
        lineRenderer.useWorldSpace = true;

        if (soiExitMarkerPrefab != null )
        {
            soiExitMarker = Instantiate(soiExitMarkerPrefab);
            soiExitMarker.SetActive(false);
        }
    }

    void Update()
    {
        if (mover != null && mover.enabled)
        {
            DrawFromAnalyticMover(mover);
            AdjustLineWidth();
            SwitchMaterial();
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
        // Parameter validation
        if (a <= 0 || e < 0 || e >= 1)
            throw new ArgumentException("Invalid ellipse parameters");

        if (normal == Vector3.zero || perigee == Vector3.zero)
            throw new ArgumentException("Vectors cannot be zero");

        Quaternion rotation = Quaternion.LookRotation(perigee, normal);
        Vector3[] points = new Vector3[hasSOIExit ? segments + 2 : segments + 2]; // +2 for ship position and potential loop closure

        if (hasSOIExit)
        {
            // Draw from ship to SOI exit (escaping orbit)
            float thetaShip = GetCurrentTrueAnomaly();
            float exitTheta = CalculateExitThetaForEllipse(a, e);

            // Ensure we go the "short way" around
            if (exitTheta < thetaShip) exitTheta += 2 * Mathf.PI;

            points[0] = transform.position; // Exact ship position

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float theta = Mathf.Lerp(thetaShip, exitTheta, t);
                points[i + 1] = CalculateEllipsePoint(a, e, rotation, theta);
            }
        }
        else
        {
            // Draw full ellipse (stable orbit)
            for (int i = 0; i <= segments; i++)
            {
                float theta = 2 * Mathf.PI * i / segments;
                points[i] = CalculateEllipsePoint(a, e, rotation, theta);
            }
            points[segments + 1] = points[0]; // Close the loop
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        UpdateMarkers(a, e, rotation);
    }

    Vector3 CalculateEllipsePoint(float a, float e, Quaternion rotation, float theta)
    {
        float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(theta));
        return centralBody.position + rotation * new Vector3(
            r * Mathf.Sin(theta),
            0,
            r * Mathf.Cos(theta)
        );
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

    void AdjustLineWidth()
    {
        if (mainCamera == null) return;

        float distance = Vector3.Distance(
            mainCamera.transform.position,
            centralBody.position
        );

        float worldWidth = lineScreenWidth * distance / mainCamera.fieldOfView;
        lineRenderer.startWidth = worldWidth;
        lineRenderer.endWidth = worldWidth;
    }

    void SwitchMaterial()
    {
        float centralBodyRadius = centralBody.GetComponent<CelestialBody>().Radius;
        float soiRadius = centralBody.GetComponent<CelestialBody>().SoiRadius;
        float periapsis = mover.SemiMajorAxis * (1 - mover.Eccentricity);
        float apoapsis = mover.SemiMajorAxis * (1 + mover.Eccentricity);

        hasSOIExit = false;

        if (mover.Eccentricity < 1f && apoapsis > soiRadius)
        {
            lineRenderer.material = EscapeMaterial;
            CalculateSOIExitPoint(soiRadius);
            hasSOIExit = true;

            if (soiExitMarker != null)
            {
                soiExitMarker.SetActive(true);
                soiExitMarker.transform.position = soiExitPoint;
            }

            if (apoapsisMarker != null)
            {
                apoapsisMarker.SetActive(false);
            }
        }
        else if (periapsis <= centralBodyRadius)
        {
            lineRenderer.material = CollisionMaterial;
        }
        else
        {
            lineRenderer.material = StableMaterial;
            if (soiExitMarker != null) soiExitMarker.SetActive(false);
        }
    }

    void CalculateSOIExitPoint(float soiRadius)
    {
        float e = mover.Eccentricity;
        float a = mover.SemiMajorAxis;

        float numerator = (e < 1f) ? a * (1 - e * e) - soiRadius : a * (e * e - 1) - soiRadius;

        float cosTheta = numerator / (e * soiRadius);
        cosTheta = Mathf.Clamp(cosTheta, -1f, 1f);

        exitTheta = Mathf.Acos(cosTheta);
        soiExitPoint = GetOrbitPoint(exitTheta);

    }


    Vector3 GetOrbitPoint(float theta)
    {
        float e = mover.Eccentricity;
        float a = mover.SemiMajorAxis;
        float r;
        if (e < 1f)
        {
            r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(theta));
        }
        else
        {
            r = (a * (e * e - 1)) / (1 + e * Mathf.Cos(theta));
        }

        Quaternion rotation = Quaternion.LookRotation(mover.EccentricityVec.normalized, mover.AngularMomentumVec.normalized);
        Vector3 localPos;
        if (e < 1f)
        {
            localPos = new Vector3(r * Mathf.Sin(theta), 0, r * Mathf.Cos(theta));
        }
        else
        {
            localPos = new Vector3(-r * Mathf.Sin(theta), 0, -r * Mathf.Cos(theta));
        }
        return centralBody.position + rotation * localPos;
    }

    float GetCurrentTrueAnomaly()
    {
        Vector3 shipPos = transform.position - centralBody.position;
        Vector3 eccentricityVec = mover.EccentricityVec.normalized;
        Vector3 angularMomentum = mover.AngularMomentumVec.normalized;

        // Project position onto the orbital plane
        Vector3 planarPos = Vector3.ProjectOnPlane(shipPos, angularMomentum);
        if (planarPos.magnitude < 0.001f) return 0f;

        // Calculate angle between eccentricity vector and position
        float cosTheta = Vector3.Dot(eccentricityVec, planarPos.normalized);
        float theta = Mathf.Acos(cosTheta);

        // Determine direction using cross product
        Vector3 cross = Vector3.Cross(eccentricityVec, planarPos.normalized);
        float sign = Mathf.Sign(Vector3.Dot(cross, angularMomentum));
        return theta * sign;
    }

    float CalculateExitThetaForEllipse(float a, float e)
    {
        float soiRadius = centralBody.GetComponent<CelestialBody>().SoiRadius;
        float numerator = a * (1 - e * e) - soiRadius;
        float cosTheta = numerator / (e * soiRadius);
        return Mathf.Acos(Mathf.Clamp(cosTheta, -1f, 1f));
    }
}

public static class GravitationalConstants
{
    public static float G = 1f;
}