using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class KinematicMovement3D {
    [System.Serializable]
    public struct CastHit {
        public float distance;
        public Vector3 normal;
        public Vector3 point;
        public Vector3 position {
            get {
                return origin + direction * distance;
            }
        }
        public Vector3 origin;
        public Vector3 direction;
        public Collider collider;

        public static CastHit None { get { return new CastHit(); } }
        public bool Exists() {
            return collider != null;
        }

        public CastHit(RaycastHit h) : this(h, Vector3.zero, Vector3.zero) { }

        public CastHit(Collider c) : this() {
            collider = c;
        }

        public CastHit(RaycastHit h, Vector3 castOrigin, Vector3 castDirection) {
            distance = h.distance;
            normal = h.normal;
            point = h.point;
            collider = h.collider;
            origin = castOrigin;
            direction = castDirection;
        }
        public void Clear() {
            this = None;
        }
    }

    [System.Serializable]
    public class KinematicCollider {
        public Rigidbody rb;
        public float skinWidth;
        public CastHit[] CastAll(Vector3 origin, Vector3 direction, float distance) {
            var hits = rb.CastAll(origin, direction, distance + skinWidth).
                Select(x => new CastHit(x, origin, direction)).ToArray();
            for(int i = 0; i < hits.Length; i++) {
                hits[i].distance -= skinWidth;
            }
            return hits;
        }
        public Vector3 Size {
            get {
                return rb.GetBounds().size;
            }
        }
    }

    public static CastHit GetFirstObstacle(KinematicCollider collider,
            Vector3 position, Vector3 direction, float distance, Func<CastHit, bool> isObstacleFunc = null) {
        foreach (var hit in collider.CastAll(position, direction, distance)) {
            if (!hit.collider.isTrigger && hit.collider.attachedRigidbody != collider.rb 
                && (isObstacleFunc == null || isObstacleFunc(hit))) {
                return hit;
            }
        }
        return CastHit.None;
    }

    public static void DisplaceToObstacle(KinematicCollider collider, ref Vector3 position,
            Vector3 direction, float distance, out CastHit hitObstacle, Func<CastHit, bool> isObstacleFunc = null) {
        DisplaceToObstacle(collider, ref position, direction, ref distance, out hitObstacle, isObstacleFunc);
    }

    public static void DisplaceToObstacle(KinematicCollider collider, ref Vector3 position,
            Vector3 direction, ref float distance, out CastHit hitObstacle, Func<CastHit, bool> isObstacleFunc = null) {
        hitObstacle = GetFirstObstacle(collider, position, direction, distance, isObstacleFunc);
        float d = hitObstacle.Exists() ? hitObstacle.distance : distance;
        position += d * direction.normalized;
        distance = Mathf.Max(0, distance - d);
    }


    public static void DisplaceAndSlide(KinematicCollider collider, ref Vector3 position, Vector3 displacement,
        out HashSet<CastHit> obstaclesHit, Func<CastHit, bool> isObstacleFunc = null, Func<CastHit, Vector3, Vector3> displacementSlideFunc = null, int maxIterations = 5) {
        DisplaceAndSlide(collider, ref position, ref displacement, out obstaclesHit, isObstacleFunc, displacementSlideFunc, maxIterations);
    }
    public static void DisplaceAndSlide(KinematicCollider collider, ref Vector3 position,
        ref Vector3 displacement,
        out HashSet<CastHit> obstaclesHit, Func<CastHit, bool> isObstacleFunc = null, Func<CastHit, Vector3, Vector3> displacementSlideFunc = null, int maxIterations = 5) {

        obstaclesHit = new HashSet<CastHit>();
        for (int i = 0; i < maxIterations; i++) {
            Vector3 direction = displacement.normalized;
            float distance = displacement.magnitude;
            if (Mathf.Approximately(distance, 0.0f)) {
                return;
            }
            DisplaceToObstacle(collider, ref position, direction, ref distance, out var hitObstacle, isObstacleFunc);
            displacement = direction * distance;
            if (hitObstacle.Exists()) {
                obstaclesHit.Add(hitObstacle);
                if (displacementSlideFunc == null) {
                    displacement = Vector3.ProjectOnPlane(displacement, hitObstacle.normal);
                } else {
                    displacement = displacementSlideFunc(hitObstacle, displacement);
                }
            }
        }
    }

    public enum IntersectionResolutionResult { NONE, RESOLVED, FAILED }
    public static IntersectionResolutionResult ResolveIntersections(KinematicCollider collider, ref Vector3 position, int maxIterations = 5, Func<Collider, bool> isObstacleFunc=null) {
        for (int i = 0; i < maxIterations; i++) {
            bool intersectionDetected = false;

            var overlaps = collider.rb.OverlapAll(position);

            overlaps = overlaps.Where(x => !x.isTrigger && x.attachedRigidbody != collider.rb && (
            isObstacleFunc == null || isObstacleFunc(x))).ToArray();
            if (overlaps.Length > 0) {
                var overlapCollider = overlaps[0];
                // Need to resolve the overlap
                foreach (var attachedCollider in collider.rb.GetComponents<Collider>()) {
                    if (Physics.ComputePenetration(attachedCollider, position, attachedCollider.transform.rotation, overlapCollider,
                        overlapCollider.transform.position,
                        overlapCollider.transform.rotation,
                        out var direction, out var distance)) {
                        position += (distance + collider.skinWidth) * direction;
                        intersectionDetected = true;
                        break;
                    }
                }
            }
            if (!intersectionDetected) {
                return i == 0 ? IntersectionResolutionResult.NONE : IntersectionResolutionResult.RESOLVED;
            }
        }
        return IntersectionResolutionResult.FAILED;
    }
}