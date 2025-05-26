using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(OrbitMoverAnalytic), typeof(MeshFilter), typeof(MeshRenderer))]
public class OrbitPredictor : MonoBehaviour
{
    private bool isSpaceship;

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
    public AudioSource soiEntryAudio;

    // Private Components
    private OrbitMoverAnalytic mover;
    private Transform centralBody;
    private Mesh orbitMesh;
    private MeshRenderer meshRenderer;
    private Camera mainCamera;

    // Private variable
    private float trueAnomalySpan = 2 * (float)Math.PI - 0.001f;

    // Cached values
    private Vector3[] orbitPoints;
    private CelestialBody centralCelestialBody;



    void Awake()
    {
        mover = GetComponent<OrbitMoverAnalytic>();
        centralBody = mover.CentralBody;
        mainCamera = Camera.main;

        orbitPoints = new Vector3[segments + 1];
        orbitMesh = new Mesh();
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = orbitMesh;
        meshRenderer = GetComponent<MeshRenderer>();
        // meshRenderer.material = new Material(Shader.Find("Custom/ConstantWidthLine"));

        centralCelestialBody = centralBody.GetComponent<CelestialBody>();

        isSpaceship = CompareTag("Ship");
    }

    void Start()
    {
        InstantiateMarkers();

        meshRenderer.material = StableMaterial;
    }

    void Update()
    {
        if (mover == null || !mover.enabled)
            return;

        if (isSpaceship)
        {
            CheckFutureSOITransition();
            PlaceMarkers();
            SwitchMaterial();
        }

        DrawOrbit();

            // 1a Check For encounter
            // 1b Check For escape
            // 1c Determine which is closer and disable further one
            // 2. Enable/Disable + locate markers.
            // 3. Switch Orbit Material
            // 4. Draw the line

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
        float soiRadius = centralCelestialBody.SoiRadius;
        float periapsis = mover.shape.rPeriapsis;
        float apoapsis = mover.shape.rApoapsis;
        bool IsClosedOrbit = mover.shape.IsClosedOrbit;

        if (IsClosedOrbit)
            hasSOIExit = apoapsis > soiRadius;
        else
            hasSOIExit = periapsis < soiRadius;

        if (!hasSOIExit)
            return;

        float[] crossings = mover.shape.GetTrueAnomaliesForRadius(soiRadius);
        if (crossings.Length > 0)
        {
            soiExitTheta = crossings.Where(f => f >= 0f)
                                          .DefaultIfEmpty(crossings[0])
                                          .Min();
            Vector3 exitLocal = mover.shape.GetOrbitPoint(soiExitTheta);
            soiExitPoint = centralBody.position + exitLocal;
        }
        else if (hasSOIExit)
        {
            Debug.LogWarning(
                $"[OrbitPredictor] no SOI exit crossing EVEN THOUGH THERE SHOULD BE"
            );
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

            if (soiEntryAudio && !soiEntryAudio.isPlaying)
            {
                // soiEntryAudio.Play();
            }
            return;
        }
        
        hasSOIEntry = false;
    }

    bool ShouldSkipBody(CelestialBody body)
    {
        return false;
    }

    void PlaceMarkers() {
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
        GenerateOrbitPoints();

        int vertCount = orbitPoints.Length;
        Vector3[] verts = new Vector3[vertCount * 2];
        Vector3[] nexts = new Vector3[vertCount * 2];
        int[] indices = new int[(vertCount - 1) * 6];

        for (int i = 0; i < vertCount; i++) {
            Vector3 p0 = orbitPoints[i];
            Vector3 p1 = orbitPoints[Mathf.Min(i + 1, vertCount - 1)];

            verts[i * 2 + 0] = p0;
            verts[i * 2 + 1] = p0;

            nexts[i * 2 + 0] = p1;
            nexts[i * 2 + 1] = p1;

            if (i < vertCount - 1) 
            {
                int idx = i * 6;
                int v = i * 2;

                // two triangles per segment quad
                indices[idx + 0] = v + 0;
                indices[idx + 1] = v + 2;
                indices[idx + 2] = v + 1;
                indices[idx + 3] = v + 1;
                indices[idx + 4] = v + 2;
                indices[idx + 5] = v + 3;
            }
        }

        orbitMesh.Clear();
        orbitMesh.vertices = verts;
        orbitMesh.SetUVs(0, nexts);
        orbitMesh.SetIndices(indices, MeshTopology.Lines, 0);

        float a = mover.shape.a;
        float e = mover.shape.e;
        Vector3 normal = mover.shape.AngularMomentumVec.normalized;
        Vector3 perigee = mover.shape.EccentricityVec.normalized;
    }

    void GenerateOrbitPoints()
    {
        // Compute common parameters
        float a = mover.shape.a;
        float e = mover.shape.e;
        Vector3 center = centralBody.position;
        Vector3 normal = mover.shape.AngularMomentumVec.normalized;
        Vector3 perigee = mover.shape.EccentricityVec.normalized;
        Quaternion rot = Quaternion.LookRotation(perigee, normal);

        if (mover.shape.IsClosedOrbit)
        {
            // Draw from current true anomaly up to span
            float thetaStart = mover.state.theta;
            float thetaEnd = thetaStart + trueAnomalySpan;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float theta = Mathf.Lerp(thetaStart, thetaEnd, t);
                float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(theta));
                Vector3 point = center + rot * new Vector3(
                    r * Mathf.Sin(theta),
                    0f,
                    r * Mathf.Cos(theta)
                );
                orbitPoints[i] = point;
            }
        }
        else
        {
            float thetaStart = mover.state.theta;
            float thetaEnd = thetaStart + trueAnomalySpan;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float theta = Mathf.Lerp(thetaStart, thetaEnd, t);
                float denom = 1 + e * Mathf.Cos(theta);
                if (Mathf.Abs(denom) < 1e-3f) { orbitPoints[i] = center; continue; }
                float r = (a * (e * e - 1)) / denom;
                Vector3 local = new Vector3(
                    -r * Mathf.Sin(theta),
                    0f,
                    -r * Mathf.Cos(theta)
                );
                orbitPoints[i] = center + rot * local;
            }

        }
    }

    // void DrawEllipse(float a, float e, Vector3 normal, Vector3 perigee)
    // {
    //     // Parameter validation
    //     if (a <= 0 || e < 0 || e >= 1)
    //         throw new ArgumentException("Invalid ellipse parameters");

    //     if (normal == Vector3.zero || perigee == Vector3.zero)
    //         throw new ArgumentException("Vectors cannot be zero");

    //     Quaternion rotation = Quaternion.LookRotation(perigee, normal);
        
    //     float thetaShip = mover.state.theta;
    //     float exitTheta = (trueAnomalySpan + thetaShip - 0.00001f) % (2 * Mathf.PI);

    //     if (exitTheta < thetaShip) exitTheta += 2 * Mathf.PI;

    //     orbitPoints[0] = transform.position;
    //     for (int i = 0; i <= segments; i++)
    //     {
    //         float t = (float)i / segments;
    //         float theta = Mathf.Lerp(thetaShip, exitTheta, t);
    //         orbitPoints[i + 1] = CalculateEllipsePoint(a, e, rotation, theta);
    //     }

    //     lineRenderer.positionCount = orbitPoints.Length;
    //     lineRenderer.SetPositions(orbitPoints);
    // }

    // Vector3 CalculateEllipsePoint(float a, float e, Quaternion rotation, float theta)
    // {
    //     float r = (a * (1 - e * e)) / (1 + e * Mathf.Cos(theta));
    //     return centralBody.position + rotation * new Vector3(
    //         r * Mathf.Sin(theta),
    //         0,
    //         r * Mathf.Cos(theta)
    //     );
    // }

    // void DrawHyperbola(float a, float e, Vector3 normal, Vector3 perigee)
    // {
    //     Quaternion rotation = Quaternion.LookRotation(perigee, normal);
    //     float thetaMax = Mathf.Acos(-1 / e);
    //     //List<Vector3> points = new List<Vector3>();

    //     for (int i = 0; i <= segments; i++)
    //     {
    //         float t = (float)i / segments;
    //         float theta = thetaMax * Mathf.Tan(t * Mathf.PI - Mathf.PI / 2) * 0.9f;
    //         theta = Mathf.Clamp(theta, -thetaMax, thetaMax);

    //         float denominator = 1 + e * Mathf.Cos(theta);
    //         if (Mathf.Abs(denominator) < 0.001f) continue;

    //         float r = (a * (e * e - 1)) / denominator;
    //         if (r > maxRenderDistance || float.IsNaN(r)) continue;

    //         Vector3 point = rotation * new Vector3(
    //            -r * Mathf.Sin(theta),
    //             0,
    //            -r * Mathf.Cos(theta)
    //         ) + centralBody.position;

    //         orbitPoints[i] = point;
    //     }

    //     // Clean up artifacts that may appear at the end of the hyperbolic trajectories.
    //     //if (points.Count > 1 &&
    //     //    Vector3.Distance(points[points.Count - 2], points[points.Count - 1]) > maxRenderDistance / 2)
    //     //{
    //     //    points.RemoveAt(points.Count - 1);
    //     //}

    //     lineRenderer.positionCount = orbitPoints.Length - 1;
    //     lineRenderer.SetPositions(orbitPoints);
    // }

    // void AdjustLineWidth()
    // {
    //     if (mainCamera == null) return;

    //     float distance = Vector3.Distance(
    //         mainCamera.transform.position,
    //         centralBody.position
    //     );

    //     float worldWidth = lineScreenWidth * distance / mainCamera.fieldOfView;
    //     lineRenderer.startWidth = worldWidth;
    //     lineRenderer.endWidth = worldWidth;
    // }

    void SwitchMaterial()
    {
        // Eventually put has collision logic into CheckSOITransition 
        // because if a collision is gonna happen *before* a soi transition, the collision takes presidence
        float centralBodyRadius = centralCelestialBody.Radius;
        float periapsis = mover.shape.rPeriapsis;
        bool hasCollision = periapsis <= centralBodyRadius;

        if (hasSOIEntry) 
        {
            meshRenderer.material = EncounterMaterial;
        } 
        else if (hasSOIExit) 
        {
            meshRenderer.material = EscapeMaterial;
        }
        else if (hasCollision) 
        {
            meshRenderer.material = CollisionMaterial;
        }
        else // Stable Orbt
        {
            meshRenderer.material = StableMaterial;
        }
    }

    // float CalculateExitThetaForEllipse(float a, float e)
    // {
    //     float soiRadius = centralBody.GetComponent<CelestialBody>().SoiRadius;
    //     float numerator = a * (1 - e * e) - soiRadius;
    //     float cosTheta = numerator / (e * soiRadius);
    //     return Mathf.Acos(Mathf.Clamp(cosTheta, -1f, 1f));
    // }
}

public static class GravitationalConstants
{
    public static float G = 1f;
}