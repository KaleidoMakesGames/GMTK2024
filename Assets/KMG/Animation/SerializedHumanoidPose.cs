using MathUtilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace KMGAnimation {
    public class SerializedHumanoidPose : ScriptableObject {
        [HideInInspector] public HumanoidPose pose;
    }

}