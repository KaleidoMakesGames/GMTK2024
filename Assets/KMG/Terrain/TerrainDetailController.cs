using NaughtyAttributes.Test;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.TerrainTools;
using UnityEngine;

[ExecuteAlways]
public class TerrainDetailController : MonoBehaviour {
    [System.Serializable]
    public struct DetailInstanceData {
        public Matrix4x4 objectToWorld;
    }
    [System.Serializable]
    public class DetailLayer {
        [System.Serializable]
        public struct SubmeshSetting {
            public Material submeshMaterial;
            public bool generateCollider;
        }
        public string name;
        public Mesh mesh;
        [HideInInspector] public List<BoxCollider> colliders;
        public SubmeshSetting[] submeshSettings;
        [HideInInspector]public List<DetailInstanceData> data;
        public bool enabled;
        [NaughtyAttributes.ReadOnly][NaughtyAttributes.AllowNesting]public int count;
    }

    public List<DetailLayer> layers;
    public bool showColliders;

    private void Update() {
        foreach(var layer in layers) {
            Render(layer);
            layer.count = layer.data.Count;
        }
    }

    private void Reset() {
        layers = new List<DetailLayer>();
    }

    [NaughtyAttributes.Button]
    private void UpdateColliders() {
        if(Application.isPlaying) {
            Debug.LogWarning("Can't update colliders in Play Mode.");
            return;
        }
        foreach(var layer in layers) {
            var bounds = Enumerable.Range(0, layer.mesh.subMeshCount).Select(x => layer.mesh.GetSubMesh(x).bounds).ToArray();
            if(layer.colliders == null) {
                layer.colliders = new List<BoxCollider>();
            }
            foreach (var collider in layer.colliders) {
                DestroyImmediate(collider.gameObject);
            }
            if (!layer.enabled) {
                continue;
            }
            layer.colliders = new List<BoxCollider>();
            for(int i = 0; i < layer.data.Count; i++) {
                for (int smI = 0; smI < bounds.Length; smI++) {
                    if (!layer.submeshSettings[smI].generateCollider) {
                        continue;
                    }
                    var c = new GameObject("Tree Collider", typeof(BoxCollider)).GetComponent<BoxCollider>();
                    c.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    c.size = bounds[smI].size;
                    c.center = bounds[smI].center;
                    c.transform.SetParent(transform);
                    c.transform.position = layer.data[i].objectToWorld.GetPosition();
                    c.transform.rotation = layer.data[i].objectToWorld.rotation;
                    c.transform.localScale = layer.data[i].objectToWorld.lossyScale;
                    layer.colliders.Add(c);
                }
            }
        }
    }

    private void OnValidate() {
        foreach(var layer in layers) {
            if(layer.mesh == null) {
                layer.submeshSettings = new DetailLayer.SubmeshSetting[0];
            } else if(layer.submeshSettings.Length != layer.mesh.subMeshCount) {
                var newList = layer.submeshSettings.Take(layer.mesh.subMeshCount).ToList();
                newList.AddRange(Enumerable.Range(0, layer.mesh.subMeshCount - newList.Count).Select(x => new DetailLayer.SubmeshSetting()));
                layer.submeshSettings = newList.ToArray();
            }
        }    
    }

    private void Render(DetailLayer layer) {
        if (!layer.enabled) {
            return;
        }
        if (layer.data == null) {
            layer.data = new List<DetailInstanceData>();
        }
        for (int i = 0; i < layer.mesh.subMeshCount; i++) {
            var material = layer.submeshSettings[i].submeshMaterial;
            if(material == null) {
                continue;
            }
            var rp = new RenderParams(material);
            rp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rp.receiveShadows = true;
            if (layer.data.Count > 0) {
                Graphics.RenderMeshInstanced(in rp, layer.mesh, i, layer.data);
            }
        }
    }
    private void OnDrawGizmosSelected() {
        ColorUtility.TryParseHtmlString("#91F48B", out var c);
        Gizmos.color = c;
        if (showColliders) {
            foreach (var layer in layers) {
                if (layer.colliders == null) {
                    layer.colliders = new List<BoxCollider>();
                }
                foreach (var collider in layer.colliders) {
                    var s = collider.transform.lossyScale;
                    s.Scale(collider.size);
                    GizmoUtilities.DrawCube(collider.transform.TransformPoint(collider.center), s, collider.transform.rotation);
                }
            }
        }
    }
}