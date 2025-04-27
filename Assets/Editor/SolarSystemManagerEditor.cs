using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SolarSystemManager))]
public class SolarSystemManagerEditor : Editor
{
    SolarSystemManager manager;

    void OnEnable()
    {
        manager = (SolarSystemManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default properties
        DrawPropertiesExcluding(serializedObject, "bodies");

        // Custom bodies list
        EditorGUILayout.LabelField("Celestial Bodies", EditorStyles.boldLabel);

        for (int i = 0; i < manager.bodies.Count; i++)
        {
            EditorGUILayout.BeginVertical("Box");

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Body {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove"))
                manager.RemoveBody(i);
            EditorGUILayout.EndHorizontal();

            // Body properties
            var body = manager.bodies[i];
            body.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", body.prefab, typeof(GameObject), false);
            body.initialPosition = EditorGUILayout.Vector3Field("Position", body.initialPosition);
            body.initialVelocity = EditorGUILayout.Vector3Field("Velocity", body.initialVelocity);

            // Moons
            EditorGUILayout.LabelField("Moons");
            for (int j = 0; j < body.moons.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                body.moons[j].prefab = (GameObject)EditorGUILayout.ObjectField(body.moons[j].prefab, typeof(GameObject), false);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    body.moons.RemoveAt(j);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Moon"))
                body.moons.Add(new SolarSystemManager.CelestialBodyData());

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Body"))
            manager.AddNewBody();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
            if (manager.previewInEditor)
                manager.InitializeSystem();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (!manager.previewInEditor) return;

        foreach (var body in manager.bodies)
        {
            if (body.prefab == null) continue;

            // Draw position handles
            body.initialPosition = Handles.PositionHandle(body.initialPosition, Quaternion.identity);

            // Draw velocity vector
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0,
                body.initialPosition,
                Quaternion.LookRotation(body.initialVelocity.normalized),
                body.initialVelocity.magnitude * 0.1f,
                EventType.Repaint);
        }
    }
}