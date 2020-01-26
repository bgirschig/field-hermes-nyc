using UnityEngine;


public class gameHandler : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Application.targetFrameRate = 30;
    }

    // Update is called once per frame
    void Update() {
        var ctrl  = (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl));
        var cmd = Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        if (ctrl && Input.GetKeyDown(KeyCode.F)) {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
