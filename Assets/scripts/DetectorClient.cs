using UnityEngine;
using System;
using UnityEngine.UI;

public enum ColorChannel { Red, Green, Blue };
public enum DetectorMode { Video, Live, SineEmulator };

public class DetectorClient : MonoBehaviour {
    DetectorStub detector;
    
    public bool invert;
    public bool allow_concurrent_detections = false;
    public ColorChannel detectorColorChannel;
    public DetectorMode detectorMode = DetectorMode.Live;
    public AnimationCurve graph;

    [NonSerialized]
    public float position;
    [NonSerialized]
    public float speed;

    bool currently_detecting;
    RollingArrayFloat prevValues;
    RollingArrayFloat prevSpeeds;


    async void Start() {
        detector = new DetectorStub("localhost:8765");
        graph = new AnimationCurve();
        
        switch (detectorMode) {
            case DetectorMode.Live:
                await detector.setCamera(0);
                break;
            case DetectorMode.Video:
                await detector.setCamera("emulator");
                break;
        }

        prevValues = new RollingArrayFloat(5);
        prevSpeeds = new RollingArrayFloat(5);
        prevValues.Add(0);
        prevSpeeds.Add(0);

    }

    async void Update() {
        if (currently_detecting && !allow_concurrent_detections) return;
        currently_detecting = true;
        
        float prevValue = prevValues[-1];
        float rawValue = prevValue;

        switch (detectorMode) {
            case DetectorMode.Live:
            case DetectorMode.Video:
                try {
                    rawValue = await detector.detect((int)detectorColorChannel);
                    if (invert) rawValue = -rawValue;
                } catch (ArgumentNullException) { }
                break;
            case DetectorMode.SineEmulator:
                rawValue = Mathf.Sin(Time.time*2) * 2;
                if (invert) rawValue = -rawValue;
                break;
        }
        prevValues.Add(rawValue);
        
        position = prevValues.average();
        prevSpeeds.Add((position - prevValue) / Time.deltaTime);
        speed = prevSpeeds.average();
        graph.AddKey(Time.time, speed);

        currently_detecting = false;
    }

    
}
