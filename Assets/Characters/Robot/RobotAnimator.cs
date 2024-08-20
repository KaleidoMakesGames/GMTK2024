using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

[RequireComponent(typeof(RobotController))]
[ExecuteInEditMode]
public class RobotAnimator : MonoBehaviour
{
    private RobotController _robot;
    public RobotController robot {
        get {
            if (_robot == null) {
                _robot = GetComponent<RobotController>();
            }
            return _robot;
        }
    }

    public Transform spawnOnDeathPrefab;

    public ArmorRenderer armorPrefab;
    public Transform powerPrefab;

    public float armorHeight;
    public float powerWidth;

    public Transform armorContainer;
    public Transform powerContainerLeft;
    public Transform powerContainerRight;

    private int intactArmor { get { return Application.isPlaying ? robot.state.intactArmor : robot.settings.startTotalArmor; } }
    private int power{ get { return Application.isPlaying ? robot.state.power : robot.settings.startPower; } }
    private int totalArmor{ get { return Application.isPlaying ? robot.state.totalArmor : robot.settings.startTotalArmor; } }

    // Update is called once per frame
    void Update()
    {
        if(armorPrefab == null || powerPrefab == null || armorContainer == null || powerContainerLeft == null || powerContainerRight == null) {
            return;
        }

        UpdateCount(armorContainer, armorPrefab.transform, totalArmor, armorHeight);
        UpdateCount(powerContainerLeft, powerPrefab, power, powerWidth);
        UpdateCount(powerContainerRight, powerPrefab, power, powerWidth);

        int i = 0;
        foreach(var armor in armorContainer.GetComponentsInChildren<ArmorRenderer>().OrderBy(x => x.transform.localPosition.y)) {
            armor.intact = intactArmor > i;
            armor.width = 1 + powerWidth * power * 2;
            i++;
        }

        if(!Application.isPlaying) {
            return;
        }
        if(robot.state.victory) {
            gameObject.SetActive(false);
        }
        if (robot.state.isDead) {
            gameObject.SetActive(false);
            Instantiate(spawnOnDeathPrefab).transform.position = transform.position;
        }
    }

    private void UpdateCount(Transform container, Transform prefab, int goalCount, float thickness) {
        for(int i = container.childCount; i < goalCount; i++) {
            var nc = ObjectPool.InstantiatePrefab(prefab.gameObject).transform;
            nc.SetParent(container, false);
            nc.transform.localRotation = Quaternion.identity;
        }
        for(int i = container.childCount; i > goalCount; i--) {
            container.GetChild(i-1).gameObject.Destroy();
        }
        for(int i = 0; i < container.childCount; i++) {
            container.GetChild(i).localPosition = Vector3.up * i * thickness;
        }
    }
}
