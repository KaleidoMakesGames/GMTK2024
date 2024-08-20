using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class VictoryScreenDisplay : MonoBehaviour {
    public Transform container;
    public TMPro.TextMeshProUGUI textField;

    public RobotController robot;
    public float displayTime;
    public float flashRate;

    bool isDone = false;

    void Awake() {
        container.gameObject.SetActive(false);
    }

    private void Update() {
        if (container == null || textField == null || robot == null) {
            return;
        }
        string textToShow = "STATUS 1: *** OPTIMIZATION COMPLETE ***\n\nTerminal Configuration:\n";
        textToShow += "[POWER " + robot.state.power + "]\n";
        textToShow += "[SHIELD UNITS " + robot.state.totalArmor + "]\n";
        textToShow += "[WEIGHT " + RobotMechanics.CalculateWeight(robot.state, robot.settings) + "]\n";
        textToShow += "\nPress any key to continue ";
        if (!Application.isPlaying) {
            textField.text = textToShow;
            return;
        }

        if (robot.state.victory && !container.gameObject.activeSelf) {
            container.gameObject.SetActive(true);
            StartCoroutine(DisplayText(textToShow));
            PlayerPrefs.SetFloat("SkipIntro", 0);
        }

        if (isDone && Keyboard.current.anyKey.isPressed) {
            SceneManager.LoadScene("Game");
            enabled = false;
        }
    }
    public IEnumerator DisplayText(string textToShow) {
        int nLines = textToShow.Split("\n").Length;
        for (int i = 0; i < textToShow.Length; i++) {
            string curr = textToShow.Substring(0, i) + "_";
            int linesCurr = curr.Split("\n").Length;
            textField.text = curr + new string('\n', 1 + nLines - linesCurr);
            yield return new WaitForSeconds(displayTime / textToShow.Length);
        }

        bool flashon = true;
        while (true) {
            textField.text = textToShow + (flashon ? "_" : " ");
            flashon = !flashon;
            isDone = true;
            yield return new WaitForSeconds(1 / flashRate);
        }

    }
}
