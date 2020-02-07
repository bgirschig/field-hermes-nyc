using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MessageProtos;

public class SwingStatus : MonoBehaviour
{
    public GameObject mainSwing;
    Text text;

    float prevSpeed = 0;
    float swingCount = 0;
    float min = 0;
    float max = 0;
    SwingState current_state;

    float range {
        get { return max - min; }
    }

    public void updateState(SwingState state) {
        current_state = state;

        if (float.IsInfinity(state.swingSpeed)) return;
        if (float.IsInfinity(state.swingPosition)) return;

        if (prevSpeed < 0 && state.swingSpeed > 0) swingCount += 1;
        prevSpeed = state.swingSpeed;

        if (state.swingPosition < min) min = state.swingPosition;
        if (state.swingPosition > max) max = state.swingPosition;

        string textFormat = @"
<b>SWG</b> {0:+0.00;-0.00}  <b>RNG</b> {1:0.000}  <b>SPD</b> {2:+00.0;-00.0}
<b>CNT</b> {3:00000}  <b>FPS</b> {4:00.00}  <b>POS</b> {5:00.0}%
            ";

        mainSwing.transform.rotation = Quaternion.Euler(0, 0, state.swingPosition * 25);
        text.text = string.Format(textFormat,
            state.swingPosition,
            range,
            state.swingSpeed,
            swingCount,
            state.fps,
            state.pathPosition*100
        ).Trim();
    }

    // Start is called before the first frame update
    void Start() {
        text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update() {
    }
}
