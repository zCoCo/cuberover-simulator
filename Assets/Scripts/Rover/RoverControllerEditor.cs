using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RoverController))]
public class RoverControllerEditor : Editor {
    public override void OnInspectorGUI() {
        RoverController controller = (RoverController)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Generate Wheels")) {
            controller.GenerateWheels();
        }
    }
}