using UnityEngine;

[System.Serializable]
public class AgentStats
{    
    [Header("Genetic / Inherited Traits")]
    public float speed = 1f;
    public static float maxPossibleSpeed = 20f;
    public float sightRange = 1f;
    public static float maxPossibleSightRange = 2f;
    public float maxSize = 1f;
    public static float maxPossibleSize = 3.5f;
    public float MaxLifetime = 60f;
    public static float maxPossibleLifeTime = 250f;
    public float growthTime = 20f;
    [ReadOnly] public int Generation = 0;
    

    [Header("Runtime State Variables")]
    [ReadOnly] public float hunger = 1f;
    [ReadOnly] public float thirst = 1f;
    [ReadOnly] public float health = 1f;
    [ReadOnly] public float age = 0f;

    [ReadOnly] public float hungerPauseUntil = 0f;
    [ReadOnly] public float thirstPauseUntil = 0f;

    [Header("Decay & Regeneration Rates")]
    public float hungerDecayRate = 0.03f;
    public float thirstDecayRate = 0.02f;
    public float healthDecayRate = 0.01f;
    public float healthRegenRate = 0.02f;

    public float CurrentSize => growthTime <= 0f
        ? maxSize
        : Mathf.Lerp(1f, maxSize, Mathf.Clamp01(age / growthTime));

    public bool IsAdult => age >= growthTime;
    public bool IsAlive => hunger > 0f && thirst > 0f && health > 0f && age < MaxLifetime;
    public int numChildren = 0;

    public AgentStats() { }

    public AgentStats(float speed, float maxSize, float sightRange, float MaxLifetime)
    {
        this.speed = speed;
        this.maxSize = maxSize;
        this.sightRange = sightRange;
        this.MaxLifetime = MaxLifetime;
    }

    public void TickDecay(float dt)
    {
        age += dt;

        if (Time.time >= hungerPauseUntil)

            DecreaseHunger(hungerDecayRate * dt);
        if (Time.time >= thirstPauseUntil)
            DecreaseThirst(thirstDecayRate * dt);

        // Health logic
        if (hunger < 0.3f || thirst < 0.3f)
            TakeDamage(healthDecayRate * dt);
        else if (hunger >= 0.5f && thirst >= 0.5f)
            IncreaseHealth(healthRegenRate * dt);
        // else do nothing (health stays same)
    }


    public void Eat(float amount)
    {
        hunger = Mathf.Clamp01(hunger + amount);
        hungerPauseUntil = Time.time + 2f;
    }

    public void Drink(float amount)
    {
        thirst = Mathf.Clamp01(thirst + amount);
        thirstPauseUntil = Time.time + 2f;
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Clamp01(health - amount);
    }

    public void IncreaseHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp01(health);
    }

    public void Mate()
    {
        DecreaseHunger(0.2f);
        DecreaseThirst(0.2f);
        numChildren += 1;
    }

    public void DecreaseHunger(float value)
    {
        hunger -= value;
        hunger = Mathf.Clamp01(hunger);
    }

    public void DecreaseThirst(float value)
    {
        thirst -= value;
        thirst = Mathf.Clamp01(thirst);
    }

    public bool CanMate => IsAdult && hunger >= 0.7f && thirst >= 0.7f;
    public bool LivedFullLife => age >= MaxLifetime;

    public static AgentStats Clone(AgentStats source)
    {
        return new AgentStats
        {
            hunger = source.hunger,
            thirst = source.thirst,
            health = source.health,
            speed = source.speed,
            sightRange = source.sightRange,
            maxSize = source.maxSize,
            hungerDecayRate = source.hungerDecayRate,
            thirstDecayRate = source.thirstDecayRate,
            healthDecayRate = source.healthDecayRate,
            healthRegenRate = source.healthRegenRate,
            MaxLifetime = source.MaxLifetime,
            growthTime = source.growthTime,
            age = 0f, // newborn
            Generation = source.Generation,
            hungerPauseUntil = 0f,
            thirstPauseUntil = 0f,
            numChildren = 0
        };
    }
}
