using DarkTonic.PoolBoss;
using UnityEngine;

[DisallowMultipleComponent]
public class SustainedConsumable : MonoBehaviour
{
    public enum Type { Food, Water, Prey }

    [Header("Consumption Type")]
    public Type consumableType;
    [SerializeField] private ResourceBar resourceBar;

    [Header("Value Settings")]
    public float totalValue = 3f;
    public float remainingValue;
    public float tickInterval = 0.2f;

    [Header("Timing")]
    public float duration = 1f;

    [HideInInspector] public float valuePerTick;
    [HideInInspector] public int totalTicks;

    private void OnEnable()
    {
        InitializeValues();
        resourceBar = GetComponentInChildren<ResourceBar>(includeInactive: true);
        resourceBar.SetConsumable(this);

        if (consumableType == Type.Food)
            SpawnerManager.Instance.OnFoodSpawned();
        else if (consumableType == Type.Water)
            SpawnerManager.Instance.OnWaterSpawned();
    }

    public void InitializeValues()
    {
        remainingValue = totalValue;
        if (tickInterval <= 0.01f) tickInterval = 0.2f;
        if (duration <= 0f) duration = 1f;

        totalTicks = Mathf.Max(1, Mathf.RoundToInt(duration / tickInterval));
        valuePerTick = totalValue / totalTicks;

        if (consumableType == Type.Prey)
        {
            duration = 2f;
            if (TryGetComponent<PreyAgent>(out var sc))
            {
                float biomass = sc.stats.CurrentSize;
                totalValue = Mathf.Clamp(biomass, 1f, 5f);
                duration = biomass * 2f;
            }
        }
    }

    /// <summary>
    /// Only for Prey, updating value based on size as animal grows
    /// </summary>
    public void UpdateFromSize(float newBiomass)
    {
        if (consumableType != Type.Prey) return;

        float clampedBiomass = Mathf.Clamp(newBiomass, 1f, 5f);
        float previousTotal = totalValue;
        float consumedFraction = 1f - (remainingValue / previousTotal);

        totalValue = clampedBiomass;
        duration = clampedBiomass * 2f;

        totalTicks = Mathf.Max(1, Mathf.RoundToInt(duration / tickInterval));
        valuePerTick = totalValue / totalTicks;

        remainingValue = totalValue * (1f - consumedFraction);
    }

    /// <summary>
    /// Consume a fixed amount. Returns how much was actually consumed.
    /// </summary>
    public float Consume(float amount)
    {
        float consumed = Mathf.Min(amount, remainingValue);
        if (consumed <= 0f) return 0f;

        remainingValue -= consumed;
        remainingValue = Mathf.Max(0f, remainingValue);

        if (remainingValue <= 0)
        {
            switch (consumableType)
            {
                case Type.Food:
                    EcosystemManager.Instance.CumulativeData.foodConsumed += 1;
                    SpawnerManager.Instance.OnFoodConsumed();
                    break;
                case Type.Water:
                    EcosystemManager.Instance.CumulativeData.waterConsumed += 1;
                    SpawnerManager.Instance.OnWaterConsumed();
                    break;
                case Type.Prey:
                    EcosystemManager.Instance.CumulativeData.animalKilled += 1;
                    break;
            }

            if (consumableType == Type.Food || consumableType == Type.Water)
                PoolBoss.Despawn(transform);
            else
                Destroy(gameObject);
        }

        return consumed;
    }
}
