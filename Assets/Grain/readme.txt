Film Grain
Matt Robinson
diggler@tpsreport.co.uk

This package adds a simple film grain effect to your game's scene, and works fine in the free version of Unity as well as Pro.

Simply drag and drop the "GrainCamera" prefab from the included package onto your scene to begin. This will add the grain via a seperate camera, which will be layered over whatever MainCamera you are already using. Be sure to move this "GrainCamera" object well out of the way so that it is not seen in the game's viewport.

Once in place, you can tweak the exposed variables on the "Grain" sub object to make the effect look appropriate for your scene and resolution. These settings include;

Grain Enabled - Simply enables and disables the grain in realtime. This setting could easily be attached to an options screen in your game's graphical settings if wanted.

Grain Style - Selecting either Default, Day, or Night will produce different styled grain. Default should be fine for most scenes, though Day can produce a more subtle effect, while Night is a heavy effect that works better in darker scenes.

Grain Sharpness - This controls the fineness of the granules. Higher values appear sharper, though smaller, while low values appear large and more blurry. Decmimal values under 1 can produce more retro, pixellated effects.

Materials - This list allows you to drag and drop your own custom grain textures into the inspector if you'd like to experiment yourself, or edit the included ones.

Shake Amount - Controls the amount of movement given to the grain. This setting should not need tweaking in most cases, though if you do, be careful to avoid too high settings (which will result in a tearing effect), or too low settings (which will stop the grain moving altogether).

Enjoy!