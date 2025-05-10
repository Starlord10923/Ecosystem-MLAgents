using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [Range(1f, 20f)] public float lerpSpeed = 10f;

    private float target = 1f;

    public void SetTargetFill(float value)
    {
        target = Mathf.Clamp01(value);
    }

    private void Update()
    {
        if (Mathf.Abs(barImage.fillAmount - target) > 0.001f)
        {
            barImage.fillAmount = Mathf.Lerp(barImage.fillAmount, target, Time.deltaTime * lerpSpeed);
        }
    }
}
