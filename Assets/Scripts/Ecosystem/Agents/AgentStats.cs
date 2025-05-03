using UnityEngine;

[System.Serializable]
public class AgentStats
{
    public float hunger = 1f;
    public float thirst = 1f;
    public float speed;
    public float size;
    public float sightRange;

    public float hungerDecayRate = 0.005f;
    public float thirstDecayRate = 0.0075f;

    public AgentStats() { }

    public AgentStats(float speed, float size, float sight)
    {
        this.speed = speed;
        this.size = size;
        this.sightRange = sight;
    }

    public void TickDecay(float dt)
    {
        hunger -= hungerDecayRate * dt;
        thirst -= thirstDecayRate * dt;
        hunger = Mathf.Clamp01(hunger);
        thirst = Mathf.Clamp01(thirst);
    }

    public void Eat(float amount) => hunger = Mathf.Clamp01(hunger + amount);
    public void Drink(float amount) => thirst = Mathf.Clamp01(thirst + amount);
    public bool CanMate => hunger >= 0.7f && thirst >= 0.7f;
    public bool IsAlive => hunger > 0.1f && thirst > 0.1f;
}
