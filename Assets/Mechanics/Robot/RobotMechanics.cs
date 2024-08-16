using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RobotMechanics
{
    [System.Serializable]
    public class RobotState {
        public RobotMovementMechanics.RobotMovementState movementState;
    }
    [System.Serializable]
    public class RobotSettings {
        public RobotMovementMechanics.RobotMovementSettings movementSettings;
    }
    [System.Serializable]
    public class RobotControl {
        public RobotMovementMechanics.RobotMovementControl movementControl;
    }

    public static void UpdateMechanics(RobotState state, RobotSettings settings, RobotControl control, RobotMovementMechanics.RobotCollider collider, float dt) {
        state.movementState.position = collider.rb.position;

        RobotMovementMechanics.UpdateMovementMechanics(state.movementState, settings.movementSettings, control.movementControl, collider, dt);

        collider.rb.MovePosition(state.movementState.position);
    }
}
