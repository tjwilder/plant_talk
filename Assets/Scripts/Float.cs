using UnityEngine;

public class Float : MonoBehaviour
{
    public float duration = 1.0f; // Duration of the float effect
    public float height = 0.5f;   // Height to float
    float curHeight = 0.0f;
    // Update is called once per frame
    void Update()
    {
        transform.position -= new Vector3(0, curHeight, 0);
        curHeight = Mathf.Sin((Time.time / duration) * (Mathf.PI / 2)) * height;
        transform.position += new Vector3(0, curHeight, 0);
    }
}
