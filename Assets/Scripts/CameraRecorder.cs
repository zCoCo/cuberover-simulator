/*
 * Records the Views from the Rover's Cameras when Triggered
 *
 * Author: Oskar Schlueb (NA)
 * Last Update: 6/2/2020, Colombo (CMU)
 */

using UnityEngine;
using System.Collections;
using System.IO;
using System;

// Screen Recorder will save individual images of active scene in any resolution and of a specific image format
// including raw, jpg, png, and ppm.  Raw and PPM are the fastest image formats for saving.
//
// You can compile these images into a video using ffmpeg:
// ffmpeg -i screen_3840x2160_%d.ppm -y test.avi

[AddComponentMenu("Iris/Rover/Camera/CameraRecorder")] // Put in add component menu in Unity editor
public class CameraRecorder : MonoBehaviour
{
    [HideInInspector]
    public bool SCREEN_CAP = false; // Trigger indicating that a new capture is due.

    [Header("Cameras")]
    public Camera frontCam;
    public Camera rearCam;

    [Header("Save & Output")]
    // folder to write output (defaults to data path)
    public string folder;

    // 4k = 3840 x 2160   1080p = 1920 x 1080
    public int captureWidth = 800;
    public int captureHeight = 600;

    [Header("General Settings")]
    // optional game object to hide during screenshots (usually your scene canvas hud)
    public GameObject hideGameObject; // TODO: Don't think this is necessary since it's Canvas can exist outside camera and thus isn't capture

    // optimize for many screenshots will not destroy any objects so future screenshots will be fast
    public bool optimizeForManyScreenshots = true;

    // configure with raw, jpg, png, or ppm (simple raw format)
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.JPG;//PPM; // use PPM for AVI reconstruction

    // private vars for screenshot
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;

    private bool initLandingPhoto = false; // Whether an initial photo has been taken of the landing site

    // commands
    private bool captureScreenshot = false;
    private bool captureVideo = false;

    private TankMovement movement;
    private BackendConnection backend;

    private bool export_images = false; // Whether to save (export) images
    // Set export images externally (from button)
    public void SetImageExport(bool export)
    {
        export_images = export;
    }

    // create a unique filename using a one-up variable
    private string uniqueFilename(int width, int height)
    {

        // if folder not specified by now use a good default
        if (folder == null || folder.Length == 0)
        {
            folder = Application.dataPath;
            if (Application.isEditor)
            {
                // put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = folder + "/..";
                folder = Path.GetFullPath(stringPath);
            }
            folder += "/screenshots";

            // make sure directoroy exists
            System.IO.Directory.CreateDirectory(folder);

            // count number of files of specified format in folder
            string mask = string.Format("screen_{0}x{1}*.{2}", width, height, format.ToString().ToLower());
            int counter = Directory.GetFiles(folder, mask, SearchOption.TopDirectoryOnly).Length;
        }


        var cam = "Front";
        if(!CameraSwitch.USE_FRONT_CAM){
          cam = "Rear";
        }

        // Get Current Unix Timestamp:
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long timestamp = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;

        // Create Filename:
        var filename = string.Format("{0}-{1}-{2}-{3}-{4}.{5}", backend.CURR_COMMLID, backend.CURR_NAME, cam, 0, timestamp, format.ToString().ToLower());

        // return unique filename
        return filename;
    }

    public void CaptureScreenshot()
    {
        captureScreenshot = true;
    }

    private void Start()
    {
        movement = GetComponent<TankMovement>();
        backend = GetComponent<BackendConnection>();
    }

    // Returns to initial (just landed) state:
    public void Reinit()
    {
        initLandingPhoto = false;
    }

    void Update()
    {
        // check keyboard 'k' for one time screenshot capture and holding down 'v' for continious screenshots
        captureScreenshot |= Input.GetKeyDown("k");
        captureVideo = Input.GetKey("v");

        if (export_images && (captureScreenshot || captureVideo || SCREEN_CAP || !initLandingPhoto && !movement.Deployed && Time.realtimeSinceStartup > 1))
        {
            initLandingPhoto = true;
            captureScreenshot = false;

            // hide optional game object if set
            if (hideGameObject != null) hideGameObject.SetActive(false);

            // create screenshot objects if needed
            if (renderTexture == null)
            {
                // creates off-screen render texture that can rendered into
                rect = new Rect(0, 0, captureWidth, captureHeight);
                renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
                screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            }

            // get main camera and manually render scene into rt
            Camera cam = frontCam;
            if (!CameraSwitch.USE_FRONT_CAM)
            {
                cam = rearCam;
            }
            cam.targetTexture = renderTexture;
            cam.Render();

            // read pixels will read from the currently active render texture so make our offscreen
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);

            // reset active camera texture and render texture
            cam.targetTexture = null;
            RenderTexture.active = null;

            // get our unique filename
            string filename = uniqueFilename((int)rect.width, (int)rect.height);

            // pull in our file header/data bytes for the specified image format (has to be done from main thread)
            byte[] fileHeader = null;
            byte[] fileData = null;
            if (format == Format.RAW)
            {
                fileData = screenShot.GetRawTextureData();
            }
            else if (format == Format.PNG)
            {
                fileData = screenShot.EncodeToPNG();
            }
            else if (format == Format.JPG)
            {
                fileData = screenShot.EncodeToJPG();
            }
            else // ppm
            {
                // create a file header for ppm formatted file
                string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
                fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
                fileData = screenShot.GetRawTextureData();
            }

            // create new thread to save the image to file (only operation that can be done in background)
            new System.Threading.Thread(() =>
            {
                // create file and write optional header with image bytes
                var f = System.IO.File.Create(filename);
                if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
                f.Write(fileData, 0, fileData.Length);
                f.Close();
                Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
            }).Start();

            // unhide optional game object if set
            if (hideGameObject != null) hideGameObject.SetActive(true);

            // cleanup if needed
            if (optimizeForManyScreenshots == false)
            {
                Destroy(renderTexture);
                renderTexture = null;
                screenShot = null;
            }
            SCREEN_CAP = false;
            CameraSwitch.USE_FRONT_CAM = !CameraSwitch.USE_FRONT_CAM;
        }
    }
}
