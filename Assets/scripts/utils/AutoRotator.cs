using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    public Vector3 offset;
    private Vector3 startOffset;
    private Vector3 startRotation;
    public DetectorClient detectorClient;

    public bool active = false;

    // Start is called before the first frame update
    void Start() {
        startRotation = transform.localRotation.eulerAngles;
        startOffset = new Vector3(offset.x, offset.y, offset.z);
    }

    // Update is called once per frame
    void Update() {
        if (active) {
            if (detectorClient.speed > 0) transform.Rotate(-detectorClient.speed * 0.02f, 0, 0);
            else transform.Rotate(-detectorClient.speed * 0.06f, 0, 0);
        }
        // transform.Rotate(offset);
    }

    public void reset() {
        Debug.Log(startRotation);
        Debug.Log(startOffset);
        transform.localRotation = Quaternion.Euler(startOffset);
        offset = new Vector3(startOffset.x, startOffset.y, startOffset.z);
    }
}
