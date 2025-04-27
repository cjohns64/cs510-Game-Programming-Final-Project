using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SolarSystemManager : MonoBehaviour
{
    [System.Serializable]
    public class CelestialBodyData
    {
        public GameObject prefab;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public List<CelestialBodyData> moons = new List<CelestialBodyData>();
        [HideInInspector] public CelestialBody instance;
    }

    public List<CelestialBodyData> bodies = new List<CelestialBodyData>();
    public bool previewInEditor = true;

    void Awake()
    {
        InitializeSystem();
    }

    public void InitializeSystem()
    {
        ClearExistingBodies();
        CreateBodiesRecursive(bodies, transform);
    }

    void CreateBodiesRecursive(List<CelestialBodyData> bodyDataList, Transform parent)
    {
        foreach (var bodyData in bodyDataList)
        {
            if (bodyData.prefab == null) continue;

            // Instantiate and configure body
            var newBody = Instantiate(bodyData.prefab, parent);
            newBody.transform.localPosition = bodyData.initialPosition;

            var celestialBody = newBody.GetComponent<CelestialBody>();
            if (celestialBody != null)
            {
                celestialBody.InitialVelocity = bodyData.initialVelocity;
                bodyData.instance = celestialBody;
            }

            // Recursively create moons
            if (bodyData.moons.Count > 0)
            {
                var moonsParent = new GameObject("Moons").transform;
                moonsParent.SetParent(newBody.transform);
                moonsParent.localPosition = Vector3.zero;
                CreateBodiesRecursive(bodyData.moons, moonsParent);
            }
        }
    }

    void ClearExistingBodies()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void OnValidate()
    {
        if (previewInEditor && !Application.isPlaying)
        {
            InitializeSystem();
        }
    }

    // Editor methods
    public void AddNewBody() => bodies.Add(new CelestialBodyData());
    public void RemoveBody(int index) => bodies.RemoveAt(index);
}