using UnityEngine;

/// <summary>
/// Анимированная процедурная вода для защитного рва.
/// Работает без внешних текстур: сама создаёт шум, двигает UV и слегка мерцает.
/// Скрипт можно вешать на тонкие Cube-сегменты воды.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class AnimatedWater : MonoBehaviour
{
    [Header("Течение")]
    public Vector2 scrollSpeed = new Vector2(0.20f, 0.06f);

    [Header("Рябь")]
    public float rippleStrength = 0.035f;
    public float rippleScale = 2.0f;

    [Header("Блики")]
    public bool enableGlitter = true;
    public float glitterSpeed = 0.95f;
    public float glitterIntensity = 0.22f;

    [Header("Цвет воды")]
    public Color waterColor = new Color(0.10f, 0.36f, 0.58f, 0.78f);
    public Color emissionTint = new Color(0.05f, 0.17f, 0.28f);

    [Header("Процедурный шум")]
    [Range(0f, 1f)] public float noiseContrast = 0.18f;
    [Range(0f, 1f)] public float noiseBrightness = 0.50f;

    private Renderer rend;
    private Material material;
    private Texture2D noiseTexture;
    private float timeOffset;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        timeOffset = Random.Range(0f, 100f);

        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Diffuse");

        material = new Material(shader);
        rend.material = material;

        SetupMaterial();
        GenerateNoiseTexture();

        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", noiseTexture);
            material.SetTextureScale("_MainTex", new Vector2(rippleScale, rippleScale));
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", noiseTexture);
            material.SetTextureScale("_BaseMap", new Vector2(rippleScale, rippleScale));
        }
    }

    private void SetupMaterial()
    {
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", waterColor);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", waterColor);

        // Standard Render Pipeline transparency.
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        // URP transparency.
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
        }

        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
            material.SetColor("_EmissionColor", emissionTint);
    }

    private void GenerateNoiseTexture()
    {
        const int size = 128;
        noiseTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        noiseTexture.wrapMode = TextureWrapMode.Repeat;
        noiseTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size * 4f;
                float ny = y / (float)size * 4f;

                float a = Mathf.PerlinNoise(nx, ny);
                float b = Mathf.PerlinNoise(nx * 2.4f + 17.3f, ny * 2.4f + 9.1f);

                float raw = a * 0.62f + b * 0.38f;
                float contrast = 1f + noiseContrast * 2.8f;
                float v = Mathf.Clamp01((raw - 0.5f) * contrast + noiseBrightness);

                pixels[x + y * size] = new Color(v, v, v, 1f);
            }
        }

        noiseTexture.SetPixels(pixels);
        noiseTexture.Apply();
    }

    private void Update()
    {
        if (material == null)
            return;

        Vector2 offset = scrollSpeed * Time.time;

        if (material.HasProperty("_MainTex"))
            material.SetTextureOffset("_MainTex", offset);

        if (material.HasProperty("_BaseMap"))
            material.SetTextureOffset("_BaseMap", offset);

        float ripple = 1f + Mathf.Sin(Time.time * 1.7f + timeOffset) * rippleStrength;
        Vector2 scale = new Vector2(rippleScale * ripple, rippleScale * ripple);

        if (material.HasProperty("_MainTex"))
            material.SetTextureScale("_MainTex", scale);

        if (material.HasProperty("_BaseMap"))
            material.SetTextureScale("_BaseMap", scale);

        if (enableGlitter && material.HasProperty("_EmissionColor"))
        {
            float glow = 0.55f + Mathf.Sin(Time.time * glitterSpeed + timeOffset) * glitterIntensity;
            material.SetColor("_EmissionColor", emissionTint * glow);
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            if (material != null) Destroy(material);
            if (noiseTexture != null) Destroy(noiseTexture);
        }
        else
        {
            if (material != null) DestroyImmediate(material);
            if (noiseTexture != null) DestroyImmediate(noiseTexture);
        }
    }
}
