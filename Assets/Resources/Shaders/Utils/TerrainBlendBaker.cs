using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBlendBaker : MonoBehaviour
{

    public Shader depthShader;
    public RenderTexture depthTexture;
    private Camera cam;
    // Start is called before the first frame update
    private void UpdateBakingCamera() {
        if (cam == null) {
            cam = GetComponent<Camera>();
        }
        //the total width of the bounding box of our cameras view
        Shader.SetGlobalFloat("TB_SCALE", GetComponent<Camera>().orthographicSize * 2);
        //find the bottom corner of the texture in world scale by subtracting the size of the camera from its x and z position
        Shader.SetGlobalFloat("TB_OFFSET_X", cam.transform.position.x - cam.orthographicSize);
        Shader.SetGlobalFloat("TB_OFFSET_Z", cam.transform.position.z - cam.orthographicSize);
        //we'll also need the relative y position of the camera, lets get this by subtracting the far clip plane from the camera y position
        Shader.SetGlobalFloat("TB_OFFSET_Y", cam.transform.position.y - cam.farClipPlane);
        //we'll also need the far clip plane itself to know the range of y values in the depth texture
        Shader.SetGlobalFloat("TB_FARCLIP", cam.farClipPlane);
    }


    // The context menu tag allows us to run methods from the inspector (https://docs.unity3d.com/ScriptReference/ContextMenu.html)
    [ContextMenu("Bake Depth Texture")]
    public void BakeTerrainDepth() {
        //call our update camera method 
        UpdateBakingCamera();

        //Make sure the shader and texture are assigned in the inspector
        if (depthShader != null && depthTexture != null) {
            cam.SetReplacementShader(depthShader, "Opaque");
            cam.targetTexture = depthTexture;
            Shader.SetGlobalTexture("TB_DEPTH", depthTexture);
        } else {
            Debug.Log("You need to assign the depth shader and depth texture in the inspector");
        }
    }
}
