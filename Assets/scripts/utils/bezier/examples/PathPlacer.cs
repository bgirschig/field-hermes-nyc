using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour {

    PathCreator pathCreator;
    GameObject g;
    
    [Range(0,1)]
    public float time;
	
	void Start () {
        pathCreator = FindObjectOfType<PathCreator>();
        pathCreator.computePoints();

        g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.SetParent(pathCreator.transform, false);
	}

    void Update() {
        g.transform.position = pathCreator.GetPointAtTime(time);
    }
}