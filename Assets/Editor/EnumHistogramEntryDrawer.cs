using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumHistogramEntry))]
public class EnumHistogramEntryDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property != null)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var amountRect = new Rect(position.x, position.y, 30, position.height);
            var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
            var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

            SerializedProperty enumVal = property.FindPropertyRelative("enumVal");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty enumType = property.FindPropertyRelative("enumType");

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(amountRect, enumVal, GUIContent.none);
            EditorGUI.PropertyField(unitRect, value, GUIContent.none);

            //string text = System.Enum.ToObject(System.Type.GetType(enumType.stringValue), enumVal.intValue).ToString();
            string typeName = enumType.stringValue;
            System.Type type = System.Type.GetType(typeName);
            string text = System.Enum.ToObject(type, enumVal.intValue).ToString();
            EditorGUI.LabelField(nameRect, text);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
