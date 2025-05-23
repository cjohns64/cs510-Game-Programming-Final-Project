using UnityEngine;

[ExecuteAlways]
public class CancelRotation : MonoBehaviour
{
    private Quaternion initialLocalRotation;

    void Awake()
    {
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (transform.parent == null)
            return;

        Quaternion parentRotation = transform.parent.parent.parent.rotation;
        // transform.rotation = Quaternion.Inverse(parentRotation) * initialLocalRotation;
        transform.rotation = Quaternion.Inverse(parentRotation);
    }
}
