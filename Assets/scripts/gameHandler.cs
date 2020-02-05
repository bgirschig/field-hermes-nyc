using UnityEngine;
using UnityEngine.SceneManagement;

public class gameHandler : MonoBehaviour {
    GameObject UI;

    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 30;
        UI = FindObjectOfType<Canvas>().gameObject;
    }

    // Update is called once per frame
    void Update() {
        var ctrl  = (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl));
        var cmd = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        
        if (Input.GetKeyDown(KeyCode.F)) {
            Screen.fullScreen = !Screen.fullScreen;
            if (Screen.fullScreen) {
                Screen.SetResolution(
                    Screen.currentResolution.width,
                    Screen.currentResolution.height,
                    FullScreenMode.FullScreenWindow,
                    60);
            } else {
                Screen.SetResolution(
                    Screen.currentResolution.width,
                    Screen.currentResolution.height,
                    false
                );
            }
        }
        if (Input.GetKeyDown(KeyCode.M)) UI.SetActive(!UI.activeSelf);
        if (Input.GetKeyDown(KeyCode.C)) SceneManager.LoadScene("ControlRoom");
    }
}
