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

        // "origin" line
        Handles.DrawLine(
            c.transform.position,
            c.transform.position + new Vector3(radius, 0, 0)
        );
    }
}

#endif