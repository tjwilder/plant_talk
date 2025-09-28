using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ScannerFollowMouse : MonoBehaviour
{
    public Transform scanner; // Reference to the UI scanner
    public Transform possibleTarget;
    public InputAction pointAction;

    private List<GameObject> children = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scanner == null)
        {
            Debug.LogError("Scanner reference is not set in the inspector.");
        }
        pointAction = InputSystem.actions.FindAction("Point");
        bool first = true;
        foreach (Transform child in possibleTarget)
        {
            if (first)
            {
                first = false;
                continue;
            }
            children.Add(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(pointAction);
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();
        var camPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, scanner.position.z - Camera.main.transform.position.z));
        // Debug.Log(mousePosition);
        // Debug.Log(camPoint);
        // camPoint.x *= 10;
        // camPoint.y *= 10;
        camPoint.z = scanner.position.z; // Keep the original z position
        scanner.position = camPoint;
        // EnableInRange(mousePosition);
    }

    // public void EnableInRange(Vector2 mousePos)
    // {
    //     foreach (GameObject child in children)
    //     {
    //         var camPoint = Camera.main.WorldToScreenPoint(child.transform.position);
    //         // var worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
    //         // Debug.Log(Vector2.Distance(child.transform.position, worldPoint));
    //         if (camPoint.x < mousePos.x + scanner.rect.width / 2 &&
    //             camPoint.x > mousePos.x - scanner.rect.width / 2 &&
    //             camPoint.y < mousePos.y + scanner.rect.height / 2 &&
    //             camPoint.y > mousePos.y - scanner.rect.height / 2)
    //         {
    //             child.SetActive(true);
    //         }
    //         else
    //         {
    //             child.SetActive(false);
    //         }
    //     }
    // }
}
