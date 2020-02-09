using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMapPlacer : MonoBehaviour
{
    private PathCreator pathCreator;

    [Range(0,1)]
    public float time;

    // Start is called before the first frame update
    void Start() {
        pathCreator = FindObjectOfType<PathCreator>();
        pathCreator.computePoints();
    }

    // Update is called once per frame
    void Update() {
        transform.position = pathCreator.GetPointAtTime(time);
    }
}
