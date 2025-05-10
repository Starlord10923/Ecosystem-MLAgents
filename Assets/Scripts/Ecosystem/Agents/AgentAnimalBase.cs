using System.Collections.Generic;
using MEC;
using UnityEngine;

public abstract class AgentAnimalBase : AgentBase
{
    private CoroutineHandle currentConsumption;
    private Collider currentTarget = null;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (currentConsumption != null || !other.TryGetComponent<SustainedConsumable>(out var target))
            return;

        currentTarget = other;
        currentConsumption = Timing.RunCoroutine(ConsumeOverTime(target, other));
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (currentTarget == other)
        {
            StopCurrentConsumption();
        }
    }

    private void StopCurrentConsumption()
    {
        if (currentConsumption.IsRunning)
        {
            Timing.KillCoroutines(currentConsumption);
        }
        currentTarget = null;
    }

    private IEnumerator<float> ConsumeOverTime(SustainedConsumable target, Collider source)
    {
        float elapsed = 0f;

        while (elapsed < target.duration)
        {
            if (target == null)
            {
                StopCurrentConsumption();
                yield break;
            }

            float consumed = Mathf.Min(target.valuePerTick, target.remainingValue);
            if (consumed <= 0f)
            {
                StopCurrentConsumption();
                yield break;
            }

            switch (target.consumableType)
            {
                case SustainedConsumable.Type.Food:
                    if (stats.hunger < 1f)
                    {
                        Eat(consumed, target);
                    }
                    break;

                case SustainedConsumable.Type.Water:
                    if (stats.thirst < 1f)
                    {
                        Drink(consumed, target);
                    }
                    break;

                case SustainedConsumable.Type.Prey:
                    if (this is PredatorAgent predator && predator.stats.hunger < 1f)
                    {
                        if (target.TryGetComponent<PreyAgent>(out var prey))
                            EatAnimal(consumed, target, prey);
                        else
                        {
                            StopCurrentConsumption();
                            yield break;
                        }
                    }
                    break;
            }

            if (target.remainingValue <= 0f && target.consumableType != SustainedConsumable.Type.Prey)
            {
                Destroy(target.gameObject);
                StopCurrentConsumption();
                yield break;
            }

            elapsed += target.tickInterval;
            yield return Timing.WaitForSeconds(target.tickInterval);
        }

        StopCurrentConsumption();
    }

    public override void Die()
    {
        RewardUtility.Instance.AddDeathPenalty(this);
        base.Die();
        StopCurrentConsumption();
    }

    protected void Eat(float amt, SustainedConsumable target)
    {
        stats.Eat(amt);
        RewardUtility.Instance.AddNutritionReward(this, amt);
        target.Consume(amt);
    }

    protected void EatAnimal(float amt, SustainedConsumable target, PreyAgent prey)
    {
        prey.stats.TakeDamage(amt);
        stats.Eat(amt);
        RewardUtility.Instance.AddPredationReward(this, amt);
        target.Consume(amt);
    }

    protected void Drink(float amt, SustainedConsumable target)
    {
        stats.Drink(amt);
        RewardUtility.Instance.AddWaterReward(this, amt);
        target.Consume(amt);
    }

    public override void UpdateSize()
    {
        if (transform.localScale.y != stats.CurrentSize)
        {
            transform.localScale = Vector3.one * stats.CurrentSize;
        }
    }
}
