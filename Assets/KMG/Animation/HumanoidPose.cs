
using MathUtilities;
using UnityEngine;

namespace KMGAnimation {
    [System.Serializable]
    public struct HumanoidPose {
        [System.Serializable]
        public struct LimbPose {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 polePosition;

            public static LimbPose Lerp(LimbPose a, LimbPose b, float t) {
                var n = new LimbPose();
                n.position = Vector3.LerpUnclamped(a.position, b.position, t);
                n.rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, t);
                n.polePosition = Vector3.LerpUnclamped(a.polePosition, b.polePosition, t);
                return n;
            }
            public static LimbPose CatmullRom(LimbPose p0, LimbPose p1, LimbPose p2, LimbPose p3, float t) {
                var n = new LimbPose();
                n.position = Interpolation.CatmullRom(p0.position, p1.position, p2.position, p3.position, t);
                n.rotation = Interpolation.CatmullRom(p0.rotation, p1.rotation, p2.rotation, p3.rotation, t);
                n.polePosition = Interpolation.CatmullRom(p0.polePosition, p1.polePosition, p2.polePosition, p3.polePosition, t);
                return n;
            }
            public LimbPose TransformPose(Matrix4x4 m) {
                var n = new LimbPose();
                n.position = m.MultiplyPoint3x4(position);
                n.rotation = m.rotation * rotation;
                n.polePosition = m.MultiplyPoint3x4(polePosition);
                return n;
            }
            public LimbPose Mirrored() {
                var n = this;
                n.position.x = -n.position.x;
                n.polePosition.x = -n.polePosition.x;
                n.rotation = n.rotation.MirroredX();
                return n;
            }
        }
        [HideInInspector] public LimbPose leftHand;
        [HideInInspector] public LimbPose rightHand;
        [HideInInspector] public LimbPose leftFoot;
        [HideInInspector] public LimbPose rightFoot;
        [HideInInspector] public Vector3 pelvisPosition;
        [HideInInspector] public Quaternion pelvisRotation;
        [HideInInspector] public Quaternion chestRotation;
        [HideInInspector] public Quaternion headRotation;
        [Range(0.0f, 1.0f)] public float leftHandOpen;
        [Range(0.0f, 1.0f)] public float rightHandOpen;

        [Range(0.0f, 1.0f)] public float neckLookWeight;
        [Range(0.0f, 1.0f)] public float lowerChestLookWeight;
        [Range(0.0f, 1.0f)] public float spineLookWeight;

        public static HumanoidPose Lerp(HumanoidPose a, HumanoidPose b, float t) {
            var newPose = new HumanoidPose();
            newPose.leftHand = LimbPose.Lerp(a.leftHand, b.leftHand, t);
            newPose.rightHand = LimbPose.Lerp(a.rightHand, b.rightHand, t);
            newPose.leftFoot = LimbPose.Lerp(a.leftFoot, b.leftFoot, t);
            newPose.rightFoot = LimbPose.Lerp(a.rightFoot, b.rightFoot, t);
            newPose.pelvisPosition = Vector3.LerpUnclamped(a.pelvisPosition, b.pelvisPosition, t);
            newPose.pelvisRotation = Quaternion.Slerp(a.pelvisRotation, b.pelvisRotation, t);
            newPose.chestRotation = Quaternion.Slerp(a.chestRotation, b.chestRotation, t);
            newPose.headRotation = Quaternion.Slerp(a.headRotation, b.headRotation, t);
            newPose.leftHandOpen = Mathf.LerpUnclamped(a.leftHandOpen, b.leftHandOpen, t);
            newPose.rightHandOpen = Mathf.LerpUnclamped(a.rightHandOpen, b.rightHandOpen, t);
            newPose.neckLookWeight = Mathf.LerpUnclamped(a.neckLookWeight, b.neckLookWeight, t);
            newPose.lowerChestLookWeight = Mathf.LerpUnclamped(a.lowerChestLookWeight, b.lowerChestLookWeight, t);
            newPose.spineLookWeight = Mathf.LerpUnclamped(a.spineLookWeight, b.spineLookWeight, t);
            return newPose;
        }
        public static HumanoidPose CatmullRom(HumanoidPose p0, HumanoidPose p1, HumanoidPose p2, HumanoidPose p3, float t) {
            var newPose = new HumanoidPose();
            newPose.leftHand = LimbPose.CatmullRom(p0.leftHand, p1.leftHand, p2.leftHand, p3.leftHand, t);
            newPose.rightHand = LimbPose.CatmullRom(p0.rightHand, p1.rightHand, p2.rightHand, p3.rightHand, t);
            newPose.leftFoot = LimbPose.CatmullRom(p0.leftFoot, p1.leftFoot, p2.leftFoot, p3.leftFoot, t);
            newPose.rightFoot = LimbPose.CatmullRom(p0.rightFoot, p1.rightFoot, p2.rightFoot, p3.rightFoot, t);
            newPose.pelvisPosition = Interpolation.CatmullRom(p0.pelvisPosition, p1.pelvisPosition, p2.pelvisPosition, p3.pelvisPosition, t);
            newPose.pelvisRotation = Interpolation.CatmullRom(p0.pelvisRotation, p1.pelvisRotation, p2.pelvisRotation, p3.pelvisRotation, t);
            newPose.chestRotation = Interpolation.CatmullRom(p0.chestRotation, p1.chestRotation, p2.chestRotation, p3.chestRotation, t);
            newPose.headRotation = Interpolation.CatmullRom(p0.headRotation, p1.headRotation, p2.headRotation, p3.headRotation, t);
            newPose.leftHandOpen = Interpolation.CatmullRom(p0.leftHandOpen, p1.leftHandOpen, p2.leftHandOpen, p3.leftHandOpen, t);
            newPose.rightHandOpen = Interpolation.CatmullRom(p0.rightHandOpen, p1.rightHandOpen, p2.rightHandOpen, p3.rightHandOpen, t);
            newPose.neckLookWeight = Interpolation.CatmullRom(p0.neckLookWeight, p1.neckLookWeight, p2.neckLookWeight, p3.neckLookWeight, t);
            newPose.lowerChestLookWeight = Interpolation.CatmullRom(p0.lowerChestLookWeight, p1.lowerChestLookWeight, p2.lowerChestLookWeight, p3.lowerChestLookWeight, t);
            newPose.spineLookWeight = Interpolation.CatmullRom(p0.spineLookWeight, p1.spineLookWeight, p2.spineLookWeight, p3.spineLookWeight, t);
            return newPose;
        }
        public HumanoidPose Mirrored() {
            var m = this;
            m.leftHandOpen = rightHandOpen;
            m.rightHandOpen = leftHandOpen;
            m.leftHand = rightHand.Mirrored();
            m.rightHand = leftHand.Mirrored();
            m.leftFoot = rightFoot.Mirrored();
            m.rightFoot = leftFoot.Mirrored();
            return m;
        }
        public HumanoidPose LToR() {
            var m = this;
            m.rightHandOpen = leftHandOpen;
            m.rightHand = leftHand.Mirrored();
            m.rightFoot = leftFoot.Mirrored();
            return m;
        }
        public HumanoidPose RToL() {
            var m = this;
            m.leftHandOpen = rightHandOpen;
            m.leftHand = rightHand.Mirrored();
            m.leftFoot = rightFoot.Mirrored();
            return m;
        }
        public HumanoidPose TransformPose(Matrix4x4 m) {
            var newPose = this;
            newPose.leftHand = leftHand.TransformPose(m);
            newPose.rightHand = rightHand.TransformPose(m);
            newPose.leftFoot = leftFoot.TransformPose(m);
            newPose.rightFoot = rightFoot.TransformPose(m);
            newPose.pelvisPosition = m.MultiplyPoint3x4(pelvisPosition);
            newPose.chestRotation = m.rotation * chestRotation;
            newPose.headRotation = m.rotation * headRotation;
            newPose.pelvisRotation = m.rotation * pelvisRotation;
            return newPose;
        }

        public HumanoidPose InverseTransformPose(Matrix4x4 m) {
            var newPose = this;
            var inverse = m.inverse;
            newPose.leftHand = leftHand.TransformPose(inverse);
            newPose.rightHand = rightHand.TransformPose(inverse);
            newPose.leftFoot = leftFoot.TransformPose(inverse);
            newPose.rightFoot = rightFoot.TransformPose(inverse);
            newPose.pelvisPosition = inverse.MultiplyPoint3x4(pelvisPosition);
            newPose.chestRotation = inverse.rotation * chestRotation;
            newPose.headRotation = inverse.rotation * headRotation;
            newPose.pelvisRotation = inverse.rotation * pelvisRotation;
            return newPose;
        }
    }
}