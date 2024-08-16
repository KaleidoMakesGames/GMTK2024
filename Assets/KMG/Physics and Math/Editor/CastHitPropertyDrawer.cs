using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(KinematicMovement3D.CastHit))]
public class CastHitPropertyDrawer : PropertyDrawer { 
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        var lPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        bool isExpanded = property.isExpanded;
        property.isExpanded = false;
        float height = EditorGUI.GetPropertyHeight(property);
        property.isExpanded = isExpanded;

        lPosition.height = height;

        var v = (KinematicMovement3D.CastHit)property.boxedValue;
        EditorGUI.LabelField(lPosition, v.ToString());

        EditorGUI.PropertyField(position, property, GUIContent.none, true);
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property);
    }
}