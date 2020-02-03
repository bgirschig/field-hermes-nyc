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

    float blinkStartTime = 0;
    float nextBlinkTime = 0;
    float blinkRate = 1f;
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
            blinkRate = 1;
            starRate = 1;
            nextBlinkTime = Time.time + 60;
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
                blinkRate += 0.05f;
            }
        }
    }
}
