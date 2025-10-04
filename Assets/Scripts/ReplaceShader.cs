using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ReplaceShader : MonoBehaviour
{
    // public Shader overrideShader;
    public Material overrideMaterial;
    public RenderTexture output;
    void Start()
    {
        // var thisCamera = GetComponent<Camera>();
        // thisCamera.SetReplacementShader(overrideShader, "");
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Debug.Log("OnRenderImage called");
        if (overrideMaterial != null)
        {
            Graphics.Blit(src, dest, overrideMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
    void Update()
    {
        // var thisCamera = GetComponent<Camera>();
        // // Check if there is a current camera rendering
        // if (thisCamera != null)
        // {
        //     // Assign the output render texture to the current camera
        //     thisCamera.targetTexture = output;
        //     // 
        //     // Render the scene by replacing all the "RenderType" shaders with the overrideShader
        //     thisCamera.RenderWithShader(overrideShader, "");
        //     // 
        //     // Set the output render texture back to null to avoid side effects.
        //     thisCamera.targetTexture = null;
        // }
        // else
        // {
        //     Debug.LogWarning("No current camera available for rendering.");
        // }
    }
}
