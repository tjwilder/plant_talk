using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Dialogue,
        GameOver
    }
    public enum ToolType
    {
        None,
        Scanner,
        WateringCan,
    }

    public GameObject activePlant;
    public List<GameObject> deadPlants;
    public LevelManager levelManager;

    public GameState currentState = GameState.Playing;
    public ToolType currentTool = ToolType.Scanner;

    public Vector2 mouseStartPosition;
    public Quaternion plantStartRotation;

    private InputAction clickAction;
    private InputAction pointAction;
    private InputAction unselectToolAction;
    private InputAction selectScannerAction;
    private InputAction selectSecondAction;
    private InputAction moveAction;

    public GameObject scanner;
    public GameObject dialogueBox;
    public GameObject labNotebook;
    public GameObject testTubeRack;

    public AudioSource audioSource;

    public List<PlantProperty> nutrientOrder = new List<PlantProperty> { PlantProperty.Nitrogen, PlantProperty.Iron, PlantProperty.Potassium, PlantProperty.Magnesium };

    private int currentNutrientIndex = 2;
    private bool rotatingNutrients = false;
    private bool stopFlashing = false;
    private bool addedNitrogen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = GetComponent<LevelManager>();
        var inputActionAsset = InputSystem.actions;
        pointAction = inputActionAsset.FindAction("Point");
        clickAction = inputActionAsset.FindAction("Click");
        unselectToolAction = inputActionAsset.FindAction("UnselectTool");
        selectScannerAction = inputActionAsset.FindAction("SelectScanner");
        selectSecondAction = inputActionAsset.FindAction("SelectSecond");
        moveAction = inputActionAsset.FindAction("Move");
        if (unselectToolAction == null || selectScannerAction == null || selectSecondAction == null)
        {
            Debug.LogError("One or more input actions not found. Please check the Input Action Asset.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activePlant == null)
        {
            return;
        }
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
                var scannerComponent = scanner.GetComponent<ScannerFollowMouse>();
                scannerComponent.Enable();
                stopFlashing = true;
                Debug.Log("Selected Tool: " + currentTool);
            }
            if (selectSecondAction.WasPerformedThisFrame() && currentTool != ToolType.WateringCan)
            {
                // currentTool = ToolType.WateringCan;
                var plantComponent = activePlant.GetComponent<Plant>();
                Debug.Log("added nitrogen: " + plantComponent.TryAddNutrients(new WateringResource
                {
                    nutrients = new Dictionary<PlantProperty, int>
                    {
                        { PlantProperty.Nitrogen, 1 },
                        // { PlantProperty.Phosphorus, 1 },
                        // { PlantProperty.Potassium, 1 }
                    }
                }));
                Debug.Log("Selected Tool: " + ToolType.WateringCan);
            }

            if (clickAction.WasPerformedThisFrame())
            {
                mouseStartPosition = pointAction.ReadValue<Vector2>();
                plantStartRotation = activePlant.transform.rotation;
                Debug.Log("Mouse Clicked at: " + mouseStartPosition);
            }

            if (clickAction.ReadValue<float>() > 0)
            {
                if (currentTool == ToolType.None)
                {
                    var mousePosition = pointAction.ReadValue<Vector2>();
                    activePlant.transform.rotation = plantStartRotation * Quaternion.Euler(mousePosition.y - mouseStartPosition.y, mousePosition.x - mouseStartPosition.x, 0);
                }
            }

            var move = moveAction.ReadValue<Vector2>();
            if (move != Vector2.zero && !rotatingNutrients)
            {
                rotatingNutrients = true;
                // TODO: We could test this with a controller
                if (Mathf.Abs(move.y - 0.1f) > 0.2f)
                {
                    var testTube = testTubeRack.transform.Find(nutrientOrder[currentNutrientIndex].ToString());
                    StartCoroutine(PoofOut(testTube.transform, 2.0f, () =>
                    {
                        // testTubeRack.SetActive(!testTubeRack.activeSelf);
                        rotatingNutrients = false;
                        activePlant.GetComponent<Plant>().TryAddNutrients(new WateringResource
                        {
                            nutrients = new Dictionary<PlantProperty, int>
                            {
                                { nutrientOrder[currentNutrientIndex], 1 },
                            }
                        });
                        addedNitrogen = true;
                    }));
                    return;
                }
                var targetAngle = testTubeRack.transform.eulerAngles.y;
                if (move.x > 0)
                {
                    currentNutrientIndex = (currentNutrientIndex + 1) % nutrientOrder.Count;
                    targetAngle += 90;
                }
                else if (move.x < 0)
                {
                    currentNutrientIndex = (currentNutrientIndex - 1 + nutrientOrder.Count) % nutrientOrder.Count;
                    targetAngle -= 90;
                }
                var targetProperty = nutrientOrder[currentNutrientIndex];
                Debug.Log("Current Nutrient: " + targetProperty);
                StartCoroutine(LerpRotation(testTubeRack.transform, Quaternion.Euler(testTubeRack.transform.eulerAngles.x, targetAngle, testTubeRack.transform.eulerAngles.z), 0.5f, () =>
                {
                    rotatingNutrients = false;
                }));
            }
        }
        else if (currentState == GameState.Paused)
        {
        }
        else if (currentState == GameState.Dialogue)
        {
            // if (clickAction.WasPerformedThisFrame())
            // {
            //     currentState = GameState.Playing;
            // }
        }
    }

    public void DisableAll()
    {
        var scannerComponent = scanner.GetComponent<ScannerFollowMouse>();
        scannerComponent.Disable();
    }

    public IEnumerator PoofOut(Transform obj, float duration, Action callback = null)
    {
        Vector3 initialScale = obj.localScale;
        float elapsed = 0f;

        while (elapsed < duration / 2f)
        {
            obj.localScale = Vector3.Lerp(initialScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            obj.localScale = Vector3.Lerp(Vector3.zero, initialScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.localScale = initialScale;
        callback?.Invoke();
    }

    public IEnumerator LerpRotation(Transform obj, Quaternion targetRotation, float duration, Action callback = null)
    {
        Quaternion initialRotation = obj.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.rotation = targetRotation;
        callback?.Invoke();
    }

    public void StartDialogue(Func<string, DialogueLine> translateMessage, List<string> messages, Action callback = null)
    {
        StartCoroutine(ShowDialogue(translateMessage, messages, callback));
    }

    private IEnumerator ShowDialogue(Func<string, DialogueLine> translateMessage, List<string> messages, Action callback)
    {
        currentState = GameState.Dialogue;
        dialogueBox.SetActive(true);
        foreach (var msgId in messages)
        {
            var msg = translateMessage(msgId);
            // Skip empty messages
            if (msg == null)
            {
                if (msgId == "highlightScanner")
                {
                    HighlightScanner();
                    currentState = GameState.Playing;
                    while (!currentTool.Equals(ToolType.Scanner))
                    {
                        yield return null;
                    }
                    continue;
                }
                else if (msgId == "progressLabNotebook")
                {
                    // TODO: Actually progress the notebook
                    HighlightLabNotebook();
                    // TODO: Only play audio while adding to the notebook
                    audioSource.time = 0.1f;
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                    audioSource.Stop();
                    continue;
                }
                else if (msgId == "highlightChemicals")
                {
                    // HighlightChemicals();
                    continue;
                }
                else if (msgId == "waitForAddNitrogen")
                {
                    addedNitrogen = false;
                    currentState = GameState.Playing;
                    while (!addedNitrogen)
                    {
                        yield return null;
                    }
                    continue;
                }
                else if (msgId == "waitForAddIron")
                {
                    currentState = GameState.Playing;
                    var plantComponent = activePlant.GetComponent<Plant>();
                    var originalIron = plantComponent.GetNutrientAmount(PlantProperty.Iron);
                    while (true)
                    {
                        if (plantComponent.GetNutrientAmount(PlantProperty.Iron) > originalIron)
                        {
                            break;
                        }
                        yield return null;
                    }
                    continue;
                }
                else if (msgId == "waitForScan")
                {
                    currentState = GameState.Playing;
                    // TODO: This should maybe be a more general action, but can only handle the FIRST SCAN AT DEFAULT ROTATION for now
                    while (true)
                    {
                        var mousePosition = pointAction.ReadValue<Vector2>();
                        // Debug.Log(mousePosition);
                        // Check to see if it's in the middle 1/3 and top 1/4 of the screen
                        if (mousePosition.x > Screen.width / 3 && mousePosition.x < Screen.width * 2 / 3 &&
                            mousePosition.y > Screen.height * 3 / 4)
                        {
                            break;
                        }
                        yield return null;
                    }
                    continue;
                }
                else if (msgId == "startGame")
                {
                    levelManager.levelNumber = 1;
                    levelManager.SetupLevel();
                    continue;
                }
                Debug.LogWarning("Dialogue line not found: " + msgId);
                continue;
            }

            // TODO: character name here
            dialogueBox.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = msg.line;
            // Make sure we always wait at least one frame to continue
            yield return null;
            while (!clickAction.WasPerformedThisFrame())
            {
                yield return null;
            }
        }
        dialogueBox.SetActive(false);
        currentState = GameState.Playing;
        callback?.Invoke();
    }

    public float labWritingSpeed = 0.05f;

    public void WriteToLabNotebook(string entry, Action callback = null)
    {
        // TODO: Just handle this by swapping out picture (maybe fading between them?)
        var notebookText = labNotebook.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var prevEntry = notebookText.text;
        StartCoroutine(UpdateNotebook(prevEntry, entry, callback));
    }

    private int FindTagPosition(string noTagText, string tagText, int charIndex)
    {
        int tagIndex = 0;
        for (int i = 0; i <= charIndex; i++)
        {
            while (tagText[tagIndex] == '<')
            {
                // Skip tag
                while (tagIndex < tagText.Length && tagText[tagIndex] != '>')
                {
                    tagIndex++;
                }
                tagIndex++; // Skip '>'
            }
            tagIndex++;
        }
        tagIndex--; // Adjust for the last increment
        return tagIndex;
    }

    private string FindTag(string text, int startIndex)
    {
        if (text[startIndex] != '<')
            return null;
        startIndex = text.IndexOf('<', startIndex);
        int endIndex = text.IndexOf('>', startIndex);
        if (endIndex == -1)
            return null;
        var tag = text.Substring(startIndex + 1, endIndex - startIndex - 1);
        Debug.Log($"Found tag: {tag}");
        return tag;
    }

    public IEnumerator UpdateNotebook(string prevEntry, string entry, Action callback = null)
    {
        var notebookText = labNotebook.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var prevEntryNoTags = System.Text.RegularExpressions.Regex.Replace(prevEntry, "<.*?>", "");
        var entryNoTags = System.Text.RegularExpressions.Regex.Replace(entry, "<.*?>", "");
        int i1 = -1;
        int i2 = -1;
        int j1 = -1;
        int j2 = -1;
        int offset1 = 0;
        int offset2 = 0;
        for (; i1 < entryNoTags.Length - 1 && i2 < prevEntryNoTags.Length - 1;)
        {
            i1++;
            i2++;
            j1 = FindTagPosition(entryNoTags, entry, i1);
            j2 = FindTagPosition(prevEntryNoTags, prevEntry, i2);
            Debug.Log($"Comparing {entryNoTags[i1]} and {prevEntryNoTags[i2]} at {i1},{i2} and {j1},{j2}");
            if (entryNoTags[i1] != prevEntryNoTags[i2])
            {
                // They differ here, so write the rest of the new one
                break;
            }
            if ((i1 + offset1 != j1 || i2 + offset2 != j2) && (offset1 != offset2 || entry[j1] != prevEntry[j2]))
            {
                Debug.Log($"Tags differ at {i1},{i2} and {j1},{j2}");
                // They match here but there's a difference in tags, so write the new one until the tags match again
                var tag = FindTag(entry, j1);
                while (entry[j1] != '<')
                {
                    // Now we take the entry up to the current point and add a closing tag
                    notebookText.text = entry.Substring(0, j1) + $"</{tag}>" + prevEntry.Substring(j2);
                    yield return new WaitForSeconds(labWritingSpeed);
                    i1++;
                    j1++;
                    i2++;
                    j2++;
                    offset1 = j1 - i1;
                    offset2 = j2 - i2;
                }
            }
        }
        Debug.Log($"Finished matching {entry} ({entryNoTags}) and {prevEntry} ({prevEntryNoTags}) at {i1},{i2} and {j1},{j2}");
        if (i1 < 0) i1 = 0;
        for (; i1 < entryNoTags.Length; i1++)
        {
            j1 = FindTagPosition(entryNoTags, entry, i1);
            // Add one to the length because of the off-by-noe in the substring
            notebookText.text = entry.Substring(0, j1 + 1);
            yield return new WaitForSeconds(labWritingSpeed);
        }

        callback?.Invoke();
    }

    public void HighlightScanner()
    {
        stopFlashing = false;
        var component = scanner.transform.Find("Scanner");
        var renderer = component.GetComponent<MeshRenderer>();
        StartCoroutine(FlashColor(renderer, Color.yellow, 1.5f, 5));
    }

    public IEnumerator FlashColor(MeshRenderer renderer, Color flashColor, float duration, int times)
    {
        // The 3rd material is the one to change
        Color originalColor = renderer.materials[2].color;
        // Get the emission as well
        Color originalEmission = renderer.materials[2].GetColor("_EmissionColor");
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        for (int i = 0; i < times; i++)
        {
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (TryStopFlashing(renderer, originalColor, originalEmission))
                {
                    yield break;
                }
                renderer.materials[2].color = Color.Lerp(originalColor, flashColor, elapsed / halfDuration);
                renderer.materials[2].SetColor("_EmissionColor", Color.Lerp(originalEmission, flashColor, elapsed / halfDuration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (TryStopFlashing(renderer, originalColor, originalEmission))
                {
                    yield break;
                }
                renderer.materials[2].color = Color.Lerp(flashColor, originalColor, elapsed / halfDuration);
                renderer.materials[2].SetColor("_EmissionColor", Color.Lerp(flashColor, originalEmission, elapsed / halfDuration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            renderer.materials[2].color = originalColor;
            renderer.materials[2].SetColor("_EmissionColor", originalEmission);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void HighlightLabNotebook()
    {
        stopFlashing = false;
        var component = labNotebook.transform.parent;
        var renderer = component.GetComponent<Image>();
        StartCoroutine(FlashPanelColor(renderer, Color.yellow, 1.5f, 2));
    }

    public IEnumerator FlashPanelColor(UnityEngine.UI.Image image, Color flashColor, float duration, int times)
    {
        Color originalColor = image.color;
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        for (int i = 0; i < times; i++)
        {
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (TryStopPanelFlashing(image, originalColor))
                {
                    yield break;
                }
                image.color = Color.Lerp(originalColor, flashColor, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (TryStopPanelFlashing(image, originalColor))
                {
                    yield break;
                }
                image.color = Color.Lerp(flashColor, originalColor, elapsed / halfDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            image.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool TryStopFlashing(MeshRenderer renderer, Color originalColor, Color originalEmission)
    {
        if (stopFlashing)
        {
            stopFlashing = false;
            renderer.materials[2].color = originalColor;
            renderer.materials[2].SetColor("_EmissionColor", originalEmission);
            return true;
        }
        return false;
    }

    private bool TryStopPanelFlashing(UnityEngine.UI.Image image, Color originalColor)
    {
        if (stopFlashing)
        {
            stopFlashing = false;
            image.color = originalColor;
            return true;
        }
        return false;
    }
}
