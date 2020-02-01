using UnityEngine;
using UnityEngine.Playables;

public class AnimationSwingController : MonoBehaviour
{
    public PlayableDirector director;
    public DetectorClient detectorClient;
    [Range(0, 1)]
    public float startPosition;

    private double time;

    // Start is called before the first frame update
    void Start() {
        time = startPosition * director.duration;
    }

    // Update is called once per frame
    void Update() {
        if (detectorClient.speed > 0) time += detectorClient.speed * 0.02;
        else time += detectorClient.speed * 0.01;
        time = time % director.duration;
        if (time < 0) time += director.duration;
        director.time = time;
    }
}
