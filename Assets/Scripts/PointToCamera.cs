using UnityEngine;

public class PointToCamera : MonoBehaviour
{
    private Transform _camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup camera transform
        _camera = GameObject.FindWithTag("MainCamera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // calculate vector pointing from this object to camera
        Vector3 target = _camera.position - this.transform.position;
        this.transform.rotation = Quaternion.LookRotation(-target); // point at camera, no animation
    }
}
