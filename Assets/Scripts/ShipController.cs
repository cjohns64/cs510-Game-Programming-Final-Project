using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float turnSpeedDegPerSec = 89.9f;
    private float thrustForceBackwardScale = 0.5f; // half of forward
    private float thrustScaleFactor = 0.1f;
    private const float inv_50 = 1.0f / 50.0f;
    private float thrust_mag;

    [Header("Resources")]
    public Slider thrust_slider;
    private TMP_Text thrust_text;
    public Animator shipAnimatior;
    public AudioSource thrustAudio;

    private OrbitMoverAnalytic orbitMoverAnalytic;
    private Transform centralBody;

    float GetThrustMagnitude()
    {
        return thrust_slider.value * thrustScaleFactor * inv_50;
    }

    void ThrustChange()
    {
        thrust_mag = GetThrustMagnitude();
        thrust_text.text = (thrust_mag / (thrustScaleFactor * inv_50)).ToString("0.##");
    }

    void Start()
    {
        orbitMoverAnalytic = GetComponent<OrbitMoverAnalytic>();
        centralBody = orbitMoverAnalytic.CentralBody;
        // setup thrust
        thrust_mag = GetThrustMagnitude();
        thrust_slider.onValueChanged.AddListener(delegate { ThrustChange(); });
        thrust_text = thrust_slider.gameObject.transform.Find("current_thrust").gameObject.GetComponent<TMP_Text>();
        ThrustChange();
        if (thrustAudio == null)
            thrustAudio = gameObject.GetComponent<AudioSource>();
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
            thrustMagnitude = GetThrustMagnitude();
        } else if (Input.GetKey(KeyCode.S))
        {
            thrustDirection = -transform.forward;
            thrustMagnitude = GetThrustMagnitude() * thrustForceBackwardScale;
        }

        thrustMagnitude *= multiplierCheat;

        if (thrustDirection != Vector3.zero)
        {
            ApplyThrust(thrustDirection * thrustMagnitude);

            if (!thrustAudio.isPlaying)
                thrustAudio.Play();
        }
        else
        {
            // inform animator
            shipAnimatior.SetBool("isImpulse", false);

            if (thrustAudio.isPlaying)
                thrustAudio.Stop();
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
