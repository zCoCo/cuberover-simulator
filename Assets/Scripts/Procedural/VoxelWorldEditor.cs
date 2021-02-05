using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Voxel.VoxelWorld))]
public class VoxelWorldEditor : Editor {
    Voxel.VoxelWorld voxelWorld;

    void OnEnable() {
        voxelWorld = (Voxel.VoxelWorld)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUILayout.Button("Refresh")) {
            voxelWorld.Initialize();
            voxelWorld.Culling();
        }
        if (GUILayout.Button("Clear")) {
            voxelWorld.ClearWorld();
        }

    }
}
