using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionIndicator : MonoBehaviour
{
    public RectTransform bar;
    public Transform container;
    public TextMeshProUGUI textField;
    public RobotController robot;

    // Update is called once per frame
    void Update() {
        if (robot.state.victory || robot.state.isDead) {
            gameObject.SetActive(false);
        }
        foreach (var t in transform.GetChildren()) {
            t.gameObject.SetActive(robot.state.actionProgress > 0);
        }
        container.gameObject.SetActive(robot.state.currentAction != RobotMechanics.RobotState.Action.NONE);
        if(robot.state.currentAction == RobotMechanics.RobotState.Action.NONE) {
            return;
        }
        bar.anchorMax = new Vector2(robot.state.actionProgress, 1);
        textField.text = robot.state.currentAction == RobotMechanics.RobotState.Action.REPAIRING ? "REPAIR" :
            robot.state.currentAction == RobotMechanics.RobotState.Action.ADDING_ARMOR ? "ADD ARMOR" :
            robot.state.currentAction == RobotMechanics.RobotState.Action.ADDING_POWER ? "ADD MOTOR" : "INSUFFICIENT";
    }
}
