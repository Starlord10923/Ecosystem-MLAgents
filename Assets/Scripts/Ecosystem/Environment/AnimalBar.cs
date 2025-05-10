using UnityEngine;
using UnityEngine.UI;

public class AnimalBar : MonoBehaviour
{
    [Header("Stats Source")]
    public AgentStats stats;

    [Header("UI References")]
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image thirstBar;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject matingIcon;

    [Header("Smoothing")]
    [Range(1f, 20f)] public float lerpSpeed = 10f;

    private float hungerTarget, thirstTarget, healthTarget;

    private void Update()
    {
        if (stats == null) return;

        // Update target values
        hungerTarget = Mathf.Clamp01(stats.hunger);
        thirstTarget = Mathf.Clamp01(stats.thirst);
        healthTarget = Mathf.Clamp01(stats.health);

        // Smooth fill transitions
        if (hungerBar) hungerBar.fillAmount = Mathf.Lerp(hungerBar.fillAmount, hungerTarget, Time.deltaTime * lerpSpeed);
        if (thirstBar) thirstBar.fillAmount = Mathf.Lerp(thirstBar.fillAmount, thirstTarget, Time.deltaTime * lerpSpeed);
        if (healthBar) healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, healthTarget, Time.deltaTime * lerpSpeed);

        // Show or hide mating icon
        if (matingIcon != null)
            matingIcon.SetActive(stats.CanMate);
    }

    public void SetStats(AgentStats newStats) => stats = newStats;
}
