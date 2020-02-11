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
    float maxBlinkRate = 15;
    int nextColorGroup = 0;

    // Start is called before the first frame update
    void Start() {
    }

    public void toggle(bool with_stars=true) {
        if (active) {
            stop();
        } else {
            active = true;
            if (with_stars) {
                starRate = 1;
            } else {
                starRate = 0;
            }
            blinkRate = 5;
            nextBlinkTime = Time.time + 73.8f;
            nextStarTime = Time.time + 1/starRate;
            stopTime = Time.time + 85;
            nextColorGroup = controller.colorGroupIndex + 1;
        }
    }

    public void stop() {
        active = false;
        controller.colorGroupIndex = nextColorGroup;
    }

    // Update is called once per frame
    void Update() {
        var ctrl  = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
        if (ctrl && Input.GetKeyDown(KeyCode.E)) toggle(false);

        if (active) {
            if (Time.time > nextStarTime) {
                shootingStarController.spawn();
                nextStarTime = Time.time + 1/starRate;
                starRate += 0.1f;
            }
            if (Time.time > nextBlinkTime) {
                controller.colorGroupIndex += 1;
                nextBlinkTime = Time.time + 1/blinkRate;
                blinkRate = Mathf.Min(maxBlinkRate, blinkRate + 1f);
            }
            if (Time.time > stopTime) stop();
        }
    }
}
