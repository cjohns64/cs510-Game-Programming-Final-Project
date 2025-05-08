using System.Collections;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Follow Target (ship or planet")]
    public Transform target;

    [Header("Click-to-Focus Settings")]
    [Tooltip("Which tags count as focusable (e.g. your Ship & Planet layers)")]
    public LayerMask focusableLayers;
    [Tooltip("Max raycast distance when clicking")]
    public float focusRayDistance = 1000f;
    [Tooltip("Time in seconds to smoothly transition focus")]
    public float focusDurationSeconds = 1f;

    [Header("Zoom Settings")]
    public float distance = 20f;
    public float zoomSpeed = 10f;
    public float minDistance = 5f;
    public float maxDistance = 100f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public float pitchMin = -30f;
    public float pitchMax = 90f;

    private float yaw = 0f;
    private float pitch = 20f;
    bool isFocusing = false;

    void Update()
    {
        HandleFocusClick();
        if (isFocusing) return;

        HandleZoom();
        HandleRotation();
        ApplyTransform();
    }

    void HandleFocusClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, focusRayDistance, focusableLayers))
            {
                StopAllCoroutines();
                StartCoroutine(FocusRoutine(hit.transform));
            }
        }
    }

    IEnumerator FocusRoutine(Transform newTarget)
    {
        isFocusing = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 offset = new Vector3(0, 0, -distance);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 endPos = newTarget.position + rot * offset;
        Quaternion endRot = Quaternion.LookRotation(newTarget.position - endPos, Vector3.up);

        float timer = 0f;
        while (timer < focusDurationSeconds)
        {
            float t = timer / focusDurationSeconds;
            t = Mathf.SmoothStep(0f, 1f, t);

            endPos = newTarget.position + rot * offset;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            timer += Time.deltaTime;
            yield return null;
        }


        transform.position = endPos;
        transform.rotation = endRot;
        target = newTarget;

        Vector3 eulers = transform.rotation.eulerAngles;
        yaw = eulers.y;
        pitch = eulers.x;

        isFocusing = false;
    }

    // Immediate focus
    public void FocusOn(Transform newTarget)
    {
        StopAllCoroutines();
        target = newTarget;
        Vector3 dir = (target.position - transform.position).normalized;
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        ApplyTransform();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }
    }

    void ApplyTransform()
    {
        if (target == null) return;

        // Compute camera position & orientation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}

