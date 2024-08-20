using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartController : MonoBehaviour
{
    public void Pickup() {
        gameObject.Destroy();
    }
}
