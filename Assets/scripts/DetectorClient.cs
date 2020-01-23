using UnityEngine;
using System;
using UnityEngine.UI;

public enum ColorChannel { Red, Green, Blue };

public class DetectorClient : MonoBehaviour {
    DetectorStub detector;
    
    public bool invert;
    public ColorChannel detectorColorChannel;

    [NonSerialized]
    public float position;
    [NonSerialized]
    public float speed;

    bool ready;
    RollingArrayFloat prevValues;
    RollingArrayFloat prevSpeeds;

    async void Start() {
        ready = false;
        detector = new DetectorStub("localhost:8765");
        
        await detector.setCamera(0);

        prevValues = new RollingArrayFloat(5);
        prevSpeeds = new RollingArrayFloat(5);
        prevValues.Add(0);
        prevSpeeds.Add(0);

        ready = true;
    }

    async void Update() {
        if (!ready) return;
        ready = false;
        
        float prevValue = prevValues[-1];
        float rawValue = prevValue;
        try {
            rawValue = await detector.detect((int)detectorColorChannel);
            if (invert) rawValue = -rawValue;
        } catch (ArgumentNullException) {
        }
        
        prevValues.Add(rawValue);
        
        position = prevValues.average();
        prevSpeeds.Add((position - prevValue) / Time.deltaTime);
        speed = prevSpeeds.average();

        ready = true;
    }
}
