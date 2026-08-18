using UnityEngine;

/// <summary>
/// Простые процедурные атмосферные loop-звуки без внешних аудиофайлов.
/// Используется для орочьей деревни: низкий гул, барабаны, костры, дальний хор и ветер.
/// </summary>
public static class L0Audio
{
    public static GameObject CreateOrcVillageSoundscape(Vector3 center, Vector3[] firePoints, Transform parent)
    {
        GameObject root = new GameObject("L0_OrcAudio");
        root.transform.SetParent(parent, true);
        root.transform.position = center;

        CreateLoopSource(root.transform, "OrcDrone", center + Vector3.up * 2.8f, CreateDroneClip(), 0.18f, 1f, 8f, 48f);
        CreateLoopSource(root.transform, "WarDrums", center + new Vector3(0f, 1.3f, 0f), CreateDrumClip(), 0.30f, 1f, 7f, 42f);
        CreateLoopSource(root.transform, "DistantOrcChant", center + L0OrcArenaConfig.BackDir * 30f + Vector3.up * 3.5f, CreateChantClip(), 0.15f, 1f, 16f, 70f);
        CreateLoopSource(root.transform, "BoneWind", center + Vector3.up * 5.5f, CreateWindClip(), 0.12f, 1f, 24f, 90f);

        if (firePoints != null)
        {
            for (int i = 0; i < firePoints.Length && i < 8; i++)
            {
                CreateLoopSource(root.transform, "FireCrackle_" + i, firePoints[i] + Vector3.up * 0.6f, CreateFireClip(i), 0.105f, 1f, 2f, 16f);
            }
        }

        return root;
    }

    private static AudioSource CreateLoopSource(Transform parent, string name, Vector3 worldPosition, AudioClip clip, float volume, float spatialBlend, float minDistance, float maxDistance)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, true);
        obj.transform.position = worldPosition;

        AudioSource src = obj.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = true;
        src.volume = volume;
        src.spatialBlend = spatialBlend;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.dopplerLevel = 0f;
        src.spread = 35f;

        if (Application.isPlaying)
            src.Play();

        return src;
    }

    private static AudioClip CreateDroneClip()
    {
        const int hz = 22050;
        const int seconds = 4;
        int sampleCount = hz * seconds;
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)hz;
            float tremolo = 0.72f + 0.28f * Mathf.Sin(t * Mathf.PI * 2f * 0.24f);
            float baseA = Mathf.Sin(t * Mathf.PI * 2f * 38f);
            float baseB = Mathf.Sin(t * Mathf.PI * 2f * 54f) * 0.72f;
            float baseC = Mathf.Sin(t * Mathf.PI * 2f * 82f) * 0.22f;
            float noise = (Mathf.PerlinNoise(t * 2.2f, 0.2f) - 0.5f) * 0.045f;
            data[i] = (baseA + baseB + baseC) * 0.075f * tremolo + noise;
        }

        AudioClip clip = AudioClip.Create("L0_OrcDrone", sampleCount, 1, hz, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateDrumClip()
    {
        const int hz = 22050;
        const int seconds = 2;
        int sampleCount = hz * seconds;
        float[] data = new float[sampleCount];
        float[] beats = { 0f, 0.36f, 0.82f, 1.22f, 1.58f };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)hz;
            float s = 0f;

            for (int b = 0; b < beats.Length; b++)
            {
                float dt = t - beats[b];
                if (dt >= 0f && dt < 0.24f)
                {
                    float env = Mathf.Exp(-dt * 17f) * (1f - Mathf.Clamp01(dt / 0.24f));
                    float weight = b == 0 || b == 3 ? 1.0f : 0.68f;
                    s += Mathf.Sin(dt * Mathf.PI * 2f * 68f) * env * 0.36f * weight;
                    s += Mathf.Sin(dt * Mathf.PI * 2f * 116f) * env * 0.11f * weight;
                }
            }

            s += (Mathf.PerlinNoise(t * 24f, 0.7f) - 0.5f) * 0.014f;
            data[i] = s;
        }

        AudioClip clip = AudioClip.Create("L0_OrcDrums", sampleCount, 1, hz, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateChantClip()
    {
        const int hz = 22050;
        const int seconds = 6;
        int sampleCount = hz * seconds;
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)hz;
            float pulse = Mathf.Max(0f, Mathf.Sin(t * Mathf.PI * 2f * 0.72f));
            float vowel = Mathf.Sin(t * Mathf.PI * 2f * 96f) * 0.55f + Mathf.Sin(t * Mathf.PI * 2f * 128f) * 0.28f;
            float throat = Mathf.Sin(t * Mathf.PI * 2f * 48f) * 0.42f;
            float crowd = (Mathf.PerlinNoise(t * 5.5f, 8.1f) - 0.5f) * 0.20f;
            data[i] = (vowel + throat + crowd) * pulse * 0.055f;
        }

        AudioClip clip = AudioClip.Create("L0_DistantOrcChant", sampleCount, 1, hz, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateWindClip()
    {
        const int hz = 22050;
        const int seconds = 5;
        int sampleCount = hz * seconds;
        float[] data = new float[sampleCount];
        float lp = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)hz;
            float white = (Mathf.PerlinNoise(t * 19f, 3.1f) - 0.5f) * 2f;
            lp = Mathf.Lerp(lp, white, 0.035f);
            float gust = 0.55f + 0.45f * Mathf.PerlinNoise(t * 0.55f, 2.7f);
            float whistle = Mathf.Sin(t * Mathf.PI * 2f * 310f) * Mathf.Max(0f, Mathf.PerlinNoise(t * 1.8f, 4.2f) - 0.68f) * 0.045f;
            data[i] = lp * 0.075f * gust + whistle;
        }

        AudioClip clip = AudioClip.Create("L0_BoneWind", sampleCount, 1, hz, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateFireClip(int seed)
    {
        const int hz = 22050;
        const int seconds = 3;
        int sampleCount = hz * seconds;
        float[] data = new float[sampleCount];

        float lp = 0f;
        float offset = 1.3f + seed * 0.37f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)hz;
            float white = (Mathf.PerlinNoise(t * 55f, offset) - 0.5f) * 2f;
            lp = Mathf.Lerp(lp, white, 0.17f);
            float crackle = Mathf.Max(0f, Mathf.PerlinNoise(t * 17f, 5.2f + seed) - 0.78f) * 0.8f;
            float body = lp * 0.07f;
            data[i] = body + crackle * Mathf.Sin(t * Mathf.PI * 2f * (860f + seed * 23f)) * 0.08f;
        }

        AudioClip clip = AudioClip.Create("L0_FireCrackle_" + seed, sampleCount, 1, hz, false);
        clip.SetData(data, 0);
        return clip;
    }
}
