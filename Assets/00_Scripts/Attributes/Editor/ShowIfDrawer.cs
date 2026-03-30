using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute attr = (ShowIfAttribute)attribute;

        if (ShouldShow(property, attr))
            return EditorGUI.GetPropertyHeight(property, label, true);

        return 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute attr = (ShowIfAttribute)attribute;

        if (ShouldShow(property, attr))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    bool ShouldShow(SerializedProperty property, ShowIfAttribute attr)
    {
        SerializedProperty condition = property.serializedObject.FindProperty(attr.conditionField) ?? 
            property.serializedObject.FindProperty(property.propertyPath.Replace(property.name, attr.conditionField));

        if (condition == null) return true;

        if (condition.propertyType == SerializedPropertyType.Enum)
        {
            string enumName = condition.enumNames[condition.enumValueIndex];
            return enumName == attr.compareValue.ToString();
        }

        if (condition.propertyType == SerializedPropertyType.Boolean)
        {
            return condition.boolValue == (bool)attr.compareValue;
        }

        return true;
    }
}
