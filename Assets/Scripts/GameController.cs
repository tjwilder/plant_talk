using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;
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
    public GameObject hintBox;
    public GameObject nutrientInfoBox;
    public GameObject labNotebook;
    public GameObject testTubeRack;
    public GameObject expandedTestTubeRack;

    public AudioSource audioSource;
    public VisualEffect poof;

    public List<PlantProperty> nutrientOrder = new List<PlantProperty> { PlantProperty.Nitrogen, PlantProperty.Iron, PlantProperty.Potassium, PlantProperty.Magnesium, };
    public List<PlantProperty> expandedNutrientOrder = new List<PlantProperty> { PlantProperty.Nitrogen, PlantProperty.Iron, PlantProperty.Potassium, PlantProperty.Magnesium, PlantProperty.Phosphorus, PlantProperty.Calcium, PlantProperty.Sulfur, PlantProperty.Zinc, PlantProperty.Boron, PlantProperty.Copper };
    public Dictionary<PlantProperty, string> nutrientCards = new Dictionary<PlantProperty, string>
    {
        [PlantProperty.Nitrogen] = "Used for photosynthesis! Help your plants stay green!",
        [PlantProperty.Iron] = "Helps other nutrients work their best!",
        [PlantProperty.Potassium] = "Got a wilty plant? Helps plants use water!",
        [PlantProperty.Phosphorus] = "How your plants energize! Check for deficiency on the bottom of leaves! ",
        [PlantProperty.Magnesium] = "Helps guide other nutrients into the plant!",
        [PlantProperty.Calcium] = "Makes new cells grow big and strong!",
        [PlantProperty.Sulfur] = "Protein power! Helps the plant make nutritious fruit!",
        [PlantProperty.Zinc] = "Helps plants produce sugar!",
        [PlantProperty.Boron] = "Got a plant with weak leaves? Keeps plants strong!",
        [PlantProperty.Copper] = "Helps the plant feed its fruit!",
    };

    private int currentNutrientIndex = 2;
    private bool rotatingNutrients = false;
    private bool stopFlashing = false;
    public bool addedNitrogen = false;
    public bool addedNutrient = false;
    public bool didScan = false;
    public bool didScanSecondary = false;

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
                SwitchTool(ToolType.None);
                currentTool = ToolType.None;
                Debug.Log("Unselected Tool");
            }
            if (selectScannerAction.WasPerformedThisFrame() && currentTool != ToolType.Scanner)
            {
                SwitchTool(ToolType.Scanner);
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
                // Did they click on the scanner?
                var ray = Camera.main.ScreenPointToRay(pointAction.ReadValue<Vector2>());
                RaycastHit hit;
                bool didHit = false;
                if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Default")))
                {
                    Debug.Log("Hit: " + hit.transform.gameObject.name);
                    if (hit.transform.gameObject.name == "Scanner Shelf")
                    {
                        didHit = true;
                        SwitchTool(ToolType.None);
                        Debug.Log("Selected Tool: " + currentTool);
                    }
                }
                if (!didHit && Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.IsChildOf(scanner.transform))
                    {
                        SwitchTool(ToolType.Scanner);
                        Debug.Log("Selected Tool: " + currentTool);
                    }
                }
                mouseStartPosition = pointAction.ReadValue<Vector2>();
                plantStartRotation = activePlant.transform.rotation;
                Debug.Log("Mouse Clicked at: " + mouseStartPosition);
            }

            if (currentTool == ToolType.Scanner)
            {
                var mousePosition = pointAction.ReadValue<Vector2>();
                // Debug.Log(mousePosition);
                // Check to see if it's in the middle 1/3 and top 1/4 of the screen
                if (mousePosition.x > Screen.width / 3 && mousePosition.x < Screen.width * 2 / 3 &&
                    mousePosition.y > Screen.height * 3 / 4)
                {
                    didScan = true;
                }
                // Default signal is on the middle part of the screen
                if (mousePosition.x > Screen.width / 3 && mousePosition.x < Screen.width * 2 / 3 &&
                    mousePosition.y > Screen.height / 3 && mousePosition.y < Screen.height / 3)
                {
                    didScanSecondary = true;
                }
            }

            // If you're holding down the click button
            if (clickAction.ReadValue<float>() > 0)
            {
                // If you don't have a tool and you've unlocked rotation
                if (currentTool == ToolType.None && levelManager.levelNumber > 2)
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
                        if (nutrientOrder[currentNutrientIndex] == PlantProperty.Nitrogen)
                        {
                            addedNitrogen = true;
                        }
                        addedNutrient = true;
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
                // Hide the nutrient info during transition
                nutrientInfoBox.SetActive(false);
                StartCoroutine(LerpRotation(testTubeRack.transform, Quaternion.Euler(testTubeRack.transform.eulerAngles.x, targetAngle, testTubeRack.transform.eulerAngles.z), 0.5f, () =>
                {
                    rotatingNutrients = false;
                    // If we've unlocked it, add the nutrient info
                    if (levelManager.levelNumber >= 1)
                    {
                        nutrientInfoBox.SetActive(true);
                        var newLoc = GetNutrientInfoLocation();
                        nutrientInfoBox.transform.position = newLoc;

                        nutrientInfoBox.transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = targetProperty.ToString();
                        nutrientInfoBox.transform.Find("Dialogue").GetComponent<TMPro.TextMeshProUGUI>().text = nutrientCards[targetProperty];
                    }
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

    public void SwitchTool(ToolType tool)
    {
        if (currentTool == tool) return;
        currentTool = tool;
        var scannerComponent = scanner.GetComponent<ScannerFollowMouse>();
        if (currentTool == ToolType.Scanner)
        {
            stopFlashing = true;
            scannerComponent.Enable();
        }
        else
        {
            scannerComponent.Disable();
        }
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
                }
                else if (msgId == "highlightChemicals")
                {
                    // HighlightChemicals();
                }
                else if (msgId == "waitForAddNitrogen")
                {
                    addedNitrogen = false;

                    currentState = GameState.Playing;
                    while (!addedNitrogen)
                    {
                        yield return null;
                    }
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
                }
                else if (msgId == "waitForScan")
                {
                    currentState = GameState.Playing;
                    // TODO: This should maybe be a more general action, but can only handle the FIRST SCAN AT DEFAULT ROTATION for now
                    didScan = false;
                    while (true)
                    {
                        if (didScan)
                            break;
                        yield return null;
                    }
                }
                else if (msgId == "waitForSecondaryScan")
                {
                    currentState = GameState.Playing;
                    // TODO: This should maybe be a more general action, but can only handle the FIRST SCAN AT DEFAULT ROTATION for now
                    didScanSecondary = false;
                    while (true)
                    {
                        if (didScanSecondary)
                            break;
                        yield return null;
                    }
                }
                else if (msgId == "startGame")
                {
                    levelManager.levelNumber = 1;
                    levelManager.SetupLevel();
                }
                else if (msgId == "h.1.a")
                {
                    levelManager.StartHintChain1a();
                }
                else if (msgId == "h.1.b")
                {
                    levelManager.StartHintChain1b();
                }
                else if (msgId == "h.2.b")
                {
                    levelManager.StartHintChain2b();
                }
                else
                {
                    Debug.LogWarning("Dialogue line not found: " + msgId);
                }
            }
            else
            {
                dialogueBox.SetActive(true);
                // TODO: character image here or delete the image
                var character = dialogueBox.transform.Find("Text_Box/Character").GetComponent<TMPro.TextMeshProUGUI>();
                var dialogue = dialogueBox.transform.Find("Text_Box/Dialogue").GetComponent<TMPro.TextMeshProUGUI>();
                character.text = msg.characterName;
                dialogue.text = msg.line;
                // Make sure we always wait at least one frame to continue
                yield return null;
                while (!clickAction.WasPerformedThisFrame())
                {
                    yield return null;
                }
                dialogueBox.SetActive(false);
            }
        }
        dialogueBox.SetActive(false);
        currentState = GameState.Playing;
        callback?.Invoke();
    }

    public Vector2 GetNutrientInfoLocation()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(testTubeRack.transform.position);
        // Move it up a bit
        screenPoint.y += 100;
        // Clamp to the screen
        screenPoint.x = Mathf.Clamp(screenPoint.x, 100, Screen.width - 100);
        screenPoint.y = Mathf.Clamp(screenPoint.y, 100, Screen.height - 100);
        return screenPoint;
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
        StartCoroutine(FlashColor(renderer, Color.yellow, 1.0f, 3));
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
        StartCoroutine(FlashPanelColor(renderer, Color.yellow, 1.0f, 2));
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

    public float hintCharacterDelay = 0.02f;
    public float hintDuration = 5.0f;
    private Coroutine hintPlaying = null;
    private Coroutine disposeHint = null;

    public Coroutine PlayHint(DialogueLine hint)
    {
        if (hintPlaying != null)
        {
            StopCoroutine(hintPlaying);
        }
        if (disposeHint != null)
        {
            StopCoroutine(disposeHint);
            disposeHint = null;
        }
        return StartCoroutine(PlayHintInternal(hint));
    }

    public IEnumerator PlayHintInternal(DialogueLine hint)
    {
        var message = "";
        hintBox.SetActive(true);
        var hintText = hintBox.transform.Find("Dialogue").GetComponent<TMPro.TextMeshProUGUI>();
        foreach (var c in hint.line)
        {
            message += c;
            hintText.text = message;
            yield return new WaitForSeconds(hintCharacterDelay);
        }
        hintPlaying = null;
        // Start the destruction coroutine but don't wait for it in case we want to play another hint
        disposeHint = StartCoroutine(DisposeHint());
    }

    public IEnumerator DisposeHint()
    {
        yield return new WaitForSeconds(hintDuration);
        hintBox.SetActive(false);
    }

    public void SetupExpandedNutrients()
    {
        nutrientOrder = expandedNutrientOrder;
        currentNutrientIndex = 0; // Start on Phosphorus
        testTubeRack.SetActive(false);
        testTubeRack = expandedTestTubeRack;
        testTubeRack.SetActive(true);
        var targetAngle = testTubeRack.transform.eulerAngles.y + (90 * (currentNutrientIndex - 2));
        testTubeRack.transform.rotation = Quaternion.Euler(testTubeRack.transform.eulerAngles.x, targetAngle, testTubeRack.transform.eulerAngles.z);
        nutrientInfoBox.SetActive(true);
        var newLoc = GetNutrientInfoLocation();
        nutrientInfoBox.transform.position = newLoc;
        var targetProperty = nutrientOrder[currentNutrientIndex];
        nutrientInfoBox.transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = targetProperty.ToString();
        nutrientInfoBox.transform.Find("Dialogue").GetComponent<TMPro.TextMeshProUGUI>().text = nutrientCards[targetProperty];
    }
}
