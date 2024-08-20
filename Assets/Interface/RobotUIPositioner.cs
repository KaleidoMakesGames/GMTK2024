using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RobotUIPositioner : MonoBehaviour
{
    public Transform container;

    private void Reset() {
        container = transform;
    }

    public RobotController robot;
    public enum AnchorSide { LEFT, TOP, RIGHT, BOTTOM }
    public AnchorSide anchorSide;

    // Update is called once per frame
    void LateUpdate() {
        Vector2 charSize = robot.robotCollider.rb.GetBounds().size;

        Vector2 charBottom = robot.transform.position;
        Vector2 charLeft = charBottom + Vector2.left * charSize.x / 2 + Vector2.up * charSize.y / 2;
        Vector2 charRight = charBottom + Vector2.right * charSize.x / 2 + Vector2.up * charSize.y / 2;
        Vector2 charTop = charBottom + Vector2.up * charSize.y;

        Vector2 target = anchorSide == AnchorSide.LEFT ? charLeft :
            (anchorSide == AnchorSide.TOP ? charTop :
            (anchorSide == AnchorSide.RIGHT ? charRight : charBottom));

        transform.position = target;
    }
}
