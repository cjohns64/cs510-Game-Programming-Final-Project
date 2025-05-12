using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class OrbitPredictor : MonoBehaviour
{
    [Header("Object Type")]
    public bool isSpaceship = false;

    [Header("Line Settings")]
    public float lineScreenWidth = 0.2f;
    public int segments = 180;
    public float maxRenderDistance = 100f;

    [Header("Orbit Materials")]
    public Material StableMaterial;
    public Material CollisionMaterial;
    public Material EscapeMaterial;
    public Material EncounterMaterial;

    [Header("Orbital Markers")]
    public GameObject PeriapsisMarkerPrefab;
    private GameObject periapsisMarker;
    private bool hasPeriapsis = false;
    private const float periapsisTheta = 0f;
    private Vector3 periapsisPoint;

    public GameObject ApoapsisMarkerPrefab;
    private GameObject apoapsisMarker;
    private bool hasApoapsis = false;
    private const float apoapsisTheta = Mathf.PI;
    private Vector3 apoapsisPoint;

    public GameObject SoiExitMarkerPrefab;
    private GameObject soiExitMarker;
    private bool hasSOIExit = false;
    private Vector3 soiExitPoint;
    private float soiExitTheta;

    public GameObject SoiEntryMarkerPrefab;
    private GameObject soiEntryMarker;
    private bool hasSOIEntry = false;
    private Vector3 soiEntryPoint;
    private float soiEntryTheta;

    // Private Components
    private OrbitMoverAnalytic mover;
    private LineRenderer lineRenderer;
    private Transform centralBody;
    private Camera mainCamera;

    // Private variable
    private float trueAnomalySpan;

    // Cached values
    private Vector3[] points;
    private CelestialBody centralCelestialBody;



    void Awake()
    {
        mover = GetComponent<OrbitMoverAnalytic>();
        lineRenderer = GetComponent<LineRenderer>();
        centralBody = mover.CentralBody;
        mainCamera = Camera.main;

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = segments + 1;

        points = new Vector3[segments + 2];
        centralCelestialBody = centralBody.GetComponent<CelestialBody>();
    }

    void Start()
    {
        InstantiateMarkers();
    }

    void Update()
    {
        if (mover != null && mover.enabled)
        {
            CheckFutureSOITransition();
            PlaceMarkers();
            AdjustLineWidth();
            SwitchMaterial();
            DrawOrbit();
        }

        // 1a Check For encounter
        // 1b Check For escape
        // 1c Determine which is closer and disable further one
        // 2. Enable/Disable + locate markers.
        // 3. Switch Orbit Material
        // 4. Scale the line
        // 5. Draw the line

        /// Thoughts on efficient encounter prediction.
        /// 1. if ships apsi's ensure that there will be no encounter with planet
        ///     ,don't check for encounter with plant!
        ///    **Between total and no speed up depending on the orbit**
        /// 2. Right now, the ships future positions are recalculated 
        ///     for EVERY PLANET FOR EVERY FRAME
        ///     Reuse the same values for the same frame at bare minimum
        ///     **1.5 ish speed up I guess**
        /// 3. Cache the ship location list. Its the same size every time!
        ///     **1.1x speed up (total guess) Garbage collector does half the work it would have**
        /// 4. Unless there is an impulse or the ship changes SOI, the ships 
        ///     future position doesn't need to be recalculated between frames
        ///     Structure the list so that list[0] is the position at theta=0
        ///     and list[i] = theta at i * period / list.length time after theta=0
        ///     another var keeps track of how far along the ship is in that list in units of period / list.length
        /// 5. Don't look for encounters if there is an upcoming encounter and no impulse
    }

    private void OnEnable()
    {
        if (mover != null) mover.OnOrbitParametersChanged += HandleOrbitUpdate;
        if (mover != null) mover.OnSOITransition += HandleSOITransition;
    }

    private void OnDisable()
    {
        if (mover != null) mover.OnOrbitParametersChanged -= HandleOrbitUpdate;
        if (mover != null) mover.OnSOITransition -= HandleSOITransition;
    }

    private void HandleOrbitUpdate()
    {
        // Debug.Log("ORBIT UPDATE");
    }

    private void HandleSOITransition(CelestialBody celestialBody) {
        // Debug.Log("New SOI!!!");
    }

    private void InstantiateMarkers()
    {
        if (PeriapsisMarkerPrefab != null)
        {
            periapsisMarker = Instantiate(PeriapsisMarkerPrefab);
            periapsisMarker.SetActive(false);
        }

        if (ApoapsisMarkerPrefab != null)
        {
            apoapsisMarker = Instantiate(ApoapsisMarkerPrefab);
            apoapsisMarker.SetActive(false);
        }

        if (SoiExitMarkerPrefab != null)
        {
            soiExitMarker = Instantiate(SoiExitMarkerPrefab);
            soiExitMarker.SetActive(false);
        }

        if (SoiEntryMarkerPrefab != null)
        {
            soiEntryMarker = Instantiate(SoiEntryMarkerPrefab);
            soiEntryMarker.SetActive(false);
        }
    }

    private void CheckFutureSOITransition()
    {
        if (!isSpaceship) return;
        CheckFutureSOIExit();
        CheckFutureSOIEntry();

        // TODO compute apsis stuff
        // periapsis always at theta=0
        // apoapsis always at theta=pi

        float theta = mover.state.theta;
        float relativeEntryTheta = soiEntryTheta - theta;
        float relativeExitTheta = soiExitTheta - theta;
        if (relativeEntryTheta < 0) relativeEntryTheta += Mathf.PI * 2f;
        if (relativeExitTheta < 0) relativeExitTheta += Mathf.PI * 2f;

        // There can only be one!!!
        if (hasSOIEntry && hasSOIExit) {
            if (relativeEntryTheta < relativeExitTheta)
                hasSOIExit = false;
            else 
                hasSOIEntry = false;

            trueAnomalySpan = Math.Min(relativeEntryTheta, relativeExitTheta);
        }
        else if (hasSOIEntry) 
        {
            trueAnomalySpan = relativeEntryTheta;
        }
        else if (hasSOIExit) 
        {
            trueAnomalySpan = relativeExitTheta;
        }
        else
        {
            trueAnomalySpan = Mathf.PI * 2f;
        }

        float dPeriapsis = periapsisTheta - mover.state.theta;
        if (dPeriapsis < 0) dPeriapsis += 2 * Mathf.PI;
        float dApoapsis = apoapsisTheta - mover.state.theta;
        if (dApoapsis < 0) dApoapsis += 2 * Mathf.PI;

        hasPeriapsis = dPeriapsis <= trueAnomalySpan;
        periapsisPoint = centralBody.position + mover.shape.GetOrbitPoint(periapsisTheta);

        hasApoapsis = dApoapsis <= trueAnomalySpan;
        apoapsisPoint = centralBody.position + mover.shape.GetOrbitPoint(apoapsisTheta);
    }

    private void CheckFutureSOIExit()
    {
        hasSOIExit = (!mover.shape.IsClosedOrbit) || (mover.shape.IsClosedOrbit && mover.shape.rApoapsis > centralCelestialBody.SoiRadius);
        
        if (!hasSOIExit) return;
        
        float? theta = mover.shape.GetTrueAnomalyForRadius(centralCelestialBody.SoiRadius);
        if (theta.HasValue) {
            soiExitTheta = theta.Value;
            soiExitPoint = centralBody.position + mover.shape.GetOrbitPoint(soiExitTheta);
        }
    }

    private void CheckFutureSOIEntry()
    {
        float closestTheta = float.MaxValue;
        CelestialBody nearestBody = null;
        Vector3 encounterPoint = Vector3.zero;
        float currentTheta = mover.state.theta;

        var allBodies = CelestialBody.GetAllCelestialBodies()
            .Where(b => b.transform != centralBody)
            .ToList();

        foreach (var body in allBodies)
        {
            body.SOIVisEnabled(false);

            if (ShouldSkipBody(body)) continue;

            var bodyMover = body.GetComponent<OrbitMoverAnalytic>();
            if (!bodyMover) continue;

            float encounterAnomaly = mover.CalculateEncounterAnomaly(bodyMover, 200);

            if (encounterAnomaly < 0) continue;

            float deltaTheta = encounterAnomaly - currentTheta;

            if (mover.shape.IsClosedOrbit && deltaTheta < 0)
                deltaTheta += 2 * Mathf.PI;

            if (deltaTheta >= closestTheta)
            {
                continue;
            }

            closestTheta = encounterAnomaly;
            nearestBody = body;
        }

        if (nearestBody != null)
        {
            hasSOIEntry = true;
            soiEntryTheta = closestTheta;
            soiEntryPoint = centralBody.position + mover.shape.GetOrbitPoint(soiEntryTheta);

            nearestBody.SOIVisEnabled(true);
            return;
        }
        
        hasSOIEntry = false;
    }

    bool ShouldSkipBody(CelestialBody body)
    {
        return false;
    }

    void PlaceMarkers() {
        if (!isSpaceship) return;

        if (hasPeriapsis) {
            periapsisMarker.transform.position = periapsisPoint;
            periapsisMarker.SetActive(true);
        } 
        else periapsisMarker.SetActive(false);

        if (hasApoapsis) {
            apoapsisMarker.transform.position = apoapsisPoint;
            apoapsisMarker.SetActive(true);
        }
        else apoapsisMarker.SetActive(false);

        if (hasSOIExit) {
            soiExitMarker.transform.position = soiExitPoint;
            soiExitMarker.SetActive(true);
        }
        else soiExitMarker.SetActive(false);

        if (hasSOIEntry) {
            soiEntryMarker.transform.position = soiEntryPoint;
            soiEntryMarker.SetActive(true);
        }
        else soiEntryMarker.SetActive(false);
    }


    void DrawOrbit()
    {
        float a = mover.shape.a;
        float e = mover.shape.e;
        Vector3 normal = mover.shape.AngularMomentumVec.normalized;
        Vector3 perigee = mover.shape.EccentricityVec.normalized;

        if (mover.shape.IsClosedOrbit)
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
        //Vector3[] points = new Vector3[hasSOIExit ? segments + 2 : segments + 2]; // +2 for ship position and potential loop closure

        // if (hasSOIExit || hasSOIEntry)
        // {
            // Draw from ship to SOI exit (escaping orbit)
            float thetaShip = mover.state.theta;

            // float exitTheta = hasSOIEntry 
            //     ? soiEntryTheta
            //     : CalculateExitThetaForEllipse(a, e);
            float exitTheta = (trueAnomalySpan + thetaShip - 0.00001f) % (2 * Mathf.PI);


            // Ensure we go the "short way" around
            if (exitTheta < thetaShip) exitTheta += 2 * Mathf.PI;

            points[0] = transform.position; // Exact ship position

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float theta = Mathf.Lerp(thetaShip, exitTheta, t);
                points[i + 1] = CalculateEllipsePoint(a, e, rotation, theta);
            }
        // }
        // else
        // {
        //     // Draw full ellipse (stable orbit)
        //     for (int i = 0; i <= segments; i++)
        //     {
        //         float theta = 2 * Mathf.PI * i / segments;
        //         points[i] = CalculateEllipsePoint(a, e, rotation, theta);
        //     }
        //     points[segments + 1] = points[0]; // Close the loop
        // }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
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
        //List<Vector3> points = new List<Vector3>();

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

            points[i] = point;
        }

        // Clean up artifacts that may appear at the end of the hyperbolic trajectories.
        //if (points.Count > 1 &&
        //    Vector3.Distance(points[points.Count - 2], points[points.Count - 1]) > maxRenderDistance / 2)
        //{
        //    points.RemoveAt(points.Count - 1);
        //}

        lineRenderer.positionCount = points.Length - 1;
        lineRenderer.SetPositions(points);
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
        // Eventually put has collision logic into CheckSOITransition 
        // because if a collision is gonna happen *before* a soi transition, the collision takes presidence
        float centralBodyRadius = centralCelestialBody.Radius;
        float periapsis = mover.shape.rPeriapsis;
        bool hasCollision = periapsis <= centralBodyRadius;

        if (hasSOIEntry) 
        {
            lineRenderer.material = EncounterMaterial;
        } 
        else if (hasSOIExit) 
        {
            lineRenderer.material = EscapeMaterial;
        }
        else if (hasCollision) 
        {
            lineRenderer.material = CollisionMaterial;
        }
        else // Stable Orbt
        {
            lineRenderer.material = StableMaterial;
        }
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