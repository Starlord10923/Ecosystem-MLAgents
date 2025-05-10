using UnityEngine;

[System.Serializable]
public class AgentStats
{
    public float hunger = 1f;
    public float thirst = 1f;

    public float speed;
    public float sightRange;
    public float maxSize;

    public float hungerDecayRate = 0.005f;
    public float thirstDecayRate = 0.0075f;

    public float lifetime = 60f;
    public float growthTime = 20f;
    public float age = 0f;

    public float CurrentSize => Mathf.Lerp(1f, maxSize, Mathf.Clamp01(age / growthTime));
    public bool IsAdult => age >= growthTime;
    public bool IsAlive => hunger > 0.1f && thirst > 0.1f && age < lifetime;

    public AgentStats() { }

    public AgentStats(float speed, float maxSize, float sightRange)
    {
        this.speed = speed;
        this.maxSize = maxSize;
        this.sightRange = sightRange;
    }

    public void TickDecay(float dt)
    {
        age += dt;
        hunger -= hungerDecayRate * dt;
        thirst -= thirstDecayRate * dt;
        hunger = Mathf.Clamp01(hunger);
        thirst = Mathf.Clamp01(thirst);
    }

    public void Eat(float amount) => hunger = Mathf.Clamp01(hunger + amount);
    public void Drink(float amount) => thirst = Mathf.Clamp01(thirst + amount);
    public bool CanMate => IsAdult && hunger >= 0.7f && thirst >= 0.7f;
}
