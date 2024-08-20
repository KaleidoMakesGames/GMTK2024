using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RockDestroyerCheck : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        var rc = collision.GetComponentInParent<RobotController>();
        if(rc != null && rc.state.power > rc.settings.startPower) {
            Destroy(gameObject);
            FXPlayer.Play(FXPlayer.Instance.bassSound);
            FXPlayer.Play(FXPlayer.Instance.deathSound);
        }
    }
}
