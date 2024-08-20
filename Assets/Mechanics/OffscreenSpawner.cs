using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class OffscreenSpawner : MonoBehaviour
{
    public float wallWidth;
    public CameraController cameraController;
    private static OffscreenSpawner _instance;
    public static OffscreenSpawner instance {
        get {
            if(_instance == null) {
                _instance = FindFirstObjectByType<OffscreenSpawner>();
            }
            return _instance;
        }
    }
    public static Transform Spawn(Transform prefab, float xPosition, float offsetY = 0, float width = 0, bool clampWall=true) {
        var o = Instantiate(prefab.gameObject).transform;
        float topPosition = instance.cameraController.camera.ViewportToWorldPoint(Vector3.up).y;
        if (clampWall) {
            xPosition = Mathf.Clamp(xPosition, width-instance.wallWidth / 2, instance.wallWidth / 2-width);
        }
        o.position = new Vector2(xPosition, topPosition + offsetY);
        return o;
    }

    private void Awake() {
        _instance = this;
    }

    public static Transform SpawnRelative(Transform prefab, float xFraction, float offsetY = 0, float width=0, bool clampWall=true) {
        return Spawn(prefab, Mathf.Lerp(-instance.wallWidth/2, instance.wallWidth/2, xFraction), offsetY, width, clampWall);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one * wallWidth);
    }
}
