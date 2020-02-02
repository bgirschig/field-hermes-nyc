using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ColorChannel { Red, Green, Blue };
public enum DetectorMode { Video, Live, SineEmulator };

public class DetectorClient : MonoBehaviour {
    DetectorStub detector;
    
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
    public float pPosition;
    [NonSerialized]
    public float speed;

    float last_detection_time = 0;
    float delayBetweenDetections = 1/25;
    RollingArrayFloat prevValues;
    RollingArrayFloat prevSpeeds;
    
    float prevValue = 0;
    float prevRawSpeed = -1;

    async void Start() {
        detector = new DetectorStub("localhost:8765");
        
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

    async void Update() {
        Boolean skipDetection = false;
        if (Time.time - last_detection_time < delayBetweenDetections) skipDetection = true;
        float deltaT = Time.time - last_detection_time;
        if (deltaT == 0) return;

        last_detection_time = Time.time;
        
        float rawValue = prevValue;

        if (!skipDetection) {
            switch (inputMode) {
                case "disabled":
                    break;
                case "emulator":
                    rawValue = Mathf.Sin(Time.time*2) * 2;
                    if (flip) rawValue = -rawValue;
                    break;
                case "detector":
                    try {
                        if (debugImage && debugImage.isActiveAndEnabled) {
                            var (value, tex) = await detector.detectWithDebug();
                            // TODO: [STABILITY] At this point, debugImage may be undefined (if the
                            // game was stopped while detect() was running), but this serves as a
                            // good reminder that we need to have deadlines on detections
                            debugImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.width / tex.height;
                            debugImage.sprite = Sprite.Create(
                                tex,
                                new Rect(0, 0, tex.width, tex.height),
                                new Vector2(0.5f, 0.5f));
                            rawValue = value;
                        } else {
                            rawValue = await detector.detect();
                        }
                        if (flip) rawValue = -rawValue;
                    } catch (ArgumentNullException) {
                    } catch (StubException e) {
                        Debug.LogException(e);
                    }
                    break;
            }
            rawValue *= influence;
        
            // Discard outlier points. Use previous value instead
            float rawSpeed = Mathf.Abs((rawValue - prevValue) / deltaT);
            float acceleration = rawSpeed - prevRawSpeed / deltaT;
            if (prevRawSpeed > 0 && rawSpeed - prevRawSpeed > 3) {
                // TODO [QUALITY] reduce false negative rate
                // TODO [QUALITY] predict value at that time from previous speed & acceleration.
                rawValue = prevValue;
            } else {
                // pass
            }
            prevRawSpeed = rawSpeed;
        }

        prevValues.Add(rawValue);
        position = prevValues.average();

        prevSpeeds.Add((position - prevValue) / deltaT);
        speed = prevSpeeds.average();
        
        prevValue = rawValue;
    }

    public async void selectInput(int id) {
        switch (inputOptions[id]) {
            case "disabled":
            case "emulator":
                inputMode = inputOptions[id];
                break;
            case "video":
                inputMode = "detector";
                await detector.setCamera("emulator");
                break;
            default:
                inputMode = "detector";
                await detector.setCamera(id);
                break;
        }
    }

    public void selectInput(string name) {
        int index = inputOptions.IndexOf(name);
        if (index < 0) index = 0;
        selectInput(index);
    }
}
