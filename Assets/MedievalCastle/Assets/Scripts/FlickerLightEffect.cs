using UnityEngine;

/// <summary>
/// Мерцание факельного освещения — Средневековый замок
/// </summary>
[RequireComponent(typeof(Light))]
public class FlickerLightEffect : MonoBehaviour
{
    public float baseIntensity = 1.5f;
    public float flickerAmount = 0.5f;
    public float flickerSpeed  = 7f;

    private Light lt;
    private float seed;

    void Start()
    {
        lt   = GetComponent<Light>();
        seed = Random.Range(0f, 100f); // уникальный сдвиг для каждого факела
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + seed, seed);
        lt.intensity = baseIntensity - flickerAmount * 0.5f + noise * flickerAmount;
    }
}
