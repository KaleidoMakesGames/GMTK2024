using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class DifficultyToggler : MonoBehaviour
{
    public int selected;
    public TextMeshProUGUI textField;

    [System.Serializable]
    public struct Difficulty {
        public DifficultyCurve curve;
        public Color color;
    }

    public List<Difficulty> difficulties;
    public HazardSpawner spawner;

    // Update is called once per frame
    void Update()
    {
        var selectedD = difficulties[selected];
        spawner.difficulty = selectedD.curve;
        textField.text = "DIFFICULTY: <color=#" + selectedD.color.ToHexString().Substring(0, 6) + ">" + selectedD.curve.name.ToUpper() + "</color>\n[TAB] to cycle";
    }

    public void Toggle() {
        selected = (int)Mathf.Repeat(selected + 1, difficulties.Count);
    }

    private void OnValidate() {
        selected = (int)Mathf.Repeat(selected, difficulties.Count);
    }
}
