using UnityEngine;

public static class GeometryUtilities {

    public static bool ClampToQuad3D(Vector3 p, Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 clamped) {
        Vector3 o = (A + B + C + D) / 4;

        p = p - o;
        A = A - o;
        B = B - o;
        C = C - o;
        D = D - o;

        Vector3 n = Vector3.Cross(B - A, C - A).normalized;
        Debug.DrawLine(o, o + n, Color.blue);

        p = Vector3.ProjectOnPlane(p, n);
        A = Vector3.ProjectOnPlane(A, n);
        B = Vector3.ProjectOnPlane(B, n);
        C = Vector3.ProjectOnPlane(C, n);
        D = Vector3.ProjectOnPlane(D, n);

        Vector3 u = Vector3.Cross(n, B - A).normalized;
        Vector3 v = Vector3.Cross(u, n).normalized;

        Debug.DrawLine(o, o + u, Color.red);
        Debug.DrawLine(o, o + v, Color.yellow);

        Vector2 p2 = new Vector2(Vector3.Dot(p, u), Vector3.Dot(p, v));
        Vector2 A2 = new Vector2(Vector3.Dot(A, u), Vector3.Dot(A, v));
        Vector2 B2 = new Vector2(Vector3.Dot(B, u), Vector3.Dot(B, v));
        Vector2 C2 = new Vector2(Vector3.Dot(C, u), Vector3.Dot(C, v));
        Vector2 D2 = new Vector2(Vector3.Dot(D, u), Vector3.Dot(D, v));

        bool wasClamped = ClampToQuad(p2, A2, B2, C2, D2, out var clamped2D);

        clamped = clamped2D.x * u + clamped2D.y * v + o;

        return wasClamped;
    }

    public static float GetACLength(Vector3 A, Vector3 B, float bcLength, Vector3 acHat) {
        Vector3 AB = B - A;
        float cabAngle = Vector3.Angle(AB, acHat) * Mathf.Deg2Rad;
        float abLength = AB.magnitude;
        float k = bcLength * bcLength - abLength * abLength * Mathf.Sin(cabAngle);
        if(k < 0) {
            return 0;
        }
        float acLength = abLength * Mathf.Cos(cabAngle) + Mathf.Sqrt(k);
        return acLength;
    }

    public static bool ClampToQuad(Vector2 p, Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 clamped) {
        Vector2 c = Vector2.zero;
        if (!LineSegmentIntersection(c, p, A, B, out clamped)) {
            if (!LineSegmentIntersection(c, p, B, C, out clamped)) {
                if (!LineSegmentIntersection(c, p, C, D, out clamped)) {
                    if (!LineSegmentIntersection(c, p, D, A, out clamped)) {
                        clamped = p;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public static bool LineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersect) {
        return FindIntersection(p1, p2, p3, p4, out intersect);
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private static bool FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection) {
        // Get the segments' parameters.
        float dx12 = p2.x - p1.x;
        float dy12 = p2.y - p1.y;
        float dx34 = p4.x - p3.x;
        float dy34 = p4.y - p3.y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);

        float t1 =
            ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34)
                / denominator;
        if (float.IsInfinity(t1)) {
            // The lines are parallel (or close enough to it).
            intersection = new Vector2(float.NaN, float.NaN);
            return false;
        }

        float t2 =
            ((p3.x - p1.x) * dy12 + (p1.y - p3.y) * dx12)
                / -denominator;

        // Find the point of intersection.
        intersection = new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        return (t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1);
    }

}