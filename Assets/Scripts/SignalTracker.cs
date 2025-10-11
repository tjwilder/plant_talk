using UnityEngine;
using System.Collections.Generic;

public enum SignalType
{
    Positive,
    Negative
}

// [ExecuteInEditMode]
public class SignalTracker : MonoBehaviour
{
    public PlantProperty property = PlantProperty.Nitrogen;
    private LevelManager levelManager;
    private Plant plant;
    private Material[] originalShaders;
    public Material negativeShader;
    private Transform positiveSignal = null;
    private Transform negativeSignal = null;

    private MeshRenderer meshRenderer;

    private bool isNegativeShader = false;

    public List<int> materialIndices;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        plant = transform.parent.parent.GetComponent<Plant>();
        meshRenderer = plant.gameObject.transform.Find("Plant Container/Plant Models/Scanner").GetComponent<MeshRenderer>();
        originalShaders = meshRenderer.materials;
        positiveSignal = transform.Find("PositiveSignal");
        negativeSignal = transform.Find("NegativeSignal");
    }

    // Update is called once per frame
    void Update()
    {
        var health = plant.GetNutrientHealth(property);
        var isPositive = health == PropertyHealth.Healthy;
        if (isPositive)
        {
            // var shouldBark = levelManager.IsSignalDiscovered(property);
            positiveSignal?.gameObject.SetActive(true);
            negativeSignal?.gameObject.SetActive(false);
            if (negativeShader != null && isNegativeShader)
            {
                isNegativeShader = false;
                Debug.Log("Switching to original shader\nIf this is not working, it's possible the original instance we're copying already changed materials before copying", this);
                meshRenderer.materials = originalShaders;
            }
        }
        else
        {
            positiveSignal?.gameObject.SetActive(false);
            negativeSignal?.gameObject.SetActive(true);
            if (negativeShader != null && !isNegativeShader)
            {
                isNegativeShader = true;
                Debug.Log("Switching to new shader", this);
                var materials = meshRenderer.materials;
                foreach (var index in materialIndices)
                {
                    materials[index] = negativeShader;
                }
                meshRenderer.materials = materials;
            }
        }
    }
}
