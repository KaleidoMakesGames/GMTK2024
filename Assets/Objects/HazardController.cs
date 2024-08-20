using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardController : MonoBehaviour
{
    public float width;
    new public FallingHazardMechanics.HazardCollider collider;
    public FallingHazardMechanics.Settings settings;
    public FallingHazardMechanics.State state;

    private void Start() {
        FallingHazardMechanics.Initialize(transform.position, state, settings, collider);
    }

    private void FixedUpdate() {
        FallingHazardMechanics.UpdateHazardMechanics(state, settings, collider, Time.fixedDeltaTime);

        if(state.hitObject.Exists()) {
            Destroy(gameObject);
        }
    }
}
