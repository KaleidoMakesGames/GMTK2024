using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.TerrainTools;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShortcutManagement;
using System.Collections.Generic;

namespace UnityEditor.TerrainTools {
    public class TerrainDetailPainter : TerrainPaintToolWithOverlays<TerrainDetailPainter> {

        public override string OnIcon => "TerrainOverlays/PaintDetails_On.png";
        public override string OffIcon => "TerrainOverlays/PaintDetails.png";
        public override bool HasToolSettings => true;
        public override bool HasBrushAttributes => true;
        public override bool HasBrushMask => true;
        public override int IconIndex {
            get { return (int)FoliageIndex.PaintDetails-1; }
        }
        public override TerrainCategory Category {
            get { return TerrainCategory.Foliage; }
        }

        public TerrainDetailController detailRenderer;
        public int currentLayerIndex;


        public float brushDensity;
        public float maxSlope;

        public TerrainDetailController.DetailLayer lastLayer;
        public int lastNumberOfLayers;

        private void UpdateSelectedLayer(Terrain terrain) {
            if (detailRenderer == null) {
                detailRenderer = terrain.GetComponent<TerrainDetailController>();
                if (detailRenderer == null) {
                    detailRenderer = terrain.gameObject.AddComponent<TerrainDetailController>();
                }
                lastNumberOfLayers = detailRenderer.layers.Count;
            } else {
                if (detailRenderer.layers.Count > lastNumberOfLayers) {
                    // We added a new layer.
                    currentLayerIndex = detailRenderer.layers.Count - 1;
                }
            }

            if (detailRenderer.layers.Count == 0) {
                // No layers exist.
                currentLayerIndex = -1;
                lastLayer = null;
                lastNumberOfLayers = 0;
                return;
            }

            if (detailRenderer.layers.Count < lastNumberOfLayers) {
                // Something was removed. Was it our current layer?
                if (detailRenderer.layers.Contains(lastLayer)) {
                    // No, make sure we are still selecting it.
                    currentLayerIndex = detailRenderer.layers.IndexOf(lastLayer);
                } else {
                    // Yes ours was removed. Select the one after it.
                    currentLayerIndex = currentLayerIndex + 1;
                }
            }

            currentLayerIndex = Mathf.Clamp(currentLayerIndex, 0, detailRenderer.layers.Count - 1);
            lastLayer = detailRenderer.layers[currentLayerIndex];
            lastNumberOfLayers = detailRenderer.layers.Count;
        }

        public override void OnToolSettingsGUI(Terrain terrain, IOnInspectorGUI editContext) {
            EditorGUILayout.BeginHorizontal();
            if (currentLayerIndex == -1) {
                EditorGUILayout.LabelField("No layers present. Add one in the TerrainDetailController inspector.");
            } else {
                EditorGUILayout.PrefixLabel("Selected Layer");
                currentLayerIndex = EditorGUILayout.Popup(currentLayerIndex, detailRenderer.layers.Select(x => x.name).ToArray());
            }
            EditorGUILayout.EndHorizontal();
            maxSlope = EditorGUILayout.FloatField("Max Slope", maxSlope);
            brushDensity = EditorGUILayout.FloatField("Detail Density", brushDensity);

        }
        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext) {
            UpdateSelectedLayer(terrain);

            editContext.ShowBrushesGUI(5, BrushGUIEditFlags.Select | BrushGUIEditFlags.Size | BrushGUIEditFlags.Opacity, terrain.terrainData.heightmapResolution);
            OnToolSettingsGUI(terrain, editContext);
        }

        public override void OnEnterToolMode() {
            currentLayerIndex = EditorPrefs.GetInt("KMGTerrainDetailLayer", 0);
            brushDensity = EditorPrefs.GetFloat("KMGTerrainDetailDensity", 0);
            maxSlope = EditorPrefs.GetFloat("KMGTerrainDetailMaxSlope", 0);
        }
        public override void OnExitToolMode() {
            EditorPrefs.SetInt("KMGTerrainDetailLayer", currentLayerIndex);
            EditorPrefs.SetFloat("KMGTerrainDetailDensity", brushDensity);
            EditorPrefs.SetFloat("KMGTerrainDetailMaxSlope", maxSlope);
        }
        public override string GetDescription() {
            return "Left click to add. Hold control and left click to remove.";
        }

        public override string GetName() {
            return "KMG Detail Painter";
        }
        public override void OnRenderBrushPreview(Terrain terrain, IOnSceneGUI editContext) {
            if (currentLayerIndex == -1) {
                return;
            }
            if (editContext.hitValidTerrain && Event.current.type == EventType.Repaint) {
                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord, editContext.brushSize, 0.0f);
                PaintContext ctx = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
                TerrainPaintUtilityEditor.DrawBrushPreview(ctx, TerrainBrushPreviewMode.SourceRenderTexture, editContext.brushTexture, brushXform, TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial(), 0);
                TerrainPaintUtility.ReleaseContextResources(ctx);
            }
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext) {
            if (currentLayerIndex == -1) {
                return false;
            }
            base.OnPaint(terrain, editContext);

            float r = editContext.brushSize / 2;
            if (Event.current.control) {
                RemoveDetails(terrain, editContext.raycastHit.point, r);
            } else {
                AddDetails(terrain, editContext.raycastHit.point, r);
            }

            return false;
        }

        private void AddDetails(Terrain terrain, Vector3 centerPos, float radius) {
            float area = Mathf.PI * radius * radius;
            int n = Mathf.RoundToInt(brushDensity * area);
            for (int i = 0; i < n; i++) {
                Vector2 pos = Random.insideUnitCircle;
                Vector3 worldPos = new Vector3(pos.x, 0.0f, pos.y) * radius + centerPos;
                float height = terrain.SampleHeight(worldPos);
                Vector3 p = terrain.transform.InverseTransformPoint(worldPos);
                Vector2 interpolated = new Vector2(p.x / terrain.terrainData.size.x, p.z / terrain.terrainData.size.z);
                if(interpolated.x < 0 || interpolated.x > 1 || interpolated.y < 0 || interpolated.y > 1) {
                    continue;
                }
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(interpolated.x, interpolated.y);
                float angle = Vector3.Angle(Vector3.up, normal);
                if(angle > maxSlope) {
                    continue;
                }
                worldPos.y = height + terrain.transform.position.y;
                var newPoint = new TerrainDetailController.DetailInstanceData();
                newPoint.objectToWorld = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one);
                detailRenderer.layers[currentLayerIndex].data.Add(newPoint);
            }
        }
        private void RemoveDetails(Terrain terrain, Vector3 centerPos, float radius) {
            var data = detailRenderer.layers[currentLayerIndex].data;
            centerPos.y = 0;
            for (int i = data.Count - 1; i >= 0; i--) {
                var p = data[i].objectToWorld.GetPosition();
                p.y = 0;
                if (Vector3.Distance(centerPos, p) <= radius) {
                    data.RemoveAt(i);
                }
            }
        }
    }
}