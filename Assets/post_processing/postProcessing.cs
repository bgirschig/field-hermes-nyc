using UnityEngine;
using System.Collections;

[System.Serializable]
public struct ColorGroup {
    public Color foreground;
    public Color background;
    public Color maskColor;
}

[ExecuteInEditMode]
public class postProcessing : MonoBehaviour {
	public Material material;
    public ColorGroup[] colorGroups;

    public float offsetX {
        get { return materialInstance.GetFloat("offsetX"); }
        set { materialInstance.SetFloat("offsetX", value); }
    }
    public float offsetY {
        get { return materialInstance.GetFloat("offsetY"); }
        set { materialInstance.SetFloat("offsetY", value); }
    }
    public float scaleX {
        get { return materialInstance.GetFloat("scaleX"); }
        set { materialInstance.SetFloat("scaleX", value); }
    }
    public float scaleY {
        get { return materialInstance.GetFloat("scaleY"); }
        set { materialInstance.SetFloat("scaleY", value); }
    }

    private Material materialInstance;
    private int _colorGroupIndex = 0;

    int lastScreenWidth = 0;
    int lastScreenHeight = 0;
    bool initialized = false;

    public int colorGroupIndex {
        get { return _colorGroupIndex; }
        set {
            _colorGroupIndex = value % colorGroups.Length;
            if (!materialInstance) return;
            
            materialInstance.SetColor("foreground", colorGroups[colorGroupIndex].foreground);
            materialInstance.SetColor("background", colorGroups[colorGroupIndex].background);
            materialInstance.SetColor("maskColor", colorGroups[colorGroupIndex].maskColor);
        }
    }

    void Start() {
        var allRenderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
        materialInstance = new Material(material);
        foreach (Renderer renderer in allRenderers) {
            if (renderer.sharedMaterial == material) renderer.sharedMaterial = materialInstance;
        }
        colorGroupIndex = 0;
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