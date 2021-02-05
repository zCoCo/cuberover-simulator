using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RockPlacer))]
public class RockPlacerEditor : Editor {
    RockPlacer placer;

    void OnEnable() {
        placer = (RockPlacer)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUILayout.Button("PlaceRocks")) {
            placer.PlaceRocks();
        }
        if (GUILayout.Button("UpdateAmount")) {
            placer.SetNumRocks();
        }

    }
}
