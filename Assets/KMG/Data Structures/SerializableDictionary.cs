using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver {
    [SerializeField]private K[] ks;
    [SerializeField] private V[] vs;

    public void OnAfterDeserialize() {
        if(ks == null || vs == null || ks.Length != vs.Length) {
            return;
        }
        for(int i = 0; i < ks.Length; i++) {
            this[ks[i]] = vs[i];
        }
    }

    public void OnBeforeSerialize() {
        ks = Keys.ToArray();
        vs = Values.ToArray();
    }
}
