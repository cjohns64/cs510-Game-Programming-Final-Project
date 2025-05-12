using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float turnSpeedDegPerSec = 89.9f;
    public float thrustForceForward = 0.1f;
    public float thrustForceBackward = 0.05f;
    public Animator shipAnimatior;

    private OrbitMoverAnalytic orbitMoverAnalytic;
    private Transform centralBody;

    void Start()
    {
        orbitMoverAnalytic = GetComponent<OrbitMoverAnalytic>();
        centralBody = orbitMoverAnalytic.CentralBody;
    }

    void Update()
    {
        HandleRotation();
        HandleThrust();
        EnforceYRotation();
    }

    void HandleRotation()
    {
        bool wantPrograde = Input.GetKey(KeyCode.P);
        bool wantRetrograde = Input.GetKey(KeyCode.R);

        if (wantPrograde || wantRetrograde)
        {
            Vector3 velocity = orbitMoverAnalytic.state.velocity;

            // Project velocity onto central body's XZ plane
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, centralBody.up);

            if (horizontalVelocity.magnitude > 0.1f)
            {
                // Calculate target direction in world space
                Vector3 targetDir = wantPrograde ?
                    horizontalVelocity.normalized :
                    -horizontalVelocity.normalized;

                // Create rotation that only affects Y-axis
                Quaternion targetRotation = Quaternion.LookRotation(targetDir, centralBody.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeedDegPerSec * Time.deltaTime
                );
            }
        }
        else
        {
            // Manual rotation
            float turnInput = Input.GetKey(KeyCode.A) ? -1 : 0;
            turnInput += Input.GetKey(KeyCode.D) ? 1 : 0;
            transform.Rotate(Vector3.up, turnInput * turnSpeedDegPerSec * Time.deltaTime);
        }
    }

    void HandleThrust()
    {
        Vector3 thrustDirection = Vector3.zero;
        float thrustMagnitude = 0f;
        float multiplierCheat = Input.GetKey(KeyCode.X) ? 5f : 1f;

        if (Input.GetKey(KeyCode.W))
        {
            thrustDirection = transform.forward;
            thrustMagnitude = thrustForceForward;
        } else if (Input.GetKey(KeyCode.S))
        {
            thrustDirection = -transform.forward;
            thrustMagnitude = thrustForceBackward;
        }

        thrustMagnitude *= multiplierCheat;

        if (thrustDirection != Vector3.zero)
        {
            ApplyThrust(thrustDirection * thrustMagnitude);
        }
        else
        {
            // inform animator
            shipAnimatior.SetBool("isImpulse", false);
        }
    }

    void EnforceYRotation()
    {
        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0, currentRotation.y, 0);
    }

    void ApplyThrust(Vector3 thrustVector)
    {
        orbitMoverAnalytic.ApplyDeltaVelocity(thrustVector * Time.deltaTime);
        // inform animator
        shipAnimatior.SetBool("isImpulse", true);
    }
}
