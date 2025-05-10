using UnityEngine;

public class AgentAnimalBase : AgentBase
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Food>(out var food) && stats.hunger < 1f)
        {
            Eat(food.nutrition);
            AddReward(0.2f);
            Destroy(other.gameObject);
        }
        else if (other.TryGetComponent<Water>(out var water) && stats.thirst < 1f)
        {
            Drink(water.hydration);
            AddReward(0.2f);
            Destroy(other.gameObject);
        }
    }

    protected void Eat(float amt) => stats.Eat(amt);
    protected void Drink(float amt) => stats.Drink(amt);
}
