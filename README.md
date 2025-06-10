This is a work-in-progress project.

The base system is almost set up.
At the moment, the product only works with procedural skyboxes and changing it's values.
All you need is to have an object in the scene with the script on it. If you like the lightsource to change directions, set the light type object on the component to the desired lightsource.
Only 1 lightsource that rotates according to the values set in the profile and works by rotating it once a keyframe (for example dawn) is hit.

Current issues:
-No way to switch between light sources
-Variables for custom shaders are in the works, but there are no custom shaders, so that may be confusing for users.

What it is intended to become:
-A product where any kind of cycle (most commonly used for day/night cycles) can be easily set-up in a scene (some basic profiles will be included).
-To put it in the scene, all you need to do is: drag & drop a prefab, pick (or create) a profile and optional: set it's values to your liking.
-Usable for many different kinds of games with variations and settings to adjust.
-Allows events for the skybox (for example in a game that has an eclipse or invasion of monsters/aliens every now and then, you can call an event that changes the skybox to fit the vibe).
-Has options that allow use for lower-end devices.
-Custom shaders implementation (will be included in the package for free).
-Nebula, galaxies, stars, other planets or multiple suns/moons and fog can be added and customized by the user. It is recommended (for performance) to stick with only 1 lightsource at a time, so other suns/moons as lightsources are optional, but not recommended.
-If I manage to do it: allow baking skyboxes into a cube-map which helps with saving performance.

Once the basics are set up I will update this product, if there are any suggestions I will take a look at them to see what I can do with it.
This is a spare-time project, which means updates can take a while.
