using UnityEngine;

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
    private Material originalShader;
    public Material negativeShader;
    private Transform positiveSignal;
    private Transform negativeSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        plant = transform.parent.parent.GetComponent<Plant>();
        originalShader = plant.gameObject.transform.Find("Plant Container/Plant Models/Scanner").GetComponent<MeshRenderer>().materials[1];
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
            if (negativeShader != null)
            {
                var meshRenderer = plant.gameObject.transform.Find("Plant Container/Plant Models/Scanner").GetComponent<MeshRenderer>();
                var materials = meshRenderer.sharedMaterials;
                if (materials[1] == originalShader) return;
                materials[1] = originalShader;
                meshRenderer.sharedMaterials = materials;
            }
        }
        else
        {
            positiveSignal?.gameObject.SetActive(false);
            negativeSignal?.gameObject.SetActive(true);
            if (negativeShader != null)
            {
                var meshRenderer = plant.gameObject.transform.Find("Plant Container/Plant Models/Scanner").GetComponent<MeshRenderer>();
                var materials = meshRenderer.sharedMaterials;
                if (materials[1] == negativeShader) return;
                materials[1] = negativeShader;
                meshRenderer.sharedMaterials = materials;
            }
        }
    }
}
