using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class swingingCamera : MonoBehaviour
{
    public DetectorClient detectorClient;

    public AnimationCurve swingInfluence;
    public AnimationCurve travelSpeed;

    CinemachineVirtualCamera camera;
    [HideInInspector]
    public CinemachineTrackedDolly dolly;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<CinemachineVirtualCamera>();
        dolly = camera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    // Update is called once per frame
    void Update() {
        float swing_speed = detectorClient.speed;

        float swingInfluence_now = swingInfluence.Evaluate(dolly.m_PathPosition - dolly.m_Path.PathLength);
        float travelspeed_now = travelSpeed.Evaluate(dolly.m_PathPosition - dolly.m_Path.PathLength);

        if (detectorClient.speed > 0) dolly.m_PathPosition -= detectorClient.speed * 1;
        else dolly.m_PathPosition -= detectorClient.speed * 2;
    }

    public float distance_to_end {
        get { return Mathf.Max(0, dolly.m_Path.PathLength - dolly.m_PathPosition); }
    }

    public float normalized_position {
        get { return dolly.m_PathPosition / dolly.m_Path.PathLength; }
    }
}
