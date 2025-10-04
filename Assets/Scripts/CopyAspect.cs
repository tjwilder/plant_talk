using UnityEngine;

[ExecuteInEditMode]
public class CopyAspect : MonoBehaviour
{
    public Camera sourceCamera;
    public Transform texturePlane;

    // Update is called once per frame
    void Update()
    {
        float aspectRatio = (float)sourceCamera.pixelWidth / (float)sourceCamera.pixelHeight;
        Camera thisCamera = GetComponent<Camera>();
        thisCamera.aspect = aspectRatio;
        if (texturePlane != null)
        {
            float height = texturePlane.localScale.y;
            texturePlane.localScale = new Vector3(height * aspectRatio, height, texturePlane.localScale.z);
        }
    }
}
