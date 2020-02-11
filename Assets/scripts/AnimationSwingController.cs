using UnityEngine;
using UnityEngine.Playables;

public class AnimationSwingController : MonoBehaviour
{
    public PlayableDirector director;
    public DetectorClient detectorClient;
    [Range(0, 1)]
    public float startPosition;

    private double time;
    public float autoAdvanceSpeed = 0.2f;
    public bool fast_forward = false;
    
    private float currentVelocity = 0;
    private double targetTime = 0;

    // Start is called before the first frame update
    void Start() {
        time = startPosition * director.duration;
        targetTime = startPosition * director.duration;
    }

    // Update is called once per frame
    void Update() {
        // auto-advance
        targetTime += autoAdvanceSpeed * Time.deltaTime;
        if (fast_forward) targetTime += 5f * Time.deltaTime;

        if (detectorClient.speed > 0) targetTime += detectorClient.speed * 0.02;
        else targetTime += detectorClient.speed * 0.01;
        targetTime = targetTime % director.duration;
        if (targetTime < 0) targetTime += director.duration;

        time = Mathf.SmoothDamp((float)director.time, (float)targetTime, ref currentVelocity, 0.3f);
        director.time = targetTime;
    }
}
