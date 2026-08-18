using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class L0StoryAudio : MonoBehaviour
{
    public static L0StoryAudio Instance { get; private set; }

    [Header("Volume")]
    public float windVolume = 0.14f;
    public float eventVolume = 0.35f;

    private AudioSource loopSource;
    private AudioSource oneShotSource;

    private void Awake()
    {
        Instance = this;

        loopSource = GetComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.spatialBlend = 0f;
        loopSource.volume = windVolume;
        loopSource.clip = MakeNoiseClip("L0_Wind", 2.8f, 90f, 0.16f);

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.loop = false;
        oneShotSource.spatialBlend = 0f;
        oneShotSource.volume = eventVolume;
    }

    private void Start()
    {
        if (loopSource != null && loopSource.clip != null && !loopSource.isPlaying)
            loopSource.Play();
    }

    public static L0StoryAudio Ensure()
    {
        if (Instance != null)
            return Instance;

        GameObject obj = new GameObject("L0_STORY_AUDIO");
        return obj.AddComponent<L0StoryAudio>();
    }

    public void PlayTask()
    {
        oneShotSource.PlayOneShot(MakeToneClip("L0_Task", 0.12f, 720f, 0.20f), eventVolume);
    }

    public void PlayHorn()
    {
        oneShotSource.PlayOneShot(MakeToneClip("L0_Horn", 1.4f, 170f, 0.42f), eventVolume * 1.15f);
    }

    public void PlayGate()
    {
        oneShotSource.PlayOneShot(MakeNoiseClip("L0_Gate", 0.45f, 70f, 0.55f), eventVolume);
    }

    public void PlayBlocked()
    {
        oneShotSource.PlayOneShot(MakeToneClip("L0_Blocked", 0.18f, 95f, 0.35f), eventVolume);
    }

    private AudioClip MakeToneClip(string name, float duration, float frequency, float amp)
    {
        int rate = 44100;
        int count = Mathf.CeilToInt(rate * duration);
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Clamp01(1f - t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amp * env;
        }

        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip MakeNoiseClip(string name, float duration, float frequency, float amp)
    {
        int rate = 44100;
        int count = Mathf.CeilToInt(rate * duration);
        float[] data = new float[count];

        float v = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            v = Mathf.Lerp(v, Random.Range(-1f, 1f), 0.04f);
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.25f;
            data[i] = (v + wave) * amp * 0.35f;
        }

        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
