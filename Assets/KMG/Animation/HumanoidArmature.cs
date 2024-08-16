using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace KMGAnimation {
    [System.Serializable]
    public class HumanoidArmature : Armature {
        [System.Serializable]
        public struct ArmatureMetrics {
            public float leftLegLength;
            public float rightLegLength;
            public float leftArmLength;
            public float rightArmLength;
            public float torsoLength;
        }
        // Bone extraction system

        [SerializeField] public SerializableDictionary<HumanBodyBones, Transform> boneTransforms;
        public ArmatureMetrics metrics;
        public HumanoidArmature(Transform root) : base(root) {
            AutomapBones();
            GenerateMetrics();
        }
        public HumanBodyBones GetTopSpineBone() {
            if (boneTransforms[HumanBodyBones.UpperChest] != null) {
                return HumanBodyBones.UpperChest;
            }
            if (boneTransforms[HumanBodyBones.Chest] != null) {
                return HumanBodyBones.Chest;
            }
            return HumanBodyBones.Spine;
        }
        public static Transform SearchForHips(Transform t) {
            var q = new Queue<Transform>();
            q.Enqueue(t);
            while (q.Count > 0) {
                var c = q.Dequeue();
                if (c.childCount == 3 && c.GetChildren().All(x => x.childCount > 0)) {
                    return c;
                }
                foreach (var gc in c.GetChildren()) {
                    q.Enqueue(gc);
                }
            }
            return null;
        }

        public void GenerateMetrics() {
            metrics.torsoLength = Vector3.Distance(boneTransforms[HumanBodyBones.Hips].position, boneTransforms[GetTopSpineBone()].position);
            metrics.leftLegLength = Vector3.Distance(boneTransforms[HumanBodyBones.LeftFoot].position, boneTransforms[HumanBodyBones.LeftUpperLeg].position);
            metrics.leftArmLength = Vector3.Distance(boneTransforms[HumanBodyBones.LeftHand].position, boneTransforms[HumanBodyBones.LeftUpperArm].position);
            metrics.rightLegLength = Vector3.Distance(boneTransforms[HumanBodyBones.RightFoot].position, boneTransforms[HumanBodyBones.RightUpperLeg].position);
            metrics.rightArmLength = Vector3.Distance(boneTransforms[HumanBodyBones.RightHand].position, boneTransforms[HumanBodyBones.RightUpperArm].position);
        }

        public void AutomapBones() {
            boneTransforms = new SerializableDictionary<HumanBodyBones, Transform>();
            foreach (HumanBodyBones v in Enum.GetValues(typeof(HumanBodyBones))) {
                boneTransforms[v] = null;
            }

            boneTransforms[HumanBodyBones.Hips] = root;

            if (boneTransforms[HumanBodyBones.Hips].childCount != 3) {
                throw new System.Exception("Hip bone needs exactly three children.");
            }
            boneTransforms[HumanBodyBones.Spine] = root.GetChildren().OrderBy(x => x.localPosition.y).Last();
            boneTransforms[HumanBodyBones.LeftUpperLeg] = root.GetChildren().OrderBy(x => x.localPosition.x).First();
            boneTransforms[HumanBodyBones.RightUpperLeg] = root.GetChildren().OrderBy(x => x.localPosition.x).Last();

            // Map chest
            var shoulderSource = HumanBodyBones.Spine;
            if (boneTransforms[HumanBodyBones.Spine].childCount == 1) {
                boneTransforms[HumanBodyBones.Chest] = boneTransforms[HumanBodyBones.Spine].GetChild(0);
                shoulderSource = HumanBodyBones.Chest;
                if (boneTransforms[HumanBodyBones.Chest].childCount == 1) {
                    boneTransforms[HumanBodyBones.UpperChest] = boneTransforms[HumanBodyBones.Chest].GetChild(0);
                    shoulderSource = HumanBodyBones.UpperChest;
                }
            }

            if (boneTransforms[shoulderSource].childCount != 3) {
                throw new System.Exception("Topmost chest/spine bone needs exactly three children.");
            }

            // Map head/neck
            var possibleNeck = boneTransforms[shoulderSource].GetChildren().OrderBy(x => x.localPosition.y).Last();
            if (possibleNeck.childCount == 1) {
                boneTransforms[HumanBodyBones.Neck] = possibleNeck;
                boneTransforms[HumanBodyBones.Head] = possibleNeck.GetChild(0);
            } else {
                boneTransforms[HumanBodyBones.Head] = possibleNeck;
            }

            // Map face
            if (boneTransforms[HumanBodyBones.Head].childCount >= 2) {
                boneTransforms[HumanBodyBones.LeftEye] = boneTransforms[HumanBodyBones.Head].GetChildren().OrderBy(x => x.localPosition.x).First();
                boneTransforms[HumanBodyBones.RightEye] = boneTransforms[HumanBodyBones.Head].GetChildren().OrderBy(x => x.localPosition.x).Last();
                if (boneTransforms[HumanBodyBones.Head].childCount == 3) {
                    boneTransforms[HumanBodyBones.Jaw] = boneTransforms[HumanBodyBones.Head].GetChildren().OrderBy(x => x.localPosition.y).First();
                }
            }

            // Map left arm.
            var l1 = boneTransforms[shoulderSource].GetChildren().OrderBy(x => x.localPosition.x).First();
            if (l1.childCount != 1) {
                throw new System.Exception("Left arm chain requires at least 3 bones.");
            }
            var l2 = l1.GetChild(0);
            if (l2.childCount != 1) {
                throw new System.Exception("Left arm chain requires at least 3 bones.");
            }
            var l3 = l2.GetChild(0);
            if (l3.childCount == 1) {
                var l4 = l3.GetChild(0);
                boneTransforms[HumanBodyBones.LeftShoulder] = l1;
                boneTransforms[HumanBodyBones.LeftUpperArm] = l2;
                boneTransforms[HumanBodyBones.LeftLowerArm] = l3;
                boneTransforms[HumanBodyBones.LeftHand] = l4;
            } else {
                boneTransforms[HumanBodyBones.LeftUpperArm] = l1;
                boneTransforms[HumanBodyBones.LeftLowerArm] = l2;
                boneTransforms[HumanBodyBones.LeftHand] = l3;
            }

            // Map right arm.
            var r1 = boneTransforms[shoulderSource].GetChildren().OrderBy(x => x.localPosition.x).Last();
            if (r1.childCount != 1) {
                throw new System.Exception("Right arm chain requires at least 3 bones.");
            }
            var r2 = r1.GetChild(0);
            if (r2.childCount != 1) {
                throw new System.Exception("Right arm chain requires at least 3 bones.");
            }
            var r3 = r2.GetChild(0);
            if (r3.childCount == 1) {
                var r4 = r3.GetChild(0);
                boneTransforms[HumanBodyBones.RightShoulder] = r1;
                boneTransforms[HumanBodyBones.RightUpperArm] = r2;
                boneTransforms[HumanBodyBones.RightLowerArm] = r3;
                boneTransforms[HumanBodyBones.RightHand] = r4;
            } else {
                boneTransforms[HumanBodyBones.RightUpperArm] = r1;
                boneTransforms[HumanBodyBones.RightLowerArm] = r2;
                boneTransforms[HumanBodyBones.RightHand] = r3;
            }

            // Map left leg.
            if (boneTransforms[HumanBodyBones.LeftUpperLeg].childCount != 1) {
                throw new System.Exception("Left leg chain requires at least 3 bones.");
            }
            boneTransforms[HumanBodyBones.LeftLowerLeg] = boneTransforms[HumanBodyBones.LeftUpperLeg].GetChild(0);
            if (boneTransforms[HumanBodyBones.LeftLowerLeg].childCount != 1) {
                throw new System.Exception("Left leg chain requires at least 3 bones.");
            }
            boneTransforms[HumanBodyBones.LeftFoot] = boneTransforms[HumanBodyBones.LeftLowerLeg].GetChild(0);

            // Map right leg.
            if (boneTransforms[HumanBodyBones.RightUpperLeg].childCount != 1) {
                throw new System.Exception("Right leg chain requires at least 3 bones.");
            }
            boneTransforms[HumanBodyBones.RightLowerLeg] = boneTransforms[HumanBodyBones.RightUpperLeg].GetChild(0);
            if (boneTransforms[HumanBodyBones.RightLowerLeg].childCount != 1) {
                throw new System.Exception("Right leg chain requires at least 3 bones.");
            }
            boneTransforms[HumanBodyBones.RightFoot] = boneTransforms[HumanBodyBones.RightLowerLeg].GetChild(0);

            // Map toes.
            if (boneTransforms[HumanBodyBones.LeftFoot].childCount == 1) {
                boneTransforms[HumanBodyBones.LeftToes] = boneTransforms[HumanBodyBones.LeftFoot].GetChild(0);
            }
            if (boneTransforms[HumanBodyBones.RightFoot].childCount == 1) {
                boneTransforms[HumanBodyBones.RightToes] = boneTransforms[HumanBodyBones.RightFoot].GetChild(0);
            }

            // Map hands.
            var handBonesL = boneTransforms[HumanBodyBones.LeftHand].GetChildren().OrderByDescending(x => x.localPosition.x).ToArray();
            for (int i = 0; i < handBonesL.Length; i++) {
                var prox = HumanBodyBones.LeftHand.GetBoneChildren()[i];
                var inter = HumanBodyBones.LeftHand.GetBoneChildren()[i].GetBoneChildren()[0];
                var dist = HumanBodyBones.LeftHand.GetBoneChildren()[i].GetBoneChildren()[0].GetBoneChildren()[0];
                boneTransforms[prox] = handBonesL[i];
                if (boneTransforms[prox].childCount > 0) {
                    boneTransforms[inter] = boneTransforms[prox].GetChild(0);
                    if (boneTransforms[inter].childCount > 0) {
                        boneTransforms[dist] = boneTransforms[inter].GetChild(0);
                    }
                }
            }

            var handBonesR = boneTransforms[HumanBodyBones.RightHand].GetChildren().OrderBy(x => x.localPosition.x).ToArray();
            for (int i = 0; i < handBonesL.Length; i++) {
                var prox = HumanBodyBones.RightHand.GetBoneChildren()[i];
                var inter = HumanBodyBones.RightHand.GetBoneChildren()[i].GetBoneChildren()[0];
                var dist = HumanBodyBones.RightHand.GetBoneChildren()[i].GetBoneChildren()[0].GetBoneChildren()[0];
                boneTransforms[prox] = handBonesR[i];
                if (boneTransforms[prox].childCount > 0) {
                    boneTransforms[inter] = boneTransforms[prox].GetChild(0);
                    if (boneTransforms[inter].childCount > 0) {
                        boneTransforms[dist] = boneTransforms[inter].GetChild(0);
                    }
                }
            }
        }
    }
    public static class HumanBodyBonesExtensions {
        public static HumanBodyBones[] GetBoneChildren(this HumanBodyBones bone) {
            if (!_boneChildren.ContainsKey(bone)) {
                return new HumanBodyBones[0];
            }
            return _boneChildren[bone];
        }
        public static HumanBodyBones GetBoneParent(this HumanBodyBones bone) {
            return _boneChildren.Where(x => x.Value.Contains(bone)).First().Key;
        }
        private static Dictionary<HumanBodyBones, HumanBodyBones[]> _boneChildren = new Dictionary<HumanBodyBones, HumanBodyBones[]>() {
        { HumanBodyBones.Hips,          new HumanBodyBones[]{HumanBodyBones.Spine, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg} },
        { HumanBodyBones.Spine,         new HumanBodyBones[]{HumanBodyBones.Chest } },
        { HumanBodyBones.Chest,         new HumanBodyBones[]{HumanBodyBones.UpperChest } },
        { HumanBodyBones.UpperChest,    new HumanBodyBones[]{HumanBodyBones.Neck, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder } },
        { HumanBodyBones.Neck,          new HumanBodyBones[]{HumanBodyBones.Head } },
        { HumanBodyBones.Head,          new HumanBodyBones[]{HumanBodyBones.LeftEye, HumanBodyBones.RightEye, HumanBodyBones.Jaw}},
        { HumanBodyBones.LeftShoulder, new HumanBodyBones[]{HumanBodyBones.LeftUpperArm } },
        { HumanBodyBones.LeftUpperArm, new HumanBodyBones[]{HumanBodyBones.LeftLowerArm } },
        { HumanBodyBones.LeftLowerArm, new HumanBodyBones[]{HumanBodyBones.LeftHand } },
        { HumanBodyBones.LeftHand, new HumanBodyBones[]{HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftLittleProximal, } },
        { HumanBodyBones.LeftThumbProximal, new HumanBodyBones[]{HumanBodyBones.LeftThumbIntermediate } },
        { HumanBodyBones.LeftIndexProximal, new HumanBodyBones[]{HumanBodyBones.LeftIndexIntermediate } },
        { HumanBodyBones.LeftMiddleProximal, new HumanBodyBones[]{HumanBodyBones.LeftMiddleIntermediate } },
        { HumanBodyBones.LeftRingProximal, new HumanBodyBones[]{HumanBodyBones.LeftRingIntermediate} },
        { HumanBodyBones.LeftLittleProximal, new HumanBodyBones[]{HumanBodyBones.LeftLittleIntermediate } },
        { HumanBodyBones.LeftThumbIntermediate, new HumanBodyBones[]{HumanBodyBones.LeftThumbDistal } },
        { HumanBodyBones.LeftIndexIntermediate, new HumanBodyBones[]{HumanBodyBones.LeftIndexDistal } },
        { HumanBodyBones.LeftMiddleIntermediate, new HumanBodyBones[]{HumanBodyBones.LeftMiddleDistal } },
        { HumanBodyBones.LeftRingIntermediate, new HumanBodyBones[]{HumanBodyBones.LeftRingDistal } },
        { HumanBodyBones.LeftLittleIntermediate, new HumanBodyBones[]{HumanBodyBones.LeftLittleDistal } },
        { HumanBodyBones.RightShoulder, new HumanBodyBones[]{HumanBodyBones.RightUpperArm } },
        { HumanBodyBones.RightUpperArm, new HumanBodyBones[]{HumanBodyBones.RightLowerArm } },
        { HumanBodyBones.RightLowerArm, new HumanBodyBones[]{HumanBodyBones.RightHand } },
        { HumanBodyBones.RightHand, new HumanBodyBones[]{HumanBodyBones.RightThumbProximal, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightRingProximal, HumanBodyBones.RightLittleProximal, } },
        { HumanBodyBones.RightThumbProximal, new HumanBodyBones[]{HumanBodyBones.RightThumbIntermediate } },
        { HumanBodyBones.RightIndexProximal, new HumanBodyBones[]{HumanBodyBones.RightIndexIntermediate } },
        { HumanBodyBones.RightMiddleProximal, new HumanBodyBones[]{HumanBodyBones.RightMiddleIntermediate } },
        { HumanBodyBones.RightRingProximal, new HumanBodyBones[]{HumanBodyBones.RightRingIntermediate} },
        { HumanBodyBones.RightLittleProximal, new HumanBodyBones[]{HumanBodyBones.RightLittleIntermediate } },
        { HumanBodyBones.RightThumbIntermediate, new HumanBodyBones[]{HumanBodyBones.RightThumbDistal } },
        { HumanBodyBones.RightIndexIntermediate, new HumanBodyBones[]{HumanBodyBones.RightIndexDistal } },
        { HumanBodyBones.RightMiddleIntermediate, new HumanBodyBones[]{HumanBodyBones.RightMiddleDistal } },
        { HumanBodyBones.RightRingIntermediate, new HumanBodyBones[]{HumanBodyBones.RightRingDistal } },
        { HumanBodyBones.RightLittleIntermediate, new HumanBodyBones[]{HumanBodyBones.RightLittleDistal } },
        {HumanBodyBones.LeftUpperLeg, new HumanBodyBones[]{HumanBodyBones.LeftLowerLeg} },
        {HumanBodyBones.LeftLowerLeg, new HumanBodyBones[]{HumanBodyBones.LeftFoot} },
        {HumanBodyBones.LeftFoot, new HumanBodyBones[]{HumanBodyBones.LeftToes} },
        {HumanBodyBones.RightUpperLeg, new HumanBodyBones[]{HumanBodyBones.RightLowerLeg} },
        {HumanBodyBones.RightLowerLeg, new HumanBodyBones[]{HumanBodyBones.RightFoot} },
        {HumanBodyBones.RightFoot, new HumanBodyBones[]{HumanBodyBones.RightToes} }
    };
    }

    [System.Serializable]
    public class Armature {
        public Transform root;
        public Armature(Transform root) {
            this.root = root;
        }

        [System.Serializable]
        public struct BoneState {
            public Transform bone;
            public Quaternion localRotation;
            public Vector3 localPosition;
        }

        public BoneState[] GetCurrentPose() {
            var pose = new List<BoneState>();
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0) {
                var bone = queue.Dequeue();
                foreach (var childBone in bone.GetChildren()) {
                    queue.Enqueue(childBone);
                }
                var poseData = new BoneState();
                poseData.bone = bone;
                poseData.localPosition = bone.localPosition;
                poseData.localRotation = bone.localRotation;
                pose.Add(poseData);
            }
            return pose.ToArray();
        }

        public void ApplyPose(BoneState[] pose) {
            foreach (var poseData in pose) {
                poseData.bone.localPosition = poseData.localPosition;
                poseData.bone.localRotation = poseData.localRotation;
            }
        }
    }
}