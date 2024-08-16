using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public RobotMovementMechanics.RobotCollider robotCollider;
    public RobotMechanics.RobotState state;
    public RobotMechanics.RobotSettings settings;
    public RobotMechanics.RobotControl control;

    private void FixedUpdate() {
        RobotMechanics.UpdateMechanics(state, settings, control, robotCollider, Time.fixedDeltaTime);
    }
}
