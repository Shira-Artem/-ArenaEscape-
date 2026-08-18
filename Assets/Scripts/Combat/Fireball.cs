using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    int _damage;
    float _speed;
    float _radius;
    float _maxDist;
    Vector3 _startPos;
    Vector3 _dir;
    bool _exploded;

    Light _light;
    TrailRenderer _trail;
    TrailRenderer _trail2;
    GameObject _core;
    GameObject _glow;
    GameObject _outerGlow;
    Transform[] _embers;
    float _spawnTime;

    public static void Launch(Vector3 pos, Vector3 dir, int damage, float speed, float radius, float maxDist)
    {
        var go = new GameObject("Fireball");
        go.transform.position = pos;
        var fb = go.AddComponent<Fireball>();
        fb._damage = damage;
        fb._speed = speed;
        fb._radius = radius;
        fb._maxDist = maxDist;
        fb._startPos = pos;
        fb._dir = dir.normalized;
        fb._spawnTime = Time.time;
        fb.BuildVisual();
    }

    static Shader SafeShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("Diffuse");
    }

    static Material EmissiveMat(Color baseCol, Color emissionCol, float power, bool transparent = false)
    {
        var mat = new Material(SafeShader());
        if (transparent)
        {
            mat.color = new Color(baseCol.r, baseCol.g, baseCol.b, baseCol.a);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
        }
        else
        {
            mat.color = baseCol;
        }
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionCol * power);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        return mat;
    }

    void BuildVisual()
    {
        // === ЯДРО — яркий раскалённый шар ===
        _core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _core.name = "Core";
        _core.transform.SetParent(transform, false);
        _core.transform.localScale = Vector3.one * 0.75f;
        Destroy(_core.GetComponent<Collider>());
        _core.GetComponent<Renderer>().material =
            EmissiveMat(new Color(1f, 0.85f, 0.3f), new Color(1f, 0.7f, 0.15f), 6f);

        // === ВНУТРЕННЕЕ СВЕЧЕНИЕ ===
        _glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _glow.name = "InnerGlow";
        _glow.transform.SetParent(transform, false);
        _glow.transform.localScale = Vector3.one * 1.4f;
        Destroy(_glow.GetComponent<Collider>());
        _glow.GetComponent<Renderer>().material =
            EmissiveMat(new Color(1f, 0.5f, 0.08f, 0.5f), new Color(1f, 0.45f, 0.05f), 4f, true);

        // === ВНЕШНЯЯ АУРА ===
        _outerGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _outerGlow.name = "OuterAura";
        _outerGlow.transform.SetParent(transform, false);
        _outerGlow.transform.localScale = Vector3.one * 2.2f;
        Destroy(_outerGlow.GetComponent<Collider>());
        _outerGlow.GetComponent<Renderer>().material =
            EmissiveMat(new Color(1f, 0.3f, 0f, 0.18f), new Color(1f, 0.25f, 0f), 2.5f, true);

        // === СВЕТ — ярче, дальше ===
        _light = gameObject.AddComponent<Light>();
        _light.type = LightType.Point;
        _light.color = new Color(1f, 0.55f, 0.1f);
        _light.range = 22f;
        _light.intensity = 6f;

        // === ГЛАВНЫЙ ТРЕЙЛ — широкий огненный ===
        _trail = gameObject.AddComponent<TrailRenderer>();
        _trail.startWidth = 1.2f;
        _trail.endWidth = 0.08f;
        _trail.time = 0.5f;
        _trail.material = new Material(Shader.Find("Sprites/Default"));
        _trail.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        _trail.endColor = new Color(1f, 0.15f, 0f, 0f);
        _trail.numCornerVertices = 4;
        _trail.numCapVertices = 4;

        // === ВТОРОЙ ТРЕЙЛ — тонкий яркий сердечник ===
        var t2go = new GameObject("InnerTrail");
        t2go.transform.SetParent(transform, false);
        _trail2 = t2go.AddComponent<TrailRenderer>();
        _trail2.startWidth = 0.35f;
        _trail2.endWidth = 0.02f;
        _trail2.time = 0.35f;
        _trail2.material = new Material(Shader.Find("Sprites/Default"));
        _trail2.startColor = new Color(1f, 0.9f, 0.4f, 1f);
        _trail2.endColor = new Color(1f, 0.7f, 0.1f, 0f);
        _trail2.numCornerVertices = 3;

        // === ИСКРЫ-ЭМБЕРЫ вокруг шара ===
        _embers = new Transform[6];
        for (int i = 0; i < _embers.Length; i++)
        {
            var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            e.name = $"Ember_{i}";
            e.transform.SetParent(transform, false);
            e.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);
            Destroy(e.GetComponent<Collider>());
            e.GetComponent<Renderer>().material =
                EmissiveMat(new Color(1f, 0.7f, 0.15f), new Color(1f, 0.5f, 0f), 5f);
            _embers[i] = e.transform;
        }
    }

    void Update()
    {
        if (_exploded) return;

        float dt = Time.deltaTime;
        float age = Time.time - _spawnTime;
        Vector3 move = _dir * _speed * dt;

        if (Physics.SphereCast(transform.position, 0.35f, _dir, out RaycastHit hit, move.magnitude + 0.5f))
        {
            var enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null || !hit.collider.isTrigger)
            {
                Explode(hit.point);
                return;
            }
        }

        transform.position += move;

        if (Vector3.Distance(_startPos, transform.position) > _maxDist)
            Explode(transform.position);

        // --- Пульсация ядра ---
        float pulse = 0.85f + Mathf.Sin(age * 25f) * 0.2f;
        _core.transform.localScale = Vector3.one * 0.75f * pulse;

        // --- Внутреннее свечение дышит ---
        float glowPulse = 1.3f + Mathf.Sin(age * 15f) * 0.2f;
        _glow.transform.localScale = Vector3.one * glowPulse;

        // --- Внешняя аура мерцает ---
        float auraPulse = 2.0f + Mathf.Sin(age * 8f + 1.3f) * 0.4f;
        _outerGlow.transform.localScale = Vector3.one * auraPulse;

        // --- Свет мерцает ---
        _light.intensity = 5f + Mathf.Sin(age * 18f) * 2f;

        // --- Искры вращаются вокруг ---
        for (int i = 0; i < _embers.Length; i++)
        {
            float a = age * (3f + i * 0.7f) + i * Mathf.PI * 2f / _embers.Length;
            float r = 0.6f + Mathf.Sin(age * 5f + i) * 0.25f;
            _embers[i].localPosition = new Vector3(
                Mathf.Cos(a) * r,
                Mathf.Sin(a * 1.3f + i) * r * 0.7f,
                Mathf.Sin(a) * r);
        }

        // --- Вращение всего шара для динамики ---
        transform.Rotate(Vector3.forward, 360f * dt, Space.Self);
    }

    void Explode(Vector3 pos)
    {
        _exploded = true;
        transform.position = pos;

        // --- AoE урон + knockback ---
        var all = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (e == null || e.state == EnemyAI.State.Dead) continue;
            float dist = Vector3.Distance(pos, e.transform.position);
            if (dist <= _radius)
            {
                float falloff = 1f - dist / _radius;
                int dmg = Mathf.RoundToInt(_damage * Mathf.Max(falloff, 0.3f));
                e.TakeDamage(dmg);

                Vector3 knockDir = (e.transform.position - pos).normalized;
                e.ApplyKnockback(knockDir, 5f * Mathf.Max(falloff, 0.4f));
            }
        }

        // --- Мощные частицы взрыва ---
        if (FeedbackManager.Instance != null)
        {
            var fb = FeedbackManager.Instance;
            fb.SpawnBurst(pos, new Color(1f, 0.6f, 0.08f), new Color(1f, 0.15f, 0f), 80, 12f, 0.4f);
            fb.SpawnBurst(pos + Vector3.up * 0.5f, new Color(1f, 0.95f, 0.3f), new Color(1f, 0.7f, 0f), 50, 8f, 0.25f);
            fb.SpawnBurst(pos, new Color(1f, 0.3f, 0f), new Color(0.6f, 0.05f, 0f), 35, 15f, 0.5f);
            fb.SpawnRing(pos, new Color(1f, 0.5f, 0.08f), 60, _radius * 0.7f);
            fb.SpawnRing(pos + Vector3.up * 0.3f, new Color(1f, 0.8f, 0.2f), 40, _radius * 0.4f);
            fb.FloatText(pos + Vector3.up * 2.5f, "ОГОНЬ!", new Color(1f, 0.7f, 0.15f), 2f);
        }

        var shake = Camera.main?.GetComponent<CameraShake>();
        shake?.ShakeHard();

        StartCoroutine(ExplodeVisual());
    }

    IEnumerator ExplodeVisual()
    {
        if (_trail) _trail.enabled = false;
        if (_trail2) _trail2.enabled = false;
        if (_core) _core.SetActive(false);
        if (_glow) _glow.SetActive(false);
        if (_outerGlow) _outerGlow.SetActive(false);
        if (_embers != null)
            foreach (var e in _embers)
                if (e) e.gameObject.SetActive(false);

        // === Огненная сфера взрыва ===
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = transform.position;
        Destroy(sphere.GetComponent<Collider>());
        sphere.GetComponent<Renderer>().material =
            EmissiveMat(new Color(1f, 0.55f, 0.1f, 0.9f), new Color(1f, 0.4f, 0.05f), 8f, true);

        // === Ударная волна (кольцо) ===
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position;
        Destroy(ring.GetComponent<Collider>());
        ring.GetComponent<Renderer>().material =
            EmissiveMat(new Color(1f, 0.7f, 0.2f, 0.6f), new Color(1f, 0.5f, 0.1f), 5f, true);

        // === Вспышка света ===
        var flashGo = new GameObject("ExplosionFlash");
        flashGo.transform.position = transform.position;
        var flash = flashGo.AddComponent<Light>();
        flash.type = LightType.Point;
        flash.color = new Color(1f, 0.65f, 0.15f);
        flash.intensity = 18f;
        flash.range = 40f;

        float t = 0f;
        float dur = 0.55f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;

            float s = Mathf.Lerp(0.3f, _radius * 2f, Mathf.Sqrt(p));
            sphere.transform.localScale = Vector3.one * s;
            var sr = sphere.GetComponent<Renderer>();
            if (sr) sr.material.color = new Color(1f, 0.55f, 0.1f, Mathf.Lerp(0.9f, 0f, p * p));

            float rs = Mathf.Lerp(0.5f, _radius * 3f, p);
            ring.transform.localScale = new Vector3(rs, 0.05f, rs);
            var rr = ring.GetComponent<Renderer>();
            if (rr) rr.material.color = new Color(1f, 0.7f, 0.2f, Mathf.Lerp(0.6f, 0f, p));

            if (_light) _light.intensity = Mathf.Lerp(10f, 0f, p);
            flash.intensity = Mathf.Lerp(18f, 0f, p);
            flash.range = Mathf.Lerp(40f, 60f, p);

            yield return null;
        }

        Destroy(sphere);
        Destroy(ring);
        Destroy(flashGo);
        Destroy(gameObject);
    }
}
