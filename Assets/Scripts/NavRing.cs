using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NavRing : MonoBehaviour
{
    private GameObject playerShip;
    private OrbitMoverAnalytic mover;
    private RectTransform rectTransform;
    private const float zeroVelocityEpsilon = 1e-6f;

    void Awake()
    {
        playerShip = GameObject.FindWithTag("Ship");
        if (playerShip == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        mover = playerShip.GetComponent<OrbitMoverAnalytic>();
        if (mover == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) 
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        Vector3 velocity = mover.state.velocity;
        if (velocity.sqrMagnitude < zeroVelocityEpsilon) return;

        Vector3 forward = mover.transform.forward;
        Vector3 velocityHat = velocity.normalized;

        float dot = Mathf.Clamp(Vector3.Dot(forward, velocityHat), -1f, 1f);
        float angleDeg = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float sign = Mathf.Sign(Vector3.Cross(velocityHat, forward).y);

        rectTransform.rotation = Quaternion.Euler(0, 0, angleDeg * sign);
    }
}
