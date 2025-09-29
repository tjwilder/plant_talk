using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    public enum ToolType
    {
        None,
        Scanner,
        Nitrogen,
    }

    public Transform plant;

    public GameState currentState = GameState.Playing;
    public ToolType currentTool = ToolType.Scanner;

    public Vector2 mouseStartPosition;
    public Quaternion plantStartRotation;

    private InputAction clickAction;
    private InputAction pointAction;
    private InputAction unselectToolAction;
    private InputAction selectScannerAction;
    private InputAction selectNitrogenAction;

    public GameObject scanner;
    public GameObject nitrogen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var inputActionAsset = InputSystem.actions;
        pointAction = inputActionAsset.FindAction("Point");
        clickAction = inputActionAsset.FindAction("Click");
        unselectToolAction = inputActionAsset.FindAction("UnselectTool");
        selectScannerAction = inputActionAsset.FindAction("SelectScanner");
        selectNitrogenAction = inputActionAsset.FindAction("SelectNitrogen");
        if (unselectToolAction == null || selectScannerAction == null || selectNitrogenAction == null)
        {
            Debug.LogError("One or more input actions not found. Please check the Input Action Asset.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == GameState.Playing)
        {
            if (unselectToolAction.WasPerformedThisFrame() && currentTool != ToolType.None)
            {
                DisableAll();
                currentTool = ToolType.None;
                Debug.Log("Unselected Tool");
            }
            if (selectScannerAction.WasPerformedThisFrame() && currentTool != ToolType.Scanner)
            {
                DisableAll();
                currentTool = ToolType.Scanner;
                scanner.SetActive(true);
                Debug.Log("Selected Tool: " + currentTool);
            }
            if (selectNitrogenAction.WasPerformedThisFrame() && currentTool != ToolType.Nitrogen)
            {
                currentTool = ToolType.Nitrogen;
                Debug.Log("Selected Tool: " + currentTool);
            }

            if (clickAction.WasPerformedThisFrame())
            {
                mouseStartPosition = pointAction.ReadValue<Vector2>();
                plantStartRotation = plant.rotation;
                Debug.Log("Mouse Clicked at: " + mouseStartPosition);
            }

            if (clickAction.ReadValue<float>() > 0)
            {
                if (currentTool == ToolType.None)
                {
                    var mousePosition = pointAction.ReadValue<Vector2>();
                    plant.rotation = plantStartRotation * Quaternion.Euler(mousePosition.y - mouseStartPosition.y, mousePosition.x - mouseStartPosition.x, 0);
                }
            }
        }
        else if (currentState == GameState.Paused)
        {
        }
    }

    public void DisableAll()
    {
        scanner.SetActive(false);
    }
}
