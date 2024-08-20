using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RobotMovementMechanics;

public static class RobotMovementMechanics {
    public enum MovementMode {FREEFALL, GROUNDED, CLIMB }
    [System.Serializable]
    public class MovementState {
        public Vector2 position;
        public Vector2 velocity;
        public MovementMode mode;
        public KinematicMovement2D.CastHit ground;
    }
    [System.Serializable]
    public class MovementSettings {
        public float climbSpeed;
        public float gravity;
        public float walkableSlope;
        public float walkSpeed;
    }
    [System.Serializable]
    public class RobotMovementControl {
        public Vector2 drive;
        public bool releaseClimb;
    }
    public class RobotMovementInfo {
        public float distanceMoved;
        public List<KinematicMovement2D.CastHit> hitDuringMovement;
        public RobotMovementInfo() {
            hitDuringMovement = new List<KinematicMovement2D.CastHit>();
        }
    }

    [System.Serializable]
    public class RobotCollider : KinematicMovement2D.KinematicCollider {
    }

    public const float SPEED_EPSILON = 1e-2f;
    public static RobotMovementInfo UpdateMovementMechanics(MovementState state, MovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        var info = new RobotMovementInfo();
        var ri = KinematicMovement2D.ResolveIntersections(robotCollider, ref state.position);
        if(ri == KinematicMovement2D.IntersectionResolutionResult.INTERSECTED) {
            return info;
        }

        Vector2 oldPosition = state.position;

        // Handle control state transitions
        if(state.mode == MovementMode.CLIMB) {
            if (control.releaseClimb) {
                state.mode = MovementMode.FREEFALL;
            }
        } else if(state.mode == MovementMode.FREEFALL || state.mode == MovementMode.GROUNDED) {
            if(control.drive.y > 0 && !control.releaseClimb) {
                state.mode = MovementMode.CLIMB;
            }
        } 

        // Update states
        if (state.mode == MovementMode.CLIMB) {
            DoClimb(info, state, settings, control, robotCollider, dt);
        } else if(state.mode == MovementMode.FREEFALL) {
            DoFreefall(info, state, settings, control, robotCollider, dt);
        } else if(state.mode == MovementMode.GROUNDED) {
            DoGrounded(info, state, settings, control, robotCollider, dt);
        }

        state.velocity = (state.position - oldPosition) / dt;
        info.distanceMoved = (state.position - oldPosition).magnitude;
        return info;
    }

    private static void DoClimb(RobotMovementInfo info, MovementState state, MovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        var delta = Vector2.ClampMagnitude(control.drive, 1) * settings.climbSpeed * dt;
        DoDisplacement(delta, info, state, settings, control, robotCollider, dt);
        state.ground = KinematicMovement2D.CastHit.None;
    }

    private static void DoFreefall(RobotMovementInfo info, MovementState state, MovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        Vector2 nextVelocity = state.velocity + Mathf.Abs(settings.gravity) * Vector2.down * dt;
        Vector2 d = (nextVelocity + state.velocity) * dt / 2;
        DoDisplacement(d, info, state, settings, control, robotCollider, dt);
        if(state.ground.Exists()) {
            state.mode = MovementMode.GROUNDED;
        }
    }

    private static bool IsObstacle(KinematicMovement2D.CastHit hit) {
        if (hit.collider.GetComponentInParent<HazardController>() != null) {
            return false;
        }
        return true;
    }

    private static void DoGrounded(RobotMovementInfo info, MovementState state, MovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        float drive = Mathf.Clamp(control.drive.x, -1, 1);

        Vector2 del = Vector2.right * drive * settings.walkSpeed * dt;
        del = del.ProjectOnNormal(state.ground.normal);
        DoDisplacement(del, info, state, settings, control, robotCollider, dt);

        float vDel = Mathf.Abs(settings.gravity) * dt * dt / 2;
        KinematicMovement2D.DisplaceToObstacle(robotCollider, ref state.position, Vector2.down, vDel*4, out var hit, IsObstacle);
        if(hit.Exists() && Vector2.Angle(Vector2.up, hit.normal) < settings.walkableSlope) {
            state.ground = hit;
        } else {
            state.ground = KinematicMovement2D.CastHit.None;
            state.mode = MovementMode.FREEFALL;
        }
    }

    private static void DoDisplacement(Vector2 del, RobotMovementInfo info, MovementState state, MovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        var newGround = KinematicMovement2D.CastHit.None;
        KinematicMovement2D.DisplaceAndSlide(robotCollider, ref state.position, del, out var hits, IsObstacle, delegate (KinematicMovement2D.CastHit hit, Vector2 remainingDisplacement) {
            if (Vector2.Angle(Vector2.up, hit.normal) < settings.walkableSlope) {
                newGround = hit;
                return Vector2.zero;
            } else {
                return remainingDisplacement.ProjectOnNormal(hit.normal);
            }
        });
        info.hitDuringMovement.AddRange(hits);
        if (newGround.Exists()) {
            state.ground = newGround;
        }
    }
}