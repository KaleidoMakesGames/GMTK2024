using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class VectorUtilities {
    public static Vector2 Flattened(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }
    public delegate float vecFunc(float comp);
    public static Vector3 ApplyFunc(Vector3 v, vecFunc f) {
        return new Vector3(f(v.x), f(v.y), f(v.z));
    }

    public static float Max(this Vector3 v) {
        return Mathf.Max(v.x, v.y, v.z);
    }

    public static Vector3 Divide(this Vector3 v, Vector3 o) {
        return new Vector3(v.x / o.x, v.y / o.y, v.z / o.z);
    }
    public static Vector3 ProjectFloorDirection(Vector3 dir, Vector3 planeNormal) {
        if(planeNormal.y == 0) {
            return Vector3.zero;
        }
        float y = (-planeNormal.x * dir.x - planeNormal.z * dir.z) / planeNormal.y;
        return new Vector3(dir.x, y, dir.z).normalized;
    }
}