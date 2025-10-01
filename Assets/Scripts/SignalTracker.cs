using UnityEngine;

public enum SignalType
{
    Positive,
    Negative
}
public class Nitrogen : MonoBehaviour
{
    public PlantProperty property = PlantProperty.Nitrogen;
    public SignalType signalType = SignalType.Negative;
    private LevelManager levelManager;
    private Plant plant;
    private GameObject signal;
    private GameObject bark;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        plant = transform.parent.GetComponent<Plant>();
        signal = transform.Find("Signal").gameObject;
        bark = transform.Find("Bark").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        var health = plant.GetNutrientHealth(property);
        var shouldShowSignal = (signalType == SignalType.Negative && health == PropertyHealth.Unhealthy) ||
                             (signalType == SignalType.Positive && health == PropertyHealth.Healthy);
        if (shouldShowSignal)
        {
            if (levelManager.IsSignalDiscovered(property))
            {
                bark.SetActive(true);
                signal.SetActive(false);
            }
            else
            {
                bark.SetActive(false);
                signal.SetActive(true);
            }
        }
        else if (!shouldShowSignal)
        {
            bark.SetActive(false);
            signal.SetActive(false);
        }
    }
}
