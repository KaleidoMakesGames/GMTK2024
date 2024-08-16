using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainUtilities
{
    public static TerrainLayer GetLayerAtPoint(this Terrain terrain, Vector3 point) {
        var local = terrain.transform.InverseTransformPoint(point);
        var tdSpace = new Vector2Int(Mathf.RoundToInt(terrain.terrainData.alphamapWidth * local.x / terrain.terrainData.size.x),
                                    Mathf.RoundToInt(terrain.terrainData.alphamapHeight * local.z / terrain.terrainData.size.z));
        var ms = terrain.terrainData.GetAlphamaps(tdSpace.x, tdSpace.y, 1, 1);
        float maxWeight = 0;
        int maxIndex = 0;
        for (int i = 0; i < ms.GetLength(2); i++) {
            if (ms[0, 0, i] > maxWeight) {
                maxIndex = i;
                maxWeight = ms[0, 0, i];
            }
        }
        return terrain.terrainData.terrainLayers[maxIndex];
    }
}
