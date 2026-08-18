using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Спецспособности орков поверх EnemyAI (контракт AI не трогаем).
///   BattleCry   — берсерк: ускорение при HP&lt;70%
///   HealPulse   — шаман: лечит ближайшего союзника
///   GroundSlam  — босс: AoE удар при дистанции &lt;5м
///   PackHowl    — волк: все волки в радиусе ускоряются
///   SpearThrow  — копьеносец: бросает копьё раз в 8с
///   RaiseDead   — некромант: призывает зомби-воина раз в 18с
///   WarCry      — вождь: ускоряет всех орков в радиусе 18м
///   PoisonCloud — отравитель: облако яда в радиусе 3.5м
/// </summary>
public class OrcSpecialAbility : MonoBehaviour
{
    public enum AbilityType { BattleCry, HealPulse, GroundSlam, PackHowl,
                              SpearThrow, RaiseDead, WarCry, PoisonCloud }

    public AbilityType abilityType;

    float _cooldown;
    float _timer;

    bool  _boostActive;
    float _boostTimer;
    float _preBoostSpeed;
    float _boostAmount;

    EnemyAI      _ai;
    NavMeshAgent _agent;
    float        _baseSpeed;
    Transform    _player;

    void Start()
    {
        _ai    = GetComponent<EnemyAI>();
        _agent = GetComponent<NavMeshAgent>();
        _baseSpeed = _agent != null ? _agent.speed : 4f;

        switch (abilityType)
        {
            case AbilityType.BattleCry:   _cooldown = 12f; _timer = Random.Range(3f,  8f);  break;
            case AbilityType.HealPulse:   _cooldown = 15f; _timer = Random.Range(5f,  10f); break;
            case AbilityType.GroundSlam:  _cooldown = 10f; _timer = Random.Range(4f,  7f);  break;
            case AbilityType.PackHowl:    _cooldown = 20f; _timer = Random.Range(6f,  12f); break;
            case AbilityType.SpearThrow:  _cooldown =  8f; _timer = Random.Range(3f,  6f);  break;
            case AbilityType.RaiseDead:   _cooldown = 18f; _timer = Random.Range(8f,  14f); break;
            case AbilityType.WarCry:      _cooldown = 14f; _timer = Random.Range(5f,  9f);  break;
            case AbilityType.PoisonCloud: _cooldown = 12f; _timer = Random.Range(6f,  10f); break;
        }
    }

    void Update()
    {
        if (_ai == null || _ai.state == EnemyAI.State.Dead) return;

        bool slowCheck = (abilityType == AbilityType.HealPulse ||
                          abilityType == AbilityType.PackHowl  ||
                          abilityType == AbilityType.WarCry)
                         && (Time.frameCount % 3 != 0);

        UpdateBoostTimer();
        if (slowCheck) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            TryTriggerAbility();
            _timer = _cooldown;
        }
    }

    void UpdateBoostTimer()
    {
        if (!_boostActive) return;
        _boostTimer -= Time.deltaTime;
        if (_boostTimer > 0f) return;

        _boostActive = false;
        if (_agent != null)
        {
            if (abilityType == AbilityType.PackHowl || abilityType == AbilityType.WarCry)
                _agent.speed = Mathf.Max(_baseSpeed, _agent.speed - _boostAmount);
            else
                _agent.speed = _preBoostSpeed;
        }
    }

    void TryTriggerAbility()
    {
        switch (abilityType)
        {
            case AbilityType.BattleCry:   TryBattleCry();   break;
            case AbilityType.HealPulse:   TryHealPulse();   break;
            case AbilityType.GroundSlam:  TryGroundSlam();  break;
            case AbilityType.PackHowl:    TryPackHowl();    break;
            case AbilityType.SpearThrow:  TrySpearThrow();  break;
            case AbilityType.RaiseDead:   TryRaiseDead();   break;
            case AbilityType.WarCry:      TryWarCry();      break;
            case AbilityType.PoisonCloud: TryPoisonCloud(); break;
        }
    }

    // ─── BattleCry ───────────────────────────────────────────────

    void TryBattleCry()
    {
        if (_ai.maxHp <= 0) return;
        float hpRatio = (float)_ai.CurrentHp / _ai.maxHp;
        if (hpRatio > 0.7f) return;
        if (_boostActive) return;

        _preBoostSpeed = _agent != null ? _agent.speed : _baseSpeed;
        if (_agent != null)
        {
            _agent.speed = _preBoostSpeed * 1.5f;
            _boostActive = true;
            _boostTimer  = 3f;
        }

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.2f,
            "ЯРОСТЬ!", new Color(1f, 0.15f, 0.02f), 1.2f);
        FeedbackManager.Instance?.SpawnBurst(
            transform.position + Vector3.up,
            new Color(1f, 0.40f, 0.05f),
            new Color(0.80f, 0.15f, 0.02f), 12, 3f, 0.10f);
    }

    // ─── HealPulse ───────────────────────────────────────────────

    void TryHealPulse()
    {
        EnemyAI target = null;
        float bestHpRatio = 0.85f;

        var all = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (e == _ai || e == null || e.state == EnemyAI.State.Dead) continue;
            if (Vector3.Distance(transform.position, e.transform.position) > 14f) continue;
            float ratio = e.maxHp > 0 ? (float)e.CurrentHp / e.maxHp : 1f;
            if (ratio < bestHpRatio) { bestHpRatio = ratio; target = e; }
        }

        if (target == null) return;

        target.Heal(20);

        FeedbackManager.Instance?.FloatText(
            target.transform.position + Vector3.up * 2f,
            "+20", new Color(0.20f, 1f, 0.30f), 1.2f);
        FeedbackManager.Instance?.SpawnBurst(
            target.transform.position + Vector3.up,
            new Color(0.20f, 1f, 0.30f),
            new Color(0.10f, 0.70f, 0.20f), 10, 2.5f, 0.09f);
    }

    // ─── GroundSlam ──────────────────────────────────────────────

    void TryGroundSlam()
    {
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) _player = go.transform;
        }
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > 5f) return;

        var ph = _player.GetComponent<PlayerHealth>();
        ph?.TakeDamage(25, transform.position);

        Camera.main?.GetComponent<CameraShake>()?.Shake(0.22f, 0.28f);

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.2f,
            "УДАР!", new Color(1f, 0.10f, 0.05f), 1.0f);
        FeedbackManager.Instance?.SpawnBurst(
            transform.position + Vector3.up * 0.2f,
            new Color(1f, 0.85f, 0.10f),
            new Color(0.90f, 0.60f, 0.05f), 20, 4.5f, 0.12f);
    }

    // ─── PackHowl ────────────────────────────────────────────────

    void TryPackHowl()
    {
        var allAbilities = Object.FindObjectsByType<OrcSpecialAbility>(FindObjectsSortMode.None);

        int count = 0;
        foreach (var ab in allAbilities)
        {
            if (ab.abilityType != AbilityType.PackHowl) continue;
            if (ab._ai != null && ab._ai.state == EnemyAI.State.Dead) continue;
            if (Vector3.Distance(transform.position, ab.transform.position) <= 12f) count++;
        }

        if (count < 2) return;

        foreach (var ab in allAbilities)
        {
            if (ab.abilityType != AbilityType.PackHowl) continue;
            if (ab._ai != null && ab._ai.state == EnemyAI.State.Dead) continue;
            if (Vector3.Distance(transform.position, ab.transform.position) > 12f) continue;
            if (ab._boostActive) continue;

            float addSpeed = ab._baseSpeed * 0.30f;
            ab._boostAmount = addSpeed;
            if (ab._agent != null) ab._agent.speed += addSpeed;
            ab._boostActive = true;
            ab._boostTimer  = 4f;

            FeedbackManager.Instance?.FloatText(
                ab.transform.position + Vector3.up * 1.8f,
                "ВОЙ!", new Color(0.80f, 0.80f, 1f), 1.0f);
        }
    }

    // ─── SpearThrow ──────────────────────────────────────────────

    void TrySpearThrow()
    {
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) _player = go.transform;
        }
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist < 3f || dist > 22f) return;

        Vector3 start = transform.position + Vector3.up * 1.3f;
        Vector3 target = _player.position + Vector3.up * 0.8f;
        Vector3 dir = (target - start).normalized;

        // Визуал копья
        GameObject spear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spear.name = "SpearThrow_Proj";
        spear.transform.position = start;
        spear.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);
        spear.transform.localScale = new Vector3(0.06f, 0.55f, 0.06f);

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.50f, 0.38f, 0.15f);
        spear.GetComponent<Renderer>().material = mat;
        Object.Destroy(spear.GetComponent<Collider>());

        // Наконечник
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.transform.SetParent(spear.transform, false);
        tip.transform.localPosition = new Vector3(0f, 0.58f, 0f);
        tip.transform.localScale    = new Vector3(0.5f, 0.25f, 0.5f);
        var tipMat = new Material(Shader.Find("Standard"));
        tipMat.color = new Color(0.55f, 0.55f, 0.60f);
        tip.GetComponent<Renderer>().material = tipMat;
        Object.Destroy(tip.GetComponent<Collider>());

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.2f,
            "БРОСОК!", new Color(0.8f, 0.6f, 0.1f), 0.9f);

        StartCoroutine(MoveSpear(spear, dir, 28f, 30));
    }

    IEnumerator MoveSpear(GameObject proj, Vector3 dir, float speed, int dmg)
    {
        float elapsed = 0f;
        bool  hit     = false;

        while (elapsed < 3f && proj != null && !hit)
        {
            proj.transform.position += dir * speed * Time.deltaTime;
            elapsed += Time.deltaTime;

            if (_player != null &&
                Vector3.Distance(proj.transform.position, _player.position) < 0.9f)
            {
                _player.GetComponent<PlayerHealth>()?.TakeDamage(dmg, proj.transform.position);
                Camera.main?.GetComponent<CameraShake>()?.Shake(0.18f, 0.22f);
                FeedbackManager.Instance?.FloatText(
                    _player.position + Vector3.up * 1.5f,
                    "КОПЬЁ! -" + dmg, new Color(1f, 0.3f, 0.1f), 1.2f);
                hit = true;
            }

            yield return null;
        }

        if (proj != null) Object.Destroy(proj);
    }

    // ─── RaiseDead ───────────────────────────────────────────────

    void TryRaiseDead()
    {
        var mgr = Object.FindFirstObjectByType<Level0OrcVillageManager>();
        if (mgr == null) return;

        Transform root = transform.parent != null ? transform.parent : transform;
        Vector3 spawnPos = transform.position
                         + transform.right * Random.Range(-2f, 2f)
                         + Vector3.forward * 1.5f;
        spawnPos.y = transform.position.y;

        var factory = L0OrcFactory.Ensure();
        GameObject zombie = factory.CreateWarrior("Zombie_" + name, spawnPos, root, null);
        if (zombie == null) return;

        // Настройки зомби
        zombie.transform.localScale = Vector3.one * 0.8f;
        EnemyAI zombieAI = zombie.GetComponent<EnemyAI>();
        if (zombieAI != null)
        {
            zombieAI.maxHp  = 20;
            zombieAI.damage = 8;
        }

        // Серый тинт
        foreach (var r in zombie.GetComponentsInChildren<Renderer>())
        {
            if (r.material == null) continue;
            Color c = r.material.color;
            float g = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
            r.material.color = new Color(g * 0.85f, g * 0.90f, g * 0.85f, c.a);
        }

        mgr.RegisterExtraOrc(zombieAI);

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.5f,
            "ВОСКРЕШЕНИЕ!", new Color(0.15f, 1f, 0.25f), 1.4f);
        FeedbackManager.Instance?.SpawnBurst(
            spawnPos + Vector3.up * 0.5f,
            new Color(0.10f, 0.90f, 0.20f),
            new Color(0.05f, 0.55f, 0.10f), 16, 3f, 0.11f);
    }

    // ─── WarCry ──────────────────────────────────────────────────

    void TryWarCry()
    {
        if (_boostActive) return;

        float radius = 18f;
        float duration = 5f;
        float speedMult = 1.25f;

        var all = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        int boosted = 0;
        foreach (var e in all)
        {
            if (e == null || e.state == EnemyAI.State.Dead) continue;
            if (Vector3.Distance(transform.position, e.transform.position) > radius) continue;

            var agent = e.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            // Запоминаем через простой coroutine на целевом компоненте
            StartCoroutine(BoostOrc(agent, speedMult, duration));
            boosted++;
        }

        if (boosted == 0) return;

        // Собственный буст
        if (_agent != null)
        {
            _preBoostSpeed = _agent.speed;
            float add = _agent.speed * (speedMult - 1f);
            _boostAmount = add;
            _agent.speed += add;
            _boostActive = true;
            _boostTimer  = duration;
        }

        FeedbackManager.Instance?.FloatText(
            transform.position + Vector3.up * 2.8f,
            "ЗА ОРДУ!", new Color(1f, 0.60f, 0.05f), 1.6f);
        FeedbackManager.Instance?.SpawnBurst(
            transform.position + Vector3.up * 1.2f,
            new Color(1f, 0.55f, 0.08f),
            new Color(0.85f, 0.25f, 0.02f), 22, 5f, 0.13f);
        Camera.main?.GetComponent<CameraShake>()?.Shake(0.14f, 0.20f);
    }

    IEnumerator BoostOrc(NavMeshAgent agent, float mult, float duration)
    {
        if (agent == null) yield break;
        float orig = agent.speed;
        agent.speed = orig * mult;
        yield return new WaitForSeconds(duration);
        if (agent != null) agent.speed = orig;
    }

    // ─── PoisonCloud ─────────────────────────────────────────────

    void TryPoisonCloud()
    {
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) _player = go.transform;
        }

        Vector3 cloudPos = transform.position;
        float radius = 3.5f;

        FeedbackManager.Instance?.SpawnBurst(
            cloudPos + Vector3.up * 0.5f,
            new Color(0.25f, 0.85f, 0.20f),
            new Color(0.10f, 0.60f, 0.10f), 28, 4.5f, 0.14f);
        FeedbackManager.Instance?.FloatText(
            cloudPos + Vector3.up * 2f,
            "ЯД!", new Color(0.20f, 0.90f, 0.15f), 1.0f);

        if (_player != null &&
            Vector3.Distance(cloudPos, _player.position) <= radius)
        {
            int dmg = 15;
            _player.GetComponent<PlayerHealth>()?.TakeDamage(dmg, cloudPos);
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.10f, 0.18f);
            FeedbackManager.Instance?.FloatText(
                _player.position + Vector3.up * 1.8f,
                "ЯД -" + dmg, new Color(0.25f, 1f, 0.15f), 1.2f);
        }
    }
}
