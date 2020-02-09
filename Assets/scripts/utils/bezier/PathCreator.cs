using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour {
    [HideInInspector]
    public Path path;
    Vector2[] precomputedPoints;

    public void CreatePath() {
        path = new Path(transform.position);
    }

    public void computePoints() {
        precomputedPoints = path.CalcualteEvenlySpacedPoints(0.1f, 1);
    }

    public Vector2 GetPointAtTime(float time) {
        if (time >= 1) return transform.TransformPoint(precomputedPoints[precomputedPoints.Length - 1]);
        int segmentStartIndex = Mathf.FloorToInt(precomputedPoints.Length * time);
        int segmentEndIndex = segmentStartIndex + 1;
        float interval = 1f / precomputedPoints.Length;

        float segmentStartTime = segmentStartIndex * interval;
        float localTime = (time - segmentStartTime) / interval;

        Vector2 localPoint = Vector2.Lerp(precomputedPoints[segmentStartIndex], precomputedPoints[segmentEndIndex], localTime);
        return transform.TransformPoint(localPoint);
    }
}
