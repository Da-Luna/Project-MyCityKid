#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TLS_DeveloperScripts;

// Custom Editor script to enhance the Inspector for TLS_DevScript component
[CustomEditor(typeof(TLS_DevScript))]
public class TLS_DevEditor : Editor
{
    // This method is called when drawing the Inspector GUI for TLS_DevScript
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector GUI for the target script
        DrawDefaultInspector();

        // Get the reference to the TLS_DevScript component being inspected
        _ = (TLS_DevScript)target;

        if (GUILayout.Button("Example Button"))
            Debug.Log("Example-Button was pressed! You can find this debug in the TLS_DevEditor.cs script or double click this line to open the TLS_DevEditor script");
    }
}
#endif