using UnityEngine;

[System.Serializable]
public class DayNightKeyframe
{
    public string Name;
    [Min(0)]
    public float time;

    //Always add variable for each value of the enum if adding values to the enum.
    [Shader("skyboxType", SkyboxType.Procedural)] public Material skyProcedural;
    [Shader("skyboxType", SkyboxType.Cubemap)] public Material skyCubemap;
    [Shader("skyboxType", SkyboxType.Panoramic)] public Material skyPanoramic;
    [Shader("skyboxType", SkyboxType.SixSided)] public Material skySixSided;

    private SkyboxType skyboxType;

    [Header("[Optional] Light settings")]
    public Color lightColor;
    public float lightIntensity = 1;
    public Vector3 lightRotation;

    public float GetNormalizedTime(float CycleLength)
    {
        return Mathf.Clamp01(time / CycleLength);
    }
    public void SetSkyboxType(SkyboxType type)
    {
        skyboxType = type;
    }
}

[CreateAssetMenu(menuName = "Day Night Keyframe Preset")]
public class DayNightKeyframePreset : ScriptableObject
{
    public DayNightKeyframe preset;
}
