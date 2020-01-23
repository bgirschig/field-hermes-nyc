﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class swingingCamera : MonoBehaviour
{
    public DetectorClient detectorClient;

    public AnimationCurve swingInfluence;
    public AnimationCurve travelSpeed;

    CinemachineVirtualCamera camera;
    CinemachineTrackedDolly dolly;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<CinemachineVirtualCamera>();
        dolly = camera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    // Update is called once per frame
    void Update() {
        // float swing_position = Mathf.Sin(Time.time*1.5f);
        float swing_speed = detectorClient.speed;

        float swingInfluence_now = swingInfluence.Evaluate(dolly.m_PathPosition - dolly.m_Path.PathLength);
        float travelspeed_now = travelSpeed.Evaluate(dolly.m_PathPosition - dolly.m_Path.PathLength);

        // always move forward a little bit
        dolly.m_PathPosition += travelspeed_now;

        float offset = swing_speed * swingInfluence_now;
        if (swing_speed > 0) dolly.m_PathPosition += offset * 20;
        else dolly.m_PathPosition += offset * 10;
    }

    public float distance_to_end {
        get { return dolly.m_Path.PathLength - dolly.m_PathPosition; }
    }

    public float normalized_position {
        get { return dolly.m_PathPosition / dolly.m_Path.PathLength; }
    }
}
