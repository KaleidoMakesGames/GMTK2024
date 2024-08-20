using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ArmorRenderer : MonoBehaviour
{
    public Transform leftRailing;
    public Transform rightRailing;
    public Transform beam;

    public bool intact;

    public float railingOffset;
    public float width;

    // Update is called once per frame
    void Update()
    {
        if(leftRailing == null || rightRailing == null || beam == null) {
            return;
        }
        beam.gameObject.SetActive(intact);
        beam.localScale = new Vector3(width, beam.localScale.y, 1);
        leftRailing.localPosition = Vector2.left * (width / 2 + railingOffset);
        rightRailing.localPosition = Vector2.right * (width / 2 + railingOffset);
    }
}
