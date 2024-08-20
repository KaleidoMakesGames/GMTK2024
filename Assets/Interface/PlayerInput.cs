using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public RobotController robotController;
    public DifficultyToggler toggler;
    public bool addArmor {
        get {
            return Keyboard.current.eKey.isPressed;
        }
    }

    public bool addPower {
        get {
            return Keyboard.current.rKey.isPressed;
        }
    }
    public bool repair {
        get {
            return Keyboard.current.qKey.isPressed;
        }
    }
    public Vector2 drive{
        get {
            Vector2 drive = Vector2.zero;
            drive += (Gamepad.current != null) ? Gamepad.current.leftStick.value : Vector2.zero;

            drive.x += (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) ? -1 : 0;
            drive.x += (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) ? 1 : 0;
            drive.y += (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) ? -1 : 0;
            drive.y += (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) ? 1 : 0;

            return Vector2.ClampMagnitude(drive, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        robotController.control.movementControl.drive = drive;
        robotController.control.doRepair = repair;
        robotController.control.doAddArmor = addArmor;
        robotController.control.doAddPower = addPower;

        if(Keyboard.current.tabKey.wasPressedThisFrame) {
            toggler.Toggle();
        }
    }
}
