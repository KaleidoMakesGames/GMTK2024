using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RobotController : MonoBehaviour
{
    public RobotMovementMechanics.RobotCollider robotCollider;
    public RobotMechanics.RobotState state;
    [SerializeField]private SerializedRobotSettings _settings;
    public RobotMechanics.RobotSettings settings { get { return _settings.settings;  } }
    public RobotMechanics.RobotControl control;

    private void Start() {
        RobotMechanics.Initialize(state, settings);
    }

    private Vector3 _lastPosition;
    private void Update() {
        if(Application.isPlaying) {
            return;
        }
        float vel = (transform.position - _lastPosition).magnitude/Time.deltaTime;
        _lastPosition = transform.position;
    }

    private void FixedUpdate() {
        RobotMechanics.UpdateMechanics(state, settings, control, robotCollider, Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        RobotMechanics.HandleTriggerCollision(state, settings, control, collision);
    }
}
