using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class gameHandler : MonoBehaviour {
    GameObject UI;

    // Start is called before the first frame update
    IEnumerator Start() {
        Application.targetFrameRate = 30;
        UI = FindObjectOfType<Canvas>().gameObject;

        yield return 0;
        UI.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        var ctrl  = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
        var maj = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if ((maj || ctrl) && Input.GetKeyDown(KeyCode.F)) StartCoroutine(setFullscreen(!Screen.fullScreen));
        if (ctrl && Input.GetKeyDown(KeyCode.M)) UI.SetActive(!UI.activeSelf);
        if (ctrl && Input.GetKeyDown(KeyCode.C)) SceneManager.LoadScene("ControlRoom");
    }

    private IEnumerator setFullscreen(bool fullscreen) {
        Screen.fullScreen = fullscreen;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Screen.SetResolution(
            Display.main.systemWidth,
            Display.main.systemHeight,
            Screen.fullScreen
        );
    }
}
