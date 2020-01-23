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
        materialInstance = new Material(material);
        foreach (Renderer renderer in allRenderers) {
            if (renderer.sharedMaterial == material) renderer.sharedMaterial = materialInstance;
        }
    }

    void OnScreenSizeChanged() {
        Camera mainCamera = this.GetComponent<Camera>();

        float ratio = (float)mainCamera.pixelWidth / mainCamera.pixelHeight;
        if (ratio > 1) {
            materialInstance.SetFloat("width", 1.0f/ratio);
            materialInstance.SetFloat("height", 1.0f);
        } else {
            materialInstance.SetFloat("width", 1.0f);
            materialInstance.SetFloat("height", ratio);
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

        if (materialInstance) Graphics.Blit(source, destination, materialInstance);
        else Graphics.Blit(source, destination);
	}
}