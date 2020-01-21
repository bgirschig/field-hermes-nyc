using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class postProcessing : MonoBehaviour {
	public Material material;
    private Material materialInstance;

    int lastScreenWidth = 0;
    int lastScreenHeight = 0;
    bool initialized = false;

    void Start() {
        var allRenderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
        var matInstance = new Material(material);
        foreach (Renderer renderer in allRenderers) {
            if (renderer.sharedMaterial == material) renderer.sharedMaterial = matInstance;
        }
    }

    void OnScreenSizeChanged() {
        Camera mainCamera = this.GetComponent<Camera>();

        float ratio = (float)mainCamera.pixelWidth / mainCamera.pixelHeight;
        if (ratio > 1) {
            material.SetFloat("width", 1.0f/ratio);
            material.SetFloat("height", 1.0f);
        } else {
            material.SetFloat("width", 1.0f);
            material.SetFloat("height", ratio);
        }
    }

	// Postprocess the image
	private void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
        if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height) {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            OnScreenSizeChanged();
        }

		Graphics.Blit(source, destination, material);
	}
}