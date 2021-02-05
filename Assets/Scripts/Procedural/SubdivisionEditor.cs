using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Subdivision))]
public class SubdivisionEditor : Editor {
    public override void OnInspectorGUI() {
        Subdivision subdiv = (Subdivision)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Process Mesh")) {
            subdiv.Test();
        }
    }
}