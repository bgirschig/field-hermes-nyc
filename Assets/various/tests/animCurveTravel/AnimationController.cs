using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AnimationController : MonoBehaviour
{
    public PlayableDirector director;

    // Start is called before the first frame update
    void Start() {
        // director.Pause();
    }

    // Update is called once per frame
    void Update() {
        director.time = (55 + Time.time + Mathf.Sin(Time.time * 2f)) % director.duration;
    }
}
