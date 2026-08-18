using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    public enum SFX { Swing, Hit, CritHit, Kill, BowShoot, ArrowHit, SpearThrow, Dodge, Coin, Damage, Block }

    AudioSource _src;

    AudioClip _swing, _hit, _critHit, _kill, _bowShoot, _arrowHit, _spearThrow, _dodge, _coin, _damage, _block;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f;
        GenerateClips();
    }

    public void Play(SFX sfx, float volume = 0.5f, float pitchVariation = 0.1f)
    {
        var clip = GetClip(sfx);
        if (clip == null || _src == null) return;
        _src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        _src.PlayOneShot(clip, volume);
    }

    AudioClip GetClip(SFX sfx) => sfx switch
    {
        SFX.Swing      => _swing,
        SFX.Hit        => _hit,
        SFX.CritHit    => _critHit,
        SFX.Kill       => _kill,
        SFX.BowShoot   => _bowShoot,
        SFX.ArrowHit   => _arrowHit,
        SFX.SpearThrow => _spearThrow,
        SFX.Dodge      => _dodge,
        SFX.Coin       => _coin,
        SFX.Damage     => _damage,
        SFX.Block      => _block,
        _ => null
    };

    void GenerateClips()
    {
        _swing      = MakeNoise("Swing",      0.08f, 800f,  400f,  0.6f);
        _hit        = MakeNoise("Hit",        0.06f, 200f,  100f,  0.8f);
        _critHit    = MakeNoise("CritHit",    0.10f, 350f,  200f,  1.0f);
        _kill       = MakeNoise("Kill",       0.18f, 120f,  60f,   0.7f);
        _bowShoot   = MakeNoise("BowShoot",   0.05f, 1200f, 600f,  0.4f);
        _arrowHit   = MakeNoise("ArrowHit",   0.04f, 400f,  200f,  0.5f);
        _spearThrow = MakeNoise("SpearThrow", 0.07f, 500f,  300f,  0.5f);
        _dodge      = MakeNoise("Dodge",      0.10f, 600f,  200f,  0.3f);
        _coin       = MakeTone ("Coin",       0.08f, 1800f, 0.35f);
        _damage     = MakeNoise("Damage",     0.12f, 250f,  150f,  0.7f);
        _block      = MakeNoise("Block",      0.05f, 500f,  250f,  0.6f);
    }

    static AudioClip MakeNoise(string name, float duration, float freqHigh, float freqLow, float attack)
    {
        int rate    = 44100;
        int samples = Mathf.CeilToInt(duration * rate);
        var data    = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t   = (float)i / rate;
            float env = t < duration * attack
                ? t / (duration * attack)
                : 1f - (t - duration * attack) / (duration * (1f - attack));
            env = Mathf.Clamp01(env);
            float freq = Mathf.Lerp(freqHigh, freqLow, t / duration);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            float noise = Random.Range(-1f, 1f);
            data[i] = (wave * 0.4f + noise * 0.6f) * env;
        }
        var clip = AudioClip.Create(name, samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip MakeTone(string name, float duration, float freq, float attack)
    {
        int rate    = 44100;
        int samples = Mathf.CeilToInt(duration * rate);
        var data    = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t   = (float)i / rate;
            float env = t < duration * attack
                ? t / (duration * attack)
                : 1f - (t - duration * attack) / (duration * (1f - attack));
            env = Mathf.Clamp01(env) * env;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env;
        }
        var clip = AudioClip.Create(name, samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
