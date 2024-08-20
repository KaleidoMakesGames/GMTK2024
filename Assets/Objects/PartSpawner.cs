using KMGMath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartSpawner : MonoBehaviour
{
    public Transform prefab;
    public Transform container;
    public Vector2 spawnBounds;

    [SerializeField][HideInInspector] private Vector2[] positions;

    public AnimationCurve densityCurve;
    public int totalToSpawn;
    public float previewSize;

    private void Start() {
        foreach(var p in positions) {
            Instantiate(prefab.gameObject, container).transform.position = p;
        }    
    }

    private Vector2 NormToWorld(Vector2 norm) {
        return new Vector2(Mathf.Lerp(transform.position.x - spawnBounds.x / 2, transform.position.x + spawnBounds.x / 2, norm.x),
            Mathf.Lerp(transform.position.y - spawnBounds.y / 2, transform.position.y + spawnBounds.y / 2, norm.y));
    }
    [NaughtyAttributes.Button("Respawn")]
    void Respawn() {
        var s = new AnimationCurveSampler(densityCurve, 1000);
        positions = new Vector2[totalToSpawn];
        for(int i = 0; i < totalToSpawn; i++) {
            positions[i] = NormToWorld(new Vector2(Random.value, s.Sample()));
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnBounds);
        if(positions != null) {
            foreach (var p in positions) {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(p, previewSize);
            }
        }
    }
}
