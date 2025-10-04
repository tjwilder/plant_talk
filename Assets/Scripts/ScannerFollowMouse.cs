using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScannerFollowMouse : MonoBehaviour
{
    public Transform scanner; // Reference to the UI scanner
    public Transform scannerCamera;
    public InputAction pointAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scanner == null)
        {
            Debug.LogError("Scanner reference is not set in the inspector.");
        }
        pointAction = InputSystem.actions.FindAction("Point");
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(pointAction);
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();
        var camPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, scanner.position.z - Camera.main.transform.position.z));
        // Debug.Log(mousePosition);
        // Debug.Log(camPoint);
        // camPoint.x *= 10;
        // camPoint.y *= 10;
        camPoint.z = scanner.position.z; // Keep the original z position
        scanner.position = camPoint;

        // var scannerPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 9.9f));
        var scannerPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, scannerCamera.position.z - Camera.main.transform.position.z));
        scannerCamera.localPosition = new Vector3(scannerPoint.x, scannerPoint.y, scannerCamera.localPosition.z);
    }
}
