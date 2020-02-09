using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path {
    [SerializeField, HideInInspector]
    List<Vector2> points;

    public Path(Vector2 center) {
        points = new List<Vector2>() {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right,
        };
    }

    public Vector2 this[int index] {
        get { return points[index]; }
    }

    public int numPoints {
        get { return points.Count; }
    }

    public int numSegment {
        get { return (points.Count - 4) / 3 + 1; }
    }

    public void addSegment(Vector2 anchorPos) {
        points.Add(points[points.Count-1]*2 - points[points.Count-2]);
        points.Add((points[points.Count-1] + anchorPos) * .5f);
        points.Add(anchorPos);
    }

    public Vector2[] GetPointInSegment(int i) {
        return new Vector2[] {
            points[i*3], points[i*3+1], points[i*3+2], points[i*3+3]
        };
    }

    public void movePoint(int i, Vector2 newPosition) {
        Vector2 deltaMove = newPosition - points[i];
        points[i] = newPosition;

        if (i%3 == 0) {
            if (i+1 < points.Count) points[i + 1] += deltaMove;
            if (i-1 >= 0) points[i - 1] += deltaMove;
        } else {
            bool nextPointIsAnchor = (i+1) % 3 == 0;
            int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
            int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count) {
                float dist = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                Vector2 direction = (points[anchorIndex] - newPosition).normalized;

                points[correspondingControlIndex] = points[anchorIndex] + direction * dist;
            }
        }
    }

    public Vector2[] CalcualteEvenlySpacedPoints(float spacing, float resolution = 1) {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < numSegment; segmentIndex++) {
            Vector2[] p = GetPointInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[1]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1) {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);  
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing) {
                    float overshootDistance = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDistance;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        return evenlySpacedPoints.ToArray();
    }

}
