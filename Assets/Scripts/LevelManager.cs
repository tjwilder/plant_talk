using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool revealAllSignals = false;
    // Update is called once per frame
    void Update()
    {

    }

    public bool IsSignalDiscovered(PlantProperty property = PlantProperty.Nitrogen)
    {
        return revealAllSignals;
    }
}
