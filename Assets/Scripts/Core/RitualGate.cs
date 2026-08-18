using UnityEngine;

public class RitualGate : MonoBehaviour
{
    public GameObject[] trackedEnemies;
    public GameObject[] totems;
    public GameObject barrier;
    public string openMessage = "Печать разрушена";

    bool _opened;
    float _fallbackTimer;
    const float FallbackTimeout = 60f;

    Material _lockedMat;
    Material _unlockedMat;

    void Start()
    {
        _lockedMat = MakeEmissiveMat("RitualGate_Locked", new Color(0.85f, 0.06f, 0.02f), 1.8f);
        _unlockedMat = MakeEmissiveMat("RitualGate_Unlocked", new Color(0.08f, 0.72f, 0.18f), 0.6f);
        ApplyTotemMaterial(_lockedMat);
    }

    void Update()
    {
        if (_opened) return;

        bool allDead = true;
        bool anyVisible = false;

        for (int i = 0; i < trackedEnemies.Length; i++)
        {
            if (trackedEnemies[i] != null)
            {
                allDead = false;
                anyVisible = true;
            }
        }

        if (allDead)
        {
            Open();
            return;
        }

        if (!anyVisible)
            _fallbackTimer += Time.deltaTime;
        else
            _fallbackTimer = 0f;

        if (_fallbackTimer >= FallbackTimeout)
            Open();
    }

    void Open()
    {
        if (_opened) return;
        _opened = true;

        if (barrier != null)
            barrier.SetActive(false);

        ApplyTotemMaterial(_unlockedMat);
        PulseTotemScale();

        GameManager.Instance?.ShowMessage(openMessage, GameManager.MsgType.Default, 3f);

        PlayOpenSound();
    }

    void ApplyTotemMaterial(Material mat)
    {
        if (totems == null) return;
        for (int i = 0; i < totems.Length; i++)
        {
            if (totems[i] == null) continue;
            foreach (var r in totems[i].GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.name.Contains("Glow") || r.gameObject.name.Contains("Orb"))
                    r.material = mat;
            }
        }
    }

    void PulseTotemScale()
    {
        if (totems == null) return;
        for (int i = 0; i < totems.Length; i++)
        {
            if (totems[i] != null)
                StartCoroutine(TotemPulse(totems[i].transform));
        }
    }

    System.Collections.IEnumerator TotemPulse(Transform t)
    {
        Vector3 orig = t.localScale;
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            float s = 1f + Mathf.Sin(elapsed * Mathf.PI / 0.4f) * 0.15f;
            t.localScale = orig * s;
            yield return null;
        }
        t.localScale = orig;
    }

    void PlayOpenSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate; // 1 second
        var clip = AudioClip.Create("RitualGateOpen", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float env = Mathf.Clamp01(1f - t) * Mathf.Clamp01(t * 12f);
            float freq = 55f + t * 30f;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.45f;
            data[i] += Mathf.Sin(2f * Mathf.PI * freq * 2.02f * t) * env * 0.15f;
            data[i] += (Random.value - 0.5f) * env * 0.08f;
        }
        clip.SetData(data, 0);
        AudioSource.PlayClipAtPoint(clip, transform.position, 0.7f);
    }

    static Material MakeEmissiveMat(string name, Color color, float intensity)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return mat;
    }
}
