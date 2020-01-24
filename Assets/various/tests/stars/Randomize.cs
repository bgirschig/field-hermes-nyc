using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomize : MonoBehaviour
{
    public bool go = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (go) {
            go = false;
            randomizeTransform();
        }
    }

    void randomizeTransform() {
        transform.position = new Vector3 (
            Random.Range(0, 300),
            Random.Range(0, 300),
            Random.Range(0, 300)
        );
        transform.rotation = Quaternion.Euler(
            Random.Range(0, 360),
            Random.Range(0, 360),
            Random.Range(0, 360)
        );
    }
}
