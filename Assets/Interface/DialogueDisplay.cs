using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueDisplay : MonoBehaviour
{
    public Transform droneTransform;
    public RectTransform container;
    public Vector2 offset;

    public TextMeshProUGUI textField;

    public float typingRate;
    public bool isTyping;
    public bool isBusy;

    // Update is called once per frame
    void Update()
    {
        container.position = droneTransform.position + (Vector3)offset;
    }

    private static DialogueDisplay _Instance;
    public static DialogueDisplay Instance {
        get {
            if(_Instance == null) {
                _Instance = FindFirstObjectByType<DialogueDisplay>();
            }
            return _Instance;
        }
    }

    public IEnumerator TypeCor(string text, float duration, Action OnComplete) {
        yield return TypeCor(text);
        isBusy = true;
        yield return new WaitForSeconds(duration);
        isBusy = false;
        Hide();
        OnComplete.Invoke();
    }

    public IEnumerator TypeCor(string text) {
        while(isBusy) {
            yield return null;
        }

        container.gameObject.SetActive(true);
        isBusy = true;
        isTyping = true;
        for(int i = 0; i < text.Length; i++) {
            textField.text = text.Substring(0, i+1);
            yield return new WaitForSeconds(1 / typingRate);
        }
        isTyping = false;
        isBusy = false;
    }

    public void Hide() {
        container.gameObject.SetActive(false);
    }

    public static void Type(string text) {
        Instance.StartCoroutine(Instance.TypeCor(text));
    }

    public static void Type(string text, float duration, Action OnComplete) {
        Instance.StartCoroutine(Instance.TypeCor(text, duration, OnComplete));
    }
}
