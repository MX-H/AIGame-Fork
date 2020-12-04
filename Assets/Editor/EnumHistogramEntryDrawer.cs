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
            var rect1 = new Rect(position.x, position.y, 150, position.height);
            var rect2 = new Rect(position.x + 155, position.y, 30, position.height);
            var rect3 = new Rect(position.x + 190, position.y, 200, position.height);
            var rect4 = new Rect(position.x + 395, position.y, 50, position.height);
            var rect5 = new Rect(position.x + 450, position.y, position.width - 450, position.height);

            SerializedProperty enumVal = property.FindPropertyRelative("enumVal");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty enumType = property.FindPropertyRelative("enumType");
            SerializedProperty total = property.FindPropertyRelative("total");

            string typeName = enumType.stringValue;
            System.Type type = System.Type.GetType(typeName);
            string typeText = System.Enum.ToObject(type, enumVal.intValue).ToString();
            EditorGUI.LabelField(rect1, typeText);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(rect2, enumVal, GUIContent.none);
            EditorGUI.PropertyField(rect3, value, GUIContent.none, true);

            float percent = 0;
            if (total.intValue > 0)
            {
                percent = (value.intValue / (float)total.intValue);
            }
            string percentText = (percent * 100).ToString("0.00") + "%";

            EditorGUI.LabelField(rect4, percentText);
            EditorGUI.ProgressBar(rect5, percent, "");

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
