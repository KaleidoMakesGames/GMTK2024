using KMGMath;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneSoundPlayer : MonoBehaviour
{
    public DialogueDisplay display;
    public AudioClip clip;

    public float[] pitches;
    public float volume;
    public float boopsPerSecond;

    [SerializeField][HideInInspector] public AudioSource[] sources;
    private void Start() {
        sources = new AudioSource[pitches.Length];
        for (int i = 0; i < pitches.Length; i++) {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].hideFlags = HideFlags.HideInInspector;
        }
    }
    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < sources.Length; i++) {
            sources[i].pitch = pitches[i];
            sources[i].volume = volume;
        }
        if(display.isTyping) {
            if(Statistics.SamplePoisson(boopsPerSecond * Time.deltaTime) > 0) {
                sources[Random.Range(0, sources.Length)].PlayOneShot(clip);
            }
        }
    }
}
