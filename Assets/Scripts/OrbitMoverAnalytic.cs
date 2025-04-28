using UnityEngine;
using System;

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
    public OrbitShape CurrentOrbitShape { get; private set; }
    public OrbitState CurrentOrbitState { get; private set; }

    private CelestialBody centralCelestialBody;

    [ContextMenu("Print Orbit Info")]
    public void PrintOrbitInfo()
    {
        if (CurrentOrbitShape == null || CurrentOrbitState == null)
        {
            Debug.LogWarning("[OrbitMoverAnalytic] Shape or State is null; nothing to print.");
            return;
        }

        Debug.Log(CurrentOrbitShape.ToString());
        Debug.Log(CurrentOrbitState.ToString());
    }

    void Awake()
    {
        if (!TryInitCentralBody()) { enabled = false; return; }

        CurrentOrbitState = new OrbitState(
            transform.position,
            InitialVelocity
        );

    }

    void Start()
    {
        if (!InitializeOrbit(InitialVelocity)) enabled = false;
        RecomputeElapsedFromPosition();
    }

    void FixedUpdate()
    {
        AdvanceTime();
        UpdateOrbitState();
        ApplyPosition();
    }

    public void ApplyDeltaVelocity(Vector3 deltaVelocity)
    {
        PrintOrbitInfo();
        CurrentOrbitState.Velocity += deltaVelocity;
        if (!InitializeOrbit(CurrentOrbitState.Velocity)) enabled = false;
        CurrentOrbitState.UpdateFromTrueAnomaly(
            CurrentOrbitState.TrueAnomaly,
            CurrentOrbitShape,
            CentralBody
        );
        ApplyPosition();
        RecomputeElapsedFromPosition();
        PrintOrbitInfo();
    }

    private bool TryInitCentralBody()
    {
        if (CentralBody == null || !CentralBody.TryGetComponent(out centralCelestialBody))
        {
            Debug.LogError("[OrbitMoverAnalytic] Central Body Initialization Error.");
            return false;
        }

        return true;
    }

    private void AdvanceTime()
    {
        CurrentOrbitState.ElapsedTime += Time.fixedDeltaTime * TimeMultiplier;
    }

    private void ApplyPosition()
    {
        transform.position = CurrentOrbitState.Position;
    }

    private bool InitializeOrbit(Vector3 velocity)
    {
        Vector3 rVec = transform.position - CentralBody.position;
        float r = rVec.magnitude;
        if (r < 1e-3)
        {
            Debug.LogError("[OrbitMoverAnalytic] Orbit Radius is zero-cannot initialize.");
            return false;
        }
        if (velocity.magnitude < 1e-3f)
        {
            Debug.LogError("[OrbitMoverAnalytic] Initial velocity is too small-please set a non-zero InitialVelocity..");
            return false;
        }

        float mu = GravitationalConstants.G * centralCelestialBody.Mass;
        CurrentOrbitShape = new OrbitShape(rVec, velocity, mu);

        if (float.IsNaN(CurrentOrbitShape.SemiMajorAxis) || float.IsInfinity(CurrentOrbitShape.Eccentricity))
        {
            Debug.LogError("[OrbitMoverAnalytic] OrbitShape came back invalid—check initial conditions.");
            return false;
        }
        return true;
    }

    private void UpdateOrbitState()
    {
        float meanAnomaly = CalculateMeanAnomaly();
        float anomaly = CurrentOrbitShape.IsClosedOrbit
            ? SolveKeplerEccentricAnomaly(meanAnomaly)
            : SolveKeplerHyperbolicAnomaly(meanAnomaly);

        CurrentOrbitState.UpdateFromTrueAnomaly(anomaly, CurrentOrbitShape, CentralBody);
    }

    private float CalculateMeanAnomaly()
    {
        float a = CurrentOrbitShape.SemiMajorAxis;
        float n = Mathf.Sqrt(CurrentOrbitShape.GravitationalParameter / (a * a * a * (CurrentOrbitShape.IsClosedOrbit ? 1 : -1)));

        return n * CurrentOrbitState.ElapsedTime;
    }

    private void RecomputeElapsedFromPosition()
    {
        const float EPS = 1e-6f;
        var shape = CurrentOrbitShape;
        if (shape == null || CentralBody == null) return;

        // 1) Project into orbital plane
        Vector3 rVec = transform.position - CentralBody.position;
        Vector3 localPos = Quaternion.Inverse(shape.OrbitalPlaneRotation) * rVec;
        localPos.y = 0f;

        if (localPos.magnitude < EPS)
        {
            CurrentOrbitState.ElapsedTime = 0f;
            //CurrentOrbitState.TrueAnomaly = 0f;
            return;
        }

        // 2) True anomaly θ ∈ (-π, π]
        float theta = Mathf.Atan2(localPos.x, localPos.z);
        //CurrentOrbitState.TrueAnomaly = theta;

        float e = shape.Eccentricity;
        float a = shape.SemiMajorAxis;
        float mu = shape.GravitationalParameter;

        // 3) Elliptical case (e < 1)
        if (e < 1f)
        {
            float cosT = Mathf.Cos(theta);
            float sinT = Mathf.Sin(theta);
            float denom = 1f + e * cosT;

            if (Mathf.Abs(denom) < EPS)
            {
                CurrentOrbitState.ElapsedTime = 0f;
                return;
            }

            // Eccentric anomaly E:
            float sqrt1me2 = Mathf.Sqrt(1f - e * e);
            float sinE = sqrt1me2 * sinT / denom;
            float cosE = (e + cosT) / denom;
            float E = Mathf.Atan2(sinE, cosE);

            // Normalize E into [0, 2π)
            //if (E < 0f) E += 2f * Mathf.PI;

            // Mean anomaly M = E − e sin E
            float M = E - e * Mathf.Sin(E);

            // Normalize M into [0, 2π)
            //if (M < 0f) M += 2f * Mathf.PI;
            //else if (M >= 2f * Mathf.PI) M -= 2f * Mathf.PI;

            // Mean motion n = sqrt(μ/a³)
            float n = Mathf.Sqrt(mu / Mathf.Abs(a * a * a));

            CurrentOrbitState.ElapsedTime = M / n;
        }
        // 4) Hyperbolic case (e ≥ 1), unchanged
        else
        {
            float cosT = Mathf.Cos(theta);
            float denom = 1f + e * cosT;
            if (Mathf.Abs(denom) < EPS)
            {
                CurrentOrbitState.ElapsedTime = 0f;
                return;
            }

            float coshH = (e + cosT) / denom;
            //coshH = Mathf.Max(coshH, 1f + EPS);   // clamp domain
            float H = Mathf.Log(coshH + Mathf.Sqrt(coshH * coshH - 1f));

            float M = e * Sinh(H) - H;
            float n = Mathf.Sqrt(-mu / (a * a * a));

            CurrentOrbitState.ElapsedTime = M / n;
        }
    }



    private float SolveKeplerEccentricAnomaly(float M, float tolerance = 1e-6f)
    {
        if (CurrentOrbitShape == null) throw new InvalidOperationException("Orbit parameters not initialized");

        float e = CurrentOrbitShape.Eccentricity;
        float E = M;
        int iterations = 0;
        const int maxIterations = 100;

        float sinE;
        float f;
        float df;
        float delta;

        do
        {
            if (float.IsNaN(E) || float.IsInfinity(E)) return M;

            sinE = Mathf.Sin(E);
            f = E - e * sinE - M;
            df = 1 - e * Mathf.Cos(E);

            if (Mathf.Abs(df) < 1e-9f) break;

            delta = f / df;
            E -= delta;

            if (++iterations >= maxIterations) break;

        } while (Mathf.Abs(f) > tolerance);

        return E;
    }

    private float SolveKeplerHyperbolicAnomaly(float M, float tolerance = 1e-6f)
    {
        if (CurrentOrbitShape == null) throw new InvalidOperationException("Orbit parameters not initialized");

        float e = CurrentOrbitShape.Eccentricity;
        float H = M;
        int iterations = 0;
        const int maxIterations = 100;
        const float epsilon = 1e-9f;

        float sh;
        float ch;
        float f;
        float df;
        float delta;

        do
        {
            if (float.IsNaN(H) || float.IsInfinity(H)) return M;

            sh = Sinh(H);
            ch = Cosh(H);
            f = e * sh - H - M;
            df = e * ch - 1;

            if (Mathf.Abs(df) < epsilon) break;

            delta = f / (df + epsilon);
            H -= delta * 0.5f;  // Damped Newton-Raphson for stability

            if (++iterations >= maxIterations) break;

        } while (Mathf.Abs(f) > tolerance);

        return H;
    }

    private bool SanityCheckAll(string contextTag = "")
    {
        bool ok = true;
        Action<string, float> chkF = (n, v) =>
        {
            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                Debug.LogError($"[SanityCheck:{contextTag}] {n} is invalid: {v}");
                ok = false;
            }
        };
        Action<string, Vector3> chkV3 = (n, v) =>
        {
            chkF($"{n}.x", v.x);
            chkF($"{n}.y", v.y);
            chkF($"{n}.z", v.z);
        };

        // 1) Central body & mu
        if (centralCelestialBody == null)
        {
            Debug.LogError($"[SanityCheck:{contextTag}] centralCelestialBody is null");
            return false;
        }
        float mu = GravitationalConstants.G * centralCelestialBody.Mass;
        chkF("G", GravitationalConstants.G);
        chkF("centralCelestialBody.Mass", centralCelestialBody.Mass);
        chkF("mu", mu);

        // 2) OrbitShape
        if (CurrentOrbitShape == null)
        {
            Debug.LogError($"[SanityCheck:{contextTag}] CurrentOrbitShape is null");
            ok = false;
        }
        else
        {
            chkF("Shape.SemiMajorAxis", CurrentOrbitShape.SemiMajorAxis);
            chkF("Shape.Eccentricity", CurrentOrbitShape.Eccentricity);
            chkF("Shape.GravitationalParameter", CurrentOrbitShape.GravitationalParameter);
            chkV3("Shape.AngularMomentum", CurrentOrbitShape.AngularMomentum);
            chkV3("Shape.EccentricityVector", CurrentOrbitShape.EccentricityVector);
            chkF("Shape.OrbitalPlaneRot.x", CurrentOrbitShape.OrbitalPlaneRotation.x);
            chkF("Shape.OrbitalPlaneRot.y", CurrentOrbitShape.OrbitalPlaneRotation.y);
            chkF("Shape.OrbitalPlaneRot.z", CurrentOrbitShape.OrbitalPlaneRotation.z);
            chkF("Shape.OrbitalPlaneRot.w", CurrentOrbitShape.OrbitalPlaneRotation.w);
        }

        // 3) OrbitState
        if (CurrentOrbitState == null)
        {
            Debug.LogError($"[SanityCheck:{contextTag}] CurrentOrbitState is null");
            ok = false;
        }
        else
        {
            chkV3("State.Position", CurrentOrbitState.Position);
            chkV3("State.Velocity", CurrentOrbitState.Velocity);
            chkF("State.TrueAnomaly", CurrentOrbitState.TrueAnomaly);
            chkF("State.ElapsedTime", CurrentOrbitState.ElapsedTime);
        }

        // 4) Transform vs CentralBody
        chkV3("transform.position", transform.position);
        chkV3("CentralBody.position", CentralBody.position);
        Vector3 r = transform.position - CentralBody.position;
        chkV3("rVector", r);

        if (!ok)
            enabled = false;  // optionally halt further updates

        return ok;
    }



    //---------------------- Math Utilities -----------------------------------
    const float Epsilon = 1e-6f;

    float Sinh(float x)
    {
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2f;
    }

    float Cosh(float x)
    {
        return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2f;
    }

    float Tanh(float x)
    {
        float ex = Mathf.Exp(x);
        float e_x = Mathf.Exp(-x);
        return (ex - e_x) / (ex + e_x + Epsilon);
    }
}


[System.Serializable]
public class OrbitShape
{
    public float SemiMajorAxis { get; private set; }
    public float Eccentricity { get; private set; }
    public Vector3 AngularMomentum { get; private set; }
    public Vector3 EccentricityVector { get; private set; }
    public Quaternion OrbitalPlaneRotation { get; private set; }
    public float GravitationalParameter { get; private set; }

    public bool IsClosedOrbit => Eccentricity < 1f;

    public OrbitShape(Vector3 position, Vector3 velocity, float mu)
    {
        CalculateOrbitalElements(position, velocity, mu);
    }

    private void CalculateOrbitalElements(Vector3 rVec, Vector3 velocity, float mu)
    {
        float rMag = rVec.magnitude;
        if (rMag < 1e-6f) throw new InvalidOperationException("Position too close to central body.");

        AngularMomentum = Vector3.Cross(rVec, velocity);

        EccentricityVector = Vector3.Cross(velocity, AngularMomentum) / mu - rVec.normalized;
        Eccentricity = EccentricityVector.magnitude;

        float specificEnergy = velocity.sqrMagnitude / 2f - mu / rVec.magnitude;
        SemiMajorAxis = -mu / (2f * specificEnergy);

        Vector3 periapsisDir = EccentricityVector.normalized;
        Vector3 orbitalNormal = AngularMomentum.normalized;
        OrbitalPlaneRotation = Quaternion.LookRotation(periapsisDir, orbitalNormal);

        GravitationalParameter = mu;
    }

    public override string ToString()
    {
        return
            $"OrbitShape:\n" +
            $"- SemiMajorAxis: {SemiMajorAxis:F4}\n" +
            $"- Eccentricity: {Eccentricity:F4}\n" +
            $"- μ: {GravitationalParameter:E4}\n" +
            $"- AngularMomentum: {AngularMomentum}\n" +
            $"- EccVector: {EccentricityVector}\n" +
            $"- PlaneRot: {OrbitalPlaneRotation.eulerAngles}\n" +
            $"- IsClosedOrbit: {IsClosedOrbit}\n";
    }
}

public class OrbitState
{
    public float TrueAnomaly { get; internal set; }
    public float ElapsedTime { get; set; }
    public Vector3 Velocity { get; internal set; }
    public Vector3 Position { get; internal set; }

    public OrbitState(Vector3 initialPosition, Vector3 initialVelocity)
    {
        Position = initialPosition;
        Velocity = initialVelocity;
        TrueAnomaly = 0f;
        ElapsedTime = 0f;
    }

    public void UpdateFromTrueAnomaly(float trueAnomaly, OrbitShape orbitShape, Transform centralBody)
    {
        TrueAnomaly = trueAnomaly;

        float r = orbitShape.SemiMajorAxis * (1 - orbitShape.Eccentricity * orbitShape.Eccentricity) / (1 + orbitShape.Eccentricity * Mathf.Cos(trueAnomaly));
        r *= orbitShape.IsClosedOrbit ? 1 : -1;

        Vector3 localPosition = orbitShape.IsClosedOrbit
                ? new Vector3(Mathf.Sin(trueAnomaly), 0, Mathf.Cos(trueAnomaly)) * r
                : new Vector3(-Mathf.Sin(trueAnomaly), 0, -Mathf.Cos(trueAnomaly)) * r;

        Position = centralBody.position + orbitShape.OrbitalPlaneRotation * localPosition;

        float velocityMagnitude = Mathf.Sqrt(orbitShape.GravitationalParameter * (2 / r - 1 / orbitShape.SemiMajorAxis));
        Velocity = orbitShape.OrbitalPlaneRotation * new Vector3(
            orbitShape.Eccentricity + Mathf.Cos(trueAnomaly),
            0,
            -Mathf.Sin(trueAnomaly)
        ).normalized * velocityMagnitude;
    }

    public override string ToString()
    {
        return
            $"OrbitState:\n" +
            $"- Position: {Position}\n" +
            $"- Velocity: {Velocity}\n" +
            $"- TrueAnomaly: {TrueAnomaly:F4}\n" +
            $"- ElapsedTime: {ElapsedTime:F4}";
    }
}