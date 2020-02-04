using UnityEngine;

public class stars : MonoBehaviour {
  public int starCount = 1000;

  private ParticleSystem.Particle[] points;
  private ParticleSystem star_particleSystem;

  private float minX;
  private float maxX;
  private float minY;
  private float maxY;
  private float minZ;
  private float maxZ;
  private float frustumHeight;
  private float frustumWidth;
  private float frustumDepth;

  public void UpdateCameraParams() {
    frustumHeight = 2.0f * Camera.main.farClipPlane * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
    frustumWidth = frustumHeight * Camera.main.aspect;
    frustumDepth = Camera.main.farClipPlane - Camera.main.nearClipPlane;

    minX = -frustumWidth / 2f;
    maxX =  frustumWidth / 2f;
    minY = -frustumHeight / 2f;
    maxY =  frustumHeight / 2f;
    minZ = -Camera.main.farClipPlane;
    maxZ = -Camera.main.nearClipPlane;
  }

  private void Create() {
    points = new ParticleSystem.Particle[starCount];

    for (int i = 0; i < starCount; i++) {
      points[i].position = Camera.main.cameraToWorldMatrix.MultiplyPoint(new Vector3(
        Random.Range(minX, maxX),
        Random.Range(minY, maxY),
        Random.Range(minZ, maxZ)
      ));

      points[i].startSize = Random.Range(1f, 5f);
      points[i].startColor = new Color(1, 1, 1, 1);
      points[i].rotation = Random.Range(0, 90);
    }

    star_particleSystem = gameObject.GetComponent<ParticleSystem>();
    star_particleSystem.SetParticles(points, points.Length);
  }

  void Start() {
    UpdateCameraParams();
    Create();
    // FitStars();
  }

  void FitStars() {
    // Loop particles around so that we can have a small number of particles "following" the
    // camera. We're using camera-space position rather than viewport positions because looping
    // particles from front to back using this technique creates artefacts (particles get
    // centered on the camera's z axis when going backward)
    // Camera-space positions allow for a much more consistent behaviour (at the cost of having
    // some stars "wasted" out of view)

    Vector3 relativePos;

    for (int i = 0; i < starCount; i++) {      
      relativePos = Camera.main.worldToCameraMatrix.MultiplyPoint(points[i].position);

      if (relativePos.x < minX) relativePos.x += frustumWidth;
      if (relativePos.x > maxX) relativePos.x -= frustumWidth;

      if (relativePos.y < minY) relativePos.y += frustumHeight;
      if (relativePos.y > maxY) relativePos.y -= frustumHeight;

      if (relativePos.z < minZ) relativePos.z += frustumDepth;
      if (relativePos.z > maxZ) relativePos.z -= frustumDepth;

      points[i].position = Camera.main.cameraToWorldMatrix.MultiplyPoint(relativePos);  
    }
    star_particleSystem.SetParticles(points, points.Length);
  }

  void Update() {
    FitStars();
  }

  float Map(float val, float from1, float from2, float to1, float to2) {
    return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
  }
  float Map(float val, float from1, float from2, float to1, float to2, bool clamp) {
    val = Map(val, from1, from2, to1, to2);
    if (clamp) val = Mathf.Clamp(val, to1, to2);
    return val;
  }

  public void setStarCount(int count) {
    starCount = count;
    Create();
  }
}