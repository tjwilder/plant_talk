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

    public InputAction pointAction;
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
                var mousePosition = pointAction.ReadValue<Vector2>();
                mouseStartPosition = mousePosition;
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

            if (currentTool == ToolType.None)
            {
                var mousePosition = pointAction.ReadValue<Vector2>();
                var camPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
                plant.rotation = Quaternion.Euler(mousePosition.y, mousePosition.x, 0);
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
