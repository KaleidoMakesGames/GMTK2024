using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainSpawner : MonoBehaviour
{
    public Transform prefab;
    public int count;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.GetComponentInParent<RobotController>() != null) {
            SpawnCurtain();
            gameObject.SetActive(false);
        }
    }

    void SpawnCurtain() {
        for (int i = 0; i < count; i++) {
            OffscreenSpawner.SpawnRelative(prefab, (float)i / count, Random.value * 2);
        }
    }
}
