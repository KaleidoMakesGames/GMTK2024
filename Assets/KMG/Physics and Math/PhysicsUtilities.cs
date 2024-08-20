using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;

public static class PhysicsUtilities {
    public static T GetComponent<T>(this RaycastHit hit) where T : class { 
        if(hit.collider == null) {
            return null;
        }
        return hit.collider.GetComponent<T>();
    }
    public static IEnumerable<Transform> GetChildren(this Transform t) {
        foreach(Transform c in t) {
            yield return c;
        }
    }
    public static Collider[] OverlapAll(this Rigidbody rb, Vector3 position) {
        var hits = new List<Collider>();
        hits.AddRange(rb.GetComponentsInChildren<BoxCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => OverlapAll(x, position)));
        hits.AddRange(rb.GetComponentsInChildren<CapsuleCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => OverlapAll(x, position)));
        hits.AddRange(rb.GetComponentsInChildren<SphereCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => OverlapAll(x, position)));
        return hits.Where(x => x.attachedRigidbody != rb).ToArray();
    }

    public static Collider[] OverlapAll(this BoxCollider bc, Vector3 position) {
        return Physics.OverlapBox(position + bc.transform.TransformVector(bc.center), Vector3.Scale(bc.transform.lossyScale, bc.size) / 2, bc.transform.rotation, ~0, QueryTriggerInteraction.Collide);
    }

    public static Collider[] OverlapAll(this SphereCollider sc, Vector3 position) {
        float maxC = Mathf.Max(sc.transform.lossyScale.x,
            sc.transform.lossyScale.y,
            sc.transform.lossyScale.z);
        return Physics.OverlapSphere(position + sc.transform.TransformVector(sc.center), sc.radius * maxC,
            ~0, QueryTriggerInteraction.Collide);
    }

    public static Bounds GetBounds(this Rigidbody rb) {
        var bounds = rb.GetComponentsInChildren<Collider>().Where(x => !x.isTrigger).Select(x => x.bounds).ToArray();
        for(int i = 1; i < bounds.Count(); i++) {
            bounds[0].Encapsulate(bounds[i]);
        }
        return bounds[0];
    }
    
    public static Bounds GetBounds(this Rigidbody2D rb) {
        var bounds = rb.GetComponentsInChildren<Collider2D>().Where(x => !x.isTrigger).Select(x => x.bounds).ToArray();
        for (int i = 1; i < bounds.Count(); i++) {
            bounds[0].Encapsulate(bounds[i]);
        }
        return bounds[0];
    }
    public static Collider[] OverlapAll(this CapsuleCollider cc, Vector3 position) {
        GetCapsulePoints(cc, out Vector3 a, out Vector3 b, out float radius, out _);
        Vector3 colliderDelta = position - cc.transform.position;
        a += position + cc.transform.TransformVector(cc.center);
        b += position + cc.transform.TransformVector(cc.center);
        return Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide);
    }

    public static RaycastHit[] CastAll(this Rigidbody rb, Vector3 position, Vector3 direction, float distance) {
        var hits = new List<RaycastHit>();
        hits.AddRange(rb.GetComponentsInChildren<BoxCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => CastAll(x, position, direction, distance)));
        hits.AddRange(rb.GetComponentsInChildren<CapsuleCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => CastAll(x, position, direction, distance)));
        hits.AddRange(rb.GetComponentsInChildren<SphereCollider>().Where(x => (rb.excludeLayers & (1 << x.gameObject.layer)) == 0).SelectMany(x => CastAll(x, position, direction, distance)));
        return hits.Where(x => x.collider.attachedRigidbody != rb).OrderBy(x => x.distance).ToArray();
    }

    public static RaycastHit[] CastAll(this BoxCollider bc, Vector3 position, Vector3 direction, float distance) {
        return Physics.BoxCastAll(position + bc.transform.TransformVector(bc.center), Vector3.Scale(bc.transform.lossyScale, bc.size) / 2, direction, bc.transform.rotation, distance, ~0, QueryTriggerInteraction.Collide)
            .OrderBy(x => x.distance).ToArray();
    }

    public static RaycastHit[] CastAll(this SphereCollider sc, Vector3 position, Vector3 direction, float distance) {
        float maxC = Mathf.Max(sc.transform.lossyScale.x,
            sc.transform.lossyScale.y,
            sc.transform.lossyScale.z);
        return Physics.SphereCastAll(position + sc.transform.TransformVector(sc.center), sc.radius * maxC, direction, distance,
            ~0, QueryTriggerInteraction.Collide).OrderBy(x => x.distance).ToArray();
    }

    public static RaycastHit[] CastAll(this CapsuleCollider cc, Vector3 position, Vector3 direction, float distance) {
        GetCapsulePoints(cc, out Vector3 a, out Vector3 b, out float radius, out _);
        a += position + cc.transform.TransformVector(cc.center);
        b += position + cc.transform.TransformVector(cc.center);
        return Physics.CapsuleCastAll(a, b, radius, direction, distance, ~0, QueryTriggerInteraction.Collide).OrderBy(x => x.distance).ToArray();
    }

    private static void GetCapsulePoints(CapsuleCollider cc, out Vector3 worldA, out Vector3 worldB, out float radius, out float height) {
        Vector3 heightAxis = new[] { Vector3.right, Vector3.up, Vector3.forward }[cc.direction];
        Vector3 perpAxis1 = new[] { Vector3.up, Vector3.forward, Vector3.right }[cc.direction];
        Vector3 perpAxis2 = new[] { Vector3.forward, Vector3.right, Vector3.up }[cc.direction];

        Vector3 heightAxisWorld = cc.transform.TransformDirection(heightAxis);

        float heightScale = Vector3.Dot(cc.transform.lossyScale, heightAxis);
        float radScale1 = Vector3.Dot(cc.transform.lossyScale, perpAxis1);
        float radScale2 = Vector3.Dot(cc.transform.lossyScale, perpAxis2);

        radius = Mathf.Max(Mathf.Abs(radScale1), Mathf.Abs(radScale2)) * Mathf.Abs(cc.radius);
        height = Mathf.Abs(heightScale * cc.height);
        float offset = Mathf.Max(height / 2 - radius, 0);
        worldA = offset * heightAxisWorld;
        worldB = -offset * heightAxisWorld;
    }
}
