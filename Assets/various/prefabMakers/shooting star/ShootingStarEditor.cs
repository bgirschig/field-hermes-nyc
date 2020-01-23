#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ShootingStar))]
public class ShootingStarEditor : Editor {
    
    private ShootingStar c;
        
    public void OnSceneGUI() {
        c = this.target as ShootingStar;

        float radius = c.transform.GetChild(0).localPosition.magnitude;
        Handles.color = Color.red;
        Handles.DrawWireDisc(
            c.transform.position,
            c.transform.forward,
            radius
        );

        float a1 = c.angleStart * Mathf.Deg2Rad;
        float a2 = c.angleEnd * Mathf.Deg2Rad;
        Vector3[] segments = {
            c.transform.position,
            c.transform.position + new Vector3(Mathf.Cos(a1)*radius, Mathf.Sin(a1)*radius,0),

            c.transform.position,
            c.transform.position + new Vector3(Mathf.Cos(a2)*radius, Mathf.Sin(a2)*radius,0),
        };
        Handles.DrawLines(segments);
    }
}

#endif