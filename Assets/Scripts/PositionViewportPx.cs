/*
 * Sets the position and size of the viewport of the attached camera.
 * If a parameter is negative, it will be ignored.
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 6/4/2020, Colombo (CMU)
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways] // Run in editor too (and support prefabs)
[AddComponentMenu("Iris/Util/PositionViewportPx")] // Put in add component menu in Unity editor
public class PositionViewportPx : MonoBehaviour
{
    public float x = -1;
    public float y = -1;
    public float width = -1;
    public float height = -1;

    private float lastScreenW, lastScreenH, lastX, lastY, lastW, lastH;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // If screen has changed size or any of the variables have changed and
        // should be acted on, update viewport
        if (
            Util.AreDiff(Screen.width, lastScreenW)
         || Util.AreDiff(Screen.height, lastScreenH)
         || (lastX > 0 || x > 0) && Util.AreDiff(x,lastX)
         || (lastY > 0 || y > 0) && Util.AreDiff(y, lastY)
         || (lastW > 0 || width > 0) && Util.AreDiff(width, lastW)
         || (lastH > 0 || height > 0) && Util.AreDiff(height, lastH)
        )
        {
            var r = cam.rect;

            var xx = x > 0 ? x : r.x * Screen.width;
            var yy = y > 0 ? y : r.y * Screen.height;
            var ww = width > 0 ? width : r.width * Screen.width;
            var hh = height > 0 ? height : r.height * Screen.height;

            cam.pixelRect = new Rect(xx, yy, ww, hh);

            lastScreenH = Screen.height;
            lastScreenW = Screen.width;

            lastX = x;
            lastY = y;
            lastW = width;
            lastH = height;
        }
    }

}
