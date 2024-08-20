using UnityEngine;

[ExecuteAlways]
public class RainPositioner : MonoBehaviour
{
    public CameraController controller;
    public ParticleSystem ps;
    private void Update() {
        if(controller == null || ps == null) {
            return;
        }
        var s = ps.shape;
        s.radius = controller.camera.aspect * controller.camera.orthographicSize;
        var m = ps.main;
        m.startLifetime = controller.camera.orthographicSize * 2 / m.startSpeed.constant + 2;
        var p = controller.camera.orthographicSize * Vector3.up + controller.transform.position;
        p.z = 0;
        transform.position = p;
    }
}
