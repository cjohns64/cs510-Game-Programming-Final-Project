using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform camera;

    void Start()
    {
        if (camera == null)
        {
            camera = Camera.main.transform;
        }
    }

    void Update()
    {
        transform.LookAt(camera);
        transform.Rotate(new Vector3(1, 0, 0), 90);
    }
}
