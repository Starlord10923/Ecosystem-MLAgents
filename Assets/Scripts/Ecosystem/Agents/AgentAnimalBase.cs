using System.Collections.Generic;
using MEC;
using UnityEngine;

public abstract class AgentAnimalBase : AgentBase
{
    private CoroutineHandle currentConsumption;
    private Collider currentTarget = null;

    private CoroutineHandle matingCoroutine;
    private bool IsMating => matingCoroutine.IsRunning;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
            RewardUtility.AddWallHitPenalty(this);

        CheckForConsumption(other);
        CheckForMating(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (currentTarget == other)
            StopCurrentConsumption();
    }

    public void CheckForConsumption(Collider other)
    {
        if (currentConsumption.IsRunning || !other.TryGetComponent<SustainedConsumable>(out var target))
            return;
        if (this is PreyAgent && target.consumableType == SustainedConsumable.Type.Prey)
            return;

        currentTarget = other;
        currentConsumption = Timing.RunCoroutine(ConsumeOverTime(target, other));
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
                    else
                    {
                        StopCurrentConsumption();
                        yield break;
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

    public void CheckForMating(Collider other)
    {
        if (!stats.CanMate || IsMating) return;

        if (other.TryGetComponent<AgentAnimalBase>(out var partner) &&
            partner != this &&
            partner.stats.CanMate &&
            !partner.IsMating &&
            partner.GetType() == GetType())
        {
            matingCoroutine = Timing.RunCoroutine(HandleMating(partner));
        }
    }

    private IEnumerator<float> HandleMating(AgentAnimalBase partner)
    {
        float matingTime = 0f;
        const float tick = 0.2f;
        while (matingTime < 2f)          // 2 s total
        {
            yield return Timing.WaitForSeconds(tick);
            RewardUtility.AddMatingReward(this);
            matingTime += tick;
        }

        stats.Mate();
        partner.stats.Mate();
        animalBar.UpdateFromStats();
        partner.animalBar.UpdateFromStats();

        // Create inherited child
        AgentStats childStats = GeneticUtility.Inherit(stats, partner.stats);
        Vector3 spawnPos = (transform.position + partner.transform.position) / 2f;
        EcosystemManager.Instance.SpawnAnimal(this, childStats, spawnPos);

        RewardUtility.AddMatingSuccessReward(this);
    }

    public override void Die()
    {
        RewardUtility.AddDeathPenalty(this);
        base.Die();

        StopCurrentConsumption();
        if (matingCoroutine.IsRunning)
            Timing.KillCoroutines(matingCoroutine);
    }

    protected void Eat(float amt, SustainedConsumable target)
    {
        stats.Eat(amt);
        RewardUtility.AddNutritionReward(this, amt);
        target.Consume(amt);
        animalBar.UpdateFromStats();
    }

    protected void EatAnimal(float amt, SustainedConsumable target, PreyAgent prey)
    {
        prey.stats.TakeDamage(amt);
        stats.Eat(amt);
        RewardUtility.AddPredationReward(this, amt);
        target.Consume(amt);
        animalBar.UpdateFromStats();
    }

    protected void Drink(float amt, SustainedConsumable target)
    {
        stats.Drink(amt);
        RewardUtility.AddWaterReward(this, amt);
        target.Consume(amt);
        animalBar.UpdateFromStats();
    }

    public override void UpdateSize()
    {
        if (!Mathf.Approximately(transform.localScale.y, stats.CurrentSize))
            transform.localScale = Vector3.one * stats.CurrentSize;
    }
}
