using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CreatureModelIndex.StatModelEntry))]
public class StatModelEntryDrawer : PropertyDrawer
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
            var rect1 = new Rect(position.x, position.y, 50, position.height);
            var rect2 = new Rect(position.x + 55, position.y, 50, position.height);
            var rect3 = new Rect(position.x + 110, position.y, 200, position.height);
            var rect4 = new Rect(position.x + 315, position.y, 50, position.height);
            var rect5 = new Rect(position.x + 370, position.y, position.width - 370, position.height);


            SerializedProperty health = property.FindPropertyRelative("health");
            SerializedProperty mana = property.FindPropertyRelative("mana");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty total = property.FindPropertyRelative("total");

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(rect1, "Atk: " + (PowerBudget.StatBudget(mana.intValue) - health.intValue).ToString());
            EditorGUI.LabelField(rect2, "Hp: " + health.intValue.ToString());

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
