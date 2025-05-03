using UnityEngine;

public class AgentAnimalBase : AgentBase
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Food>(out var food) && stats.hunger < 1f)
        {
            Eat(food.nutrition);
            Destroy(other.gameObject);
            AddReward(0.2f);
        }
        else if (other.TryGetComponent<Water>(out var water) && stats.thirst < 1f)
        {
            Drink(water.hydration);
            Destroy(other.gameObject);
            AddReward(0.2f);
        }
    }

    protected void Eat(float amt) => stats.Eat(amt);
    protected void Drink(float amt) => stats.Drink(amt);
}
