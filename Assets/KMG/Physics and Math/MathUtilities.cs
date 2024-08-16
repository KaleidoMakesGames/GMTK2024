using UnityEngine;
namespace MathUtilities {
    public static class MathUtilities {
        public static float QuadraticInterpolation(Vector2 point1, Vector2 point2, Vector2 point3, float x) {
            if (point1.x == point2.x || point1.x == point3.x || point2.x == point3.x) {
                throw new System.Exception("Duplicate x coordinates passed.");
            }
            point2 -= point1;
            point3 -= point1;
            x -= point1.x;
            float detInv = 1 / ((point2.x * point3.x) * (point2.x - point3.x));
            float M = detInv * (point2.y * point3.x - point3.y * point2.x);
            float N = detInv * (point3.y * point2.x * point2.x - point2.y * point3.x * point3.x);
            return point1.y + M * x * x + N * x;
        }
    }
    [System.Serializable]
    public abstract class MovingAverage<T> {
        [SerializeField]private T[] prevValues;
        [SerializeField] private int nextOut;
        [SerializeField] private T _current;
        public int windowSize { get; private set; }
        public T current {
            get { return _current; }
        }
        private T sum;
        public MovingAverage(int windowSize) {
            this.windowSize = windowSize;
            prevValues = new T[windowSize];
            nextOut = 0;
        }
        public void Append(T n) {
            sum = Add(Add(sum, Negate(prevValues[nextOut])), n);
            prevValues[nextOut] = n;
            _current = Divide(sum, prevValues.Length);
            nextOut = (nextOut + 1) % prevValues.Length;
        }
        public abstract T Add(T a, T b);
        public abstract T Negate(T a);
        public abstract T Divide(T a, int b);

        public static implicit operator T(MovingAverage<T> a) => a.current;
    }
    public class MovingFloatAverage : MovingAverage<float> {
        public MovingFloatAverage(int windowSize) : base(windowSize) {
        }

        public override float Add(float a, float b) { return a + b; }
        public override float Divide(float a, int b) { return a / b; }
        public override float Negate(float a) { return -a;}
    }
    public class MovingVector3Average : MovingAverage<Vector3> {
        public MovingVector3Average(int windowSize) : base(windowSize) {
        }

        public override Vector3 Add(Vector3 a, Vector3 b) { return a + b; }
        public override Vector3 Divide(Vector3 a, int b) { return a / b; }
        public override Vector3 Negate(Vector3 a) { return -a; }
    }
    public static class QuaternionExtensions {
        public static Quaternion MirroredX(this Quaternion q) {
            var u = q * Vector3.up;
            var f = q * Vector3.forward;
            u.x = -u.x;
            f.x = -f.x;
            q = Quaternion.LookRotation(f, u);
            return q;
        }
        public static Quaternion Abs(this Quaternion q) {
            if (q.w < 0) {
                return new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }
            return q;
        }
        public static Quaternion FromScaledAngleAxis(Vector3 v) {
            return Quaternion.AngleAxis(v.magnitude * Mathf.Rad2Deg, v.normalized);
        }

        public static Vector3 ToScaledAngleAxis(this Quaternion q) {
            q.ToAngleAxis(out var angle, out var axis);
            return angle * axis.normalized * Mathf.Deg2Rad;
        }
    }
    public static class Interpolation {
        // Adapted from https://theorangeduck.com/page/cubic-interpolation-quaternions.
        public static float CubicHermite(float p0, float p1, float v0, float v1, float t) {
            float p1_sub_p0 = p1 - p0;

            float w1 = 3 * t * t - 2 * t * t * t;
            float w2 = t * t * t - 2 * t * t + t;
            float w3 = t * t * t - t * t;

            return w1 * p1_sub_p0 + w2 * v0 + w3 * v1 + p0;
        }
        public static Vector3 CubicHermite(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1, float t) {
            Vector3 p1_sub_p0 = p1 - p0;

            float w1 = 3 * t * t - 2 * t * t * t;
            float w2 = t * t * t - 2 * t * t + t;
            float w3 = t * t * t - t * t;

            return w1 * p1_sub_p0 + w2 * v0 + w3 * v1 + p0;
        }
        public static Quaternion CubicHermite(Quaternion r0, Quaternion r1, Vector3 v0, Vector3 v1, float t) {
            Vector3 r1_sub_r0 = (r1 * Quaternion.Inverse(r0)).Abs().ToScaledAngleAxis();

            float w1 = 3 * t * t - 2 * t * t * t;
            float w2 = t * t * t - 2 * t * t + t;
            float w3 = t * t * t - t * t;

            return QuaternionExtensions.FromScaledAngleAxis(w1 * r1_sub_r0 + w2 * v0 + w3 * v1) * r0;
        }
        public static float CatmullRom(float pPrev, float p0, float p1, float pNext, float t, float dtPrev = 1, float dtNext = 1, float dt = 1) {
            dtPrev /= dt;
            dtNext /= dt;
            t /= dt;
            float v0 = (p1 - pPrev) / (1 + dtPrev);
            float v1 = (pNext - p0) / (1 + dtNext);
            return CubicHermite(p0, p1, v0, v1, t);
        }
        public static Vector3 CatmullRom(Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 pNext, float t, float dtPrev = 1, float dtNext = 1, float dt = 1) {
            dtPrev /= dt;
            dtNext /= dt;
            t /= dt;
            Vector3 v0 = (p1 - pPrev) / (1 + dtPrev);
            Vector3 v1 = (pNext - p0) / (1 + dtNext);
            return CubicHermite(p0, p1, v0, v1, t);
        }

        public static Quaternion CatmullRom(Quaternion rPrev, Quaternion r0, Quaternion r1, Quaternion rNext, float t, float dtPrev = 1, float dtNext = 1, float dt = 1) {
            dtPrev /= dt;
            dtNext /= dt;
            t /= dt;

            Vector3 r1_sub_r0 = ((r0 * Quaternion.Inverse(rPrev)).Abs().ToScaledAngleAxis())/dtPrev;
            Vector3 r2_sub_r1 = ((r1 * Quaternion.Inverse(r0)).Abs().ToScaledAngleAxis());
            Vector3 r3_sub_r2 = ((rNext * Quaternion.Inverse(r1)).Abs().ToScaledAngleAxis())/dtNext;

            Vector3 v0 = (r1_sub_r0 * dtPrev) / (1 + dtPrev) + (r2_sub_r1) / (1 + dtPrev);
            Vector3 v1 = (r2_sub_r1 * dtNext) / (1 + dtNext) + (r3_sub_r2) / (1 + dtNext);
            return CubicHermite(r0, r1, v0, v1, t);
        }
    }
}