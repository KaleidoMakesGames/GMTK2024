using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RobotMovementMechanics {
    [System.Serializable]
    public class RobotMovementState {
        public Vector2 position;
    }
    [System.Serializable]
    public class RobotMovementSettings {
        public float speed;
    }
    [System.Serializable]
    public class RobotMovementControl {
        public Vector2 drive;
    }
    [System.Serializable]
    public class RobotCollider : KinematicMovement2D.KinematicCollider {
    }

    public static void UpdateMovementMechanics(RobotMovementState state, RobotMovementSettings settings, RobotMovementControl control, RobotCollider robotCollider, float dt) {
        var delta = Vector2.ClampMagnitude(control.drive, 1) * settings.speed * dt;
        KinematicMovement2D.DisplaceToObstacle(robotCollider, ref state.position, delta.normalized, delta.magnitude, out var hit);
    }
}