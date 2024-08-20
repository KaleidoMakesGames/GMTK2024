using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FallingHazardMechanics {
    [System.Serializable]
    public class State {
        public Vector2 position;
        public float downSpeed;
        public KinematicMovement2D.CastHit hitObject;
    }

    [System.Serializable]
    public class Settings {
        public float initialSpeed;
        public float gravity;
        public int damage;
    }

    [System.Serializable]
    public class HazardCollider : KinematicMovement2D.KinematicCollider { }

    public static void Initialize(Vector2 spawnPosition, State state, Settings settings, HazardCollider collider) {
        state.position = spawnPosition;
        collider.rb.position = spawnPosition;
        state.downSpeed = Mathf.Abs(settings.initialSpeed);
        collider.rb.isKinematic = true;
    }

    public static void UpdateHazardMechanics(State state, Settings settings, HazardCollider collider, float dt) {
        if(state.hitObject.Exists()) {
            return;
        }

        state.position = collider.rb.position;
        Vector2 lastPosition = state.position;

        var newSpeed = state.downSpeed + settings.gravity * dt;
        var delta = dt * (state.downSpeed + newSpeed) / 2;
        KinematicMovement2D.DisplaceToObstacle(collider, ref state.position, Vector2.down, delta, out state.hitObject);
        if (state.hitObject.Exists()) {
            var robot = state.hitObject.collider.GetComponentInParent<RobotController>();
            if (robot != null) {
                RobotMechanics.TakeDamage(robot.state, settings.damage);
            }
        }

        state.downSpeed = Mathf.Abs(((state.position - lastPosition)/dt).y);
        collider.rb.MovePosition(state.position);
    }
}
