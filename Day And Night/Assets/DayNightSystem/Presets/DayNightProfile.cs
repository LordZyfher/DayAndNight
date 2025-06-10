using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Day Night Profile")]
public class DayNightProfile : ScriptableObject
{
    [Header("Unit Setup")]
    public TimeScale UnitInGameScale;
    public TimeScale UnitRealTimeScale;

    [Min(0.01f), Tooltip("0.1 means the in-game time goes 10 times faster then 1,\n 2 means the in-game time passes at half the speed of 1")] 
    public float InGameUnitValue = 1;

    [Header("Cycle Setup \n1 cycle = 1 day")]
    [Min(1)] public uint InGameCycleHours = 24;

    public SkyboxType skyboxType;//make sure to add variables in the DayNightKeyframe class

    [SerializeField]
    public List<DayNightKeyframe> keyframes = new List<DayNightKeyframe>
      {   new DayNightKeyframe { Name = "Day", time = 0.2f, lightColor = Color.yellow},
          new DayNightKeyframe { Name = "Night", time = 0.8f, lightColor = Color.white},
      };
}


#if UNITY_EDITOR

//Custom UI made to make cycle editing easier

[CustomEditor(typeof(DayNightProfile))]
public class DayNightProfileEditor : Editor
{
    private const int barHeight = 10;
    private const int spacing = 30;
    private const int handleSize = 20;

    int draggingIndex = -1;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        //title for unit setup
        EditorGUILayout.LabelField("Unit Setup", EditorStyles.boldLabel);

        //draw other properties before the cycle timeline normally
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UnitInGameScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UnitRealTimeScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("InGameUnitValue"));

        //title for the cycle setup values is automatically rendered

        EditorGUILayout.PropertyField(serializedObject.FindProperty("InGameCycleHours"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skyboxType"));


        DayNightProfile profile = (DayNightProfile)target;

        //title for the cycle timeline
        EditorGUILayout.LabelField("Day/Night cycle keyframes", EditorStyles.boldLabel);

        EditorGUILayout.Space(spacing);

        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, barHeight);
        EditorGUI.DrawRect(rect, Color.grey);

        //define cycle handle interactable area within the cycle timeline
        if (Event.current.type == EventType.MouseDown && draggingIndex == -1)
        {

            for (int i = 0; i < profile.keyframes.Count; i++)
            {
                var key = profile.keyframes[i];
                float normalized = key.GetNormalizedTime(profile.InGameCycleHours);

                float x = rect.x + rect.width * normalized;

                //Make sure that the visuals drawn below have the handle visual set correctly
                Vector2 handlePos = new(x, rect.center.y);
                Rect handleRect = new Rect(handlePos.x - handleSize / 2, handlePos.y - 10, handleSize, handleSize);

                //Make the cursor show the correct icon for dragging
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.SlideArrow);

                //start dragging
                if (handleRect.Contains(Event.current.mousePosition))
                {
                    draggingIndex = i;
                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    break;
                }
            }
        }

        //draw keyframes for the cycle timeline
        for (int i = 0; i < profile.keyframes.Count; i++)
        {
            var key = profile.keyframes[i];
            var nextKey = profile.keyframes[(i + 1) % profile.keyframes.Count];

            float normalized = key.GetNormalizedTime(profile.InGameCycleHours);
            float normalizedEnd = nextKey.GetNormalizedTime(profile.InGameCycleHours);

            float x = 0;
            float endX = 0;

            //draws last value's color segment first to prevent visual sorting issues with handles
            if (i == 0)
            {
                float wrapX = rect.x;
                float wrapEndX = rect.x + rect.width * normalized;
                Rect segmentRect2 = new Rect(wrapX, rect.y, wrapEndX - wrapX, rect.height);
                EditorGUI.DrawRect(segmentRect2, profile.keyframes[^1].lightColor);
            }

            //Make sure it wraps the last index to the first
            if (normalizedEnd <= normalized)
            {
                //makes the last value segment go to the end of the bar
                x = rect.x + rect.width * normalized;
                endX = rect.x + rect.width;
            }
            else
            {
                //makes value segment go to the next value
                x = rect.x + rect.width * normalized;
                endX = rect.x + rect.width * normalizedEnd;
            }

            Rect segmentRect = new Rect(x, rect.y, endX - x, rect.height);
            EditorGUI.DrawRect(segmentRect, key.lightColor);

            //Draw the handle visual where the segment can be grabbed
            Vector2 handlePos = new(x, rect.center.y);
            //outline
            Handles.color = key.lightColor * 0.7f;
            Handles.DrawSolidDisc(handlePos, Vector3.forward, handleSize / 2);
            //center
            Handles.color = key.lightColor * 1.4f;
            Handles.DrawSolidDisc(handlePos, Vector3.forward, handleSize / 2.75f);

            //prepare time value to show as time
            int h = (int)Mathf.Floor(key.time);
            int m = Mathf.RoundToInt((key.time - h) * 60);
            string time = $"{h}:";
            if (m < 10) time += $"0{m}";
            else time += $"{m}";

            //make the hour and name of the keyframe show near the start of the segment
            EditorGUI.LabelField(new Rect(segmentRect.x, segmentRect.yMax + 2, 40, 20), time);
           // EditorGUI.LabelField(new Rect(segmentRect.x, segmentRect.yMax + 2, 40, 20), key.time.ToString("0.##"));
            EditorGUI.LabelField(new Rect(segmentRect.x, segmentRect.yMin - 20, 40, 20), key.Name);

            key.SetSkyboxType(profile.skyboxType);
        }

        if (Event.current.type == EventType.MouseUp)
        {
            profile.keyframes = profile.keyframes.OrderBy(x => x.time).ToList();

            if (GUIUtility.hotControl == controlId)
            {
                draggingIndex = -1;
                GUIUtility.hotControl = 0;
                Event.current.Use();
            }
        }

        if (draggingIndex != -1 && Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
        {
            float percent = Mathf.InverseLerp(rect.x, rect.xMax, Event.current.mousePosition.x);
            float newTime = Mathf.Clamp(percent * profile.InGameCycleHours, 0, profile.InGameCycleHours);
            profile.keyframes[draggingIndex].time = newTime;
            Event.current.Use();
            GUI.changed = true;
        }
        EditorGUILayout.Space(spacing);


        SerializedProperty keyframesProp = serializedObject.FindProperty("keyframes");
        if (GUILayout.Button("Add Keyframe"))
        {
            profile.keyframes.Add(new DayNightKeyframe { Name = "New keyframe", time = 1, lightColor = Color.white, lightIntensity = 1 });
        }



        EditorGUILayout.Space(spacing);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("keyframes"));


        serializedObject.ApplyModifiedProperties();
    }
}

#endif

