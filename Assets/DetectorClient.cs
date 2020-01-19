using UnityEngine;
using System;

public class DetectorClient : MonoBehaviour {
    DetectorStub detector;
    
    [Range(-1, 1)]
    public float value;
    
    async void Start() {
        detector = new DetectorStub("localhost:8765");
        await detector.setCamera("emulator");
        Texture tex = await detector.getMask();
    }

    async void Update() {
        try {
            value = await detector.detect();
        } catch (ArgumentNullException) {
            Debug.Log("Value is null");
            throw;
        }
    }
}
