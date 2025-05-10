using UnityEngine;

[System.Serializable]
public class AgentStats
{
    public float hunger = 1f;
    public float thirst = 1f;
    public float health = 1f;

    public float speed;
    public float sightRange;
    public float maxSize;

    public float hungerDecayRate = 0.005f;
    public float thirstDecayRate = 0.0075f;
    public float healthDecayRate = 0.005f;
    public float healthRegenRate = 0.01f;

    public float lifetime = 60f;
    public float growthTime = 20f;
    public float age = 0f;

    public float hungerPauseUntil = 0f;
    public float thirstPauseUntil = 0f;

    public float CurrentSize => Mathf.Lerp(1f, maxSize, Mathf.Clamp01(age / growthTime));
    public bool IsAdult => age >= growthTime;
    public bool IsAlive => hunger > 0f && thirst > 0f && health > 0f && age < lifetime;

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

        if (Time.time >= hungerPauseUntil)
            hunger -= hungerDecayRate * dt;

        if (Time.time >= thirstPauseUntil)
            thirst -= thirstDecayRate * dt;

        hunger = Mathf.Clamp01(hunger);
        thirst = Mathf.Clamp01(thirst);

        // Health logic
        if (hunger < 0.3f || thirst < 0.3f)
        {
            health -= healthDecayRate * dt;
        }
        else if (hunger >= 0.5f || thirst >= 0.5f)
        {
            health += healthRegenRate * dt;
        }
        // else do nothing (health stays same)

        health = Mathf.Clamp01(health);
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

    public bool CanMate => IsAdult && hunger >= 0.7f && thirst >= 0.7f;
}
