using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform _camera;

    void Start()
    {
        if (_camera == null)
        {
            _camera = Camera.main.transform;
        }
    }

    void Update()
    {
        transform.LookAt(_camera);
        transform.Rotate(new Vector3(1, 0, 0), 90);
    }
}
