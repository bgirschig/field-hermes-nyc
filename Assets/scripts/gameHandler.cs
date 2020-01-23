using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class gameHandler : MonoBehaviour
{
    swingingCamera swingingCamera;

    public CinemachineVirtualCamera travellingCamera;
    public CinemachineVirtualCamera planetCamera;
    public AutoRotator planetCameraRotator;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 30;

        swingingCamera = travellingCamera.gameObject.GetComponent<swingingCamera>();

        travellingCamera.m_Priority = 1;
        planetCamera.m_Priority = 0;
    }

    // Update is called once per frame
    void Update() {
        var ctrl  = (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl));
        var cmd = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        if (ctrl && Input.GetKeyDown(KeyCode.F)) {
            Screen.fullScreen = !Screen.fullScreen;
            Debug.Log("fullscreen");
        }

        if (swingingCamera.distance_to_end <= 2) {
            travellingCamera.m_Priority = 0;
            planetCamera.m_Priority = 1;
            planetCameraRotator.offset.Set(0.1f, 0, 0);
        }
    }
}
