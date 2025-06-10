
using System.ComponentModel;

public enum SkyboxType 
{
    [Description("Skybox/Procedural")] Procedural,
    [Description("Skybox/Cubemap")] Cubemap,
    [Description("Skybox/Panorama")] Panoramic,
    [Description("Skybox/6 Sided")] SixSided,
}
