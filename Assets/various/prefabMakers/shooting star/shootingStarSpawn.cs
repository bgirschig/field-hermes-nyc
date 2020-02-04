using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootingStarSpawn : MonoBehaviour
{
    public GameObject[] prefabs;
    Vector2 intersection1;
    Vector2 intersection2;
    float frustum_radius;
    public float spawn_interval = 15;

    // Distance from camera to spawned star
    float spawn_distance = 90;
    float next_spawn_time = 0;

    // Start is called before the first frame update
    void Start() {
        spawn();
        next_spawn_time = getNextSpawnTime();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.S)) spawn();
        if (Time.time > next_spawn_time) {
            spawn();
            next_spawn_time = getNextSpawnTime();
        }
    }

    // Finds out when the next shooting star should be spawned (based on spawn_interval, with some
    // randomness)
    float getNextSpawnTime() {
        float spawn_min_interval = spawn_interval * 0.666f;
        float spawn_max_interval = spawn_interval * 1.333f;
        return Time.time + Random.Range(spawn_min_interval, spawn_min_interval);
    }

    public void setSpawnInterval(float value) {
        spawn_interval = value;
        next_spawn_time = getNextSpawnTime();
    }

    public void spawn() {
        // Instantiate the star
        int prefabIndex = Random.Range(0, prefabs.Length);
        var instance = GameObject.Instantiate(prefabs[prefabIndex]);
        instance.transform.parent = transform;

        // What point does the star rotate around. This is expressed as an angle:
        // where that angle "intersects" with the camera bounds, at the given distance
        float spawn_origin_angle = Random.Range(0, Mathf.PI * 2);

        // Find out the size of the circle visible to the camera at 'spawn_distance' from it
        float frustumHeight = 2.0f * spawn_distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * Camera.main.aspect;
        frustum_radius = Mathf.Min(frustumHeight, frustumWidth) / 2.0f;

        // From the radius and spawn angle, we can compute the actual position for the rotation axis
        Vector3 spawn_position = new Vector3(
            Mathf.Cos(spawn_origin_angle) * frustum_radius,
            Mathf.Sin(spawn_origin_angle) * frustum_radius,
            spawn_distance
        );
        instance.transform.localPosition = spawn_position;

        // Find out how much to rotate the shooting star so that it sits right on the edge of the viewport
        FindCircleCircleIntersections(
            new Vector2(), frustum_radius,
            spawn_position, instance.transform.GetChild(0).localPosition.magnitude,
            out intersection1, out intersection2);
        // The intersection of the two circles (camera view and shooting star path) produces 2
        // points, but we are only interested in the second one (Taking the first point would put
        // the comet's tail inside the viewport, moving outside)
        Vector2 delta2 = intersection2 - (Vector2)spawn_position;
        float angle2 = Mathf.Atan2(delta2.y, delta2.x);
        instance.transform.localRotation = Quaternion.Euler(0, 0, angle2 * Mathf.Rad2Deg);
    }

    // Find the points where the two circles intersect.
    private int FindCircleCircleIntersections(
        Vector2 c0, float radius0,
        Vector2 c1, float radius1,
        out Vector2 intersection1, out Vector2 intersection2) {

        // Find the distance between the centers.
        float dx = c0.x - c1.x;
        float dy = c0.y - c1.y;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1) {
            // No solutions, the circles are too far apart.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        } else if (dist < Mathf.Abs(radius0 - radius1)) {
            // No solutions, one circle contains the other.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        } else if ((dist == 0) && (radius0 == radius1)) {
            // No solutions, the circles coincide.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        } else {
            // Find a and h.
            float a = (radius0 * radius0 -
                radius1 * radius1 + dist * dist) / (2 * dist);
            float h = Mathf.Sqrt(radius0 * radius0 - a * a);

            // Find P2.
            
            Vector2 c2 = new Vector2(
                c0.x + a * (c1.x - c0.x) / dist,
                c0.y + a * (c1.y - c0.y) / dist
            );

            // Get the points P3.
            intersection1 = new Vector2(
                (c2.x + h * (c1.y - c0.y) / dist),
                (c2.y - h * (c1.x - c0.x) / dist));
            intersection2 = new Vector2(
                (c2.x - h * (c1.y - c0.y) / dist),
                (c2.y + h * (c1.x - c0.x) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return 1;
            return 2;
        }
    }
}
