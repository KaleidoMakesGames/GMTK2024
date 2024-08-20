using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ObjectPool {
    public static void Destroy(this UnityEngine.Object o) {
        if(o == null) {
            return;
        }
        if(Application.isPlaying) {
            Object.Destroy(o);
        } else {
            Object.DestroyImmediate(o);
        }
    }

    public static GameObject InstantiatePrefab(GameObject o) {
        if(Application.isPlaying) {
            return Object.Instantiate(o);
        }
#if UNITY_EDITOR
        return PrefabUtility.InstantiatePrefab(o) as GameObject;
#endif
        return null;
    }
}