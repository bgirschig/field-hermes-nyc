using UnityEngine;
using UnityEngine.UI;
using MessageProtos;
using Newtonsoft.Json;
using System.Collections;

public class SwingStatus : MonoBehaviour
{
    public int swing_id = -1;
    public GameObject mainSwing;
    public string displayName;
    public Text nameLabel;
    Text text;

    float prevSpeed = 0;
    float swingCount = 0;
    float min = 0;
    float max = 0;
    SwingState current_state;
    ControlRoom controlRoom;
    SwingMapPlacer mapCursor;

    float swingPositionOffset = 0;
    float lastReceivedPosition = 0;

    float range {
        get { return max - min; }
    }

    public void updateState(SwingState state) {
        current_state = state;
        lastReceivedPosition = state.swingPosition;
        state.swingPosition -= swingPositionOffset;

        if (float.IsInfinity(state.swingSpeed)) return;
        if (float.IsInfinity(state.swingPosition)) return;

        if (prevSpeed < 0 && state.swingSpeed > 0 && Mathf.Abs(state.swingPosition - (float)swingPositionOffset) > 0.1) swingCount += 1;
        prevSpeed = state.swingSpeed;

        if (state.swingPosition < min) min = state.swingPosition;
        if (state.swingPosition > max) max = state.swingPosition;

        string textFormat = @"
<b>SWG</b> {0:+0.00;-0.00}  <b>RNG</b> {1:0.000}  <b>SPD</b> {2:+00.0;-00.0}
<b>CNT</b> {3:00000}  <b>FPS</b> {4:00.00}  <b>POS</b> {5:00.0}%
            ";

        mainSwing.transform.rotation = Quaternion.Euler(0, 0, state.swingPosition * 90);
        text.text = string.Format(textFormat,
            state.swingPosition,
            range,
            state.swingSpeed,
            swingCount,
            state.fps,
            state.pathPosition*100
        ).Trim();
        
        mapCursor.time = state.pathPosition;
    }

    public void eureka() {
        for (int id = 0; id < controlRoom.swing_count; id++) {
            var message = new SwingControl();
            message.swing_id = id;
            if (id == swing_id) message.action = "eureka";
            else message.action = "eureka_dry";
            controlRoom.send(JsonConvert.SerializeObject(message));
        }
    }

    public void calibrate() {
        swingPositionOffset = lastReceivedPosition;
    }

    public void sendControl(string action) {
        var message = new SwingControl();
        message.swing_id = swing_id;
        message.action = action;
        controlRoom.send(JsonConvert.SerializeObject(message));
    }

    // Start is called before the first frame update
    IEnumerator Start() {
        swing_id = transform.GetSiblingIndex();

        text = GetComponentInChildren<Text>();
        controlRoom = FindObjectOfType<ControlRoom>();
        mapCursor = FindObjectOfType<PathCreator>().transform.GetComponentsInChildren<SwingMapPlacer>()[swing_id];
        
        yield return 0;

        mapCursor.displayName = displayName;
        nameLabel.text = displayName;
    }
}
