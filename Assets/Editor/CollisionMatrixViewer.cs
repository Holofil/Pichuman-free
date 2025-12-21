// File: Editor/CollisionMatrixViewer.cs

using UnityEditor;
using UnityEngine;

public class CollisionMatrixViewer : EditorWindow
{
    [MenuItem("Tools/Collision Matrix Viewer")]
    static void Init()
    {
        CollisionMatrixViewer window = (CollisionMatrixViewer)EditorWindow.GetWindow(typeof(CollisionMatrixViewer));
        window.titleContent = new GUIContent("Collision Matrix Viewer");
        window.Show();
    }

    void OnGUI()
    {
        int layerCount = 32;
        float labelWidth = 150;

        EditorGUILayout.LabelField("Physics Collision Matrix (Full View)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(labelWidth));
        for (int i = 0; i < layerCount; i++)
        {
            EditorGUILayout.LabelField(LayerMask.LayerToName(i), GUILayout.Width(20));
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < layerCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(LayerMask.LayerToName(i), GUILayout.Width(labelWidth));

            for (int j = 0; j < layerCount; j++)
            {
                bool currentValue = Physics.GetIgnoreLayerCollision(i, j);
                bool newValue = !EditorGUILayout.Toggle(!currentValue, GUILayout.Width(20));

                if (newValue != currentValue)
                {
                    Physics.IgnoreLayerCollision(i, j, newValue);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
