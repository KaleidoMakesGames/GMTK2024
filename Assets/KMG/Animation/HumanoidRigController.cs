using NaughtyAttributes;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KMGAnimation {
    [ExecuteAlways]
    public class HumanoidRigController : MonoBehaviour {
        public bool showGizmos;

        public delegate void IKHandler();
        public event IKHandler BeforeIK;
        public event IKHandler AfterTPose;
        public event IKHandler AfterIK;

        [System.Serializable]
        public struct IKLimbOffset {
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
        }

        [System.Serializable]
        public struct HandPose {
            public Vector3 leftThumb;
            public Vector3 rightThumb;
        }
        public HandPose openHand;
        public HandPose closedHand;

        [Header("IK Offsets")]
        public IKLimbOffset leftFootOffset;
        public IKLimbOffset rightFootOffset;
        public IKLimbOffset leftHandOffset;
        public IKLimbOffset rightHandOffset;

        [SerializeField][HideInInspector] private Armature.BoneState[] _tPose;


        [Header("Armature")]
        [Space]
        public Transform armatureHipsRoot;
        public HumanoidArmature armature;
        public Transform ChestBone {
            get {
                return armature.boneTransforms[armature.GetTopSpineBone()];
            }
        }

        [Header("Pose Data")]
        public HumanoidPose currentPose; // Current pose in world space

        [ReadOnly] public SerializedHumanoidPose loadedPose;
        [Button]
        public void NewPose() {
            loadedPose = null;
        }
        [Button]
        public void LoadPose() {
            EditorGUIUtility.ShowObjectPicker<SerializedHumanoidPose>(loadedPose, false, "", -1);
        }
        [Button]
        public void SavePose() {
            if(loadedPose == null) {
                SavePoseAs();
            } else {
                loadedPose.pose = currentPose.InverseTransformPose(transform.localToWorldMatrix);
            }
        }
        [Button]
        [ShowIf("hasLoaded")]
        public void SavePoseAs() {
            var p = "Assets";
            if(loadedPose != null) {
                p = AssetDatabase.GetAssetPath(loadedPose);
            }
            var s = EditorUtility.SaveFilePanelInProject("Save Pose", "Pose", "asset", "", p);
            if(s == "") {
                return;
            }
            var n = ScriptableObject.CreateInstance<SerializedHumanoidPose>();
            n.pose = currentPose.InverseTransformPose(transform.localToWorldMatrix);
            AssetDatabase.CreateAsset(n, s);
            loadedPose = n;
        }
        private bool hasLoaded { get { return loadedPose != null; } }
        [ShowIf("hasLoaded")]
        [Button]
        public void RevertPose() {
            if (loadedPose != null) {
                currentPose = loadedPose.pose.TransformPose(transform.localToWorldMatrix);
            }
        }

        [Button(group: "Pose Utilities")]
        public void Mirror() {
            currentPose = currentPose.InverseTransformPose(transform.localToWorldMatrix).Mirrored().TransformPose(transform.localToWorldMatrix);
        }
        [Button(text: "L => R", group: "Pose Utilities")]
        public void LToR() {
            currentPose = currentPose.InverseTransformPose(transform.localToWorldMatrix).LToR().TransformPose(transform.localToWorldMatrix);
        }
        [Button(text: "R => L", group: "Pose Utilities")]
        public void RToL() {
            currentPose = currentPose.InverseTransformPose(transform.localToWorldMatrix).RToL().TransformPose(transform.localToWorldMatrix);
        }

        [Button(group:"Armature")]
        public void RemapArmature() {
            armature = new HumanoidArmature(armatureHipsRoot);
        }

        [Button(group: "Armature")]
        public void MarkAsTPose() {
            _tPose = armature.GetCurrentPose();
        }
        [Button(group: "Armature")]
        public void ResetToTPose() {
            if (_tPose != null) {
                armature.ApplyPose(_tPose);
            }
            IKToBones();
        }
        public void IKToBones() {
            void MapLimb(ref HumanoidPose.LimbPose p, in IKLimbOffset l, HumanBodyBones b, HumanBodyBones j) {
                p.rotation = GetBone(b).rotation * Quaternion.Inverse(Quaternion.Euler(l.rotationOffset));
                p.position = GetBone(b).position - p.rotation * l.positionOffset;
                p.polePosition = GetBone(j).position;
            }
            MapLimb(ref currentPose.leftFoot, leftFootOffset, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg);
            MapLimb(ref currentPose.rightFoot,rightFootOffset, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg);
            MapLimb(ref currentPose.leftHand,leftHandOffset, HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm);
            MapLimb(ref currentPose.rightHand, rightHandOffset, HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm);
            currentPose.chestRotation = ChestBone.rotation;
            currentPose.headRotation = GetBone(HumanBodyBones.Head).rotation;
            currentPose.pelvisPosition = GetBone(HumanBodyBones.Hips).position;
            currentPose.pelvisRotation = GetBone(HumanBodyBones.Hips).rotation;
        }

        private void Reset() {
            armatureHipsRoot = HumanoidArmature.SearchForHips(transform);
            RemapArmature();
            IKToBones();
        }
        private void Update() {
            IKUpdate();
        }

        public void IKUpdate() {
            if (Application.isPlaying && BeforeIK != null) {
                BeforeIK.Invoke();
            }
            if (_tPose != null) {
                armature.ApplyPose(_tPose);
            }
            if (Application.isPlaying && AfterTPose != null) {
                AfterTPose.Invoke();
            }
            ApplyIKPass();
            if (Application.isPlaying && AfterIK != null) {
                AfterIK.Invoke();
            }
        }

        private void ApplyLimbIK(in HumanoidPose.LimbPose limb, in IKLimbOffset limbOffset, HumanBodyBones b, int chainLength) {
            InverseKinematics.IKConstraint constraint = new InverseKinematics.IKConstraint();
            constraint.rotation = limb.rotation * Quaternion.Euler(limbOffset.rotationOffset);
            constraint.rotation = limb.rotation * Quaternion.Euler(limbOffset.rotationOffset);
            constraint.target = limb.position + limb.rotation * limbOffset.positionOffset;
            constraint.poleTarget = limb.polePosition;
            constraint.bones = new Transform[chainLength];
            constraint.bones[chainLength - 1] = armature.boneTransforms[b];
            for (int i = chainLength - 2; i >= 0; i--) {
                constraint.bones[i] = constraint.bones[i + 1].parent;
            }

            InverseKinematics.ApplyConstraint(in constraint);
        }

        private void ApplyOrientation(Quaternion orientation, float weight, HumanBodyBones bone) {
            var boneTransform = armature.boneTransforms[bone];
            if (boneTransform == null) {
                return;
            }
            boneTransform.rotation = Quaternion.Lerp(boneTransform.rotation, orientation, weight);
        }

        private void SetSubboneRotation(HumanBodyBones bone, Quaternion localRotation) {
            foreach (var child in bone.GetBoneChildren()) {
                var boneTransform = armature.boneTransforms[child];
                if (boneTransform != null) {
                    boneTransform.localRotation = localRotation;
                }
                SetSubboneRotation(child, localRotation);
            }
        }
        public void ApplyIKPass() {
            // Position the pelvis
            armature.boneTransforms[HumanBodyBones.Hips].transform.position = currentPose.pelvisPosition;
            armature.boneTransforms[HumanBodyBones.Hips].transform.rotation = currentPose.pelvisRotation;

            // Orient the spine
            ApplyOrientation(currentPose.chestRotation, currentPose.spineLookWeight, HumanBodyBones.Spine);
            ApplyOrientation(currentPose.chestRotation, currentPose.lowerChestLookWeight, HumanBodyBones.Chest);
            ApplyOrientation(currentPose.chestRotation, 1, armature.GetTopSpineBone());

            // Orient the head
            ApplyOrientation(currentPose.headRotation, currentPose.neckLookWeight, HumanBodyBones.Neck);
            ApplyOrientation(currentPose.headRotation, 1, HumanBodyBones.Head);

            // Apply constraints to the limbs
            ApplyLimbIK(in currentPose.leftFoot, leftFootOffset, HumanBodyBones.LeftFoot, 3);
            ApplyLimbIK(in currentPose.rightFoot, rightFootOffset, HumanBodyBones.RightFoot, 3);
            ApplyLimbIK(in currentPose.leftHand, leftHandOffset, HumanBodyBones.LeftHand, 3);
            ApplyLimbIK(in currentPose.rightHand, rightHandOffset, HumanBodyBones.RightHand, 3);

            // Apply finger state
            SetSubboneRotation(HumanBodyBones.LeftHand, Quaternion.Euler(Mathf.Lerp(90.0f, 0.0f, currentPose.leftHandOpen), 0.0f, 0.0f));
            SetSubboneRotation(HumanBodyBones.RightHand, Quaternion.Euler(Mathf.Lerp(90.0f, 0.0f, currentPose.rightHandOpen), 0.0f, 0.0f));

            Quaternion closedQuatLT = Quaternion.Euler(closedHand.leftThumb);
            Quaternion openQuatLT = Quaternion.Euler(openHand.leftThumb);
            armature.boneTransforms[HumanBodyBones.LeftThumbProximal].localRotation = Quaternion.Lerp(closedQuatLT, openQuatLT, currentPose.leftHandOpen);

            Quaternion closedQuatRT = Quaternion.Euler(closedHand.rightThumb);
            Quaternion openQuatRT = Quaternion.Euler(openHand.rightThumb);
            armature.boneTransforms[HumanBodyBones.RightThumbProximal].localRotation = Quaternion.Lerp(closedQuatRT, openQuatRT, currentPose.rightHandOpen);
        }

        public Transform GetBone(HumanBodyBones leftHand) {
            return armature.boneTransforms[leftHand];
        }

        private void OnDrawGizmos() {
            if (Selection.activeGameObject != gameObject && !showGizmos) {
                return;
            }
            var hp = GetBone(HumanBodyBones.Head).position;
            var cp = ChestBone.position;
            Gizmos.color = Color.white;
            DrawLine(currentPose.pelvisPosition, cp);
            DrawLine(currentPose.pelvisPosition, currentPose.leftFoot.polePosition);
            DrawLine(currentPose.pelvisPosition, currentPose.rightFoot.polePosition);
            DrawLine(cp, hp);
            DrawLine(cp, currentPose.leftHand.polePosition);
            DrawLine(cp, currentPose.rightHand.polePosition);
            DrawLine(currentPose.leftFoot.polePosition, currentPose.leftFoot.position);
            DrawLine(currentPose.rightFoot.polePosition, currentPose.rightFoot.position);
            DrawLine(currentPose.leftHand.polePosition, currentPose.leftHand.position);
            DrawLine(currentPose.rightHand.polePosition, currentPose.rightHand.position);
            if (Selection.activeGameObject == gameObject) {
                return;
            }
            DrawCone(cp, ChestBone.forward);
            DrawCone(hp, GetBone(HumanBodyBones.Head).forward);
            DrawCone(currentPose.pelvisPosition, currentPose.pelvisRotation * Vector3.forward);
            Gizmos.color = Color.blue;
            DrawCone(currentPose.leftFoot.position, currentPose.leftFoot.rotation * Vector3.forward);
            DrawSphere(currentPose.leftFoot.polePosition);
            DrawCone(currentPose.leftHand.position, currentPose.leftHand.rotation * Vector3.forward);
            DrawSphere(currentPose.leftHand.polePosition);
            Gizmos.color = Color.red;
            DrawCone(currentPose.rightFoot.position, currentPose.rightFoot.rotation * Vector3.forward);
            DrawSphere(currentPose.rightFoot.polePosition);
            DrawCone(currentPose.rightHand.position, currentPose.rightHand.rotation * Vector3.forward);
            DrawSphere(currentPose.rightHand.polePosition);
        }
        private void DrawSphere(Vector3 p) {
            float size = HandleUtility.GetHandleSize(p) * 0.2f;
            Gizmos.DrawSphere(p, size/2);
        }
        private void DrawCone(Vector3 p, Vector3 forward) {
            float size = HandleUtility.GetHandleSize(p) * 0.2f;
            var c = Gizmos.color;
            c.a = 0.5f;
            Handles.color = c;
            Handles.ConeHandleCap(-1, p, Quaternion.LookRotation(forward, Vector3.up), size, EventType.Repaint);
        }
        private void DrawLine(Vector3 a, Vector3 b) {
            Handles.DrawDottedLine(a, b, 5.0f);
        }
    }
}