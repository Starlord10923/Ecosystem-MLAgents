using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    public SustainedConsumable consumable;
    [SerializeField] private Image resourceBar;
    [Range(1f, 20f)] public float lerpSpeed = 10f;

    public void SetConsumable(SustainedConsumable newConsumable) => consumable = newConsumable;

    public float value;

    private void Update()
    {
        // if (resourceBar && resourceBar.fillAmount != consumable.remainingValue)
        //     resourceBar.fillAmount = Mathf.Lerp(resourceBar.fillAmount, consumable.remainingValue, Time.deltaTime * lerpSpeed);
        resourceBar.fillAmount = Mathf.Lerp(resourceBar.fillAmount, value, Time.deltaTime * lerpSpeed);
    }
}
