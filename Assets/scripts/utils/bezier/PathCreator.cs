using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour {
    [HideInInspector]
    public Path path;
    Vector2[] precomputedPoints;
    
    [Range(0,1)]
    public float timeOffset = 0.837f;

    public void CreatePath() {
        path = new Path(transform.position);
    }

    public void computePoints(float spacing=0.1f, float resolution=1f) {
        precomputedPoints = path.CalcualteEvenlySpacedPoints(spacing, resolution);
    }

    public Vector2 GetPointAtTime(float time) {
        time = (time+timeOffset)%1;
        if (time >= 1) return transform.TransformPoint(precomputedPoints[precomputedPoints.Length - 1]);
        if (time <= 0) return transform.TransformPoint(precomputedPoints[0]);
        int segmentStartIndex = Mathf.FloorToInt((precomputedPoints.Length - 1) * time);
        int segmentEndIndex = segmentStartIndex + 1;
        float interval = 1f / precomputedPoints.Length;

        float segmentStartTime = segmentStartIndex * interval;
        float localTime = (time - segmentStartTime) / interval;

        Vector2 localPoint = Vector2.Lerp(precomputedPoints[segmentStartIndex], precomputedPoints[segmentEndIndex], localTime);
        return transform.TransformPoint(localPoint);
    }
}
