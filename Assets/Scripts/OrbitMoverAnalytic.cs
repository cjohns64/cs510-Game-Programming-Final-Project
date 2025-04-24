using UnityEngine;

public class OrbitMoverAnalytic : MonoBehaviour
{
    //--------------------- Inspector Configuration ----------------------
    [Header("Central Body")]
    public Transform CentralBody;

    [Header("Initial Conditions")]
    public Vector3 InitialVelocity;

    [Header("Time Settings")]
    public float TimeMultiplier = 1f;

    //---------------------- Public State ---------------------------------
    public float SemiMajorAxis { get; private set; }
    public float Eccentricity { get; private set; }
    public Vector3 AngularMomentumVec { get; private set; }
    public Vector3 EccentricityVec { get; private set; }
    public Quaternion OrbitalPlaneRotation { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }
    public float ElapsedTime { get; private set; }

    //---------------------- Private State ---------------------------------
    private Mass centralBodyMass;
    private float mu;
    private float aCubed;

    // --------------------- Unity Callbacks -------------------------------
    void Start()
    {
        ElapsedTime = 0f;

        if (CentralBody == null || !CentralBody.TryGetComponent(out centralBodyMass))
        {
            Debug.LogError("Central body configuration error!");
            enabled = false;
            return;
        }

        CurrentVelocity = InitialVelocity;
        InitializeOrbitParameters(CurrentVelocity);
    }

    void FixedUpdate()
    {
        ElapsedTime += Time.fixedDeltaTime * TimeMultiplier;
        if (Eccentricity < 1f)
            UpdateEllipticalOrbit();
        else
            Debug.LogWarning("[OrbitMoverAnalytic] Non-elliptical orbit  (e >= 1). TODO: implement elliptical orbits");
    }

    //---------------------- Public API ---------------------------------------
    /// <summary>
    /// Apply an impulsive deltaV at the current position, re-compute orbital elements,
    /// and update elapsed time so the orbit picks up at the correct phase.
    /// </summary>
    /// <param name="deltaV"></param>
    public void ApplyDeltaVelocity(Vector3 deltaV)
    {
        CurrentVelocity += deltaV;
        InitializeOrbitParameters(CurrentVelocity);
        ComputeElapsedTimeFromCurrentPosition();
    }

    //---------------------- Orbit Initialization -----------------------------
    void InitializeOrbitParameters(Vector3 velocity)
    {
        Vector3 rVec = transform.position - CentralBody.position;
        mu = GravitationalConstants.G * centralBodyMass.mass;

        AngularMomentumVec = Vector3.Cross(rVec, velocity);

        EccentricityVec = Vector3.Cross(velocity, AngularMomentumVec) / mu - rVec.normalized;
        Eccentricity = EccentricityVec.magnitude;

        float specificEnergy = velocity.sqrMagnitude / 2f - mu / rVec.magnitude;
        SemiMajorAxis = -mu / (2f * specificEnergy);
        aCubed = SemiMajorAxis * SemiMajorAxis * SemiMajorAxis;

        Vector3 periapsisDir = EccentricityVec.normalized;
        Vector3 orbitalNormal = AngularMomentumVec.normalized;
        OrbitalPlaneRotation = Quaternion.LookRotation(periapsisDir, orbitalNormal);
    }

    //---------------------- Elliptical Propagation ---------------------------
    // TODO update this
    void UpdateEllipticalOrbit()
    {
        float meanMotion = Mathf.Sqrt(mu / Mathf.Pow(SemiMajorAxis, 3));
        float meanAnomaly = meanMotion * ElapsedTime;

        float eccentricAnomaly = SolveKeplerEccentricAnomaly(meanAnomaly, Eccentricity);
        float trueAnomaly = 2f * Mathf.Atan(
            Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)) *
            Mathf.Tan(eccentricAnomaly / 2f)
        );

        float distance = SemiMajorAxis * (1f - Eccentricity * Eccentricity) /
                        (1 + Eccentricity * Mathf.Cos(trueAnomaly));

        UpdatePositionAndVelocity(trueAnomaly, distance);
    }
    
    // TODO update this
    void UpdatePositionAndVelocity(float trueAnomaly, float r)
    {
        // Position
        Vector3 localPos = new Vector3(
            Mathf.Sin(trueAnomaly),
            0,
            Mathf.Cos(trueAnomaly)
        ) * r;
        transform.position = CentralBody.position + OrbitalPlaneRotation * localPos;

        // Velocity
        float vMag = Mathf.Sqrt(mu * (2 / r - 1 / SemiMajorAxis));
        CurrentVelocity = OrbitalPlaneRotation *
                new Vector3(
                    Eccentricity + Mathf.Cos(trueAnomaly),
                    0,
                    -Mathf.Sin(trueAnomaly)
                ).normalized * vMag;
    }

    //---------------------- Time Sync After deltaV ---------------------------
    /// <summary>
    /// Computes ElapsedTime such that at this orbital phase
    /// the analytic propagation picks up exactly from the current position.
    /// </summary>
    void ComputeElapsedTimeFromCurrentPosition()
    {
        Vector3 rVec = transform.position - CentralBody.position;
        Vector3 localPos = Quaternion.Inverse(OrbitalPlaneRotation) * rVec;
        localPos.y = 0f; // Project onto the orbital plane

        if (localPos.magnitude < 1e-6f)
        {
            ElapsedTime = 0f;
            return;
        }

        float theta = Mathf.Atan2(localPos.x, localPos.z);
        float e = Eccentricity;
        if (e >= 1f)
        {
            ElapsedTime = 0f;
            return;
        }

        float cosTheta = Mathf.Cos(theta);
        float sinTheta = Mathf.Sin(theta);
        float denominator = 1f + e * cosTheta;

        if (Mathf.Abs(denominator) < 1e-6f)
        {
            ElapsedTime = 0f;
            return;
        }

        float cosE = (e + cosTheta) / denominator;
        float sinE = (Mathf.Sqrt(1f - e * e) * sinTheta) / denominator;
        float E = Mathf.Atan2(sinE, cosE);

        float M = E - e * Mathf.Sin(E);
        float n = Mathf.Sqrt(mu / aCubed);
        ElapsedTime = M / n;
    }

    //---------------------- Kepler Solver ------------------------------------
    float SolveKeplerEccentricAnomaly(float M, float e, float tolerance = 1e-6f)
    {
        float E = M;
        float delta;
        int iterations = 0;
        const int maxIterations = 100;

        do
        {
            if (float.IsNaN(E) || float.IsInfinity(E)) return M;

            float f = E - e * Mathf.Sin(E) - M;
            float df = 1 - e * Mathf.Cos(E);
            if (Mathf.Abs(df) < 1e-9f) break;

            delta = f / df;
            E -= delta;
            iterations++;
        } while (Mathf.Abs(delta) > tolerance && iterations < maxIterations);

        return E;
    }
}