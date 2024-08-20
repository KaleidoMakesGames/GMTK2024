using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class PartsDisplay : MonoBehaviour
{
    public Transform container;
    public RobotController robotController;
    public TextMeshProUGUI textField;

    // Update is called once per frame
    void Update()
    {
        if(robotController == null || textField == null) {
            return;
        }

        textField.text = robotController.state.nParts.ToString();

        if(!Application.isPlaying) {
            return;
        }
        container.gameObject.SetActive(robotController.state.movementState.mode == RobotMovementMechanics.MovementMode.CLIMB);
        if (robotController.state.victory || robotController.state.isDead) {
            gameObject.SetActive(false);
        }
    }
}
