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
    private Transform positiveSignal;
    private Transform negativeSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        plant = transform.parent.parent.GetComponent<Plant>();
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
        }
        else
        {
            positiveSignal?.gameObject.SetActive(false);
            negativeSignal?.gameObject.SetActive(true);
        }
    }
}
