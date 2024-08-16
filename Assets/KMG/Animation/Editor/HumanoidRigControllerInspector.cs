using UnityEditor;
using UnityEngine;
using NaughtyAttributes.Editor;
using System.Linq;

namespace KMGAnimation {
    [CustomEditor(typeof(HumanoidRigController))]
    public class HumanoidRigControllerInspector : NaughtyInspector {
        static int selectedHandleIndex = -1;
        static Quaternion preClickRotation;
        static Quaternion handleRotation;
        void DrawIKLimbTargetHandle(string name, ref int handleIndex, ref HumanoidPose.LimbPose t) {
            DrawTransformHandle(name, ref handleIndex, ref t.position, ref t.rotation);
            DrawPositionHandle(name + " Pole", ref handleIndex, ref t.polePosition);
        }
        void DrawTransformHandle(string name, ref int handleIndex, ref Vector3 position, ref Quaternion rotation) {
            int id = DrawSelectionHandle(name, handleIndex, position, rotation);
            if (handleIndex == selectedHandleIndex) {
                var oldRotation = rotation;
                bool notClicked = GUIUtility.hotControl == 0 || GUIUtility.hotControl == id;
                if (notClicked) {
                    handleRotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : rotation;
                    preClickRotation = rotation;
                }
                
                Handles.TransformHandle(ref position, ref handleRotation);
                rotation = Tools.pivotRotation == PivotRotation.Global ? handleRotation * preClickRotation : handleRotation;
                if(Quaternion.Angle(oldRotation, rotation) < 1e-5) {
                    rotation = oldRotation;
                }
                rotation = rotation.normalized;
            }
            handleIndex++;
        }
        void DrawPositionHandle(string name, ref int handleIndex, ref Vector3 position) {
            if (handleIndex == selectedHandleIndex) {
                position = Handles.PositionHandle(position, Quaternion.identity);
            }
            DrawSelectionHandle(name, handleIndex, position);
            handleIndex++;
        }
        void DrawRotationHandle(string name, ref int handleIndex, Vector3 position, ref Quaternion rotation) {
            int id = DrawSelectionHandle(name, handleIndex, position, rotation);
            if (handleIndex == selectedHandleIndex) {
                var oldRotation = rotation;
                bool notClicked = GUIUtility.hotControl == 0 || GUIUtility.hotControl == id;
                if (notClicked) {
                    handleRotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : rotation;
                    preClickRotation = rotation;
                }
                handleRotation = Handles.RotationHandle(handleRotation, position);
                rotation = Tools.pivotRotation == PivotRotation.Global ? handleRotation * preClickRotation : handleRotation;

                if (Quaternion.Angle(oldRotation, rotation) < 1e-5) {
                    rotation = oldRotation;
                }
            }
            handleIndex++;
        }
        int DrawSelectionHandle(string name, int handleIndex, Vector3 position, Quaternion? rotation = null) {
            float size = HandleUtility.GetHandleSize(position) * 0.2f;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            if (handleIndex == selectedHandleIndex) {
                Handles.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
                if (rotation.HasValue) {
                    Handles.ConeHandleCap(id, position, rotation.Value, size, EventType.Repaint);
                } else {
                    Handles.SphereHandleCap(id, position, Quaternion.identity, size, EventType.Repaint);
                }
            } else {
                Handles.color = id == HandleUtility.nearestControl ? Color.yellow : Color.green;
                if (rotation.HasValue) {
                    Handles.Slider(id, position, rotation.Value * Vector3.forward, size, Handles.ConeHandleCap, 0);
                } else {
                    Handles.FreeMoveHandle(id, position, size, Vector2.zero, Handles.SphereHandleCap);
                }
            }
            GUIStyle a = new GUIStyle(GUI.skin.label);
            a.alignment = TextAnchor.LowerCenter;
            a.normal.textColor = Handles.color;
            a.hover.textColor = Handles.color;
            a.contentOffset = Vector2.down * 10.0f;
            Handles.Label(position, name, a);
            if (id == GUIUtility.hotControl) {
                selectedHandleIndex = handleIndex;
            }
            return id;
        }
        private HumanoidRigController rc;
        private void OnSceneGUI() {
            var t = target as HumanoidRigController;
            rc = t;
            int i = 0;
            Undo.RecordObject(t, "Manipulate rig");

            EditorGUI.BeginChangeCheck();
            DrawTransformHandle("Pelvis", ref i, ref t.currentPose.pelvisPosition, ref t.currentPose.pelvisRotation);

            var hp = t.GetBone(HumanBodyBones.Head).position;
            var cp = t.ChestBone.position;

            DrawRotationHandle("Head", ref i, hp, ref t.currentPose.headRotation);
            DrawRotationHandle("Chest", ref i, cp, ref t.currentPose.chestRotation);

            DrawIKLimbTargetHandle("L Foot", ref i, ref t.currentPose.leftFoot);
            DrawIKLimbTargetHandle("R Foot", ref i, ref t.currentPose.rightFoot);
            DrawIKLimbTargetHandle("L Hand", ref i, ref t.currentPose.leftHand);
            DrawIKLimbTargetHandle("R Hand", ref i, ref t.currentPose.rightHand);

            if (EditorGUI.EndChangeCheck()) {
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if(Event.current.commandName == "ObjectSelectorUpdated") {
                var o = EditorGUIUtility.GetObjectPickerObject();
                if(o == null) {
                    return;
                } else {
                    var lp = o as SerializedHumanoidPose;
                    if(lp == null) {
                        return;
                    }
                    rc.loadedPose = lp;
                    rc.RevertPose();
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
            }
        }
    }
}