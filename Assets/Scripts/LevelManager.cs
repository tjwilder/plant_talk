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
public class LevelDialogue
{
    public List<string> startDialogue;
    public List<string> endDialogue;
    public List<LevelHint> hints;
    public string labNotebookText;

    // TODO: Localization for the text here
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
    public int levelNumber = 1;
    private bool levelCompleted = false;
    public int levelAttempts = 0;
    // For future work, we will use a list, but for now there's only one plant
    public bool revealAllSignals = false;
    public SignalLevel[] signalLevels;
    public List<LevelDialogue> levelDialogues;
    public List<LevelPlant> levelPlants;

    public void Start()
    {
        SetupLevel();
    }

    public void Update()
    {
        if (levelCompleted) return;
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
            gameController.StartDialogue(levelDialogues[levelNumber - 1].endDialogue, () =>
            {
                Debug.Log("Finalized level " + levelNumber);
                levelNumber++;
                levelAttempts = 0;
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
            gameController.WriteToLabNotebook(levelDialogues[levelNumber - 1].labNotebookText, () =>
            {
                gameController.StartDialogue(levelDialogues[levelNumber - 1].startDialogue);
            });
        }
        else
        {
            foreach (var hint in levelDialogues[levelNumber - 1].hints)
            {
                if (levelAttempts == hint.deadPlants)
                {
                    gameController.StartDialogue(new List<string> { hint.hint });
                    break;
                }
            }
        }
    }

    public IEnumerator MoveDeadPlant(GameObject deadPlant, float deadDuration = 1.0f, float moveDuration = 1.0f, Action callback = null)
    {
        gameController.deadPlants.Add(deadPlant);
        float startX = -25f;
        float spacing = 3f;
        Vector3 startPos = deadPlant.transform.position;
        Vector3 endPos = new Vector3(startX + (gameController.deadPlants.Count - 1) * spacing, 0, 20);
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
        float startX = -25f;
        float spacing = 3f;
        for (int i = 0; i < gameController.deadPlants.Count; i++)
        {
            if (gameController.deadPlants[i] != null)
            {
                gameController.deadPlants[i].transform.position = new Vector3(startX + i * spacing, 0, 20);
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
}
