using System.Collections;
using UnityEditor;
using UnityEngine;

public class DayNightBehavior : MonoBehaviour
{
    public DayNightProfile profile;
    [Min(0)] public float StartTime;
    public Light directionalLight;

    public float SkyboxTransitionSpeed = 0.2f;

    protected float unitGameTime = 1;
    protected int unitRealTime = 1;

    protected float updatedTime;
    public float inGameTimeNow { get; private set; }
    protected DayNightKeyframe currentState;

    private void Awake()
    {
        inGameTimeNow = StartTime;

        unitGameTime = CycleUnitConvert.ConvertUnit(profile.UnitInGameScale) * profile.InGameUnitValue;
        unitRealTime = (int)CycleUnitConvert.ConvertUnit(profile.UnitRealTimeScale);

        UpdateAll();
    }

    //replace with custom update that doesn't run every frame
    private void Update()
    {
        updatedTime = Time.time;

        Debug.Log(GetInGameTime());

        for (int i = 0; i < profile.keyframes.Count; i++)
        {
            DayNightKeyframe current = profile.keyframes[i];

            if (i < profile.keyframes.Count - 1)
            {
                DayNightKeyframe next = profile.keyframes[i + 1];
                if (inGameTimeNow >= current.time && inGameTimeNow < next.time)
                {
                    currentState = current;
                    break;
                }
            }
            else
            {
                currentState = current;
            }
        }

        UpdateAll();
    }

    public string GetInGameTime()
    {
        //converted to hours how many in-game seconds have passed
        float unitCalc = updatedTime * (unitGameTime / unitRealTime) / 3600;

        inGameTimeNow = (StartTime + unitCalc) % profile.InGameCycleHours;

        float h = Mathf.Floor(inGameTimeNow);
        float m = Mathf.Floor((inGameTimeNow - h) * 60);
        float mrough = (inGameTimeNow - h) * 60;
        float s = Mathf.Floor((mrough - m) * 60);

        return $"{h} : {m} : {s}";
    }

    protected void UpdateAll()
    {

        UpdateSkybox();
        UpdateLight();
    }

    protected void UpdateLight()
    {
        if (directionalLight == null) return;

        directionalLight.color = currentState.lightColor;

        if (updateLightTransition != null) StopCoroutine(updateLightTransition);   //interrupt old coroutine before calling a new one of this type.         
        StartCoroutine(UpdateLightTransition());
    }

    private Coroutine updateLightTransition = null;
    protected IEnumerator UpdateLightTransition()
    {
        while (directionalLight.transform.rotation != GetPolarRotation(Quaternion.Euler(currentState.lightRotation)) || 
            directionalLight.intensity !=currentState.lightIntensity || 
            directionalLight.color != currentState.lightColor)
        {
            directionalLight.transform.rotation = Quaternion.Lerp(directionalLight.transform.rotation, Quaternion.Euler(currentState.lightRotation), Time.deltaTime * 0.001f);

            directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, currentState.lightIntensity, Time.deltaTime * 0.001f);
            yield return null;
        }
    }

    protected void SetLightParameters(Light light, Color color)
    {
        light.color = color;
    }
    protected void SetLightParameters(Light light, Color color, float intensity)
    {
        SetLightParameters(light, color);
        light.intensity = intensity;
    }
    protected void SetLightParameters(Light light, Color color, float intensity, Quaternion rotation)
    {
        SetLightParameters(light, color, intensity);
        light.transform.rotation = rotation;
    }

    /// <summary>
    /// Adds polar directions to the given rotation.
    /// </summary>
    /// <param name="addRotation"></param>
    /// <returns></returns>
    protected Quaternion GetPolarRotation(Quaternion addRotation)
    {
        return Quaternion.Euler(addRotation.eulerAngles + transform.eulerAngles);
    }

    protected void UpdateSkybox()
    {
        switch (profile.skyboxType)
        {
            case SkyboxType.Procedural:
                if (updateProceduralSky != null) StopCoroutine(updateProceduralSky);//only 1 allowed at a time. Old one gets interrupted if new one is ready to be called.
                StartCoroutine(UpdateProceduralSky());
                break;
            case SkyboxType.Cubemap:
                break;
            case SkyboxType.Panoramic:
                break;
            case SkyboxType.SixSided:
                break;
        }
    }

    private Coroutine updateProceduralSky = null;
    protected IEnumerator UpdateProceduralSky()
    {
        while (!SetLerpedColor("_SkyTint", currentState.skyProcedural) &&
            !SetLerpedColor("_GroundColor", currentState.skyProcedural) &&
            !SetLerpedFloat("_SunSizeConvergence", currentState.skyProcedural) &&
            !SetLerpedFloat("_AtmosphereThickness", currentState.skyProcedural) &&
            !SetLerpedFloat("_Exposure", currentState.skyProcedural) &&
            !SetLerpedFloat("_SunSize", currentState.skyProcedural))
        {
            yield return null;
        }

        Debug.Log($"ColorChanged {currentState.Name}");
        updateProceduralSky = null;
    }

    protected bool SetLerpedColor(string valName, Material target)
    {
        Color col = RenderSettings.skybox.GetColor(name: valName);
        Color targCol = target.GetColor(valName);

        if (col == targCol) return true;

        RenderSettings.skybox.SetColor(name: valName, Color.Lerp(col, targCol, Time.deltaTime * SkyboxTransitionSpeed));

        return false;
    }

    protected bool SetLerpedFloat(string valName, Material target)
    {
        if (target.HasProperty(valName))
        {
            float currentF = RenderSettings.skybox.GetFloat(name: valName);
            float targF = target.GetFloat(valName);

            if (currentF == targF) return true;

            RenderSettings.skybox.SetFloat(name: valName, Mathf.Lerp(currentF, targF, Time.deltaTime * SkyboxTransitionSpeed));
        }
        else { Debug.LogError(">:( " + valName); }

        return false;
    }



#if UNITY_EDITOR

    private void OnValidate()
    {
        if (profile == null) return;


        if (StartTime > profile.InGameCycleHours) StartTime = profile.InGameCycleHours;


        //show the state of the cycle 0:00 needs to be start time later
        if (profile.keyframes.Count > 0)
        {
            DayNightKeyframe keyframe = profile.keyframes[0];

            if (keyframe.time == 0) currentState = keyframe;
            else currentState = profile.keyframes[^1];
        }
        UpdateAll();

    }

    public void OnDrawGizmos()
    {
        float lineLength = 2;
        float lineShortener = 0.9f;
        GUIStyle style = new();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontSize = 32;

        Vector3 dir = transform.right;

        Vector3 West = transform.position - dir * lineLength;
        Vector3 East = transform.position + dir * lineLength;

        Vector3 dirV = transform.forward;

        Vector3 South = transform.position - dirV * lineLength;
        Vector3 North = transform.position + dirV * lineLength;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(West * lineShortener, East * lineShortener);
        Handles.Label(West, "W", style);
        Handles.Label(East, "E", style);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(South * lineShortener, North * lineShortener);
        Handles.Label(South, "S", style);
        Handles.Label(North, "N", style);

        transform.eulerAngles = new(0, transform.eulerAngles.y, 0);

    }

#endif

}

