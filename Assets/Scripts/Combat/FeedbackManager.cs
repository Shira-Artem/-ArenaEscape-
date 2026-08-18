using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    AudioSource _src;
    AudioClip _clipCoin, _clipHit, _clipDeath, _clipCheckpoint, _clipDamage, _clipWhoosh, _clipHealth;
    AudioClip _clipCrit, _clipHeadshot, _clipStreak, _clipKillConfirm;

    // Floating text в мире
    class FTEntry
    {
        public string  text;
        public Color   color;
        public Vector3 worldPos;
        public float   life, maxLife;
    }
    readonly List<FTEntry> _floats = new List<FTEntry>();
    Camera _cam;

    // Kill feed (экранные popup)
    class KFEntry
    {
        public string text;
        public Color  color;
        public float  life, maxLife;
        public float  yOffset;
    }
    readonly List<KFEntry> _killFeed = new List<KFEntry>();
    float _nextKfY;

    // Hit marker
    float _hitMarkerTimer;
    Color _hitMarkerColor = Color.white;

    // Kill flash (красная вспышка экрана)
    float _killFlashAlpha;

    // Kill slowmo
    float _slowmoTimer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _src = gameObject.AddComponent<AudioSource>();
        _src.spatialBlend = 0f;
        GenerateClips();
    }

    void Update()
    {
        float udt = Time.unscaledDeltaTime;

        for (int i = _floats.Count - 1; i >= 0; i--)
        {
            _floats[i].life     -= udt;
            _floats[i].worldPos += Vector3.up * udt * 2f;
            if (_floats[i].life <= 0f) _floats.RemoveAt(i);
        }

        for (int i = _killFeed.Count - 1; i >= 0; i--)
        {
            _killFeed[i].life -= udt;
            if (_killFeed[i].life <= 0f) { _killFeed.RemoveAt(i); _nextKfY = 0f; }
        }

        if (_hitMarkerTimer > 0f) _hitMarkerTimer -= udt;
        if (_killFlashAlpha > 0f) _killFlashAlpha -= udt * 3f;

        if (_slowmoTimer > 0f)
        {
            _slowmoTimer -= udt;
            if (_slowmoTimer <= 0f)
                Time.timeScale = 1f;
        }
    }

    // ════════════════════════════════════════════════════════
    // ЗВУКИ — всё генерируется кодом, никаких файлов
    // ════════════════════════════════════════════════════════
    void GenerateClips()
    {
        _clipCoin        = BuildArpeggio(new float[]{ 523f, 659f, 784f  }, 0.055f);
        _clipHit         = BuildImpact(0.14f, 280f);
        _clipDeath       = BuildDeath();
        _clipCheckpoint  = BuildArpeggio(new float[]{ 261f, 329f, 392f, 523f }, 0.07f);
        _clipDamage      = BuildDamage();
        _clipWhoosh      = BuildWhoosh();
        _clipHealth      = BuildArpeggio(new float[]{ 392f, 494f, 587f  }, 0.065f);
        _clipCrit        = BuildImpact(0.18f, 180f);
        _clipHeadshot    = BuildArpeggio(new float[]{ 880f, 1100f }, 0.035f);
        _clipStreak      = BuildArpeggio(new float[]{ 440f, 554f, 659f, 880f }, 0.045f);
        _clipKillConfirm = BuildImpact(0.08f, 520f);
    }

    AudioClip BuildArpeggio(float[] freqs, float noteLen)
    {
        const int sr = 44100;
        int total = (int)(sr * (freqs.Length * noteLen + 0.15f));
        float[] d = new float[total];
        for (int n = 0; n < freqs.Length; n++)
        {
            int st  = (int)(n * noteLen * sr);
            int dur = (int)(noteLen * sr * 2.2f);
            for (int i = 0; i < dur && st + i < total; i++)
            {
                float t = (float)i / sr;
                float e = Mathf.Exp(-t * 9f);
                float s = Mathf.Sin(2*Mathf.PI*freqs[n]*t)*0.6f
                        + Mathf.Sin(4*Mathf.PI*freqs[n]*t)*0.12f;
                d[st+i] = Mathf.Clamp(d[st+i] + s*e*0.38f, -1f, 1f);
            }
        }
        var c = AudioClip.Create("arp", total, 1, sr, false);
        c.SetData(d, 0); return c;
    }

    AudioClip BuildImpact(float len, float freq)
    {
        const int sr = 44100;
        int n = (int)(sr*len); float[] d = new float[n];
        var rng = new System.Random(42);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i/sr;
            d[i] = ((float)(rng.NextDouble()*2-1)*0.45f
                  + Mathf.Sin(2*Mathf.PI*freq*t*Mathf.Exp(-t*8f))*0.35f)
                  * Mathf.Exp(-t*18f) * 0.75f;
        }
        var c = AudioClip.Create("hit", n, 1, sr, false); c.SetData(d,0); return c;
    }

    AudioClip BuildDeath()
    {
        const int sr = 44100;
        int n = (int)(sr*0.38f); float[] d = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i/sr;
            d[i] = (Mathf.Sin(2*Mathf.PI*Mathf.Max(40f,350f-t*800f)*t)*0.45f
                  + (float)(rng.NextDouble()*2-1)*0.2f) * Mathf.Exp(-t*5.5f) * 0.7f;
        }
        var c = AudioClip.Create("death", n, 1, sr, false); c.SetData(d,0); return c;
    }

    AudioClip BuildDamage()
    {
        const int sr = 44100;
        int n = (int)(sr*0.22f); float[] d = new float[n];
        var rng = new System.Random(13);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i/sr;
            d[i] = (Mathf.Sin(2*Mathf.PI*75f*t)*0.55f
                  + (float)(rng.NextDouble()*2-1)*0.35f) * Mathf.Exp(-t*11f) * 0.8f;
        }
        var c = AudioClip.Create("dmg", n, 1, sr, false); c.SetData(d,0); return c;
    }

    AudioClip BuildWhoosh()
    {
        const int sr = 44100; float len = 0.16f;
        int n = (int)(sr*len); float[] d = new float[n];
        var rng = new System.Random(99);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i/sr;
            float e = Mathf.Exp(-Mathf.Pow((t/len-0.3f)*6f, 2f));
            d[i] = (float)(rng.NextDouble()*2-1)
                 * Mathf.Sin(2*Mathf.PI*1200f*t+t*300f) * e * 0.42f;
        }
        var c = AudioClip.Create("whoosh", n, 1, sr, false); c.SetData(d,0); return c;
    }

    // ── Публичные Play ────────────────────────────────────────────────────
    public void PlayCoin()        => Play(_clipCoin,        0.70f, 1.10f);
    public void PlayHit()         => Play(_clipHit,         0.85f, Random.Range(0.9f, 1.1f));
    public void PlayDeath()       => Play(_clipDeath,       0.90f, 0.90f);
    public void PlayCheckpoint()  => Play(_clipCheckpoint,  0.80f, 1.00f);
    public void PlayDamage()      => Play(_clipDamage,      0.90f, 0.95f);
    public void PlayWhoosh()      => Play(_clipWhoosh,      0.50f, Random.Range(0.95f,1.05f));
    public void PlayHealth()      => Play(_clipHealth,      0.70f, 0.92f);
    public void PlayCrit()        => Play(_clipCrit,        0.95f, 0.75f);
    public void PlayHeadshot()    => Play(_clipHeadshot,    0.90f, 1.20f);
    public void PlayKillConfirm() => Play(_clipKillConfirm, 0.70f, 1.30f);
    public void PlayStreak(int count)
    {
        float pitch = 1.0f + Mathf.Min(count, 10) * 0.08f;
        Play(_clipStreak, 0.85f, pitch);
    }

    void Play(AudioClip clip, float vol, float pitch)
    {
        if (!clip || !_src) return;
        _src.pitch = pitch;
        _src.PlayOneShot(clip, vol);
    }

    // ════════════════════════════════════════════════════════
    // HIT MARKER — крестик в центре экрана при попадании
    // ════════════════════════════════════════════════════════
    public void ShowHitMarker(Color color)
    {
        _hitMarkerTimer = 0.3f;
        _hitMarkerColor = color;
    }
    public void ShowHitMarker() => ShowHitMarker(Color.white);

    // ════════════════════════════════════════════════════════
    // KILL FLASH — красная вспышка экрана при убийстве
    // ════════════════════════════════════════════════════════
    public void ShowKillFlash() => _killFlashAlpha = 0.35f;

    // ════════════════════════════════════════════════════════
    // KILL SLOWMO — кратковременное замедление при убийстве
    // ════════════════════════════════════════════════════════
    public void KillSlowmo(float scale = 0.35f, float duration = 0.18f)
    {
        Time.timeScale = scale;
        _slowmoTimer = duration;
    }

    public void BossKillSlowmo()
    {
        KillSlowmo(0.15f, 0.5f);
    }

    // ════════════════════════════════════════════════════════
    // RAGDOLL EXPLODE — тело разлетается при смерти
    // ════════════════════════════════════════════════════════
    public void RagdollExplode(Transform root, Vector3 deathPos)
    {
        if (root == null) return;
        var rends = root.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            if (r == null || r.gameObject == root.gameObject) continue;
            var go = r.gameObject;
            go.transform.SetParent(null, true);

            if (go.GetComponent<Collider>() == null)
            {
                var sc = go.AddComponent<SphereCollider>();
                sc.radius = 0.15f;
            }

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.3f, 1.2f);
            Vector3 dir = (go.transform.position - deathPos).normalized + Vector3.up * 0.6f;
            rb.AddForce(dir * Random.Range(3f, 8f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 12f, ForceMode.Impulse);

            Object.Destroy(go, Random.Range(1.2f, 2.5f));
        }
    }

    // ════════════════════════════════════════════════════════
    // KILL FEED — экранные popup (+100, HEADSHOT!, STREAK x5)
    // ════════════════════════════════════════════════════════
    public void ShowKillFeed(string text, Color color, float duration = 1.5f)
    {
        _killFeed.Add(new KFEntry {
            text = text, color = color,
            life = duration, maxLife = duration,
            yOffset = _nextKfY
        });
        _nextKfY += 28f;
        if (_nextKfY > 200f) _nextKfY = 0f;
    }

    // ════════════════════════════════════════════════════════
    // ЧАСТИЦЫ
    // ════════════════════════════════════════════════════════
    public void SpawnBurst(Vector3 pos, Color a, Color b,
                           int count=14, float speed=3.5f, float size=0.12f)
    {
        var go = new GameObject("FX_Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var mn = ps.main;
        mn.startLifetime   = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        mn.startSpeed      = new ParticleSystem.MinMaxCurve(speed*0.5f, speed*1.5f);
        mn.startSize       = new ParticleSystem.MinMaxCurve(size*0.6f, size*1.5f);
        mn.startColor      = new ParticleSystem.MinMaxGradient(a, b);
        mn.gravityModifier = new ParticleSystem.MinMaxCurve(0.25f);
        mn.simulationSpace = ParticleSystemSimulationSpace.World;
        mn.stopAction      = ParticleSystemStopAction.Destroy;
        var em = ps.emission; em.enabled = false;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.1f;
        var sl = ps.sizeOverLifetime; sl.enabled = true;
        sl.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0,1), new Keyframe(1,0)));
        SetParticleShader(go.GetComponent<ParticleSystemRenderer>());
        ps.Emit(count);
        Destroy(go, 2.5f);
    }

    public void SpawnRing(Vector3 pos, Color color, int count=24, float radius=0.8f)
    {
        var go = new GameObject("FX_Ring");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var mn = ps.main;
        mn.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        mn.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        mn.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        mn.startColor      = new ParticleSystem.MinMaxGradient(color, color*0.5f);
        mn.gravityModifier = 0f;
        mn.simulationSpace = ParticleSystemSimulationSpace.World;
        mn.stopAction      = ParticleSystemStopAction.Destroy;
        var em = ps.emission; em.enabled = false;
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = radius; sh.radiusThickness = 0f;
        var sl = ps.sizeOverLifetime; sl.enabled = true;
        sl.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0,1), new Keyframe(1,0)));
        SetParticleShader(go.GetComponent<ParticleSystemRenderer>());
        ps.Emit(count);
        Destroy(go, 2.5f);
    }

    static void SetParticleShader(ParticleSystemRenderer r)
    {
        if (r == null) return;
        // Перебираем шейдеры: URP → Built-in → Legacy → fallback
        string[] names = {
            "Universal Render Pipeline/Particles/Unlit",
            "Particles/Standard Unlit",
            "Legacy Shaders/Particles/Additive",
            "Legacy Shaders/Particles/Alpha Blended",
            "Sprites/Default", "Unlit/Color"
        };
        foreach (var name in names)
        {
            var s = Shader.Find(name);
            if (s == null) continue;
            r.material = new Material(s); return;
        }
    }

    // ════════════════════════════════════════════════════════
    // ПЛАВАЮЩИЙ ТЕКСТ (метод называется FloatText — класс FTEntry)
    // ════════════════════════════════════════════════════════
    public void FloatText(Vector3 worldPos, string text, Color color, float duration=1.1f)
    {
        _floats.Add(new FTEntry {
            text=text, color=color,
            worldPos=worldPos+Vector3.up*0.5f,
            life=duration, maxLife=duration
        });
    }

    void OnGUI()
    {
        int sw = Screen.width, sh = Screen.height;
        float cx = sw * 0.5f, cy = sh * 0.5f;

        // Kill flash
        if (_killFlashAlpha > 0f)
        {
            GUI.color = new Color(1f, 0.08f, 0.02f, _killFlashAlpha);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Hit marker — X в центре экрана
        if (_hitMarkerTimer > 0f)
        {
            float a = Mathf.Clamp01(_hitMarkerTimer / 0.3f);
            float s = 8f + (1f - a) * 4f;
            GUI.color = new Color(_hitMarkerColor.r, _hitMarkerColor.g, _hitMarkerColor.b, a * 0.95f);
            // Четыре диагональные линии = X
            DrawRotatedLine(cx, cy, s, 45f);
            DrawRotatedLine(cx, cy, s, -45f);
            DrawRotatedLine(cx, cy, s, 135f);
            DrawRotatedLine(cx, cy, s, -135f);
            GUI.color = Color.white;
        }

        // Kill feed — правая часть экрана
        if (_killFeed.Count > 0)
        {
            GUIStyle kfStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };

            float baseY = sh * 0.35f;
            for (int i = 0; i < _killFeed.Count; i++)
            {
                var kf = _killFeed[i];
                float alpha = Mathf.Clamp01(kf.life / kf.maxLife * 2f);
                float slide = (1f - Mathf.Clamp01(kf.life / kf.maxLife * 5f)) * 0f; // instant
                kfStyle.normal.textColor = new Color(0, 0, 0, alpha * 0.7f);
                GUI.Label(new Rect(sw - 322, baseY + i * 28 + 2, 300, 28), kf.text, kfStyle);
                kfStyle.normal.textColor = new Color(kf.color.r, kf.color.g, kf.color.b, alpha);
                GUI.Label(new Rect(sw - 320, baseY + i * 28, 300, 28), kf.text, kfStyle);
            }
        }

        // Floating text в мире
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;
        foreach (var ft in _floats)
        {
            Vector3 sp = _cam.WorldToScreenPoint(ft.worldPos);
            if (sp.z < 0f) continue;
            float alpha = Mathf.Clamp01(ft.life / ft.maxLife);
            float sc    = Mathf.Lerp(0.7f, 1.3f, Mathf.Clamp01(ft.life/ft.maxLife*3f));
            int   fs    = Mathf.RoundToInt(20f * sc);
            float sx = sp.x, sy = Screen.height - sp.y;
            var style = new GUIStyle(GUI.skin.label)
                { fontSize=fs, fontStyle=FontStyle.Bold, alignment=TextAnchor.MiddleCenter };
            style.normal.textColor = new Color(0,0,0, alpha*0.8f);
            GUI.Label(new Rect(sx-61, sy-13, 124, 32), ft.text, style);
            style.normal.textColor = new Color(ft.color.r, ft.color.g, ft.color.b, alpha);
            GUI.Label(new Rect(sx-60, sy-15, 124, 32), ft.text, style);
        }
    }

    void DrawRotatedLine(float cx, float cy, float len, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float dx = Mathf.Cos(rad) * len;
        float dy = Mathf.Sin(rad) * len;
        float x1 = cx + dx * 0.4f, y1 = cy + dy * 0.4f;
        float x2 = cx + dx, y2 = cy + dy;
        GUI.DrawTexture(new Rect(Mathf.Min(x1, x2), Mathf.Min(y1, y2),
            Mathf.Max(Mathf.Abs(x2 - x1), 2.5f), Mathf.Max(Mathf.Abs(y2 - y1), 2.5f)),
            Texture2D.whiteTexture);
    }

    // ── Вспомогательное для EnemyAI ──────────────────────────────────────
    public IEnumerator FlashColor(Renderer[] rends, Color[] orig, Color flash, float dur=0.12f)
    {
        SetColors(rends, flash);
        yield return new WaitForSeconds(dur);
        SetColors(rends, orig);
    }
    public void SetColors(Renderer[] rends, Color c)
    {
        if (rends==null) return;
        foreach (var r in rends) if (r) r.material.color = c;
    }
    public void SetColors(Renderer[] rends, Color[] cols)
    {
        if (rends==null||cols==null) return;
        for (int i=0; i<rends.Length&&i<cols.Length; i++)
            if (rends[i]) rends[i].material.color = cols[i];
    }
}
