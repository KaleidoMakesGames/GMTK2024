using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
 
public static class Springs {
    public static DampedSpringSettings Lerp(DampedSpringSettings a, DampedSpringSettings b, float t) {
        var r = new DampedSpringSettings();
        r.stiffness = Mathf.Lerp(a.stiffness, b.stiffness, t);
        r.dampingRatio = Mathf.Lerp(a.dampingRatio, b.dampingRatio, t);
        return r;
    }

    [System.Serializable]
    public class DampedSpringSettings {
        public float stiffness = 1;
        public float dampingRatio = 1;
        public float damping {
            get {
                return dampingRatio * 2 * Mathf.Sqrt(stiffness);
            }
        }
        public float mass {
            get {
                return 1;
            }
        }

        public void Set(float stiffness, float dampingRatio) {
            this.stiffness = stiffness;
            this.dampingRatio = dampingRatio;
        }
    }

    private static void ConvertAnglesToClosest(ref float currentAngle, ref float goalAngle) {
        currentAngle = Mathf.Repeat(currentAngle, 360.0f);
        goalAngle = Mathf.Repeat(goalAngle, 360.0f);
        
        if(goalAngle > currentAngle) {
            float ccwGoalAngle = goalAngle - 360.0f;
            if((goalAngle-currentAngle) > (currentAngle-ccwGoalAngle)) {
                goalAngle = ccwGoalAngle;
            }
        } else {
            float cwGoalAngle = goalAngle + 360.0f;
            if((cwGoalAngle - currentAngle) < (currentAngle - goalAngle)) {
                goalAngle = cwGoalAngle;
            }
        }
    }
    public static void ComputeMassSpringDamper(ref Quaternion o, ref Vector3 v, Quaternion oGoal, DampedSpringSettings settings, float? dt = null) {
        var currentO = o.eulerAngles;
        var goalO = oGoal.eulerAngles;

        ComputeMassSpringDamperAngle(ref currentO.x, ref v.x, goalO.x, settings, dt);
        ComputeMassSpringDamperAngle(ref currentO.y, ref v.y, goalO.y, settings, dt);
        ComputeMassSpringDamperAngle(ref currentO.z, ref v.z, goalO.z, settings, dt);

        o = Quaternion.Euler(currentO);
    }

    public static void ComputeMassSpringDamperAngle(ref float aX, ref float v, float aGoal, DampedSpringSettings settings, float? dt=null) {
        ConvertAnglesToClosest(ref aX, ref aGoal);
        ComputeMassSpringDamper(ref aX, ref v, aGoal, settings, dt);
    }

    public static void ComputeMassSpringDamper(ref Vector3 x, ref Vector3 v, Vector3 xGoal, DampedSpringSettings settings, float? dt = null) {
        ComputeMassSpringDamper(ref x.x, ref v.x, xGoal.x, settings, dt);
        ComputeMassSpringDamper(ref x.y, ref v.y, xGoal.y, settings, dt);
        ComputeMassSpringDamper(ref x.z, ref v.z, xGoal.z, settings, dt);
    }

    public static void ComputeMassSpringDamper(ref float x, ref float v, float xGoal, DampedSpringSettings settings, float? d = null) {
        float dt = d.HasValue ? d.Value : Time.deltaTime;
        float c = settings.damping;
        float k = settings.stiffness;
        float m = settings.mass;
        float x0 = x - xGoal;
        float v0 = v;
        float a = -c / (2 * m);
        float discriminant = a * a - k / m;
        if (Mathf.Abs(discriminant) < Mathf.Epsilon) {
            float A = x0;
            float B = v0 - x0 * a;

            float exp = Mathf.Exp(a * dt);
            x = A * exp + dt * B * exp;
            v = A * a * exp + B * exp * (1 + a * dt);
        } else if (discriminant > 0) {
            float b = Mathf.Sqrt(discriminant);
            float B = (v0 - a * x0 - b * x0) / (-2 * b);
            float A = x0 - B;

            float expA = Mathf.Exp((a + b) * dt);
            float expB = Mathf.Exp((a - b) * dt);

            x = A * expA + B * expB;
            v = A * (a + b) * expA + B * (a - b) * expB;
        } else {
            float b = Mathf.Sqrt(-discriminant);
            float A = x0;
            float B = (v0 - a * x0) / b;

            float exp = Mathf.Exp(a * dt);

            x = exp * (A * Mathf.Cos(b * dt) + B * Mathf.Sin(b * dt));
            v = exp * (B * b * Mathf.Cos(b * dt) - A * b * Mathf.Sin(b * dt)) + a * x;
        }
        x += xGoal;
    }
}