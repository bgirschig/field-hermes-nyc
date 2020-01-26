using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    public Vector3 offset;
    private Vector3 startOffset;
    private Vector3 startRotation;

    // Start is called before the first frame update
    void Start() {
        startRotation = transform.localRotation.eulerAngles;
        startOffset = new Vector3(offset.x, offset.y, offset.z);
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(offset);
    }

    public void reset() {
        transform.localRotation = Quaternion.Euler(startOffset);
        offset = new Vector3(startOffset.x, startOffset.y, startOffset.z);
    }
}
