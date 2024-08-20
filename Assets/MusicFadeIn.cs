using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicFadeIn : MonoBehaviour
{
    public float fadeTime;
    public AnimationCurve fadeCurve;

    [SerializeField][HideInInspector]public AudioSource[] sources;
    [SerializeField][HideInInspector] public float[] volumes;
    private void Start() {
        sources = GetComponentsInChildren<AudioSource>().ToArray();
        volumes = sources.Select(x => x.volume).ToArray();
    }
    private void Update() {
        if(Time.time > fadeTime) {
            return;
        }
        for(int i = 0; i < sources.Length; i++) {
            sources[i].volume = fadeCurve.Evaluate(Mathf.Clamp01(Time.time / fadeTime) * volumes[i]);
        }
    }
}
