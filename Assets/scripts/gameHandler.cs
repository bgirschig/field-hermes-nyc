using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class gameHandler : MonoBehaviour {
    swingingCamera swingingCamera;

    public CinemachineVirtualCamera travellingCamera;
    public CinemachineVirtualCamera planetCamera;
    private CinemachineBrain cinemachineBrain;
    public AutoRotator planetCameraRotator;
    public GameObject pegase;
    public GameObject pegaseTarget;
    public GameObject landscape;

    private enum Mode { Orbiting, Traveling };
    private Mode currentMode = Mode.Traveling;
    float resetPlanetAt = -1;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 30;

        cinemachineBrain = GetComponent<CinemachineBrain>();
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

        switch (currentMode) {
            case Mode.Traveling:
                if (swingingCamera.distance_to_end <= 2) startOrbiting();
                if (resetPlanetAt>0 && Time.time > resetPlanetAt) {
                    pegase.transform.position = pegaseTarget.transform.position;
                    pegase.transform.rotation = pegaseTarget.transform.rotation;
                    planetCameraRotator.reset();
                    resetPlanetAt = -1;
                }
                break;
            case Mode.Orbiting:
                if (Input.GetKeyDown(KeyCode.T)) startTravelling();
                break;
        }
    }

    void startOrbiting() {
        Debug.Log("startOrbiting");
        travellingCamera.m_Priority = 0;
        planetCamera.m_Priority = 1;
        planetCameraRotator.offset.Set(0.05f, 0, 0);
        currentMode = Mode.Orbiting;
    }

    void startTravelling() {
        Debug.Log("startOrbiting");
        swingingCamera.dolly.m_PathPosition = 0;
        
        // // Move everything so that the camera goes to 0,0,0 (to avoid overflowing the coordinates floats)
        // Vector3 offset = Camera.main.transform.position;
        // GameObject[] gameObjects =
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        // foreach (GameObject gameObject in gameObjects) gameObject.transform.position -= offset;

        landscape.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 15;
        landscape.transform.rotation = Camera.main.transform.rotation;
        // landscape.transform.Rotate(0.2f * 15, 0, 0);

        travellingCamera.m_Priority = 1;
        planetCamera.m_Priority = 0;
        planetCameraRotator.offset.Set(0.2f, 0, 0);

        resetPlanetAt = Time.time + 5;
        currentMode = Mode.Traveling;
    }
}
