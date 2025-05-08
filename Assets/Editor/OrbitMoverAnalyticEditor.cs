using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitMoverAnalytic))]
public class OrbitMoverAnalyticEditor : Editor
{
    private bool showOrbitShape = true;
    private bool showOrbitState = true;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        OrbitMoverAnalytic mover = (OrbitMoverAnalytic)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Orbit Data", EditorStyles.boldLabel);

        showOrbitShape = EditorGUILayout.Foldout(showOrbitShape, "Orbit Shape (Read-Only)");
        if (showOrbitShape && mover.shape != null)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField("mu", mover.shape.mu);
                EditorGUILayout.FloatField("a", mover.shape.a);
                EditorGUILayout.FloatField("e", mover.shape.e);
                EditorGUILayout.Vector3Field("Angular Momentum", mover.shape.AngularMomentumVec);
                EditorGUILayout.Vector3Field("Eccentricity Vector", mover.shape.EccentricityVec);
                EditorGUILayout.FloatField("p (Semi-Latus Rectum)", mover.shape.p);
                EditorGUILayout.FloatField("n (Mean Motion)", mover.shape.n);
                EditorGUILayout.FloatField("Period", mover.shape.period);
                EditorGUILayout.FloatField("rPeriapsis", mover.shape.rPeriapsis);
                EditorGUILayout.FloatField("rApoapsis", mover.shape.rApoapsis);
                EditorGUILayout.FloatField("h (Angular Momentum)", mover.shape.h);
                EditorGUILayout.FloatField("aCubed", mover.shape.aCubed);
            }
        }

        showOrbitState = EditorGUILayout.Foldout(showOrbitState, "Orbit State (Read-Only)");
        if (showOrbitState && mover.state != null)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Vector3Field("Velocity", mover.state.velocity);
                EditorGUILayout.FloatField("Elapsed Time", mover.state.ElapsedTime);
            }
        }
    }
}
