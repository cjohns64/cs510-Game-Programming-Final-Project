using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DistanceGraph : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public OrbitMoverAnalytic bodyA, bodyB;
    public int samples = 100;

    void Update()
    {
        List<float> distances = bodyA.CalculateDistanceOverPeriod(bodyB, samples);

        lineRenderer.positionCount = distances.Count;
        for (int i = 0; i < distances.Count; i++)
        {
            float x = i / (float)samples;
            float y = distances[i];
            lineRenderer.SetPosition(i, new Vector3(x * 10f, y, 0));
        }
    }
}