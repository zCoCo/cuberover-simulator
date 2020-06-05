/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImageSynthesis))]
[AddComponentMenu("Iris/Util/ImageSaver")] // Put in add component menu in Unity editor
public class ImageSaver : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ImageSynthesis imageSynthesis = (ImageSynthesis)target;

        // Only display the "Save" button if playing
        if (EditorApplication.isPlaying && GUILayout.Button("Save Captures")) 
        {
            Vector2 gameViewSize = Handles.GetMainGameViewSize();
            imageSynthesis.Save(imageSynthesis.filename, width: (int)gameViewSize.x, height: (int)gameViewSize.y, imageSynthesis.filepath);
        }
    }
}*/
