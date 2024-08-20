using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FXPlayer : MonoBehaviour
{
    private static FXPlayer _Instance;
    public static FXPlayer Instance {
        get {
            if(_Instance == null) {
                _Instance = FindFirstObjectByType<FXPlayer>();
            }
            return _Instance;
        }
    }
    public AudioSource source;

    public AudioClip bassSound;//
    public AudioClip errorSound;//
    public AudioClip buildSound;//
    public AudioClip buildCompleteSound;//
    public AudioClip repairCompleteSound;//
    public AudioClip damageTakenSound;
    public AudioClip deathSound;
    public AudioClip partCollectedSound;

    public static void Play(AudioClip clip) {
        Instance.source.PlayOneShot(clip);
    }
}
