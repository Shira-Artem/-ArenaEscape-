using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// Трёхфазный бой с вождём орков.
/// Phase1 (100→60%): стандарт.
/// Phase2 (60→30%): скорость +40%, аура, атака-таран каждые 7с.
/// Phase3 (<30%): скорость +85%, волна орков, АОЕ-удар каждые 10с.
public class BossArenaController : MonoBehaviour
{
    public EnemyAI boss;
    public GameObject entranceBarrier;

    [Header("Спавн подмоги")]
    public Vector3[] minionSpawnPoints;
    public Vector3[] archerSpawnPoints;
    public Transform enemiesRoot;
    public Transform waypointsRoot;

    enum Phase { Waiting, Phase1, Phase2, Phase3, BossDead }
    Phase _phase = Phase.Waiting;
    bool _playerEntered;

    NavMeshAgent _bossNav;
    float _bossBaseSpeed;
    float _bossCurrentSpeed;

    Light _bossAuraLight;

    // Таран
    float _chargeTimer;
    const float ChargeInterval    = 7f;
    const float ChargeDuration    = 0.9f;
    float _chargeCountdown;
    bool  _isCharging;

    // АОЕ-удар (только Phase3)
    float _slamTimer;
    const float SlamInterval    = 10f;
    const float SlamWarnTime    = 1.4f;

    Transform _playerTr;

    // ── ВХОД В АРЕНУ ───────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (_playerEntered) return;
        if (!other.CompareTag("Player")) return;

        _playerEntered = true;
        _phase = Phase.Phase1;
        _playerTr = other.transform;

        _bossNav = boss != null ? boss.GetComponent<NavMeshAgent>() : null;
        if (_bossNav != null)
        {
            _bossBaseSpeed    = _bossNav.speed;
            _bossCurrentSpeed = _bossBaseSpeed;
        }

        if (entranceBarrier != null) entranceBarrier.SetActive(true);

        SetupBossAuraLight();

        _chargeTimer = 10f; // первый таран Phase1 через 10с
        _slamTimer   = SlamInterval   * 0.7f;

        GameManager.Instance?.ShowZoneTitle("Логово Вождя", "Убей вождя орков!", 4f);
    }

    void SetupBossAuraLight()
    {
        if (boss == null) return;
        var go = new GameObject("BossAuraLight");
        go.transform.SetParent(boss.transform);
        go.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        _bossAuraLight = go.AddComponent<Light>();
        _bossAuraLight.type      = LightType.Point;
        _bossAuraLight.color     = new Color(1f, 0.42f, 0.08f);
        _bossAuraLight.range     = 12f;
        _bossAuraLight.intensity = 0f;
        _bossAuraLight.enabled   = false;
    }

    // ── UPDATE ─────────────────────────────────────────────────────

    void Update()
    {
        if (_phase == Phase.Waiting || _phase == Phase.BossDead) return;

        if (boss == null || boss.state == EnemyAI.State.Dead)
        {
            OnBossDead();
            return;
        }

        // Принудительно держим нашу скорость (EnemyAI может её сбрасывать)
        if (_bossNav != null && !_isCharging)
            _bossNav.speed = _bossCurrentSpeed;

        CheckPhaseTransitions();

        UpdateCharge();
        if (_phase >= Phase.Phase3) UpdateSlam();

        PulseAuraLight();
    }

    void CheckPhaseTransitions()
    {
        float pct = (float)boss.CurrentHp / boss.maxHp;
        if (_phase == Phase.Phase1 && pct <= 0.6f) EnterPhase2();
        if (_phase == Phase.Phase2 && pct <= 0.3f) EnterPhase3();
    }

    void PulseAuraLight()
    {
        if (_bossAuraLight == null || !_bossAuraLight.enabled) return;
        float baseI = _phase >= Phase.Phase3 ? 5.5f : 2.5f;
        _bossAuraLight.intensity = baseI + Mathf.Sin(Time.time * 3.8f) * baseI * 0.3f;
    }

    // ── ФАЗЫ ──────────────────────────────────────────────────────

    void EnterPhase2()
    {
        _phase = Phase.Phase2;
        _bossCurrentSpeed = _bossBaseSpeed * 1.4f;

        if (_bossAuraLight != null)
        {
            _bossAuraLight.enabled   = true;
            _bossAuraLight.color     = new Color(1f, 0.42f, 0.08f);
            _bossAuraLight.intensity = 2.5f;
        }

        FeedbackManager.Instance?.SpawnBurst(
            boss.transform.position + Vector3.up * 1.5f,
            new Color(1f, 0.3f, 0.05f), new Color(1f, 0.65f, 0.1f), 30, 5f, 0.12f);
        GameManager.Instance?.ShowMessage("Вождь разъярён!", GameManager.MsgType.Damage, 3.5f);
        DoScreenShake(0.35f, 0.65f);
    }

    void EnterPhase3()
    {
        _phase = Phase.Phase3;
        _bossCurrentSpeed = _bossBaseSpeed * 1.85f;

        if (_bossAuraLight != null)
        {
            _bossAuraLight.color = new Color(1f, 0.05f, 0.02f);
            _bossAuraLight.range = 22f;
        }

        SpawnPhase3Wave();
        GameManager.Instance?.ShowMessage("КРОВАВАЯ ЯРОСТЬ!  Вождь призывает орду!", GameManager.MsgType.Damage, 5f);
        FeedbackManager.Instance?.SpawnBurst(
            boss.transform.position + Vector3.up * 2f,
            new Color(1f, 0f, 0f), new Color(1f, 0.2f, 0.02f), 60, 9f, 0.08f);
        DoScreenShake(0.8f, 1.5f);

        _slamTimer = SlamInterval * 0.3f; // первый слэм — скоро
    }

    // ── ТАРАН ─────────────────────────────────────────────────────

    void UpdateCharge()
    {
        if (_chargeCountdown > 0f)
        {
            _chargeCountdown -= Time.deltaTime;
            if (_chargeCountdown <= 0f) EndCharge();
            return;
        }

        _chargeTimer -= Time.deltaTime;
        if (_chargeTimer <= 0f) StartCharge();
    }

    void StartCharge()
    {
        if (boss == null || _bossNav == null) return;
        _isCharging      = true;
        _chargeCountdown = ChargeDuration;
        _chargeTimer     = _phase == Phase.Phase1 ? 14f : ChargeInterval;
        _bossNav.speed   = _bossBaseSpeed * (_phase == Phase.Phase1 ? 3.2f : 4.2f);

        FeedbackManager.Instance?.SpawnBurst(
            boss.transform.position + Vector3.up * 1f,
            new Color(1f, 0.1f, 0f), new Color(1f, 0.45f, 0.1f), 18, 5f, 0.18f);
        GameManager.Instance?.ShowMessage("ТАРАН!", GameManager.MsgType.Damage, 1.2f);
    }

    void EndCharge()
    {
        _isCharging = false;
        if (_bossNav != null) _bossNav.speed = _bossCurrentSpeed;
    }

    // ── АОЕ-УДАР ──────────────────────────────────────────────────

    void UpdateSlam()
    {
        _slamTimer -= Time.deltaTime;
        if (_slamTimer <= 0f && _playerTr != null)
        {
            _slamTimer = SlamInterval;
            StartCoroutine(GroundSlam(_playerTr.position));
        }
    }

    IEnumerator GroundSlam(Vector3 target)
    {
        GameManager.Instance?.ShowMessage("БЕРЕГИСЬ!", GameManager.MsgType.Default, SlamWarnTime);

        // Предупреждающий круг
        var warn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warn.name = "SlamWarning";
        Destroy(warn.GetComponent<Collider>());
        warn.transform.position   = new Vector3(target.x, target.y + 0.07f, target.z);
        warn.transform.localScale = new Vector3(7f, 0.03f, 7f);
        var wMat = MakeMat(new Color(1f, 0.08f, 0f));
        wMat.color = new Color(1f, 0.08f, 0f, 0.6f);
        warn.GetComponent<Renderer>().material = wMat;

        float elapsed = 0f;
        while (elapsed < SlamWarnTime)
        {
            elapsed += Time.deltaTime;
            float pulse = Mathf.Abs(Mathf.Sin(elapsed * Mathf.PI * 5f));
            wMat.SetColor("_BaseColor", new Color(1f, 0.05f + 0.2f * pulse, 0f, 0.3f + 0.5f * pulse));
            yield return null;
        }
        Destroy(warn);

        // Удар
        FeedbackManager.Instance?.SpawnBurst(target + Vector3.up * 0.5f,
            new Color(1f, 0.04f, 0f), new Color(1f, 0.3f, 0.05f), 45, 7f, 0.09f);
        DoScreenShake(0.6f, 0.45f);
        GameManager.Instance?.ShowMessage("УДАР!", GameManager.MsgType.Damage, 0.8f);

        // Урон игроку в радиусе
        var hits = Physics.OverlapSphere(target, 3.5f);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;
            h.GetComponent<PlayerHealth>()?.TakeDamage(35, target);
        }
    }

    // ── ВОЛНА ФАЗ 3 ───────────────────────────────────────────────

    void SpawnPhase3Wave()
    {
        // Позиции относительно контроллера (на полу арены Y=3.95)
        float bz = transform.position.z;
        float by = 3.95f;

        // 4 гранта по углам
        SpawnOrcGrunt("P3_Grunt_FL", new Vector3(-16f, by, bz + 8f));
        SpawnOrcGrunt("P3_Grunt_FR", new Vector3( 16f, by, bz + 8f));
        SpawnOrcGrunt("P3_Grunt_BL", new Vector3(-16f, by, bz + 30f));
        SpawnOrcGrunt("P3_Grunt_BR", new Vector3( 16f, by, bz + 30f));

        // 2 лучника с тыла
        SpawnOrcArcher("P3_Archer_L", new Vector3(-10f, by, bz + 37f));
        SpawnOrcArcher("P3_Archer_R", new Vector3( 10f, by, bz + 37f));

        // 1 тяжёлый орк-танк через центр
        SpawnOrcTank("P3_Tank", new Vector3(0f, by, bz + 15f));
    }

    // ── ФАБРИКА ВРАГОВ ────────────────────────────────────────────

    void SpawnOrcGrunt(string name, Vector3 pos)
    {
        var g = new GameObject(name);
        g.tag = "Enemy";
        g.transform.position = pos;
        if (enemiesRoot != null) g.transform.SetParent(enemiesRoot);

        var col = g.AddComponent<CapsuleCollider>();
        col.height = 1.8f; col.radius = 0.45f;
        col.center = new Vector3(0f, 0.9f, 0f);

        var nav = g.AddComponent<NavMeshAgent>();
        nav.speed = 3.8f; nav.angularSpeed = 300f; nav.acceleration = 18f;
        nav.stoppingDistance = 0.45f; nav.height = 1.8f; nav.radius = 0.4f;
        nav.autoRepath = true;

        var ai = g.AddComponent<EnemyAI>();
        ai.damage = 14; ai.maxHp = 60;
        ai.chaseRange = 20f; ai.attackRange = 2.0f;
        ai.isFinalBoss = false;
        ai.enemyScoreType = RunScoreManager.EnemyType.OrcWarrior;
        g.AddComponent<EnemyHitDetector>();

        BuildSimpleOrcVisual(g.transform, new Color(0.22f, 0.50f, 0.14f), 1.05f);
        WarpToNavMesh(nav, ai, pos);
    }

    void SpawnOrcArcher(string name, Vector3 pos)
    {
        var g = new GameObject(name);
        g.tag = "Enemy";
        g.transform.position = pos;
        if (enemiesRoot != null) g.transform.SetParent(enemiesRoot);

        var col = g.AddComponent<CapsuleCollider>();
        col.height = 1.8f; col.radius = 0.45f;
        col.center = new Vector3(0f, 0.9f, 0f);

        var nav = g.AddComponent<NavMeshAgent>();
        nav.speed = 3.2f; nav.angularSpeed = 300f; nav.acceleration = 18f;
        nav.stoppingDistance = 0.45f; nav.height = 1.8f; nav.radius = 0.4f;
        nav.autoRepath = true;

        var ai = g.AddComponent<EnemyAI>();
        ai.damage = 9; ai.maxHp = 45;
        ai.chaseRange = 22f; ai.attackRange = 1.75f;
        ai.useRangedAttack = true;
        ai.rangedDamage = 10; ai.rangedRange = 20f;
        ai.rangedCooldown = 2.0f; ai.rangedProjectileSpeed = 17f;
        ai.rangedProjectileColor = new Color(1f, 0.72f, 0.12f);
        ai.rangedAttackLabel = "ВЫСТРЕЛ!";
        ai.isFinalBoss = false;
        ai.enemyScoreType = RunScoreManager.EnemyType.OrcArcher;
        g.AddComponent<EnemyHitDetector>();

        BuildSimpleOrcVisual(g.transform, new Color(0.20f, 0.54f, 0.17f), 0.92f);
        WarpToNavMesh(nav, ai, pos);
    }

    void SpawnOrcTank(string name, Vector3 pos)
    {
        var g = new GameObject(name);
        g.tag = "Enemy";
        g.transform.position = pos;
        if (enemiesRoot != null) g.transform.SetParent(enemiesRoot);

        var col = g.AddComponent<CapsuleCollider>();
        col.height = 2.4f; col.radius = 0.7f;
        col.center = new Vector3(0f, 1.2f, 0f);

        var nav = g.AddComponent<NavMeshAgent>();
        nav.speed = 2.0f; nav.angularSpeed = 180f; nav.acceleration = 10f;
        nav.stoppingDistance = 0.7f; nav.height = 2.4f; nav.radius = 0.65f;
        nav.autoRepath = true;

        var ai = g.AddComponent<EnemyAI>();
        ai.damage = 22; ai.maxHp = 140;
        ai.chaseRange = 18f; ai.attackRange = 2.5f;
        ai.isFinalBoss = false;
        ai.enemyScoreType = RunScoreManager.EnemyType.Elite;
        g.AddComponent<EnemyHitDetector>();

        BuildSimpleOrcVisual(g.transform, new Color(0.16f, 0.40f, 0.10f), 1.75f);
        WarpToNavMesh(nav, ai, pos);
    }

    static void WarpToNavMesh(NavMeshAgent nav, EnemyAI ai, Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 6f, NavMesh.AllAreas))
        {
            nav.Warp(hit.position);
            ai.OnNavMeshReady();
        }
    }

    // ── СМЕРТЬ БОССА ──────────────────────────────────────────────

    void OnBossDead()
    {
        _phase = Phase.BossDead;
        if (_bossAuraLight != null) _bossAuraLight.enabled = false;
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        StartCoroutine(VictoryFX());
    }

    IEnumerator VictoryFX()
    {
        var fb = FeedbackManager.Instance;
        for (int i = 0; i < 8; i++)
        {
            if (fb != null)
            {
                Vector3 p = transform.position + new Vector3(
                    Random.Range(-14f, 14f), Random.Range(1f, 9f), Random.Range(-6f, 14f));
                fb.SpawnBurst(p, new Color(1f, 0.9f, 0.15f), new Color(1f, 0.5f, 0.05f), 40, 8f, 0.15f);
            }
            yield return new WaitForSeconds(0.2f);
        }

        GameManager.Instance?.ShowZoneTitle("ПОБЕДА!", "Вождь орков повержен.", 6f);

        var lo = new GameObject("Victory_Flash");
        lo.transform.position = transform.position + Vector3.up * 8f;
        var lt = lo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.85f, 0.22f);
        lt.intensity = 14f; lt.range = 90f;

        float t = 0f;
        while (t < 3.5f)
        {
            t += Time.unscaledDeltaTime;
            lt.intensity = Mathf.Lerp(14f, 0f, t / 3.5f);
            yield return null;
        }
        Destroy(lo);
    }

    // ── ВИЗУАЛ ОРКA ───────────────────────────────────────────────

    void BuildSimpleOrcVisual(Transform root, Color skin, float scale)
    {
        root.localScale = Vector3.one * scale;

        var torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(root);
        torso.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        torso.transform.localScale    = new Vector3(0.7f, 0.6f, 0.45f);
        torso.GetComponent<Renderer>().material = MakeMat(skin);
        Destroy(torso.GetComponent<Collider>());

        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(root);
        head.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        head.transform.localScale    = new Vector3(0.5f, 0.45f, 0.45f);
        head.GetComponent<Renderer>().material = MakeMat(skin);
        Destroy(head.GetComponent<Collider>());

        var legL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        legL.name = "Leg_L";
        legL.transform.SetParent(root);
        legL.transform.localPosition = new Vector3( 0.18f, -0.05f, 0f);
        legL.transform.localScale    = new Vector3(0.18f, 0.38f, 0.18f);
        legL.GetComponent<Renderer>().material = MakeMat(new Color(0.12f, 0.30f, 0.09f));
        Destroy(legL.GetComponent<Collider>());

        var legR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        legR.name = "Leg_R";
        legR.transform.SetParent(root);
        legR.transform.localPosition = new Vector3(-0.18f, -0.05f, 0f);
        legR.transform.localScale    = new Vector3(0.18f, 0.38f, 0.18f);
        legR.GetComponent<Renderer>().material = MakeMat(new Color(0.12f, 0.30f, 0.09f));
        Destroy(legR.GetComponent<Collider>());

        var ai = root.GetComponent<EnemyAI>();
        if (ai != null) ai.bodyParts = new GameObject[] { torso, head };
    }

    static void DoScreenShake(float intensity, float duration)
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.GetComponent<CameraShake>()?.Shake(intensity, duration);
    }

    static Material MakeMat(Color c)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ??
                     Shader.Find("Standard") ??
                     Shader.Find("Diffuse");
        var m = new Material(shader);
        m.SetColor("_BaseColor", c);
        m.color = c;
        return m;
    }
}
