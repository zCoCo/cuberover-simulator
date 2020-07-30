using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Thermal : MonoBehaviour
{

    public RenderTexture lightCheckTexture;
    private float Lightlevel;
    private float light;
    public Text ThermalData;




    // Use this for initialization
    void Start()
    {



    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RenderTexture tmpTexture = RenderTexture.GetTemporary(lightCheckTexture.width, lightCheckTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(lightCheckTexture, tmpTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmpTexture;

        Texture2D temp2DTexture = new Texture2D(lightCheckTexture.width, lightCheckTexture.height);
        temp2DTexture.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
        temp2DTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmpTexture);

        Color32[] colors = temp2DTexture.GetPixels32();

        Lightlevel = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            Lightlevel += (0.2126f * colors[i].r) + (0.7152f * colors[i].g) + (0.0722f + colors[i].b);

        }
        light = Lightlevel - 251775;
        light = Lightlevel / 14475000;
        light = Mathf.Round(light * 1000.0f) * 0.001f;

        ThermalData.text = "External temperature range(0-1): " + light;
    }

}