using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace KMGAnimation {
    [CustomPropertyDrawer(typeof(HumanoidArmature))]
    public class HumanoidArmaturePropertyDrawer : PropertyDrawer {
        private bool _Foldout;
        private HumanoidArmature _armature;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            CheckInitialize(property, label);
            if (_Foldout)
                return (Enum.GetNames(typeof(HumanBodyBones)).Length + 1) * 17f;
            return 17f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            CheckInitialize(property, label);

            position.height = 17f;

            var foldoutRect = position;
            EditorGUI.BeginChangeCheck();
            _Foldout = EditorGUI.Foldout(foldoutRect, _Foldout, label, true);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(label.text, _Foldout);

            if (!_Foldout)
                return;

            void DrawBoneAndChildren(HumanBodyBones bone, int level) {
                position.y += 17f;
                var r = position;
                r.x += level * 5;
                r.width -= level * 5;
                var fieldPosition = EditorGUI.PrefixLabel(r, new GUIContent(bone.ToString()));
                EditorGUI.BeginChangeCheck();
                var boneAssignment = EditorGUI.ObjectField(fieldPosition, _armature.boneTransforms[bone], typeof(Transform), true) as Transform;
                if (EditorGUI.EndChangeCheck()) {
                    _armature.boneTransforms[bone] = boneAssignment;
                }
                foreach (var child in bone.GetBoneChildren()) {
                    DrawBoneAndChildren(child, bone.GetBoneChildren().Length > 1 ? level + 1 : level);
                }
            }

            DrawBoneAndChildren(HumanBodyBones.Hips, 0);
        }

        private void CheckInitialize(SerializedProperty property, GUIContent label) {
            _armature = property.boxedValue as HumanoidArmature;
            _Foldout = EditorPrefs.GetBool(label.text);
        }
    }
}