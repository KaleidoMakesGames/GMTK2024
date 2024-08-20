using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class IconController : MonoBehaviour
{
    [System.Serializable]
    public struct Icon {
        public Image container;
        public TextMeshProUGUI hotkey;
        public TextMeshProUGUI cost;
        public RawImage blackout;
    }

    public Icon repairIcon;
    public Icon armorIcon;
    public Icon powerIcon;

    public RobotController robot;

    // Update is called once per frame
    void Update()
    {
        Set(repairIcon, robot.state.intactArmor < robot.state.totalArmor && robot.state.nParts >= robot.settings.partsPerRepair, "Q", robot.settings.partsPerRepair);
        Set(armorIcon, robot.state.nParts >= robot.settings.partsPerArmor, "E", robot.settings.partsPerArmor);
        Set(powerIcon, robot.state.nParts >= robot.settings.partsPerPower, "R", robot.settings.partsPerPower);
    }

    private void Set(Icon icon, bool enabled, string key, int cost) {
        icon.blackout.gameObject.SetActive(!enabled);
        icon.hotkey.text = key;
        icon.cost.text = cost.ToString();
    }
}
