using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ColorChannel { Red, Green, Blue };
public enum DetectorMode { Video, Live, SineEmulator };

public class DetectorClient : MonoBehaviour {
    DetectorStub stub;
    
    public bool flip;
    public float influence = 1;
    public bool allow_concurrent_detections = false;
    public ColorChannel detectorColorChannel;
    public DetectorMode detectorMode = DetectorMode.Live;
    public Image debugImage;

    public List<string> inputOptions;
    string inputMode;

    [NonSerialized]
    public float position;
    [NonSerialized]
    public float detector_value;
    [NonSerialized]
    public float pPosition;
    [NonSerialized]
    public float speed;

    float last_detection_time = 0;
    float delayBetweenDetections = 1/25;
    RollingArrayFloat prevValues;
    RollingArrayFloat prevSpeeds;
    
    float prevRawValue = 0;
    float detectedValue = 0;
    float detectedSpeed = 0;

    bool _debug;
    public bool debug {
        get { return _debug; }
        set { stub.sendAction<bool>("setDebug", value); }
    }
    bool _active;
    public bool active {
        get { return _active; }
        set { stub.sendAction<bool>("setActive", value); }
    }

    void Start() {
        stub = new DetectorStub("localhost:9000");
        stub.onValue += onValue;

        prevValues = new RollingArrayFloat(5);
        prevSpeeds = new RollingArrayFloat(5);
        prevValues.fill(0);
        prevSpeeds.fill(0);

        WebCamDevice[] devices = WebCamTexture.devices;
        inputOptions = new List<string>();
        for (int i = 0; i < devices.Length; i++) inputOptions.Add(devices[i].name);
        inputOptions.Add("emulator");
        inputOptions.Add("video");
        inputOptions.Add("disabled");
    }

    void Update() {
        stub.update();

        float rawValue = 0;
        float rawSpeed = 0;
        switch (inputMode) {
            case "disabled":
                rawValue = 0;
                rawSpeed = 0;
                break;
            case "emulator":
                rawValue = Mathf.Sin(Time.time*2f) * 0.5f;
                rawSpeed = (rawValue - prevRawValue) / Time.deltaTime;
                break;
            case "detector":
                rawValue = detectedValue;
                rawSpeed = detectedSpeed;
                break;
        }
        prevRawValue = rawValue;
        detector_value = rawValue;

        position = rawValue * influence * (flip ? -1 : 1);
        speed = rawSpeed * influence * (flip ? -1 : 1);
    }

    public void onValue(object sender, OnValueEvent evt) {
        float deltaT = evt.time - last_detection_time;
        last_detection_time = evt.time;
        if (deltaT == 0 || deltaT < 0) return;

        detectedSpeed = (evt.value - detectedValue) / deltaT;
        detectedValue = evt.value;
    }

    public void selectInput(int id) {
        switch (inputOptions[id]) {
            case "disabled":
            case "emulator":
                inputMode = inputOptions[id];
                break;
            case "video":
                inputMode = "detector";
                stub.sendAction<string>("setCamera", "emulator");
                break;
            default:
                inputMode = "detector";
                stub.sendAction<int>("setCamera", id);
                break;
        }
    }

    public void selectInput(string name) {
        int index = inputOptions.IndexOf(name);
        if (index < 0) index = 0;
        selectInput(index);
    }

    void OnApplicationQuit() {
        stub.destroy();
    }
    void OnDestroy() {
        stub.destroy();
    }
}
