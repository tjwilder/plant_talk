using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum PlantProperty
{
    Nitrogen,
    Iron,
    Potassium,
    Magnesium,
    Phosphorus,
    Calcium
}

[System.Serializable]
public enum PropertyHealth
{
    Healthy,
    Unhealthy,
    Dead
}

[System.Serializable]
public class Nutrient
{
    public PlantProperty property;
    public int amount;
    public int neededAmount;
    public int deadAmount;

    public Nutrient(PlantProperty property, int amount)
    {
        this.property = property;
        this.amount = amount;
    }

    public PropertyHealth GetHealthStatus()
    {
        if (amount >= deadAmount)
            return PropertyHealth.Dead;
        else if (amount < neededAmount)
            return PropertyHealth.Unhealthy;
        else
            return PropertyHealth.Healthy;
    }
}

public class WateringResource
{
    public Dictionary<PlantProperty, int> nutrients = new Dictionary<PlantProperty, int>();
}

public class Plant : MonoBehaviour
{
    public List<Nutrient> nutrients = new List<Nutrient>();

    public bool AnyDead()
    {
        foreach (var nutrient in nutrients)
        {
            if (nutrient.amount < nutrient.neededAmount)
            {
                // Plant is unhealthy
                // Debug.Log($"Plant is unhealthy due to lack of {nutrient.property}");
            }

            if (nutrient.amount >= nutrient.deadAmount)
            {
                // Plant dies

                return true;
            }
        }
        return false;
    }

    public bool AllHealthy()
    {
        foreach (var nutrient in nutrients)
        {
            if (nutrient.amount < nutrient.neededAmount || nutrient.amount >= nutrient.deadAmount)
            {
                return false;
            }
        }
        return true;
    }

    public int GetNutrientAmount(PlantProperty property)
    {
        var nutrient = nutrients.Find(n => n.property == property);
        return nutrient != null ? nutrient.amount : 0;
    }

    public PropertyHealth GetNutrientHealth(PlantProperty property)
    {
        var nutrient = nutrients.Find(n => n.property == property);
        return nutrient != null ? nutrient.GetHealthStatus() : PropertyHealth.Healthy;
    }

    public bool TryAddNutrients(WateringResource resource)
    {
        // Assert just so we don't screw anything up at this point
        UnityEngine.Assertions.Assert.IsTrue(resource.nutrients.Count == 1);
        foreach (var kvp in resource.nutrients)
        {
            AddNutrient(kvp.Key, kvp.Value);
        }
        return AnyDead();
    }

    public void AddNutrient(PlantProperty property, int amount)
    {
        var nutrient = nutrients.Find(n => n.property == property);
        if (nutrient != null)
        {
            if (nutrient.property == PlantProperty.Nitrogen)
            {
                if (GetNutrientHealth(PlantProperty.Iron) != PropertyHealth.Healthy)
                {
                    Debug.Log($"Cannot add Nitrogen without the presense of enough Iron.");
                    return;
                }
            }
            if (nutrient.property == PlantProperty.Phosphorus)
            {
                if (GetNutrientHealth(PlantProperty.Magnesium) != PropertyHealth.Healthy)
                {
                    Debug.Log($"Cannot add Phosphorus without the presense of enough Magnesium or with too much Calcium.");
                    return;
                }
                else if (GetNutrientHealth(PlantProperty.Calcium) == PropertyHealth.Healthy)
                {
                    // GameObject.Find("Managers").GetComponent<GameController>().AddLog("Cannot add Phosphorus without the presense of enough Magnesium or with too much Calcium.");
                    // Add tons to forcibly kill the plant
                    nutrient.amount += 10;
                    return;
                }
            }
            // if (nutrient.property == PlantProperty.Calcium)
            // {
            //     if (GetNutrientHealth(PlantProperty.Phosphorus) != PropertyHealth.Healthy)
            //     {
            //         return;
            //     }
            // }
            nutrient.amount += amount;
            Debug.Log($"Added {amount} of {property}. New amount: {nutrient.amount}");
        }
        else
        {
            Debug.LogWarning($"Nutrient {property} not found in plant.");
        }
    }
}
