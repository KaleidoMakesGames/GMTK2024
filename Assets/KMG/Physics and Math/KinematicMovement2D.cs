using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class KinematicMovement2D {
    [System.Serializable]
    public struct CastHit {
        public float distance;
        public Vector2 normal;
        public Vector2 point;
        public Vector2 position {
            get {
                return origin + direction * distance;
            }
        }
        public Vector2 origin;
        public Vector2 direction;
        public Collider2D collider;

        public static CastHit None { get { return new CastHit(); } }
        public bool Exists() {
            return collider != null;
        }

        public CastHit(RaycastHit2D h) : this(h, Vector2.zero, Vector2.zero) { }

        public CastHit(Collider2D c) : this() {
            collider = c;
        }

        public CastHit(RaycastHit2D h, Vector2 castOrigin, Vector2 castDirection) {
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
        public Rigidbody2D rb;
        public float skinWidth;
        public CastHit[] CastAll(Vector2 origin, Vector2 direction, float distance) {
            var hitsRaw = new List<RaycastHit2D>();
            rb.Cast(origin, 0, direction, hitsRaw, distance + skinWidth);
            var hits = hitsRaw.Select(x => new CastHit(x, origin, direction)).ToArray();
            for (int i = 0; i < hits.Length; i++) {
                hits[i].distance -= skinWidth;
            }
            return hits;
        }
    }

    public static CastHit GetFirstObstacle(KinematicCollider collider,
            Vector2 position, Vector2 direction, float distance, Func<CastHit, bool> isObstacleFunc = null) {
        foreach (var hit in collider.CastAll(position, direction, distance)) {
            if (!hit.collider.isTrigger && hit.collider.attachedRigidbody != collider.rb
                && (isObstacleFunc == null || isObstacleFunc(hit))) {
                return hit;
            }
        }
        return CastHit.None;
    }

    public static void DisplaceToObstacle(KinematicCollider collider, ref Vector2 position,
            Vector2 direction, float distance, out CastHit hitObstacle, Func<CastHit, bool> isObstacleFunc = null) {
        DisplaceToObstacle(collider, ref position, direction, ref distance, out hitObstacle, isObstacleFunc);
    }

    public static void DisplaceToObstacle(KinematicCollider collider, ref Vector2 position,
            Vector2 direction, ref float distance, out CastHit hitObstacle, Func<CastHit, bool> isObstacleFunc = null) {
        hitObstacle = GetFirstObstacle(collider, position, direction, distance, isObstacleFunc);
        float d = hitObstacle.Exists() ? hitObstacle.distance : distance;
        position += d * direction.normalized;
        distance = Mathf.Max(0, distance - d);
    }

    public static Vector2 ProjectOnPerpLine(Vector2 v, Vector2 n) {
        var l = Vector2.Perpendicular(n).normalized;
        return Vector2.Dot(v, l) * l;
    }
    public static void DisplaceAndSlide(KinematicCollider collider, ref Vector2 position, Vector2 displacement,
        out HashSet<CastHit> obstaclesHit, Func<CastHit, bool> isObstacleFunc = null, Func<CastHit, Vector2, Vector2> displacementSlideFunc = null, int maxIterations = 5) {
        DisplaceAndSlide(collider, ref position, ref displacement, out obstaclesHit, isObstacleFunc, displacementSlideFunc, maxIterations);
    }
    public static void DisplaceAndSlide(KinematicCollider collider, ref Vector2 position,
        ref Vector2 displacement,
        out HashSet<CastHit> obstaclesHit, Func<CastHit, bool> isObstacleFunc = null, Func<CastHit, Vector2, Vector2> displacementSlideFunc = null, int maxIterations = 5) {

        obstaclesHit = new HashSet<CastHit>();
        for (int i = 0; i < maxIterations; i++) {
            Vector2 direction = displacement.normalized;
            float distance = displacement.magnitude;
            if (Mathf.Approximately(distance, 0.0f)) {
                return;
            }
            DisplaceToObstacle(collider, ref position, direction, ref distance, out var hitObstacle, isObstacleFunc);
            displacement = direction * distance;
            if (hitObstacle.Exists()) {
                obstaclesHit.Add(hitObstacle);
                if (displacementSlideFunc == null) {
                    displacement = ProjectOnPerpLine(displacement, hitObstacle.normal);
                } else {
                    displacement = displacementSlideFunc(hitObstacle, displacement);
                }
            }
        }
    }

    public enum IntersectionResolutionResult { NONE, INTERSECTED }
    public static IntersectionResolutionResult ResolveIntersections(KinematicCollider collider, ref Vector2 position, Func<Collider2D, bool> isObstacleFunc = null) {
        var overlaps = new List<Collider2D>();
        collider.rb.Overlap(position, 0, overlaps);

        overlaps = overlaps.Where(x => !x.isTrigger && x.attachedRigidbody != collider.rb && (
        isObstacleFunc == null || isObstacleFunc(x))).ToList();
        if (overlaps.Count > 0) {
            var overlapCollider = overlaps[0];
            // Need to resolve the overlap
            var d = collider.rb.Distance(overlapCollider);
            if(d.isOverlapped) {
                position += (d.distance - collider.skinWidth) * d.normal;
                return IntersectionResolutionResult.INTERSECTED;
            }
        }
        return IntersectionResolutionResult.NONE;
    }
}