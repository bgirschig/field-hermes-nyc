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
    public Image debugImage;

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

        switch (detectorMode) {
            case DetectorMode.Live:
                await detector.setCamera(0);
                break;
            case DetectorMode.Video:
                await detector.setCamera("emulator");
                break;
        }
    }

    async void Update() {
        Boolean skipDetection = false;
        if (Time.time - last_detection_time < delayBetweenDetections) skipDetection = true;
        float deltaT = Time.time - last_detection_time;
        if (deltaT == 0) return;

        last_detection_time = Time.time;
        
        float rawValue = prevValue;

        if (!skipDetection) {
            switch (detectorMode) {
                case DetectorMode.Live:
                case DetectorMode.Video:
                    try {
                        if (debugImage) {
                            var (value, tex) = await detector.detectWithDebug();
                            debugImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.width / tex.height;
                            debugImage.sprite = Sprite.Create(
                                tex,
                                new Rect(0, 0, tex.width, tex.height),
                                new Vector2(0.5f, 0.5f));
                            rawValue = value;
                        } else {
                            rawValue = await detector.detect();
                        }
                        if (invert) rawValue = -rawValue;
                    } catch (ArgumentNullException) {}
                    break;
                case DetectorMode.SineEmulator:
                    rawValue = Mathf.Sin(Time.time*2) * 2;
                    if (invert) rawValue = -rawValue;
                    break;
            }
        
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
}
