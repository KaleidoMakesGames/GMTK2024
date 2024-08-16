using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace KMGAnimation {
    [RequireComponent(typeof(Animator))]
    [ExecuteInEditMode]
    public class MecanimIKHelper : MonoBehaviour {
        public enum IKPass { Pelvis, ChestLegs, HeadHands, Final, Invalid }
        public delegate void IKHandler(IKPass pass);
        public event IKHandler OnIKEvent;

        public bool enableIK;

        [System.Serializable]
        public struct IKLimb {
            public Transform target;
            [AllowNesting][ShowIf("hasTarget")] public Vector3 positionOffset;
            [AllowNesting][ShowIf("hasTarget")] public Vector3 rotationOffset;
            [ShowIf("hasTarget")][AllowNesting] public Transform jointTarget;
            [ShowIf("hasTarget")][AllowNesting][Range(0.0f, 1.0f)] public float positionWeight;
            [AllowNesting][ShowIf("hasTarget")][Range(0.0f, 1.0f)] public float rotationWeight;
            [AllowNesting][ShowIf(EConditionOperator.And, "hasTarget", "hasHint")][Range(0.0f, 1.0f)] public float hintWeight;
            public bool hasTarget {
                get {
                    return target != null;
                }
            }
            public bool hasHint {
                get {
                    return jointTarget != null;
                }
            }
        }
        [Header("IK")]
        public IKLimb leftFoot;
        public IKLimb rightFoot;
        public IKLimb leftHand;
        public IKLimb rightHand;

        [Space]
        public Transform chestTarget;
        [ShowIf("hasChestTarget")][Range(0.0f, 1.0f)] public float chestWeight;
        [Space]
        public Transform headTarget;
        [ShowIf("hasHeadTarget")][Range(0.0f, 1.0f)] public float headWeight;

        public Transform pelvisTarget;
        [ShowIf("hasPelvisTarget")][Range(0.0f, 1.0f)] public float pelvisPositionWeight;
        [ShowIf("hasPelvisTarget")][Range(0.0f, 1.0f)] public float pelvisRotationWeight;

        public bool showGizmos;
        [ShowIf("showGizmos")] public float gizmoScale;
        [ShowIf("showGizmos")] public bool wireFrame;

        private Animator _animator;
        public Animator animator {
            get {
                if (_animator == null) {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }
        public Transform GetBone(HumanBodyBones bone) {
            return animator.GetBoneTransform(bone);
        }

        public Vector3 pelvisPosition {
            get { return animator.bodyPosition; }
        }
        public Quaternion pelvisRotation {
            get { return animator.bodyRotation; }
        }
        public bool hasChestTarget { get { return chestTarget != null; } }
        public bool hasHeadTarget { get { return headTarget != null; } }
        public bool hasPelvisTarget { get { return pelvisTarget != null; } }

        private void ApplyLimbIK(IKLimb c, AvatarIKGoal goal, AvatarIKHint hint) {
            bool active = c.target != null && c.target.gameObject.activeSelf;
            bool hintActive = active && c.jointTarget != null && c.jointTarget.gameObject.activeSelf;
            if (active) {
                Quaternion r = c.target.rotation * Quaternion.Euler(c.rotationOffset);
                animator.SetIKPosition(goal, c.target.position + r * c.positionOffset);
                animator.SetIKRotation(goal, r);
            }
            if (hintActive) {
                animator.SetIKHintPosition(hint, c.jointTarget.position);
            }
            animator.SetIKPositionWeight(goal, active ? c.positionWeight : 0);
            animator.SetIKRotationWeight(goal, active ? c.rotationWeight : 0);
            animator.SetIKHintPositionWeight(hint, hintActive ? c.hintWeight : 0.0f);
        }

        private void ApplyLookAt(Transform target, float weight, HumanBodyBones bone) {
            if (target == null || !target.gameObject.activeSelf) {
                return;
            }
            var boneTransform = animator.GetBoneTransform(bone);
            Vector3 delta = (target.position - boneTransform.position).normalized;
            Vector3 deltaLocal = boneTransform.parent.InverseTransformDirection(delta);
            Quaternion rotLocal = Quaternion.LookRotation(deltaLocal, Vector3.up);
            animator.SetBoneLocalRotation(bone, Quaternion.Lerp(boneTransform.localRotation, rotLocal, weight));
        }

        private IKPass IndexToPass(int layerIndex) {
            string layerName = animator.GetLayerName(layerIndex);
            if (layerName == "PelvisIK") {
                return IKPass.Pelvis;
            }
            if (layerName == "ChestLegsIK") {
                return IKPass.ChestLegs;
            }
            if (layerName == "HeadArmsIK") {
                return IKPass.HeadHands;
            }
            if (layerName == "FinalIK") {
                return IKPass.Final;
            }
            return IKPass.Invalid;
        }
        private void OnAnimatorIK(int layerIndex) {
            if (!enableIK) {
                return;
            }

            var pass = IndexToPass(layerIndex);
            if (OnIKEvent != null) {
                OnIKEvent.Invoke(pass);
            }
            if (pass == IKPass.Pelvis) {
                if (pelvisTarget != null) {
                    animator.bodyPosition = pelvisTarget.position;
                    animator.bodyRotation = pelvisTarget.rotation;
                }
            } else if (pass == IKPass.ChestLegs || pass == IKPass.Final) {
                ApplyLimbIK(leftFoot, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee);
                ApplyLimbIK(rightFoot, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee);
                ApplyLookAt(chestTarget, chestWeight, HumanBodyBones.Chest);
            } else if (pass == IKPass.HeadHands || pass == IKPass.Final) {
                ApplyLimbIK(leftHand, AvatarIKGoal.LeftHand, AvatarIKHint.LeftElbow);
                ApplyLimbIK(rightHand, AvatarIKGoal.RightHand, AvatarIKHint.RightElbow);
                ApplyLookAt(headTarget, headWeight, HumanBodyBones.Head);
            }

            GetBone(HumanBodyBones.LeftHand).rotation = Quaternion.identity;
        }

        private void Update() {
            if (!Application.isPlaying) {
                if (animator != null) {
                    animator.Update(0);
                }
            }
        }

        private void DrawLimb(in IKLimb limb, Color c) {
            Gizmos.color = c;
            if (limb.target != null && limb.target.gameObject.activeSelf) {
                if (wireFrame) {
                    Gizmos.DrawWireSphere(limb.target.position, gizmoScale);
                } else {
                    Gizmos.DrawSphere(limb.target.position, gizmoScale);
                }
                if (limb.jointTarget != null && limb.jointTarget.gameObject.activeSelf) {
                    Gizmos.color = Color.Lerp(c, Color.white, 0.5f);
                    if (wireFrame) {
                        Gizmos.DrawWireSphere(limb.jointTarget.position, gizmoScale);
                    } else {
                        Gizmos.DrawSphere(limb.jointTarget.position, gizmoScale);
                    }
                }
            }
        }
        private void OnDrawGizmos() {
            if (showGizmos) {
                if (headTarget != null && headTarget.gameObject.activeSelf) {
                    Gizmos.color = Color.yellow;
                    if (wireFrame) {
                        Gizmos.DrawWireSphere(headTarget.position, gizmoScale);
                    } else {
                        Gizmos.DrawSphere(headTarget.position, gizmoScale);
                    }
                    Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.Head).position, headTarget.position);
                }
                if (chestTarget != null && chestTarget.gameObject.activeSelf) {
                    Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
                    if (wireFrame) {
                        Gizmos.DrawWireSphere(chestTarget.position, gizmoScale);
                    } else {
                        Gizmos.DrawSphere(chestTarget.position, gizmoScale);
                    }
                    Gizmos.DrawLine(animator.GetBoneTransform(HumanBodyBones.Chest).position, chestTarget.position);
                }
                if (pelvisTarget != null && pelvisTarget.gameObject.activeSelf) {
                    Gizmos.color = Color.black;
                    if (wireFrame) {
                        Gizmos.DrawWireSphere(pelvisTarget.position, gizmoScale);
                    } else {
                        Gizmos.DrawSphere(pelvisTarget.position, gizmoScale);
                    }
                }
                DrawLimb(in leftFoot, Color.blue);
                DrawLimb(in leftHand, Color.cyan);
                DrawLimb(in rightFoot, Color.red);
                DrawLimb(in rightHand, Color.magenta);
            }
        }
    }
}