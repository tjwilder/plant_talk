using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum PlantProperty
{
    Nitrogen,
    Iron,
    Potassium,
    Magnesium
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

    public PropertyHealth GetNutrientHealth(PlantProperty property)
    {
        var nutrient = nutrients.Find(n => n.property == property);
        return nutrient != null ? nutrient.GetHealthStatus() : PropertyHealth.Healthy;
    }

    public bool TryAddNutrients(WateringResource resource)
    {
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
            nutrient.amount += amount;
            Debug.Log($"Added {amount} of {property}. New amount: {nutrient.amount}");
        }
        else
        {
            Debug.LogWarning($"Nutrient {property} not found in plant.");
        }
    }
}
