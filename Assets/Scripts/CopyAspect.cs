using UnityEngine;

[ExecuteInEditMode]
public class CopyAspect : MonoBehaviour
{
    public Camera sourceCamera;
    public Transform texturePlane;
    public float scaleFactor = 1.05f;
    float height;

    void Start()
    {
        // height = texturePlane.localScale.z;
        height = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float aspectRatio = (float)sourceCamera.pixelWidth / (float)sourceCamera.pixelHeight;
        Camera thisCamera = GetComponent<Camera>();
        thisCamera.aspect = aspectRatio;
        if (texturePlane != null)
        {
            texturePlane.localScale = new Vector3(scaleFactor * height * aspectRatio, texturePlane.localScale.y, scaleFactor * height);
        }
    }
}
