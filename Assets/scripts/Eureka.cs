using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eureka : MonoBehaviour
{
    public bool active = false;
    public postProcessing controller;
    public shootingStarSpawn shootingStarController;
    
    float nextStarTime = 0;
    float starRate = 1f;

    float nextBlinkTime;
    float stopTime;
    float blinkRate;
    int nextColorGroup = 0;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.E) && active) {
            active = false;
            controller.colorGroupIndex = nextColorGroup;
        } else if (Input.GetKeyUp(KeyCode.E) && !active) {
            active = true;
            blinkRate = 5;
            starRate = 1;
            nextBlinkTime = Time.time + 60;
            stopTime = Time.time + 63;
            nextColorGroup += 1;
        }

        if (active) {
            if (Time.time > nextStarTime) {
                shootingStarController.spawn();
                nextStarTime = Time.time + 1/starRate;
                starRate += 0.1f;
            }
            if (Time.time > nextBlinkTime) {
                controller.colorGroupIndex += 1;
                nextBlinkTime = Time.time + 1/blinkRate;
                blinkRate += 1f;
            }
            if (Time.time > stopTime) {
                active = false;
            }
        }
    }
}
