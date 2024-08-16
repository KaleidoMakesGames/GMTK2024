using System.Linq;
using UnityEngine;

namespace KMGAnimation {
    public static class InverseKinematics {
        public class IKConstraint {
            public Transform[] bones;
            public Vector3 target;
            public Quaternion rotation;
            public Vector3? poleTarget;
            public int iterations = 500;
            public float epsilon = 1e-3f;
        }

        public static void ApplyConstraint(in IKConstraint constraint) {
            if (constraint.bones.Length == 0) {
                return;
            }
            var positions = constraint.bones.Select(x => x.position).ToArray();
            if (constraint.poleTarget.HasValue) {
                AlignToPoleTarget(ref positions, constraint.target, constraint.poleTarget.Value);
            }
            SolveFABRIK(ref positions, constraint.target, constraint.iterations, constraint.epsilon);
            for (int i = 0; i < positions.Length - 1; i++) {
                // Need to rotate the bones to match the given positions.
                Vector3 current = constraint.bones[i + 1].position - constraint.bones[i].position;
                Vector3 goal = positions[i + 1] - positions[i];
                Quaternion rot = Quaternion.FromToRotation(current, goal);
                constraint.bones[i].rotation = rot * constraint.bones[i].rotation;
            }
            constraint.bones[positions.Length - 1].rotation = constraint.rotation;
        }

        public static void AlignToPoleTarget(ref Vector3[] positions, Vector3 target, Vector3 poleTarget) {
            for (int i = 0; i < positions.Length - 1; i++) {
                Vector3 limbDirection = (target - positions[i]).normalized;
                Vector3 poleDirection = Vector3.ProjectOnPlane(poleTarget - positions[i], limbDirection).normalized;

                Vector3 a = positions[i];
                Vector3 b = positions[i + 1];
                Vector3 d = b - a;

                Vector3 n = Vector3.ProjectOnPlane(d, limbDirection).normalized;
                float angle = Vector3.SignedAngle(n, poleDirection, limbDirection);

                Quaternion rotation = Quaternion.AngleAxis(angle, limbDirection);

                for (int j = i + 1; j < positions.Length; j++) {
                    positions[j] = rotation * (positions[j] - positions[i]) + positions[i];
                }
            }
        }

        // FABRIK solver.
        public static void SolveFABRIK(ref Vector3[] positions, Vector3 target, int iterations, float epsilon) {

            var lengths = new float[positions.Length - 1];
            for (int i = 0; i < positions.Length - 1; i++) {
                lengths[i] = Vector3.Distance(positions[i], positions[i + 1]);
            }

            Vector3 root = positions[0];
            for (int iterationCount = 0; iterationCount < iterations; iterationCount++) {
                if (Vector3.Distance(target, positions[positions.Length - 1]) < epsilon) {
                    return;
                }

                // Forward pass.
                positions[positions.Length - 1] = target;
                for (int i = positions.Length - 2; i >= 0; i--) {
                    positions[i] = (positions[i] - positions[i + 1]).normalized * lengths[i] + positions[i + 1];
                }

                // Backward pass.
                positions[0] = root;
                for (int i = 1; i < positions.Length; i++) {
                    positions[i] = (positions[i] - positions[i - 1]).normalized * lengths[i - 1] + positions[i - 1];
                }
            }
        }
    }
}