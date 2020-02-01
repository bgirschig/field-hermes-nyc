using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eureka : MonoBehaviour
{
    public bool active = false;
    public postProcessing controller;
    float nextBlinkTime = 0;
    float blinkRate = 1f;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.E) && active) {
            active = false;
            blinkRate = 1;
        } else if (Input.GetKeyUp(KeyCode.E) && !active) {
            active = true;
        }

        if (!active) return;
        if (Time.time > nextBlinkTime) {
            controller.colorGroupIndex += 1;
            nextBlinkTime = Time.time + 1/blinkRate;
        }
        blinkRate += 0.01f;
    }
}
