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
    [SerializeField] private Image matingIcon;

    [Header("Smoothing")]
    [Range(1f, 20f)] public float lerpSpeed = 10f;

    public void SetStats(AgentStats newStats) => stats = newStats;

    private void Start()
    {
        if (!EcosystemManager.Instance.showValues) gameObject.SetActive(false);
    }

    private void Update()
    {
        if (stats == null) return;

        if (hungerBar && hungerBar.fillAmount != stats.hunger) hungerBar.fillAmount = Mathf.Lerp(hungerBar.fillAmount, stats.hunger, Time.deltaTime * lerpSpeed);
        if (thirstBar && thirstBar.fillAmount != stats.thirst) thirstBar.fillAmount = Mathf.Lerp(thirstBar.fillAmount, stats.thirst, Time.deltaTime * lerpSpeed);
        if (healthBar && healthBar.fillAmount != stats.health) healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, stats.health, Time.deltaTime * lerpSpeed);

        Color mateColor = stats.CanMate ? Color.red : Color.white;
        if (matingIcon && matingIcon.color != mateColor) matingIcon.color = mateColor;
    }

    public void UpdateFromStats()
    {
        return;
    }
}
