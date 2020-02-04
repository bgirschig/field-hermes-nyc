using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingStar : MonoBehaviour
{
    public float rotateSpeed = 0;

    private float rotationAccumulator = 0;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(0, 0, rotateSpeed);

        rotationAccumulator += Mathf.Abs(rotateSpeed);

        // TODO: [PERFORMANCE] we could kill the shootingstar as soon as it leaves the screen. This
        // is easier for now
        if (rotationAccumulator > 300) GameObject.Destroy(this.gameObject);
    }
}