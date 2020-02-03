using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingStatus : MonoBehaviour
{
    public GameObject mainSwing;
    Text text;

    float prevTime = 0;
    float prevSwingPosition = 0;
    float prevSpeed = 0;
    float speed = 0;
    float min = 0;
    float max = 0;
    int swingCount = 0;
    float dataFramerate = 0;

    float range {
        get { return max - min; }
    }

    float _pathPosition = 0;
    public float pathPosition {
        get { return _pathPosition; }
        set {
            _pathPosition = value;
            updateDebug();
        }
    }

    public float swingPosition {
        get { return prevSwingPosition; }
        set {
            float now = Time.time;
            float deltaTime = now - prevTime;
            dataFramerate = 1/deltaTime;
            speed = (prevSwingPosition - value) / deltaTime;
            
            if (prevSpeed < 0 && speed > 0) swingCount += 1;
            prevSpeed = speed;

            if (value < min) min = value;
            if (value > max) max = value;

            prevSwingPosition = value;
            prevTime = now;
            mainSwing.transform.rotation = Quaternion.Euler(0, 0, value * 30);

            updateDebug();
        }
    }

    void updateDebug() {
        text.text = string.Format(@"
<b>SWG</b> {0:+0.00;-0.00}  <b>RNG</b> {1:0.000}
<b>SPD</b> {2:+0.00;-0.00}  <b>CNT</b> {3:00000}
<b>FPS</b> {4:00.00}  <b>POS</b> {5:00.0}%
            ", swingPosition, range, speed, swingCount, dataFramerate, pathPosition*100).Trim();
    }

    // Start is called before the first frame update
    void Start() {
        text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update() {
    }
}
