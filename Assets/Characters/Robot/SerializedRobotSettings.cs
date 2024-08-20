using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="GMTK_2024/Robot Settings")]
public class SerializedRobotSettings : ScriptableObject
{
    public RobotMechanics.RobotSettings settings;
}
