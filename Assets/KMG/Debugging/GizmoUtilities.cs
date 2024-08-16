using UnityEngine;

public static class GizmoUtilities {
    public static bool dashed = false;
    public static float tickLength = 0.5f;
    public static float gapLength = 0.5f;
    public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Vector3? perpendicular = null, float arcStart = 0, float arcEnd = 360, float interval = 5) {
        Vector3 r = (normal == Vector3.up ? Vector3.forward : Vector3.Cross(normal, Vector3.up));
        if(perpendicular.HasValue) {
            var v = Vector3.ProjectOnPlane(perpendicular.Value, normal);
            if(v != Vector3.zero) {
                r = v;
            }
        }
        for(float angle = arcStart; angle < arcEnd; angle += interval) {
            float angleNext = Mathf.Min(angle + interval, arcEnd);
            Vector3 a = center + Quaternion.AngleAxis(angle, normal) * r.normalized * radius;
            Vector3 b = center + Quaternion.AngleAxis(angleNext, normal) * r.normalized * radius;
            DrawLine(a, b);
        }
    }

    public static void DrawCone (Vector3 center, Vector3 direction, float angle, float distance) {
        float circleRadius = Mathf.Tan(angle*Mathf.Deg2Rad) * distance;
        Vector3 r = (direction == Vector3.up ? Vector3.forward : Vector3.Cross(direction, Vector3.up)).normalized * circleRadius;
        DrawCircle(center + direction.normalized * distance, r, circleRadius);
        for (int i = 0; i < 4; i++) {
            DrawLine(center, center + direction * distance + (Quaternion.AngleAxis(90 * i, direction) * r));
        }
    }

    public static void DrawLine(Vector3 a, Vector3 b) {
        if(dashed) {
            DrawDashedLine(a, b);
        } else {
            Gizmos.DrawLine(a, b);
        }
    }

    public static void DrawCube(Vector3 center, Vector3 size, Quaternion rotation) {
        var forward = rotation * (Vector3.forward * size.z)/2;
        var up = rotation * (Vector3.up  * size.y) / 2;
        var right = rotation * (Vector3.right* size.x) / 2;

        var p0 = center + forward + up + right;
        var p1 = center + forward + up - right;
        var p2 = center + forward - up + right;
        var p3 = center + forward - up - right;
        var p4 = center - forward + up + right;
        var p5 = center - forward + up - right;
        var p6 = center - forward - up + right;
        var p7 = center - forward - up - right;

        DrawLine(p0, p1);
        DrawLine(p0, p2);
        DrawLine(p0, p4);

        DrawLine(p3, p2);
        DrawLine(p3, p1);
        DrawLine(p3, p7);

        DrawLine(p5, p1);
        DrawLine(p5, p7);
        DrawLine(p5, p4);

        DrawLine(p6, p2);
        DrawLine(p6, p4);
        DrawLine(p6, p7);
    }

    public static void DrawSphereSegment(Vector3 center, Vector3 heading, float viewDistance, float angle) {
        angle /= 2;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, heading);
        Vector3 circleCenter = viewDistance * Mathf.Cos(Mathf.Deg2Rad * angle) * Vector3.forward;
        float circleRadius = viewDistance * Mathf.Sin(Mathf.Deg2Rad * angle);
        DrawLine(center, center + rot * (circleCenter + Vector3.up * circleRadius));
        DrawLine(center, center + rot * (circleCenter - Vector3.up * circleRadius));
        DrawLine(center, center + rot * (circleCenter + Vector3.right * circleRadius));
        DrawLine(center, center + rot * (circleCenter - Vector3.right * circleRadius));
        DrawCircle(center + rot * circleCenter, rot * Vector3.forward, circleRadius);
        DrawCircle(center, rot * Vector3.right, viewDistance, rot * Vector3.forward, -angle, angle);
        DrawCircle(center, rot * Vector3.up, viewDistance, rot * Vector3.forward, -angle, angle);
    }

    public static void DrawDashedLine(Vector3 a, Vector3 b) {
        if(tickLength <= 0 && gapLength <= 0) {
            return;
        }
        float d = 0;
        Vector3 r = b - a;
        float max = r.magnitude;
        r = r.normalized;
        while(d < max) {
            float tickEnd = d + tickLength;
            Gizmos.DrawLine(a + r * d, a + r * tickEnd);
            d = tickEnd + gapLength;
        }
    }
}
