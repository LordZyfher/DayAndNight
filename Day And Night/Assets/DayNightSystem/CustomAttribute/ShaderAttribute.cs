using UnityEditor;
using UnityEngine;

public class ShaderAttribute : PropertyAttribute
{
    public string EnumFieldName;
    public object ExpectedValue;

    public ShaderAttribute(string enumFieldName, object expectedValue)
    {
        EnumFieldName = enumFieldName;
        ExpectedValue = expectedValue;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ShaderAttribute))]
public class ShaderTypeFilterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShaderAttribute condition = (ShaderAttribute)attribute;
        SerializedProperty enumProp = property.serializedObject.FindProperty(condition.EnumFieldName);

        if (enumProp != null && enumProp.enumValueIndex == (int)condition.ExpectedValue)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShaderAttribute condition = (ShaderAttribute)attribute;
        SerializedProperty enumProp = property.serializedObject.FindProperty(condition.EnumFieldName);

        if (enumProp != null && enumProp.enumValueIndex == (int)condition.ExpectedValue)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return 0f;
    }
}
#endif
