using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ScannerFollowMouse : MonoBehaviour
{
    public Transform scanner; // Reference to the UI scanner
    public Transform scannerCamera;
    public float scannerDistance = 9.35f; // Distance from the camera to the scanner
    public Vector3 originalScannerPosition; // Original position of the scanner
    public Quaternion originalScannerRotation; // Original position of the scanner
    public InputAction pointAction;

    public float grabDuration = 0.5f;

    private bool isEnabled = false;
    private bool processingGrab = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scanner == null)
        {
            Debug.LogError("Scanner reference is not set in the inspector.");
        }
        pointAction = InputSystem.actions.FindAction("Point");
        originalScannerPosition = scanner.position;
        originalScannerRotation = scanner.rotation;
    }

    public bool Enable()
    {
        if (processingGrab || isEnabled) return false;
        StartCoroutine(GrabScanner());
        return true;
    }

    public IEnumerator GrabScanner()
    {
        processingGrab = true;
        float elapsedTime = 0f;
        Vector3 startingPosition = originalScannerPosition;
        Quaternion startingRotation = originalScannerRotation;
        Quaternion goalRotation = Camera.main.transform.rotation;
        Vector3 goalPosition = GetMousePosition();
        while (elapsedTime < grabDuration)
        {
            Vector3 curPosition = scanner.position;
            Quaternion curRotation = scanner.rotation;
            scanner.position = Vector3.Lerp(curPosition, Vector3.Lerp(startingPosition, goalPosition, (elapsedTime / grabDuration)), 0.5f);
            scanner.rotation = Quaternion.Slerp(curRotation, Quaternion.Slerp(startingRotation, goalRotation, (elapsedTime / grabDuration)), 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
            goalPosition = GetMousePosition();
        }
        scanner.position = goalPosition;
        scanner.rotation = goalRotation;
        transform.Find("Screen").gameObject.SetActive(true);
        isEnabled = true;
        processingGrab = false;
    }

    public IEnumerator ReleaseScanner()
    {
        processingGrab = true;
        isEnabled = false;
        transform.Find("Screen").gameObject.SetActive(false);
        float elapsedTime = 0f;
        Vector3 startingPosition = scanner.position;
        Quaternion startingRotation = scanner.rotation;
        while (elapsedTime < grabDuration)
        {
            scanner.position = Vector3.Lerp(startingPosition, originalScannerPosition, (elapsedTime / grabDuration));
            scanner.rotation = Quaternion.Slerp(startingRotation, originalScannerRotation, (elapsedTime / grabDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scanner.position = originalScannerPosition;
        scanner.rotation = originalScannerRotation;
        processingGrab = false;
    }

    public bool Disable()
    {
        if (processingGrab || !isEnabled) return false;
        StartCoroutine(ReleaseScanner());
        return true;
    }

    public Vector3 GetMousePosition()
    {
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();
        return Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, scannerDistance));
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEnabled) return;

        Vector2 mousePosition = pointAction.ReadValue<Vector2>();

        scanner.position = GetMousePosition();
        scanner.rotation = Camera.main.transform.rotation;

        // Find positions relative to the camera so we can position the projecting camera
        var above = Vector3.Dot(scanner.position - Camera.main.transform.position, Camera.main.transform.up);
        var right = Vector3.Dot(scanner.position - Camera.main.transform.position, Camera.main.transform.right);
        // var scannerPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 9.9f));
        // mouseDistance = Vector3.Dot(scannerCamera.position - Camera.main.transform.position, Camera.main.transform.forward);
        scannerCamera.localPosition = new Vector3(right, above, scannerCamera.localPosition.z);
    }
}
