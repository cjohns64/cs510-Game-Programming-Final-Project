using UnityEngine;
using System;
using System.Collections.Generic;

public class OrbitMoverAnalytic : MonoBehaviour
{
    public enum InitializationType { InitialVelocity, CircularOrbit }

    //--------------------- Inspector Configuration ----------------------
    [Header("Central Body")]
    public Transform CentralBody;

    [Header("Initialization Mode")]
    public InitializationType initializationType = InitializationType.InitialVelocity;

    [Header("Initial Conditions")]
    [Tooltip("Required for Initial Velocity mode")]
    public Vector3 InitialVelocity;

    [Header("Time Settings")]
    public float TimeMultiplier = 1f;

    //--------------------- Public State ---------------------------------
    public OrbitShape shape;
    public OrbitState state;

    //--------------------- Events ---------------------------------------
    public event Action OnOrbitParametersChanged;

    //--------------------- Private State --------------------------------
    private CelestialBody centralCelestialBody;

    // TODO DELTE THIS IS TO QUICKLY VISUALIZE ENCOUNTER CODE
    public OrbitMoverAnalytic otherBody;
    // AAAAAHHHHHHH

    // --------------------- Unity Callbacks -----------------------------
    void Start()
    {
        if (CentralBody == null || !CentralBody.TryGetComponent(out centralCelestialBody))
        {
            Debug.LogError("Central body configuration error!");
            enabled = false;
            return;
        }

        InitializeOrbit();
    }

    void FixedUpdate()
    {
        UpdateOrbitState();
    }

    //--------------------- Core Methods ---------------------------------
    private void InitializeOrbit()
    {
        float mu = GravitationalConstants.G * centralCelestialBody.Mass;

        if (initializationType == InitializationType.CircularOrbit)
        {
            float circularVelocity = Mathf.Sqrt(mu / (transform.position - CentralBody.position).magnitude) * 1.0001f;
            Vector3 direction = Vector3.Cross(transform.position - CentralBody.position, Vector3.up).normalized;
            InitialVelocity = direction * circularVelocity;
        }

        shape = new OrbitShape(CentralBody.position, transform.position, InitialVelocity, mu);
        state = new OrbitState(shape);

        state.velocity = InitialVelocity;
        state.SyncElapsedTimeToCurrentPosition(transform.position, CentralBody.position);
    }

    private void UpdateOrbitState()
    {
        state.UpdateOrbit(Time.fixedDeltaTime * TimeMultiplier, CentralBody.position, transform);
    }

    //--------------------- Public API -----------------------------------
    /// <summary>
    /// Apply an impulsive deltaV at the current position, re-compute orbital elements,
    /// and update elapsed time so the orbit picks up at the correct phase.
    /// </summary>
    /// <param name="deltaV"></param>
    public void ApplyDeltaVelocity(Vector3 deltaV)
    {
        state.velocity += deltaV;
        shape.RecomputeOrbitShape(CentralBody.position, transform.position, state.velocity);

        const float ECCENTRICITY_THRESHOLD = 0.001f;
        const float CORRECTION_FACTOR = 0.1f;

        if (Mathf.Abs(1f - shape.e) < ECCENTRICITY_THRESHOLD)
        {
            Vector3 dir = Vector3.Cross(shape.AngularMomentumVec, shape.EccentricityVec).normalized;
            Debug.Log(dir.x + " " + dir.y + " " + dir.z);
            state.velocity += dir * CORRECTION_FACTOR;
            shape.RecomputeOrbitShape(CentralBody.position, transform.position, state.velocity);
        }
        state.SyncElapsedTimeToCurrentPosition(transform.position, CentralBody.position);
        OnOrbitParametersChanged?.Invoke();
    }

    /// <summary>
    /// Predicts where this body will be after deltaTime seconds,
    /// without affecting the running simulation.
    /// </summary>
    public Vector3 PredictPositionAtTime(float deltaTime)
    {
        return state.PredictPosition(deltaTime, CentralBody.position);
    }

    // Get the distances between this and other over time for one period of this.
    // Mostly useful for cool graphs.
    public List<float> CalculateDistanceOverPeriod(
        OrbitMoverAnalytic other,
        int samples = 100)
    {
        List<float> distances = new List<float>();
        float period = shape.period;

        for (int i = 0; i < samples; i++)
        {
            float time = (i / (float)samples) * period;
            Vector3 positionThis = PredictPositionAtTime(time);
            Vector3 positionOther = other.PredictPositionAtTime(time);
            distances.Add(Vector3.Distance(positionThis, positionOther));
        }

        return distances;
    }

    // Negative number means no encounter with other within one period of this
    //public float CalculateEncounterAnomaly(OrbitMoverAnalytic other, int samples = 100)
    //{
    //    CelestialBody otherBody = other.gameObject.GetComponent<CelestialBody>(); 
    //    if (!otherBody) return -1f;
    //    float period = shape.period;
    //    float timeStep = period / samples;
    //    float otherSoi = otherBody.SoiRadius;
    //    float otherSoiSqr = otherSoi * otherSoi;

    //    for (int i = 0; i < samples; i++)
    //    {
    //        float deltaTime = i * timeStep;
    //        Vector3 positionThis = PredictPositionAtTime(deltaTime);
    //        Vector3 positionOther = other.PredictPositionAtTime(deltaTime);

    //        if ((positionThis - positionOther).sqrMagnitude <= otherSoiSqr)
    //        {
    //            return state.ComputeTrueAnomalyInFuture(deltaTime);
    //        }
    //    }

    //    return -1.0f;

    //}

    public float CalculateEncounterAnomaly(OrbitMoverAnalytic other, int samples = 100, int refineIters = 5)
    {
        // 1) One‐time work:
        CelestialBody otherBody = other.gameObject.GetComponent<CelestialBody>();
        if (otherBody == null) return -1f;

        float P = shape.period;
        float dt = P / samples;
        float R2 = otherBody.SoiRadius * otherBody.SoiRadius;

        // Pre‐allocate delegates for f(t)
        //float f0 = 0f;
        Func<float, float> F = (t) => {
            Vector3 d = PredictPositionAtTime(t) - other.PredictPositionAtTime(t);
            return d.sqrMagnitude - R2;
        };

        // 2) Coarse sampling: find first sign‐change f((i-1)dt)>0 to f(i⋅dt)<=0
        float prevT = 0f;
        float prevF = F(prevT);
        for (int i = 1; i <= samples; i++)
        {
            float t = i * dt;
            float curF = F(t);
            if (curF <= 0f && prevF > 0f)
            {
                // bracket [prevT, t]
                float tLo = prevT, tHi = t;
                float fLo = prevF, fHi = curF;

                // 3) Refine via bisection (or false‐position, etc.)
                float tMid = 0;
                for (int k = 0; k < refineIters; k++)
                {
                    tMid = 0.5f * (tLo + tHi);
                    float fMid = F(tMid);

                    // Replace the half‐interval that still brackets the root
                    if (fMid > 0f)
                    {
                        tLo = tMid;
                        fLo = fMid;
                    }
                    else
                    {
                        tHi = tMid;
                        fHi = fMid;
                    }
                }

                // 4) Compute and return the true anomaly at the refined time
                return state.ComputeTrueAnomalyInFuture(tMid);
            }

            prevT = t;
            prevF = curF;
        }

        // no encounter
        return -1f;
    }



}

[System.Serializable]
public class OrbitShape
{
    // Orbital Parameters
    public float mu { get; private set; } // Gravitational Parameter
    public float a { get; private set; } // Semi-Major Axis
    public float e { get; private set; } // Eccentricity
    public Vector3 AngularMomentumVec { get; private set; }
    public Vector3 EccentricityVec { get; private set; }
    public Quaternion OrbitalPlaneRotation { get; private set; }

    // Derived Fields
    public bool IsClosedOrbit => e < 1f;
    public float p { get; private set; } // semi-latus rectum
    public float n { get; private set; } // mean motion
    public float period { get; private set; } // 2pi/N
    public float rPeriapsis { get; private set; }
    public float rApoapsis { get; private set; }
    public float h { get; private set; } // specific angular momentum
    public float aCubed { get; private set; } // cached commonly used value

    public OrbitShape(Vector3 centralBodyPosition, Vector3 position, Vector3 velocity, float mu)
    {
        this.mu = mu;
        RecomputeOrbitShape(centralBodyPosition, position, velocity);
    }

    public void RecomputeOrbitShape(Vector3 centralBodyPosition, Vector3 position, Vector3 velocity)
    {
        Vector3 rVec = position - centralBodyPosition;
        float rMag = rVec.magnitude;
        Vector3 rNorm = rVec / rMag;

        AngularMomentumVec = Vector3.Cross(rVec, velocity);
        EccentricityVec = Vector3.Cross(velocity, AngularMomentumVec) / mu - rNorm;
        e = EccentricityVec.magnitude;

        float specificEnergy = velocity.sqrMagnitude / 2f - mu / rMag;
        a = -mu / (2f * specificEnergy);

        Vector3 periapsisDir = EccentricityVec.normalized;
        Vector3 orbitalNormal = AngularMomentumVec.normalized;
        OrbitalPlaneRotation = Quaternion.LookRotation(periapsisDir, orbitalNormal);

        // Compute derived parameters
        p = a * (1 - e * e);
        aCubed = a * a * a;
        n = Mathf.Sqrt(mu / Mathf.Abs(aCubed));
        period = (2 * Mathf.PI) / n;
        rPeriapsis = a * (1 - e);
        rApoapsis = a * (1 + e);
        h = Mathf.Sqrt(mu * p);
    }
}

[System.Serializable]
public class OrbitState
{
    private OrbitShape shape;
    public Vector3 velocity { get; internal set; } = Vector3.zero;
    public float ElapsedTime { get; private set; } = 0f;
    public float theta { get; private set; }
    public float r { get; private set; }

    public OrbitState(OrbitShape shape) => this.shape = shape;

    // Compute OrbitState given OrbitState.velocity and OrbitShape
    public void SyncElapsedTimeToCurrentPosition(Vector3 currentPosition, Vector3 centralBodyPosition)
    {
        Vector3 rVec = currentPosition - centralBodyPosition;
        Vector3 localPos = Quaternion.Inverse(shape.OrbitalPlaneRotation) * rVec;
        localPos.y = 0f;

        if (localPos.magnitude < 1e-6f)
        {
            ElapsedTime = 0f;
            return;
        }

        theta = Mathf.Atan2(localPos.x, localPos.z);
        ComputeTimeFromTrueAnomaly(theta);
    }

    public void ComputeTimeFromTrueAnomaly(float theta)
    {
        float cosTheta = Mathf.Cos(theta);
        float denominator = 1f + shape.e * cosTheta;

        if (Mathf.Abs(denominator) < 1e-6f)
        {
            ElapsedTime = 0f;
            return;
        }

        if (shape.IsClosedOrbit)
        {
            float cosE = (shape.e + cosTheta) / denominator;
            float sinE = Mathf.Sqrt(1f - shape.e * shape.e) * Mathf.Sin(theta) / denominator;
            float E = Mathf.Atan2(sinE, cosE);
            float M = E - shape.e * Mathf.Sin(E);
            ElapsedTime = M / shape.n;
        }
        else
        {
            float coshH = (shape.e + cosTheta) / denominator;
            float H = Mathf.Log(coshH + Mathf.Sqrt(coshH * coshH - 1f));
            float M = shape.e * Sinh(H) - H;
            ElapsedTime = M / shape.n;
        }
    }

    public float ComputeTrueAnomalyInFuture(float deltaTime)
    {
        if (shape.IsClosedOrbit)
        {
            float M = shape.n * (ElapsedTime + deltaTime);
            float E = SolveKeplerEquation(M, shape.e, true);
            return 2f * Mathf.Atan(
                Mathf.Sqrt((1 + shape.e) / (1 - shape.e)) *
                Mathf.Tan(E / 2f)
            );
        }
        else
        {
            float M = shape.n * (ElapsedTime + deltaTime);
            float H = SolveKeplerEquation(M, shape.e, false);
            return 2f * Mathf.Atan(
                Mathf.Sqrt((shape.e + 1) / (shape.e - 1)) *
                Tanh(H / 2f)
            );
        }
    }

    // Compute Rest of orbit state given a new time.
    public void UpdateOrbit(float deltaTime, Vector3 centralBodyPosition, Transform transform)
    {
        ElapsedTime += deltaTime;

        if (shape.IsClosedOrbit)
            UpdateEllipticalOrbit(centralBodyPosition, transform);
        else
            UpdateHyperbolicOrbit(centralBodyPosition, transform);
    }

    void UpdateEllipticalOrbit(Vector3 centralBodyPosition, Transform transform)
    {
        float M = shape.n * ElapsedTime;

        float E = SolveKeplerEquation(M, shape.e, true);
        theta = 2f * Mathf.Atan(
            Mathf.Sqrt((1 + shape.e) / (1 - shape.e)) *
            Mathf.Tan(E / 2f)
        );

        r = shape.a * (1f - shape.e * shape.e) /
                        (1 + shape.e * Mathf.Cos(theta));

        UpdatePositionAndVelocity(centralBodyPosition, transform);
    }

    //---------------------- Hyperbolic Propagation ---------------------------
    void UpdateHyperbolicOrbit(Vector3 centralBodyPosition, Transform transform)
    {
        float M = shape.n * ElapsedTime;

        float H = SolveKeplerEquation(M, shape.e, false);
        theta = 2f * Mathf.Atan(
            Mathf.Sqrt((shape.e + 1) / (shape.e - 1)) *
            Tanh(H / 2f)
        );

        r = shape.a * (shape.e * shape.e - 1) / (1 + shape.e * Mathf.Cos(theta));

        UpdatePositionAndVelocity(centralBodyPosition, transform);
    }

    void UpdatePositionAndVelocity(Vector3 centralBodyPosition, Transform transform)
    {
        // Position in local orbital plane
        Vector3 localPos = new Vector3(
            Mathf.Sin(theta),
            0,
            Mathf.Cos(theta)
        ) * r;
        transform.position = centralBodyPosition + shape.OrbitalPlaneRotation * localPos;

        // Velocity
        float vMag = Mathf.Sqrt(shape.mu * (2 / r - 1 / shape.a));
        velocity = shape.OrbitalPlaneRotation *
                new Vector3(
                    shape.e + Mathf.Cos(theta),
                    0,
                    -Mathf.Sin(theta)
                ).normalized * vMag;
    }

    private float SolveKeplerEquation(float M, float e, bool elliptical)
    {
        float guess = M;
        float delta;
        int iterations = 0;
        const int maxIterations = 10;
        const float tolerance = 1e-6f;

        do
        {
            float f, df, d2f;
            if (elliptical)
            {
                float sinGuess = Mathf.Sin(guess);
                float cosGuess = Mathf.Cos(guess);
                f = guess - e * sinGuess - M;
                df = 1 - e * cosGuess;
                d2f = e * sinGuess;
            }
            else
            {
                float sinhGuess = Sinh(guess);
                float coshGuess = Cosh(guess);
                f = e * sinhGuess - guess - M;
                df = e * coshGuess - 1;
                d2f = e * sinhGuess;
            }

            if (Mathf.Abs(df) < 1e-9f) break;
            delta = f / (df - (f * d2f) / (2 * df + 1e-9f));
            guess -= delta;
        } while (Mathf.Abs(delta) > tolerance && ++iterations < maxIterations);

        return guess;
    }

    const float Epsilon = 1e-6f;

    private float Sinh(float x)
    {
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2f;
    }

    private float Cosh(float x)
    {
        return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2f;
    }

    private float Tanh(float x)
    {
        float ex = Mathf.Exp(x);
        float e_x = Mathf.Exp(-x);
        return (ex - e_x) / (ex + e_x + Epsilon);
    }

    public Vector3 PredictPosition(float deltaTime, Vector3 centralBodyPosition)
    {
        float futureTime = ElapsedTime + deltaTime;
        float M = shape.n * futureTime;

        float theta, r;
        if (shape.IsClosedOrbit)
        {
            float E = SolveKeplerEquation(M, shape.e, true);
            theta = 2f * Mathf.Atan(
                Mathf.Sqrt((1 + shape.e) / (1 - shape.e)) *
                Mathf.Tan(E / 2f)
            );
            r = shape.a * (1f - shape.e * shape.e) /
                (1f + shape.e * Mathf.Cos(theta));
        }
        else
        {
            float H = SolveKeplerEquation(M, shape.e, false);
            theta = 2f * Mathf.Atan(
                Mathf.Sqrt((shape.e + 1f) / (shape.e - 1f)) *
                Tanh(H / 2f)
            );
            r = shape.a * (shape.e * shape.e - 1f) /
                (1f + shape.e * Mathf.Cos(theta));
        }

        Vector3 localPos = new Vector3(
            Mathf.Sin(theta), 0f, Mathf.Cos(theta)
        ) * r;
        return centralBodyPosition + shape.OrbitalPlaneRotation * localPos;
    }
}