using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionMapTest : MonoBehaviour
{
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void OnGUI()
    {
        Vector3 point = new Vector3();
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2 {
            // Get the mouse position from Event.
            // Note that the y position from Event is inverted.
            x = currentEvent.mousePosition.x,
            y = cam.pixelHeight - currentEvent.mousePosition.y
        };

        point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

        GUILayout.BeginArea(new Rect(20, 20, 400, 120));
        GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F5"));
        GUILayout.EndArea();
    }
}
