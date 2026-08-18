using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Dead }
    public State state = State.Patrol;

    [Header("Маршрут")]
    public Transform[] waypoints;

    [Header("Дальность")]
    public float chaseRange  = 11f;
    public float attackRange = 1.9f;

    [Header("Бой")]
    public int   damage   = 15;
    public float cooldown = 1.69f;

    [Header("Дальняя атака")]
    public bool  useRangedAttack = false;
    public int   rangedDamage = 10;
    public float rangedRange = 16f;
    public float rangedCooldown = 2.4f;
    public float rangedProjectileSpeed = 14f;
    public Color rangedProjectileColor = new Color(1f, 0.65f, 0.12f);
    public string rangedAttackLabel = "СТРЕЛА";

    [Header("Здоровье")]
    public int maxHp = 60;

    [Header("Специальные параметры")]
    public float rageHpThreshold = 0.3f;
    public float retreatRange = 0f;

    [Header("Flanking")]
    public float flankRadius = 0f; // 0 = disabled; >0 = side offset from player when chasing

    [Header("Финальный босс")]
    public bool isFinalBoss = false;

    [Header("Scoring")]
    public RunScoreManager.EnemyType enemyScoreType = RunScoreManager.EnemyType.OrcWarrior;

    [Header("Части тела для мигания")]
    public GameObject[] bodyParts;

    int    _hp;
    public int CurrentHp => _hp;
    bool   _isRaging;
    float  _baseSpeed;
    RunScoreManager.KillBonus _lastHitBonus = RunScoreManager.KillBonus.None;
    float _lastKillDistance;

    internal static Transform    _cachedPlayer;
    internal static PlayerHealth _cachedPh;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { _cachedPlayer = null; _cachedPh = null; }

    NavMeshAgent _agent;
    Transform    _player;
    PlayerHealth _ph;
    int          _wp;
    float        _lastAtk;
    float        _lastRangedAtk;

    float   _walkTimer;
    float   _flankTimer;
    Vector3 _flankDest;
    Transform[] _visuals;
    Vector3[]   _visualBaseLocalPositions;
    Renderer[] _bodyRends;
    Color[]    _origColors;

    Transform _magicOrb;
    float     _baseOrbLocalY;
    Transform _bossEyeL, _bossEyeR;
    bool      _bossEyesRaging;

    bool AgentReady => _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _hp    = maxHp;
        _baseSpeed = _agent != null ? _agent.speed : 0f;

        if (_cachedPlayer == null)
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) { _cachedPlayer = po.transform; _cachedPh = po.GetComponent<PlayerHealth>(); }
        }
        _player = _cachedPlayer;
        _ph = _cachedPh;

        CacheVisuals();
        CacheColors();
        CacheSpecialParts();
        GameManager.Instance?.RegisterEnemy();

        // Не двигаемся до готовности NavMesh — ждём OnNavMeshReady().
        SafeStop(true);
    }

    /// Вызывается из SceneBuilder после BuildNavMesh()
    public void OnNavMeshReady()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_agent == null) return;

        if (!TryPlaceOnNavMesh(5.5f)) return;

        SafeStop(false);
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
            TrySetDestination(waypoints[0].position, 5.5f);
    }

    bool TryPlaceOnNavMesh(float radius)
    {
        if (_agent == null) return false;
        if (_agent.isOnNavMesh) return true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
            return _agent.isOnNavMesh;
        }
        return false;
    }

    void SafeStop(bool stop)
    {
        if (AgentReady) _agent.isStopped = stop;
    }

    bool TrySetDestination(Vector3 dest, float sampleRadius = 4f)
    {
        if (!AgentReady) return false;

        if (NavMesh.SamplePosition(dest, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            dest = hit.position;

        _agent.isStopped = false;
        return _agent.SetDestination(dest);
    }

    void CacheVisuals()
    {
        int count = transform.childCount;
        _visuals = new Transform[count];
        _visualBaseLocalPositions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            _visuals[i] = transform.GetChild(i);
            _visualBaseLocalPositions[i] = _visuals[i].localPosition;
        }
    }

    void CacheColors()
    {
        if (bodyParts == null || bodyParts.Length == 0) return;
        _bodyRends  = new Renderer[bodyParts.Length];
        _origColors = new Color[bodyParts.Length];
        for (int i = 0; i < bodyParts.Length; i++)
        {
            if (!bodyParts[i]) continue;
            var r = bodyParts[i].GetComponent<Renderer>();
            if (!r) continue;
            _bodyRends[i]  = r;
            _origColors[i] = r.material.color;
        }
    }

    void Update()
    {
        if (state == State.Dead) return;

        if (_player == null)
        {
            if (_cachedPlayer != null)
            {
                _player = _cachedPlayer;
                _ph = _cachedPh;
            }
            else
            {
                var po = GameObject.FindGameObjectWithTag("Player");
                if (po) { _cachedPlayer = po.transform; _cachedPh = po.GetComponent<PlayerHealth>(); _player = _cachedPlayer; _ph = _cachedPh; }
                else return;
            }
        }

        bool onNavMesh = AgentReady;
        if (!onNavMesh)
            TryPlaceOnNavMesh(5.5f);

        onNavMesh = AgentReady;

        float dist = Vector3.Distance(transform.position, _player.position);
        WalkBob();
        AnimateMagicOrb();
        AnimateBossEyes();

        // Fallback: no NavMesh — chase and attack using direct transform movement
        if (!onNavMesh)
        {
            FallbackCombat(dist);
            return;
        }

        switch (state)
        {
            case State.Patrol:
                if (useRangedAttack && dist < rangedRange && CanSeePlayer())
                {
                    state = State.Attack;
                    break;
                }
                if (dist < chaseRange) { state = State.Chase; break; }
                DoPatrol();
                break;

            case State.Chase:
                if (useRangedAttack && dist <= rangedRange && CanSeePlayer())
                {
                    state = State.Attack;
                    break;
                }

                if (useRangedAttack && retreatRange > 0f && dist < retreatRange)
                {
                    Vector3 fleeDir = (transform.position - _player.position);
                    fleeDir.y = 0f;
                    if (fleeDir.sqrMagnitude > 0.01f)
                    {
                        Vector3 fleePos = transform.position + fleeDir.normalized * 5f;
                        TrySetDestination(fleePos, 5.5f);
                    }
                    break;
                }

                if (!useRangedAttack && dist <= attackRange) { state = State.Attack; break; }
                if (dist > chaseRange * 1.8f) { state = State.Patrol; break; }

                TrySetDestination(GetFlankDestination(), 5.5f);
                break;

            case State.Attack:
                FacePlayer();

                if (useRangedAttack && dist <= rangedRange * 1.15f && CanSeePlayer())
                {
                    SafeStop(true);

                    if (dist <= attackRange && Time.time > _lastAtk + cooldown)
                        DoAttack();
                    else if (dist > attackRange && Time.time > _lastRangedAtk + rangedCooldown)
                        DoRangedAttack();
                }
                else
                {
                    if (dist > attackRange * 1.25f) { state = State.Chase; break; }
                    SafeStop(true);
                    if (Time.time > _lastAtk + cooldown) DoAttack();
                }
                break;
        }
    }

    // Direct movement + combat when NavMesh is unavailable
    void FallbackCombat(float dist)
    {
        float speed = _baseSpeed > 0f ? _baseSpeed : 3.2f;

        if (dist < chaseRange || state == State.Chase || state == State.Attack)
        {
            state = dist <= attackRange ? State.Attack : State.Chase;

            if (dist > attackRange * 1.1f)
            {
                Vector3 dir = (_player.position - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f)
                {
                    dir.Normalize();
                    transform.position += dir * speed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), 10f * Time.deltaTime);
                }
            }
            else
            {
                FacePlayer();
                if (useRangedAttack && dist > attackRange && dist <= rangedRange
                    && Time.time > _lastRangedAtk + rangedCooldown)
                    DoRangedAttack();
                else if (dist <= attackRange && Time.time > _lastAtk + cooldown)
                    DoAttack();
            }
        }
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 from = transform.position + Vector3.up * 1.25f;
        Vector3 to = _player.position + Vector3.up * 0.9f;
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;

        if (Physics.Raycast(from, dir.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player")) return true;
            if (hit.collider.GetComponentInParent<PlayerHealth>() != null) return true;

            // Собственные декоративные части врага не должны блокировать видимость.
            if (hit.collider.GetComponentInParent<EnemyAI>() == this) return true;

            return false;
        }
        return true;
    }

    void WalkBob()
    {
        if (!AgentReady || _agent.isStopped || _agent.velocity.magnitude < 0.5f || _visuals == null) return;

        _walkTimer += Time.deltaTime * (_isRaging ? 14f : 9f);
        float bob = Mathf.Sin(_walkTimer) * 0.055f;
        for (int i = 0; i < _visuals.Length && i < _visualBaseLocalPositions.Length; i++)
        {
            if (_visuals[i]) _visuals[i].localPosition = _visualBaseLocalPositions[i] + Vector3.up * bob;
        }
    }

    void DoPatrol()
    {
        if (waypoints == null || waypoints.Length == 0 || !AgentReady) return;

        SafeStop(false);
        if (!_agent.pathPending && _agent.remainingDistance < 0.7f)
        {
            _wp = (_wp + 1) % waypoints.Length;
            if (waypoints[_wp] == null) return;
            TrySetDestination(waypoints[_wp].position, 5.5f);
        }
    }

    bool _isWindingUp;

    void DoAttack()
    {
        if (_isWindingUp) return;
        _lastAtk = Time.time;
        StartCoroutine(AttackWindup());
    }

    IEnumerator AttackWindup()
    {
        _isWindingUp = true;
        SafeStop(true);

        float windupTime = isFinalBoss ? 0.55f : 0.38f;

        // Визуальный телеграф: отвод назад + красная вспышка
        float t = 0f;
        while (t < windupTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / windupTime);

            // Тело отводится назад и поднимается — замах
            float pullback = Mathf.Sin(k * Mathf.PI * 0.5f) * -0.25f;
            float rise = Mathf.Sin(k * Mathf.PI) * 0.1f;
            for (int i = 0; i < _visuals.Length && i < _visualBaseLocalPositions.Length; i++)
                if (_visuals[i])
                    _visuals[i].localPosition = _visualBaseLocalPositions[i]
                        + Vector3.forward * pullback + Vector3.up * rise;

            // Красная пульсация
            if (k > 0.3f)
                SetRenderColors(Color.Lerp(Color.white, new Color(1f, 0.15f, 0.05f),
                    Mathf.PingPong((k - 0.3f) * 8f, 1f)));

            FacePlayer();
            yield return null;
        }

        RestoreColors();

        // Проверяем что игрок ещё в дистанции и жив
        if (_player != null && state != State.Dead)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= attackRange * 1.6f && _ph != null && !_ph.isDead)
            {
                _ph.TakeDamage(damage, transform.position);
                FeedbackManager.Instance?.PlayDamage();
            }
        }

        StartCoroutine(LungeAnim());
        SafeStop(false);
        _isWindingUp = false;
    }

    void DoRangedAttack()
    {
        _lastRangedAtk = Time.time;

        Vector3 from = transform.position + Vector3.up * (isFinalBoss ? 2.1f : 1.25f) + transform.forward * 0.55f;
        Vector3 target = _player.position + Vector3.up * 0.85f;
        Vector3 dir = (target - from).normalized;

        var proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proj.name = isFinalBoss ? "Boss_Firebolt" : "Enemy_Ranged_Projectile";
        proj.transform.position = from;
        proj.transform.localScale = Vector3.one * (isFinalBoss ? 0.45f : 0.28f);

        var r = proj.GetComponent<Renderer>();
        if (r)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = rangedProjectileColor;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", rangedProjectileColor * (isFinalBoss ? 2.3f : 1.5f));
            r.material = m;
        }

        var c = proj.GetComponent<Collider>();
        if (c) c.isTrigger = true;

        var rb = proj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var trail = proj.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.startWidth = isFinalBoss ? 0.20f : 0.11f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(rangedProjectileColor.r, rangedProjectileColor.g, rangedProjectileColor.b, 0.85f);
        trail.endColor = new Color(rangedProjectileColor.r, rangedProjectileColor.g, rangedProjectileColor.b, 0f);

        var ep = proj.AddComponent<EnemyRangedProjectile>();
        ep.Init(dir, rangedProjectileSpeed, rangedDamage, rangedProjectileColor, isFinalBoss ? 4.5f : 3.4f);

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * (isFinalBoss ? 3.6f : 2.1f),
            rangedAttackLabel, rangedProjectileColor, 0.7f);

        StartCoroutine(CastAnim());
    }

    IEnumerator CastAnim()
    {
        if (_visuals == null || _visualBaseLocalPositions == null) yield break;
        float t = 0f;
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI) * 0.12f;
            for (int i = 0; i < _visuals.Length && i < _visualBaseLocalPositions.Length; i++)
                if (_visuals[i]) _visuals[i].localPosition = _visualBaseLocalPositions[i] + Vector3.up * k;
            yield return null;
        }
    }

    IEnumerator LungeAnim()
    {
        // Важно: не двигаем transform врага напрямую, иначе можно сбить NavMeshAgent.
        if (_visuals == null || _visualBaseLocalPositions == null) yield break;
        float t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.08f) * 0.18f;
            for (int i = 0; i < _visuals.Length && i < _visualBaseLocalPositions.Length; i++)
                if (_visuals[i]) _visuals[i].localPosition = _visualBaseLocalPositions[i] + Vector3.forward * k;
            yield return null;
        }
        while (t < 0.16f)
        {
            t += Time.deltaTime;
            float k = (1f - Mathf.Clamp01((t - 0.08f) / 0.08f)) * 0.18f;
            for (int i = 0; i < _visuals.Length && i < _visualBaseLocalPositions.Length; i++)
                if (_visuals[i]) _visuals[i].localPosition = _visualBaseLocalPositions[i] + Vector3.forward * k;
            yield return null;
        }
    }

    public void SetNextHitBonus(RunScoreManager.KillBonus bonus) => _lastHitBonus = bonus;
    public void SetKillDistance(float dist) => _lastKillDistance = dist;

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (state == State.Dead) return;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;
        StartCoroutine(KnockbackCo(direction.normalized, force));
    }

    IEnumerator KnockbackCo(Vector3 dir, float force)
    {
        float duration = 0.18f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float strength = force * (1f - t / duration) * Time.deltaTime * 10f;
            if (AgentReady)
                _agent.Move(dir * strength);
            else
                transform.position += dir * strength;
            yield return null;
        }
        TryPlaceOnNavMesh(3f);
    }

    public void TakeDamage(int dmg)
    {
        if (state == State.Dead) return;
        _hp -= dmg;

        if (state == State.Patrol) state = State.Chase;

        if (!_isRaging && _hp <= maxHp * rageHpThreshold)
        {
            _isRaging = true;
            if (_agent != null) _agent.speed = Mathf.Max(_agent.speed, _baseSpeed * 1.6f);
            cooldown = cooldown * 0.65f;
            rangedCooldown = rangedCooldown * 0.72f;
            FeedbackManager.Instance?.FloatText(
                transform.position + Vector3.up * 2f,
                "ЯРОСТЬ!", new Color(1f, 0.2f, 0f), 1.5f);
            StartCoroutine(RageFlash());
        }

        if (FeedbackManager.Instance != null)
        {
            var fb = FeedbackManager.Instance;
            fb.PlayHit();
            fb.ShowHitMarker();
            fb.FloatText(
                transform.position + Vector3.up * 1.8f,
                $"-{dmg}", new Color(1f, 0.2f, 0.1f), 0.7f);
            fb.SpawnBurst(
                transform.position + Vector3.up,
                new Color(0.85f, 0.1f, 0.05f),
                new Color(0.55f, 0.05f, 0.05f), 8, 2.5f, 0.08f);
        }

        StartCoroutine(ShowHpBar());

        if (_hp <= 0) { state = State.Dead; StartCoroutine(DieFX()); }
        else          { StartCoroutine(HitFlash()); }
    }

    public void Heal(int amount)
    {
        if (state == State.Dead || amount <= 0) return;
        _hp = Mathf.Min(_hp + amount, maxHp);
    }

    IEnumerator HitFlash()
    {
        if (_bodyRends == null) CacheColors();
        SetRenderColors(Color.white);
        yield return new WaitForSeconds(0.055f);
        SetRenderColors(new Color(1f, 0.1f, 0.1f));
        yield return new WaitForSeconds(0.055f);
        RestoreColors();
    }

    IEnumerator RageFlash()
    {
        for (int i = 0; i < 5; i++)
        {
            SetRenderColors(new Color(1f, 0.15f, 0f));
            yield return new WaitForSeconds(0.08f);
            RestoreColors();
            yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator ShowHpBar()
    {
        float pct  = Mathf.Clamp01((float)_hp / maxHp);
        string bar = "[";
        int filled = Mathf.RoundToInt(pct * 8);
        for (int i = 0; i < 8; i++) bar += i < filled ? "█" : "░";
        bar += "]";

        Color c = Color.Lerp(Color.red, Color.green, pct);
        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.5f, bar, c, 1.2f);
        yield break;
    }

    IEnumerator DieFX()
    {
        SafeStop(true);

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        GameManager.Instance?.AddScore(maxHp / 3);
        GameManager.Instance?.OnEnemyKilled(isFinalBoss);
        KingAbility.Instance?.AddCharge();

        int pts = RunScoreManager.Instance != null
            ? RunScoreManager.Instance.RegisterKill(enemyScoreType, _lastHitBonus, _lastKillDistance)
            : 0;

        if (FeedbackManager.Instance != null)
        {
            var fb = FeedbackManager.Instance;
            fb.PlayDeath();
            fb.PlayKillConfirm();
            fb.ShowKillFlash();
            fb.ShowHitMarker(new Color(1f, 0.3f, 0.1f));

            fb.SpawnBurst(
                transform.position + Vector3.up,
                new Color(1f, 0.4f, 0.05f), new Color(0.8f, 0.05f, 0.05f), 28, 5.5f, 0.15f);
            fb.SpawnBurst(
                transform.position + Vector3.up * 0.4f,
                new Color(1f, 0.9f, 0.1f), new Color(1f, 0.3f, 0f), 14, 3.5f, 0.09f);

            if (pts > 0)
                fb.FloatText(transform.position + Vector3.up * 2.2f,
                    $"+{pts}", new Color(1f, 0.85f, 0.1f), 1.6f);

            Color gold = new Color(1f, 0.9f, 0.15f);
            fb.ShowKillFeed($"+{pts}", gold);

            if (_lastHitBonus == RunScoreManager.KillBonus.Headshot)
            {
                fb.PlayHeadshot();
                fb.ShowKillFeed("HEADSHOT!", new Color(1f, 0.3f, 0.1f));
            }
            else if (_lastHitBonus == RunScoreManager.KillBonus.CriticalHit)
            {
                fb.PlayCrit();
                fb.ShowKillFeed("КРИТ!", new Color(1f, 0.6f, 0.1f));
            }
            else if (_lastHitBonus == RunScoreManager.KillBonus.Stomp)
            {
                fb.ShowKillFeed("STOMP!", new Color(0.4f, 1f, 0.5f));
            }

            if (_lastKillDistance >= 30f)
                fb.ShowKillFeed($"СНАЙПЕР! {_lastKillDistance:F0}м", new Color(0.3f, 0.8f, 1f));
            else if (_lastKillDistance >= 15f)
                fb.ShowKillFeed($"ДАЛЬНИЙ! {_lastKillDistance:F0}м", new Color(0.5f, 0.9f, 1f));

            var run = RunScoreManager.Instance?.Data;
            if (run != null && run.currentStreak >= 3)
            {
                fb.PlayStreak(run.currentStreak);
                fb.ShowKillFeed($"СЕРИЯ x{run.currentStreak}!",
                    new Color(1f, 0.5f, 0.1f));
            }

            var shake = Camera.main?.GetComponent<CameraShake>();
            shake?.ShakeKill();

            fb.KillSlowmo();

            if (isFinalBoss)
            {
                fb.SpawnBurst(
                    transform.position + Vector3.up * 1.2f,
                    new Color(1f, 0.95f, 0.2f), new Color(1f, 0.5f, 0f), 50, 8f, 0.25f);
                fb.FloatText(
                    transform.position + Vector3.up * 3f,
                    "БОСС ПОВЕРЖЕН!", new Color(1f, 0.9f, 0.1f), 3f);
                fb.ShowKillFeed("★ БОСС ПОВЕРЖЕН! ★", new Color(1f, 0.95f, 0.2f), 3f);
                shake?.ShakeHard();
                fb.BossKillSlowmo();
            }
        }

        for (int i = 0; i < 2; i++)
        {
            SetRenderColors(i % 2 == 0 ? Color.white : new Color(1f, 0.2f, 0f));
            yield return new WaitForSecondsRealtime(0.06f);
        }

        FeedbackManager.Instance?.RagdollExplode(transform, transform.position + Vector3.up * 0.5f);
        Destroy(gameObject);
    }

    Vector3 GetFlankDestination()
    {
        if (_player == null) return transform.position;
        if (flankRadius <= 0f) return _player.position;

        _flankTimer -= Time.deltaTime;
        if (_flankTimer <= 0f)
        {
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            float side = (_wp % 2 == 0 ? 1f : -1f) * flankRadius;
            Vector3 lateral = toPlayer.sqrMagnitude > 0.01f
                ? Vector3.Cross(Vector3.up, toPlayer.normalized) * side
                : Vector3.zero;
            _flankDest = _player.position + lateral;
            _flankTimer = 1.5f;
        }
        return _flankDest;
    }

    void FacePlayer()
    {
        if (_player == null) return;
        Vector3 d = _player.position - transform.position; d.y = 0;
        if (d.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(d), 12f * Time.deltaTime);
    }

    void SetRenderColors(Color c)
    {
        if (_bodyRends == null) return;
        foreach (var r in _bodyRends) if (r) r.material.color = c;
    }

    void RestoreColors()
    {
        if (_bodyRends == null || _origColors == null) return;
        for (int i = 0; i < _bodyRends.Length && i < _origColors.Length; i++)
            if (_bodyRends[i]) _bodyRends[i].material.color = _origColors[i];
    }

    void CacheSpecialParts()
    {
        _magicOrb = FindDeepChild(transform, "MagicOrb");
        if (_magicOrb != null) _baseOrbLocalY = _magicOrb.localPosition.y;
        if (isFinalBoss)
        {
            _bossEyeL = FindDeepChild(transform, "Eye_L");
            _bossEyeR = FindDeepChild(transform, "Eye_R");
        }
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t;
        return null;
    }

    void AnimateMagicOrb()
    {
        if (_magicOrb == null) return;
        Vector3 lp = _magicOrb.localPosition;
        lp.y = _baseOrbLocalY + Mathf.Sin(Time.time * 2.5f) * 0.12f;
        _magicOrb.localPosition = lp;
    }

    void AnimateBossEyes()
    {
        if (!isFinalBoss || _bossEyesRaging || _hp > maxHp * 0.3f) return;
        _bossEyesRaging = true;
        Color brightRed = new Color(1f, 0.04f, 0.02f);
        if (_bossEyeL) { var r = _bossEyeL.GetComponent<Renderer>(); if (r) r.material.color = brightRed; }
        if (_bossEyeR) { var r = _bossEyeR.GetComponent<Renderer>(); if (r) r.material.color = brightRed; }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.cyan;   Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

public class EnemyRangedProjectile : MonoBehaviour
{
    Vector3 _dir;
    float _speed;
    float _life;
    int _damage;
    Color _color;
    bool _hit;
    Transform _player;
    PlayerHealth _ph;
    Vector3 _prevPos;

    public void Init(Vector3 dir, float speed, int damage, Color color, float life)
    {
        _dir = dir.normalized;
        _speed = speed;
        _damage = damage;
        _color = color;
        _life = life;

        if (EnemyAI._cachedPlayer != null)
        {
            _player = EnemyAI._cachedPlayer;
            _ph = EnemyAI._cachedPh;
        }
        else
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) { _player = po.transform; _ph = po.GetComponent<PlayerHealth>(); }
        }
        _prevPos = transform.position;
    }

    void Update()
    {
        if (_hit) return;

        transform.position += _dir * _speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_dir);

        _life -= Time.deltaTime;
        if (_life <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Sweep check to prevent tunneling at high speeds
        Vector3 curPos = transform.position;
        Vector3 sweep = curPos - _prevPos;
        float sweepDist = sweep.magnitude;
        if (sweepDist > 0.01f && Physics.Raycast(_prevPos, sweep.normalized, out RaycastHit sweepHit, sweepDist))
        {
            if (sweepHit.collider.CompareTag("Player") || sweepHit.collider.GetComponentInParent<PlayerHealth>() != null)
            {
                transform.position = sweepHit.point;
                HitPlayer();
                _prevPos = curPos;
                return;
            }
        }
        _prevPos = curPos;

        if (_player != null && Vector3.Distance(curPos, _player.position + Vector3.up * 0.75f) < 0.75f)
            HitPlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hit) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerHealth>() != null)
        {
            HitPlayer();
            return;
        }

        if (other.GetComponentInParent<EnemyAI>() != null) return;
        if (other.isTrigger) return;

        Impact();
    }

    void HitPlayer()
    {
        if (_hit) return;
        _hit = true;

        if (_ph == null)
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) _ph = po.GetComponent<PlayerHealth>();
        }

        _ph?.TakeDamage(_damage, transform.position);
        FeedbackManager.Instance?.PlayDamage();
        FeedbackManager.Instance?.SpawnBurst(
            transform.position,
            _color,
            new Color(0.4f, 0.05f, 0.02f), 8, 2.2f, 0.07f);

        Destroy(gameObject);
    }

    void Impact()
    {
        _hit = true;
        FeedbackManager.Instance?.SpawnBurst(
            transform.position,
            _color,
            new Color(0.25f, 0.18f, 0.10f), 6, 1.4f, 0.05f);
        Destroy(gameObject);
    }
}

public class EnemyBlink : MonoBehaviour
{
    Renderer _r; float _timer; bool _on = true;
    void Start() => _r = GetComponent<Renderer>();
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0) return;
        _on = !_on; _timer = _on ? 0.9f : 0.3f;
        if (_r) _r.material.color = _on
            ? new Color(1f, 0.2f, 0.05f) : new Color(0.15f, 0.15f, 0.15f);
    }
}
