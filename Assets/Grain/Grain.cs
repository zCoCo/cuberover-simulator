/// <summary>
/// Grain.cs
/// Matt Robinson
/// diggler@tpsreport.co.uk
/// 
/// This script is part of the "Film Grain" asset available on the Unity store.
/// It adds a film grain effect to your game's scene, and works fine in the free version of Unity.
/// 
/// Simply drag and drop the "GrainCamera" prefab from the included package onto your scene to begin. 
/// This will add the grain in a seperate camera, which be layered over whatever MainCamera you are
/// currently using. Be sure to move this "GrainCamera" object well out of the way so that it is never
/// seen in the main viewport.
/// 
/// Once in place, you can tweak the exposed variables to make the effect look appropriate for your scene 
/// and resolution. More info in the comments below.
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class Grain : MonoBehaviour
{
	//Enables and disables the grain effect
	public bool grainEnabled = true;
	
	//The style of grain we want (e.g. Day, Night, etc).
	//Default adds a subtle effect that works well in most scenes.
	//Night is primarily for dark scenes, and adds lots of noise.
	//Day is the most subtle of the three and will be practically invisible in any dark scene.
	public GrainStyle grainStyle = GrainStyle.Default;
	
	//An array to hold our different materials. You can replace these with your own custom grains
	//by dragging them into the Inspector.
	public Material[] materials;
	
	//This is the amount of overall movement to the grain.
	//This value will need tweaking based on your specific taste and resolution.
	//Too high a value, and you will see "flickering".
	//Too low a value will stop the grain moving completely.
	//10 - 20 should work fine for most cases.
	public float shakeAmount = 15f;
	
	//This controls the sharpness and size of the individual grain particles.
	//Higher values result in smaller grain.
	//You can also use decimal values < 1 for a more retro "pixelated" effect.
	public float grainSharpness = 4f;
	
	//Assorted cached variables used behind the scenes to produce the final effect
	private Vector3 _direction = new Vector3 (0f, 0f, 0f);
	private MeshRenderer _myRenderer;
	private Transform _myTransform;
	
	void Start () {
		//Our private variables are initiliazed here		
		_myTransform = transform;
		_myRenderer = GetComponent<MeshRenderer>();
		_myRenderer.material.mainTextureScale = new Vector2(grainSharpness, grainSharpness);
		
		//Now we position the grain "card" directly in front of the camera in case it's been moved at all
		_myTransform.position = Vector3.zero;
		_myTransform.rotation = _myTransform.parent.rotation;
		transform.Rotate(270,0,0);
	}
	
	//The grain effect being applied per frame
	void Update () {
		
		//Enable the grain if the user toggles it on in the inspector
		if (_myRenderer.enabled == false && grainEnabled == true) {
			_myRenderer.enabled = true;
		}
		
		//If grain is disabled, we do nothing
		if (grainEnabled == false) {
			_myRenderer.enabled = false;
		}
		
		//If grain is enabled, we "shake" the grain plane object every frame to give the illusion of
		//the particles moving around randomly
		else {
			_direction.x = (Random.Range (-shakeAmount / 10, shakeAmount / 10));
			_direction.y = (Random.Range (-shakeAmount / 10, shakeAmount / 10));
			_direction.z = (2f);
			
			//Finally we adjust the grain sharpness and look as per the options in the Inspector
			if(_myRenderer.material != materials[(int)grainStyle])
				_myRenderer.material = materials[(int)grainStyle];
				
			if (_myRenderer.material.mainTextureScale.x != grainSharpness)
				_myRenderer.material.mainTextureScale = new Vector2(grainSharpness, grainSharpness);
			
			_myTransform.localPosition = _direction;
		}
	}
	
	//Destroy our instanced material if we switch to a new one
	void OnDestroy() {
        DestroyImmediate(_myRenderer.material);
    }
	
	//Enumeration for our different styles of grain
	public enum GrainStyle {
		Default,
		Day,
		Night
	}
}
