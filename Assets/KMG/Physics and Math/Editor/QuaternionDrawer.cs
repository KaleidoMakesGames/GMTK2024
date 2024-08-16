using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Quaternion))]
public class QuaternionDrawer: PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 17f;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginChangeCheck();
        var v = EditorGUI.Vector3Field(position, label, property.quaternionValue.eulerAngles);
        if (EditorGUI.EndChangeCheck()) {
            property.quaternionValue = Quaternion.Euler(v).normalized;
        }
    }
}