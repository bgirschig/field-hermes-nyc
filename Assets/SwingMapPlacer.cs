using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwingMapPlacer : MonoBehaviour
{
    private PathCreator pathCreator;
    private Text label;

    [Range(0,1)]
    public float time;
    public string displayName {
        get { return label.text; }
        set { label.text = value; }
    }

    // Start is called before the first frame update
    void Start() {
        pathCreator = FindObjectOfType<PathCreator>();
        pathCreator.computePoints();
        label = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update() {
        transform.position = pathCreator.GetPointAtTime(time);
    }
}
