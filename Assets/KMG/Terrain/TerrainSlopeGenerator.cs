using UnityEngine;

namespace UnityEditor.TerrainTools {
    public class TerrainSlopeGenerator : TerrainPaintTool<TerrainSlopeGenerator> {
        public float maxHeight;
        public float minHeight;

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext) {
            base.OnInspectorGUI(terrain, editContext);
            maxHeight = EditorGUILayout.Slider("Max Height", maxHeight, 0.0f, 1.0f);
            minHeight = EditorGUILayout.Slider("Min Height", minHeight, 0.0f, 1.0f);
            if(GUILayout.Button("Make Slope")) {
                MakeSlope(terrain);
            }
        }

        private void MakeSlope(Terrain terrain) {
            Vector3 size = terrain.terrainData.size.Divide(terrain.terrainData.heightmapScale) + Vector3.one;
            var heightmap = new float[(int)size.x, (int)size.z];
            for(float i = 0; i < size.x; i++) {
                float h = Mathf.Lerp(maxHeight, minHeight, i / size.x);
                for(float j = 0; j < size.z; j++) {
                    heightmap[(int)i, (int)j] = h;
                }
            }
            terrain.terrainData.SetHeights(0, 0, heightmap);
        }

        public override string GetDescription() {
            return "Creates a slope on the terrain.";
        }

        public override string GetName() {
            return "Slope generator";
        }
    }
}