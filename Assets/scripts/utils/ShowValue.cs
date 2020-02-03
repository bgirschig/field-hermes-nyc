// Basic script for showing a value in a text field, nicely formatted (used to display the value of
// an input slider in the ui)

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShowValue : MonoBehaviour {
    Text text;

    void Start() {
        text = GetComponent<Text>();
    }

    public void show(int value) {
        text.text = StringUtils.nice((double) value, 3);
    }
    public void show(float value) {
        text.text = StringUtils.nice((double) value, 3);
    }
}
