using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SignalLevel
{
    public PlantProperty property;
    public int levelRequired;
}

[System.Serializable]
public class LevelHint
{
    public int deadPlants;
    public string hint;
}

[System.Serializable]
public class DialogueLine
{
    public string lineId;
    public string characterName;
    public string line;
}

[System.Serializable]
public class LevelDialogue
{
    public List<string> startDialogue;
    public List<string> endDialogue;
    public List<LevelHint> hints;
    public string labNotebookText;
}

[System.Serializable]
public class EndDialogue
{
    public int maxAttempts;
    public List<string> dialogue;
    public string title;
}

[System.Serializable]
public class LevelPlant
{
    public int levelNumber;
    public GameObject plantObject;
}

public class LevelManager : MonoBehaviour
{
    public GameController gameController;
    public Dictionary<string, DialogueLine> localization;
    public int levelNumber = 0;
    private bool levelCompleted = false;
    public int levelAttempts = 0;
    // For future work, we will use a list, but for now there's only one plant
    public bool revealAllSignals = false;
    public SignalLevel[] signalLevels;
    public List<LevelDialogue> levelDialogues;
    public List<LevelPlant> levelPlants;
    public List<EndDialogue> endDialogues;

    public Transform deadPlantAnchor;
    public float spacing = 3f;

    private bool step = false;
    private Coroutine hintCoroutine = null;

    public void Start()
    {
        LoadLocalization();
        SetupLevel();
    }

    public void LoadLocalizationText(string text)
    {
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {

            string[] parts = line.Split(new[] { '\t' }, StringSplitOptions.None);
            if (parts.Length >= 7)
            {
                string speaker = parts[1].Trim();
                string dialogue = parts[2].Trim();
                string key = parts[6].Trim();
                if (key == "" || dialogue == "" || key == "ID")
                    continue;
                if (localization.ContainsKey(key))
                {
                    Debug.LogError("Loaded key " + key + " Multiple times; overwriting with most recent");
                }
                localization[key] = new DialogueLine
                {
                    characterName = speaker,
                    lineId = key,
                    line = dialogue,
                };
            }
        }
        Debug.Log("Loaded " + localization.Count + " localization entries.");
    }

    public void LoadLocalization()
    {
        // Loads a .tsv (Tab Separated Values) file from Resources and parses it into the localization dictionary
        // But Unity has issues with .tsv files, so we save it as .txt
        localization = new Dictionary<string, DialogueLine>();
        var localizationFile = Resources.Load<TextAsset>("Narrative Design - RobotMessages");
        if (localizationFile == null)
        {
            Debug.LogError("Localization file not found!");
            return;
        }
        LoadLocalizationText(localizationFile.text);
        localizationFile = Resources.Load<TextAsset>("Narrative Design - RobotHints");
        if (localizationFile == null)
        {
            Debug.LogError("Localization file not found!");
            return;
        }
        LoadLocalizationText(localizationFile.text);
    }

    public DialogueLine GetDialogue(string lineId)
    {
        if (localization.ContainsKey(lineId))
        {
            return localization[lineId];
        }
        return null;
    }

    public void Update()
    {
        if (levelCompleted) return;
        if (gameController.activePlant == null) return;
        if (gameController.activePlant.GetComponent<Plant>().AllHealthy())
        {
            LevelResult(true);
        }
        else if (gameController.activePlant.GetComponent<Plant>().AnyDead())
        {
            LevelResult(false);
        }
    }

    public bool LevelResult(bool success)
    {
        levelAttempts++;
        if (success)
        {
            levelCompleted = true;
            Debug.Log("Completed level " + levelNumber);
            gameController.activePlant.transform.Find("Plant Container/Plant Models/Unhealthy").gameObject.SetActive(false);
            gameController.activePlant.transform.Find("Plant Container/Plant Models/Healthy").gameObject.SetActive(true);
            gameController.StartDialogue(GetDialogue, levelDialogues[levelNumber].endDialogue, () =>
            {
                Debug.Log("Finalized level " + levelNumber);
                // If it's the last level, we'll add the last screen
                if (levelNumber == levelDialogues.Count - 1)
                {
                    foreach (var endDialogue in endDialogues)
                    {
                        if (levelAttempts <= endDialogue.maxAttempts)
                        {
                            // Say congratulations!
                            gameController.StartDialogue(GetDialogue, endDialogue.dialogue, () =>
                            {
                                // Show end screen
                                // Set title based on levelAttempts
                                // gameController.ShowEndScreen(endDialogue.title);
                            });
                            break;
                        }
                    }

                    return;
                }
                levelNumber++;
                levelAttempts = 0;
                gameController.poof.enabled = true;
                gameController.poof.Play();
                GameObject.Destroy(gameController.activePlant);
                SetupLevel(false);
            });
            return true;
        }

        gameController.activePlant.transform.Find("Plant Container/Plant Models/Scanner").gameObject.SetActive(false);
        gameController.activePlant.transform.Find("Plant Container/Plant Models/Unhealthy").gameObject.SetActive(false);
        gameController.activePlant.transform.Find("Plant Container/Plant Models/Dead").gameObject.SetActive(true);
        gameController.activePlant.transform.Find("Signals").gameObject.SetActive(false);
        Debug.Log("Failed level " + levelNumber);
        levelCompleted = true;
        StartCoroutine(MoveDeadPlant(gameController.activePlant, 1.0f, 1.0f, () =>
        {
            // TODO: possibly remove the dialogue
            SetupLevel(true);
        }));
        return false;
    }

    public void SetupLevel(bool isReset = false)
    {
        levelCompleted = false;

        foreach (var levelPlant in levelPlants)
        {
            levelPlant.plantObject.SetActive(false);
            if (levelPlant.levelNumber == levelNumber)
            {
                // Instantiate here so we make a copy and can reuse it
                gameController.activePlant = Instantiate(levelPlant.plantObject);
                gameController.activePlant.SetActive(true);
                gameController.activePlant.transform.Find("Plant Container/Plant Models/Unhealthy").gameObject.SetActive(true);
                gameController.activePlant.transform.Find("Plant Container/Plant Models/Healthy").gameObject.SetActive(false);
                gameController.activePlant.transform.Find("Plant Container/Plant Models/Dead").gameObject.SetActive(false);
            }
        }
        PositionDeadPlants();
        if (!isReset)
        {
            // For level 3 we also need to help setup the new
            gameController.WriteToLabNotebook(levelDialogues[levelNumber].labNotebookText, () =>
            {
                gameController.StartDialogue(GetDialogue, levelDialogues[levelNumber].startDialogue);
            });
        }
        else
        {
            foreach (var hint in levelDialogues[levelNumber].hints)
            {
                if (levelAttempts == hint.deadPlants)
                {
                    gameController.StartDialogue(GetDialogue, new List<string> { hint.hint });
                    break;
                }
            }
        }
    }

    public IEnumerator MoveDeadPlant(GameObject deadPlant, float deadDuration = 1.0f, float moveDuration = 1.0f, Action callback = null)
    {
        gameController.deadPlants.Add(deadPlant);
        float startX = deadPlantAnchor.position.x;
        float startY = deadPlantAnchor.position.y;
        float startZ = deadPlantAnchor.position.z;
        Vector3 startPos = deadPlant.transform.position;
        Vector3 endPos = new Vector3(startX + (gameController.deadPlants.Count - 1) * spacing, startY, startZ);
        float elapsed = 0f;
        while (elapsed < deadDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            deadPlant.transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        deadPlant.transform.position = endPos;
        callback?.Invoke();
    }

    public void PositionDeadPlants()
    {
        float startX = deadPlantAnchor.position.x;
        float startY = deadPlantAnchor.position.y;
        float startZ = deadPlantAnchor.position.z;
        for (int i = 0; i < gameController.deadPlants.Count; i++)
        {
            if (gameController.deadPlants[i] != null)
            {
                gameController.deadPlants[i].transform.position = new Vector3(startX + i * spacing, startY, startZ);
            }
        }
    }

    public bool IsSignalDiscovered(PlantProperty property)
    {
        if (revealAllSignals) return true;
        foreach (SignalLevel signalLevel in signalLevels)
        {
            if (signalLevel.property != property)
                continue;
            return levelNumber >= signalLevel.levelRequired;
        }
        Debug.LogWarning("Signal is not monitored but cannot be discovered: " + property);
        return revealAllSignals;
    }

    public float hintDelay = 5f;

    public void HintStep()
    {
        step = true;
    }

    public void CancelHint()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
    }

    public void StartHintChain1a()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintChain1a());
    }

    public void StartHintChain1b()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintChain1b());
    }

    public void StartHintChain2b()
    {
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintChain2b());
    }

    private IEnumerator HintChain1a()
    {
        yield return gameController.PlayHint(GetDialogue("h.1.a.1"));
        float elapsed = 0.0f;
        while (elapsed < hintDelay)
        {
            elapsed += Time.deltaTime;
            if (gameController.didScan)
            {
                yield return gameController.PlayHint(GetDialogue("h.1.a.4"));
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
            yield return null;
        }
        yield return gameController.PlayHint(GetDialogue("h.1.a.2"));
        elapsed = 0.0f;
        while (elapsed < hintDelay)
        {
            elapsed += Time.deltaTime;
            if (gameController.didScan)
            {
                yield return gameController.PlayHint(GetDialogue("h.1.a.4"));
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
            yield return null;
        }
        yield return gameController.PlayHint(GetDialogue("h.1.a.3"));
        while (true)
        {
            if (gameController.didScan)
            {
                yield return gameController.PlayHint(GetDialogue("h.1.a.4"));
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator HintChain1b()
    {
        yield return gameController.PlayHint(GetDialogue("h.1.b.1"));
        var messages = new List<string> { "h.1.b.2", "h.1.b.3", "h.1.b.4", "h.1.b.5" };
        var ind = 0;
        while (true)
        {
            if (gameController.addedNitrogen)
            {
                yield return gameController.PlayHint(GetDialogue("h.1.b.6"));
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
            if (gameController.addedNutrient && ind < 4)
            {
                gameController.addedNutrient = false;
                yield return gameController.PlayHint(GetDialogue(messages[ind++]));
            }
            yield return null;
        }
    }

    private IEnumerator HintChain2b()
    {
        gameController.didScanSecondary = false;
        while (true)
        {
            if (gameController.didScanSecondary)
            {
                yield return gameController.PlayHint(GetDialogue("h.2.b.1"));
                yield return new WaitForSeconds(1.0f);
                yield return gameController.PlayHint(GetDialogue("h.2.b.2"));
                yield break;
            }
            yield return null;
        }
    }

    // TODO: Handle random hints for non-action or adding the wrong chemical
    // Maybe just treat them as "Level 3" things for now since there's existing hints in the first two
}
