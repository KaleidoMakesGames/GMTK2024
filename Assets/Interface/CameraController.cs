using KMGPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Springs.DampedSpringSettings followSpring;
    public RobotController robotController;
    public float framingBorder;

    [System.Serializable]
    public struct SizeProp {
        public float robotSize;
        public float orthoSize;
    }

    public SizeProp minSize;
    public SizeProp maxSize;
    public float sizeChangeSpeed;
    private Camera _camera;
    new public Camera camera {
        get {
            if(_camera == null) {
                _camera = GetComponent<Camera>();
            }
            return _camera;
        }
    }

    public bool trackX;
    private Vector3 _vel;

    // Update is called once per frame
    void LateUpdate()
    {
        var s = robotController.robotCollider.rb.GetBounds().size.Max();
        float os = Mathf.Lerp(minSize.orthoSize, maxSize.orthoSize, Mathf.InverseLerp(minSize.robotSize, maxSize.robotSize, s));

        if (robotController.gameObject.activeSelf) {
            camera.orthographicSize = Mathf.MoveTowards(camera.orthographicSize, os, sizeChangeSpeed * Time.deltaTime);
        }

        Vector2 robotBottom = robotController.transform.position;
        robotBottom.y -= framingBorder;

        Vector2 goalCameraPosition = robotBottom + Vector2.up * camera.orthographicSize;
        if(!trackX) {
            goalCameraPosition.x = 0;
        }

        Vector3 h = transform.position;
        Springs.ComputeMassSpringDamper(ref h, ref _vel, goalCameraPosition, followSpring, Time.deltaTime);
        transform.position = new Vector3(h.x, h.y, -10);    
    }
}
