using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DifficultyCurve : ScriptableObject {
    [System.Serializable]
    public struct Hazard {
        public HazardController prefab;
        public AnimationCurve probabilityVsHeight;
    }

    public List<Hazard> hazards;
    public float maxHazardsPerSecond;
    public AnimationCurve hazardsPerSecondOverHeight;
}
