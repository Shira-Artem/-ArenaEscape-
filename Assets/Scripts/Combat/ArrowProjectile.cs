using System.Collections;
using UnityEngine;

/// <summary>
/// ArrowProjectile v13 — улучшенная стрела для лука.
/// Что делает:
/// - даёт стреле заметный оранжево-жёлтый шлейф;
/// - при попадании во врага наносит урон, показывает красный текст и трясёт камеру;
/// - при попадании в стену/пол/декор втыкает стрелу в поверхность, даёт каменные искры и удаляет её через 3 секунды.
/// </summary>
public class ArrowProjectile : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 35;

    [Header("Impact")]
    public float enemySearchRadius = 2.0f;
    public float stuckLifetime = 3.0f;

    [HideInInspector] public Vector3 shooterPosition;
    [HideInInspector] public bool    pierce;          // полный заряд — стрела пробивает насквозь

    bool _hit;
    bool _isFire;
    Rigidbody _rb;
    Collider _myCollider;
    TrailRenderer _trail;
    Vector3 _lastVelocityDir = Vector3.forward;

    // Свип-детект попаданий (против туннелирования у быстрых стрел)
    Vector3 _prevPos;
    readonly System.Collections.Generic.HashSet<EnemyAI> _pierced =
        new System.Collections.Generic.HashSet<EnemyAI>();
    static readonly int RaycastMask = ~0;
    // Статический буфер — нет аллокаций каждый кадр, нет мусора для GC.
    static readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();

        // Лук теперь без падения и без физических коллизий — попадания через
        // свип-raycast (Update). Коллайдер отключаем, чтобы стрела не отскакивала.
        if (_rb != null) _rb.useGravity = false;
        if (_myCollider != null) _myCollider.enabled = false;
        _prevPos = transform.position;

        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
            _lastVelocityDir = _rb.linearVelocity.normalized;
        else
            _lastVelocityDir = transform.up;

        // Огненный режим — потребляем один заряд при рождении стрелы
        if (FireArrowMode.Active && FireArrowMode.ConsumeArrow())
        {
            _isFire = true;
            damage  = Mathf.RoundToInt(damage * 1.4f);  // +40% базового урона
        }

        EnsureTrail();
        if (_isFire) ApplyFireVisuals();
        else EnsureArrowLooksVisible();
    }

    void Update()
    {
        if (_hit && !pierce) return;

        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            _lastVelocityDir = _rb.linearVelocity.normalized;

            // Держим цилиндр стрелы вдоль направления полёта.
            // У Unity-цилиндра длинная ось — local Y, поэтому используем FromToRotation(Vector3.up, dir).
            transform.rotation = Quaternion.FromToRotation(Vector3.up, _lastVelocityDir);
        }

        SweepForHit();
    }

    /// Луч от прошлой позиции к текущей: ловит врагов и стены даже на больших
    /// скоростях (обычный коллайдер бы проскочил сквозь тонкую цель).
    void SweepForHit()
    {
        Vector3 cur = transform.position;
        Vector3 seg = cur - _prevPos;
        float dist = seg.magnitude;
        _prevPos = cur;
        if (dist < 0.0001f) return;

        Vector3 dir = seg / dist;
        // NonAlloc — пишем в статический буфер, ноль аллокаций GC.
        int count = Physics.RaycastNonAlloc(_prevPos - dir * dist, dir, _hitBuffer,
            dist + 0.05f, RaycastMask, QueryTriggerInteraction.Ignore);
        if (count == 0) return;

        // Сортируем только занятую часть буфера.
        System.Array.Sort(_hitBuffer, 0, count, HitDistanceComparer.Instance);
        for (int i = 0; i < count; i++)
        {
            var h = _hitBuffer[i];
            if (h.collider == null) continue;
            if (h.collider.CompareTag("Player")) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;

            EnemyAI ai = h.collider.GetComponent<EnemyAI>() ?? h.collider.GetComponentInParent<EnemyAI>();
            if (ai != null)
            {
                if (ai.state == EnemyAI.State.Dead) continue;
                if (pierce && _pierced.Contains(ai)) continue;
                HitEnemy(ai, h.point);
                if (!pierce) return;
                continue;
            }

            // Стена/пол/декор — стрела останавливается.
            _hit = true;
            Vector3 incoming = dir;
            StopPhysics();
            StickIntoSurface(h.collider.transform, h.point, incoming, h.normal);
            return;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (_hit) return;
        if (col.collider.CompareTag("Player")) return;

        _hit = true;

        Vector3 hitPoint = col.contacts.Length > 0 ? col.contacts[0].point : transform.position;
        Vector3 hitNormal = col.contacts.Length > 0 ? col.contacts[0].normal : -_lastVelocityDir;
        Vector3 incomingDir = GetIncomingDirection();

        StopPhysics();

        EnemyAI ai = FindEnemyFromCollision(col, hitPoint);
        if (ai != null && ai.state != EnemyAI.State.Dead)
        {
            HitEnemy(ai, hitPoint);
            return;
        }

        StickIntoSurface(col.transform, hitPoint, incomingDir, hitNormal);
    }

    Vector3 GetIncomingDirection()
    {
        if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f)
            return _rb.linearVelocity.normalized;

        if (_lastVelocityDir.sqrMagnitude > 0.01f)
            return _lastVelocityDir.normalized;

        return transform.up;
    }

    void StopPhysics()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        if (_myCollider != null)
            _myCollider.enabled = false;
    }

    EnemyAI FindEnemyFromCollision(Collision col, Vector3 hitPoint)
    {
        EnemyAI ai = col.collider.GetComponent<EnemyAI>()
                  ?? col.collider.GetComponentInParent<EnemyAI>();

        if (ai != null) return ai;

        // На случай, если стрела попала не в главный коллайдер врага, а рядом с ним.
        Collider[] around = Physics.OverlapSphere(hitPoint, enemySearchRadius);
        foreach (Collider c in around)
        {
            EnemyAI found = c.GetComponent<EnemyAI>() ?? c.GetComponentInParent<EnemyAI>();
            if (found != null && found.state != EnemyAI.State.Dead)
                return found;
        }

        return null;
    }

    void HitEnemy(EnemyAI ai, Vector3 hitPoint)
    {
        if (pierce) _pierced.Add(ai);
        bool isHeadshot = DetectHeadshot(ai, hitPoint);
        float killDist = shooterPosition.sqrMagnitude > 0.01f
            ? Vector3.Distance(shooterPosition, hitPoint)
            : 0f;

        if (isHeadshot)
            ai.SetNextHitBonus(RunScoreManager.KillBonus.Headshot);

        ai.SetKillDistance(killDist);
        ai.TakeDamage(damage);

        if (_isFire) DoFireAoE(ai, hitPoint);

        if (FeedbackManager.Instance != null)
        {
            var fb = FeedbackManager.Instance;
            if (_isFire)
            {
                // Огненный взрыв вместо красного burst
                fb.SpawnBurst(hitPoint,
                    new Color(1f, 0.45f, 0.02f), new Color(1f, 0.12f, 0f),
                    22, 4.5f, 0.07f);
                fb.SpawnRing(hitPoint, new Color(1f, 0.35f, 0.0f), 16, 3.5f);
                fb.FloatText(ai.transform.position + Vector3.up * 1.3f,
                    "ОГОНЬ! -" + damage, new Color(1f, 0.5f, 0.05f), 1.3f);
            }
            else
            {
                fb.SpawnBurst(hitPoint,
                    new Color(1f, 0.2f, 0.08f), new Color(0.5f, 0f, 0f),
                    10, 2.8f, 0.06f);
                fb.FloatText(ai.transform.position + Vector3.up * 1.3f,
                    "-" + damage, Color.red, 1.0f);
            }
            fb.ShowHitMarker();

            if (isHeadshot)
            {
                fb.PlayHeadshot();
                fb.FloatText(ai.transform.position + Vector3.up * 2.6f,
                    "HEADSHOT!", new Color(1f, 0.3f, 0.1f), 1.2f);
            }
        }

        CameraShake shake = null;
        if (Camera.main != null) shake = Camera.main.GetComponent<CameraShake>();
        if (shake == null) shake = Object.FindFirstObjectByType<CameraShake>();
        shake?.Shake(isHeadshot ? 0.24f : 0.11f, isHeadshot ? 0.22f : 0.12f);

        // Hit-stop — попадание стрелой теперь «бьёт» так же весомо, как меч.
        HitStop.Freeze(isHeadshot ? 0.060f : 0.030f);

        // Пробивающая стрела (полный заряд) летит дальше сквозь врага.
        if (pierce)
        {
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.8f)); // затухание на каждой цели
            return;
        }

        Destroy(gameObject);
    }

    void DoFireAoE(EnemyAI primaryTarget, Vector3 hitPoint)
    {
        const float AOE_RADIUS = 4.5f;
        const int   AOE_DAMAGE = 30;

        GameManager.Instance?.ShowMessage(
            $"★ Огненный взрыв! (x{FireArrowMode.ArrowsLeft + 1} осталось)",
            GameManager.MsgType.Damage, 1.6f);

        Collider[] hits = Physics.OverlapSphere(hitPoint, AOE_RADIUS);
        foreach (Collider c in hits)
        {
            EnemyAI aoe = c.GetComponent<EnemyAI>() ?? c.GetComponentInParent<EnemyAI>();
            if (aoe == null || aoe == primaryTarget || aoe.state == EnemyAI.State.Dead) continue;
            float dist = Vector3.Distance(hitPoint, aoe.transform.position);
            int dmg = Mathf.RoundToInt(Mathf.Lerp(AOE_DAMAGE, AOE_DAMAGE * 0.35f, dist / AOE_RADIUS));
            aoe.TakeDamage(dmg);
            FeedbackManager.Instance?.FloatText(
                aoe.transform.position + Vector3.up * 1.2f,
                "ОГОНЬ -" + dmg, new Color(1f, 0.4f, 0f), 0.9f);
        }
    }

    bool DetectHeadshot(EnemyAI ai, Vector3 hitPoint)
    {
        var col = ai.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            float topY = ai.transform.position.y + col.center.y + col.height * 0.5f;
            float headThreshold = topY - col.height * 0.18f;
            return hitPoint.y >= headThreshold;
        }
        float fallbackTop = ai.transform.position.y + 2f;
        return hitPoint.y >= fallbackTop - 0.4f;
    }

    void StickIntoSurface(Transform hitTransform, Vector3 hitPoint, Vector3 incomingDir, Vector3 hitNormal)
    {
        transform.SetParent(hitTransform, true);

        // Ставим стрелу так, будто она немного вошла в поверхность.
        transform.position = hitPoint + incomingDir.normalized * 0.18f;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, incomingDir.normalized);

        CreateWallSparks(hitPoint, hitNormal);
        FeedbackManager.Instance?.PlayWhoosh();

        if (_trail != null)
            StartCoroutine(FadeTrail());

        Destroy(gameObject, stuckLifetime);
    }

    void EnsureTrail()
    {
        _trail = GetComponent<TrailRenderer>();
        if (_trail == null)
            _trail = gameObject.AddComponent<TrailRenderer>();

        _trail.time = 0.22f;
        _trail.startWidth = 0.09f;
        _trail.endWidth = 0.0f;
        _trail.minVertexDistance = 0.02f;
        _trail.autodestruct = false;
        _trail.material = MakeTrailMaterial();
        _trail.startColor = new Color(1f, 0.92f, 0.35f, 0.75f);
        _trail.endColor = new Color(1f, 0.35f, 0f, 0f);
    }

    Material MakeTrailMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.color = new Color(1f, 0.65f, 0.15f, 0.7f);
        return mat;
    }

    void ApplyFireVisuals()
    {
        // Оранжево-красный шлейф
        if (_trail != null)
        {
            _trail.startColor = new Color(1f, 0.45f, 0.02f, 0.90f);
            _trail.endColor   = new Color(1f, 0.08f, 0.0f, 0f);
            _trail.startWidth = 0.14f;
            _trail.time = 0.32f;
        }

        // PointLight на стреле
        var lgGo = new GameObject("FireLight");
        lgGo.transform.SetParent(transform, false);
        var lt = lgGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.40f, 0.02f);
        lt.range = 4.0f; lt.intensity = 2.2f;

        // Сам снаряд — огненно-оранжевый и светящийся
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Shader s = Shader.Find("Standard");
            if (s != null) r.material = new Material(s) { color = new Color(1f, 0.38f, 0.02f) };
            else r.material.color = new Color(1f, 0.38f, 0.02f);
            r.material.EnableKeyword("_EMISSION");
            if (r.material.HasProperty("_EmissionColor"))
                r.material.SetColor("_EmissionColor", new Color(1f, 0.40f, 0.05f) * 2.4f);
        }

        AttachFlameParticles();
    }

    // Живой огненный хвост на стреле (пламя, а не только шлейф).
    void AttachFlameParticles()
    {
        var go = new GameObject("FireArrowFlame");
        go.transform.SetParent(transform, false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.20f, 0.95f), new Color(1f, 0.25f, 0.02f, 0.95f));
        main.startSize     = new ParticleSystem.MinMaxCurve(0.10f, 0.28f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
        main.gravityModifier = -0.05f;            // язычки тянутся вверх
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.rateOverTime = 55f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f, 0.2f, 0.0f), 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        if (rend != null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Sprites/Default");
            if (sh != null) rend.material = new Material(sh) { color = new Color(1f, 0.5f, 0.1f) };
        }

        ps.Play();
    }

    void EnsureArrowLooksVisible()
    {
        // Усиливаем видимость самой стрелы, но не ломаем объект, который создаёт WeaponController.
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = new Color(0.85f, 0.55f, 0.18f);
                r.material = mat;
            }
            else
            {
                r.material.color = new Color(0.85f, 0.55f, 0.18f);
            }
        }

        // Маленький яркий наконечник, чтобы стрела не выглядела как простой коричневый цилиндр.
        if (transform.Find("Arrow_Tip") == null)
        {
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Arrow_Tip";
            tip.transform.SetParent(transform, false);
            tip.transform.localPosition = Vector3.up * 0.43f;
            tip.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
            Destroy(tip.GetComponent<Collider>());

            Renderer tr = tip.GetComponent<Renderer>();
            if (tr != null)
            {
                Shader shader = Shader.Find("Standard");
                Material mat = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1f, 0.9f, 0.25f);
                tr.material = mat;
            }
        }

        // Простые перья сзади — чисто визуал, без коллайдеров.
        if (transform.Find("Arrow_Feather_A") == null)
        {
            CreateFeather("Arrow_Feather_A", new Vector3(0.09f, -0.36f, 0f), new Vector3(0.02f, 0.12f, 0.06f));
            CreateFeather("Arrow_Feather_B", new Vector3(-0.09f, -0.36f, 0f), new Vector3(0.02f, 0.12f, 0.06f));
        }
    }

    void CreateFeather(string name, Vector3 localPos, Vector3 localScale)
    {
        GameObject feather = GameObject.CreatePrimitive(PrimitiveType.Cube);
        feather.name = name;
        feather.transform.SetParent(transform, false);
        feather.transform.localPosition = localPos;
        feather.transform.localScale = localScale;
        Destroy(feather.GetComponent<Collider>());

        Renderer r = feather.GetComponent<Renderer>();
        if (r != null)
        {
            Shader shader = Shader.Find("Standard");
            Material mat = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.95f, 0.95f, 0.75f);
            r.material = mat;
        }
    }

    void CreateWallSparks(Vector3 pos, Vector3 normal)
    {
        GameObject sparksObj = new GameObject("FX_Arrow_Wall_Sparks");
        sparksObj.transform.position = pos;
        sparksObj.transform.rotation = Quaternion.LookRotation(normal == Vector3.zero ? Vector3.up : normal);

        ParticleSystem ps = sparksObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.85f, 0.72f, 0.5f, 0.9f),
            new Color(0.45f, 0.45f, 0.45f, 0.8f)
        );
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.6f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.45f);
        main.gravityModifier = 0.2f;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 14)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.08f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null) renderer.material = new Material(shader);
        }

        ps.Play();
        Destroy(sparksObj, 2.0f);
    }

    IEnumerator FadeTrail()
    {
        if (_trail == null) yield break;

        float t = 0f;
        float duration = 1.0f;
        Color startCol = _trail.startColor;
        Color endCol = _trail.endColor;

        while (t < duration)
        {
            if (_trail == null) yield break;
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            _trail.startColor = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0f), k);
            _trail.endColor = Color.Lerp(endCol, new Color(endCol.r, endCol.g, endCol.b, 0f), k);
            yield return null;
        }
    }
}

// Без аллокации лямбды — статический компаратор для Array.Sort(_hitBuffer).
class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
{
    public static readonly HitDistanceComparer Instance = new HitDistanceComparer();
    public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
}
