using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FollowMouse : MonoBehaviour
{
    public RectTransform panel; // Reference to the UI panel
    public Transform possibleTarget;
    public int scannerPadding;
    public InputAction pointAction;

    private List<GameObject> children = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (panel == null)
        {
            Debug.LogError("Panel reference is not set in the inspector.");
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
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();
        panel.position = mousePosition + new Vector2(-panel.rect.width / 2, -panel.rect.height / 2);
        EnableInRange(mousePosition);
    }

    public void EnableInRange(Vector2 mousePos)
    {
        foreach (GameObject child in children)
        {
            var camPoint = Camera.main.WorldToScreenPoint(child.transform.position);
            // var worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
            // Debug.Log(Vector2.Distance(child.transform.position, worldPoint));
            if (camPoint.x < mousePos.x + panel.rect.width / 2 - scannerPadding &&
                camPoint.x > mousePos.x - panel.rect.width / 2 + scannerPadding &&
                camPoint.y < mousePos.y + panel.rect.height / 2 - scannerPadding &&
                camPoint.y > mousePos.y - panel.rect.height / 2 + scannerPadding)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
    }
}
