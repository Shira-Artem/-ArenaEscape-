using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// WeaponController v8 — combo меч, конусный хитбокс, hit-freeze, stagger, URP-шейдеры
public class WeaponController : MonoBehaviour
{
    struct Weapon { public string name, icon; public int dmg; public float cd; }

    const float MELEE_RANGE  = 3.2f;
    const float COMBO_RESET  = 0.65f;   // пауза без удара → сброс комбо
    const float CONE_DOT     = 0.30f;   // минимальный dot для хитбокса (конус ~106°)

    // ── Оружия ─────────────────────────────────────────────────────────────
    readonly Weapon _sword = new Weapon { name = "Меч",   icon = "⚔",  dmg = 40, cd = 0.28f };
    readonly Weapon _spear = new Weapon { name = "Копьё", icon = "↟",  dmg = 65, cd = 0.85f };  // было 1.2
    readonly Weapon _bow   = new Weapon { name = "Лук",   icon = "🏹", dmg = 32, cd = 0.26f };  // бластер: быстрый темп

    [HideInInspector] public int bonusSwordDamage;
    [HideInInspector] public int bonusSpearDamage;
    [HideInInspector] public int bonusBowDamage;

    Weapon _cur;
    int    _wi;
    float  _lastSwing;
    bool   _swinging, _isAiming, _built;
    float  _hitFlash, _swingFlash, _crossScale = 1f;
    float  _fovKick;

    // Bow — анимация отката тетивы при выстреле (1 → 0)
    float _bowFireFlash;
    float _bowChargeStart = -1f;   // legacy (заряд убран; держим для совместимости)

    // Kickback
    Vector3 _kickPos;
    Vector3 _kickRot;

    // Weapon bob
    float _bobTimer;
    float _bobAmount;

    // ── Комбо-система ──────────────────────────────────────────────────────
    int   _comboStep;           // 0=первый удар, 1=второй, 2=третий (тяжёлый)
    float _comboResetTimer;     // таймер окна следующего удара
    float _effectiveSwordCd;    // кд текущего удара в цепи

    Camera      _cam;
    CameraShake _shake;
    MouseLook   _mouseLook;
    Transform   _playerT;
    Rigidbody   _playerRb;
    CharacterController _cc;

    GameObject _root, _swordGO, _spearGO, _bowGO;
    LineRenderer _bowStringLR;
    Vector3    _sPos;
    Quaternion _sRot;

    static readonly string SwordPrefab = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Sword.prefab";
    static readonly string SpearPrefab = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/2Spear.prefab";

    // ── Позиции оружия от первого лица ────────────────────────────────────
    static readonly Vector3 S_REST  = new Vector3(0.33f, -0.36f, 0.58f);
    static readonly Vector3 SR_REST = new Vector3(8f, -12f, 0f);

    static readonly Vector3 SP_REST  = new Vector3(0.22f, -0.28f, 0.62f);
    static readonly Vector3 SPR_REST = new Vector3(5f, -6f, 12f);

    static readonly Vector3 B_REST  = new Vector3(0.30f, -0.34f, 0.55f);
    static readonly Vector3 B_AIM   = new Vector3(0.06f, -0.20f, 0.42f);
    static readonly Vector3 BR_REST = new Vector3(6f, -10f, 0f);
    static readonly Vector3 BR_AIM  = Vector3.zero;

    // Поза уклонения — оружие опускается и поворачивается
    static readonly Vector3 DODGE_POS = new Vector3(0.20f, -0.50f, 0.48f);
    static readonly Vector3 DODGE_ROT = new Vector3(25f, 10f, 18f);

    // ══════════════════════════════════════════════════════════════════════
    // INIT
    // ══════════════════════════════════════════════════════════════════════

    void Start()
    {
        _cam      = GetComponent<Camera>();
        _shake    = GetComponent<CameraShake>();
        _mouseLook = GetComponent<MouseLook>();
        if (_cam != null) gameObject.tag = "MainCamera";

        _playerT = transform.parent;
        if (_playerT == null || !_playerT.CompareTag("Player"))
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) _playerT = po.transform;
        }
        if (_playerT != null)
        {
            _playerRb = _playerT.GetComponent<Rigidbody>();
            _cc       = _playerT.GetComponent<CharacterController>();
        }

        _effectiveSwordCd = _sword.cd;
        StartCoroutine(BuildAfterFrame());
    }

    IEnumerator BuildAfterFrame()
    {
        yield return null;
        yield return null;
        Build();
        _cur  = _sword;
        _sPos = S_REST;
        _sRot = Quaternion.Euler(SR_REST);
        _built = true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════════════════

    void Update()
    {
        if (!_built || Time.timeScale == 0) return;
        float dt = Time.deltaTime;

        // ── Смена оружия ──
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Equip(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Equip(2);
        if (Input.GetKeyDown(KeyCode.Tab))    Equip((_wi + 1) % 3);

        // ── Кулдаун готовности ──
        float activeCd = _wi == 0 ? _effectiveSwordCd : _cur.cd;
        bool ready = Time.time >= _lastSwing + activeCd;

        // ── Прицеливание луком (ПКМ) ──
        _isAiming = (_wi == 2) && Input.GetMouseButton(1);

        // ── FOV ──
        float baseFov = _isAiming ? 62f : 90f;
        // Кик FOV на спринте — мир расширяется, скорость читается телом.
        if (!_isAiming && Input.GetKey(KeyCode.LeftShift) && GetMoveSpeed() > 7f)
            baseFov += 7f;
        _fovKick = Mathf.Lerp(_fovKick, 0f, 12f * dt);
        if (_cam != null)
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, baseFov + _fovKick, 16f * dt);

        // ── ПКМ для меча/копья = смена оружия ──
        if (_wi != 2 && Input.GetMouseButtonDown(1) && !_swinging)
            Equip(_wi == 0 ? 1 : 0);

        // ── Атака ──
        bool fire = _wi == 0 ? Input.GetMouseButton(0) : (_wi == 1 ? Input.GetMouseButtonDown(0) : false);
        if (fire && !_swinging && ready)
        {
            if      (_wi == 0) StartCoroutine(MeleeSwing());
            else if (_wi == 1) StartCoroutine(SpearThrow());
        }

        // ── Лук: бластер — зажатие = быстрый авто-огонь, без заряда ──
        if (_wi == 2 && Input.GetMouseButton(0) && !_swinging && ready)
            StartCoroutine(BowShoot());

        // ── Тетива лука ──
        if (_wi == 2) UpdateBowString();

        // ── Комбо: сброс при паузе ──
        if (_comboResetTimer > 0f)
        {
            _comboResetTimer -= dt;
            if (_comboResetTimer <= 0f) _comboStep = 0;
        }

        // ── Weapon bob при ходьбе ──
        float moveSpd = GetMoveSpeed();
        if (moveSpd > 0.5f)
        {
            _bobTimer  += dt * (moveSpd * 1.8f + 4f);
            _bobAmount  = Mathf.Lerp(_bobAmount, Mathf.Clamp01(moveSpd * 0.12f), 8f * dt);
        }
        else
        {
            _bobAmount = Mathf.Lerp(_bobAmount, 0f, 6f * dt);
        }

        // ── Позиция оружия ──
        Vector3 tPos; Quaternion tRot;
        GetWeaponPose(out tPos, out tRot);

        float bobX = Mathf.Sin(_bobTimer) * 0.012f * _bobAmount;
        float bobY = Mathf.Sin(_bobTimer * 2f) * 0.008f * _bobAmount;
        tPos += new Vector3(bobX, bobY, 0f);

        bool dodging = PlayerController.Instance != null && PlayerController.Instance.IsDodging;
        float lerpSpd = _swinging ? 22f : dodging ? 20f : 10f;
        _sPos = Vector3.Lerp(_sPos, tPos, lerpSpd * dt);
        _sRot = Quaternion.Slerp(_sRot, tRot, lerpSpd * dt);

        _kickPos = Vector3.Lerp(_kickPos, Vector3.zero, 14f * dt);
        _kickRot = Vector3.Lerp(_kickRot, Vector3.zero, 14f * dt);

        if (_root != null)
        {
            _root.transform.localPosition = _sPos + _kickPos;
            _root.transform.localRotation = _sRot * Quaternion.Euler(_kickRot);
        }

        // ── Эффекты ──
        _hitFlash     = Mathf.Max(0, _hitFlash     - dt * 6f);
        _swingFlash   = Mathf.Max(0, _swingFlash    - dt * 10f);
        _bowFireFlash = Mathf.Max(0, _bowFireFlash  - dt * 14f);

        float crossTarget = 1f + moveSpd * 0.06f;
        _crossScale = Mathf.Lerp(_crossScale, crossTarget, 8f * dt);
    }

    float GetMoveSpeed()
    {
        if (_cc  != null) return _cc.velocity.magnitude;
        if (_playerRb != null) return _playerRb.linearVelocity.magnitude;
        return 0f;
    }

    void GetWeaponPose(out Vector3 pos, out Quaternion rot)
    {
        // Поза уклонения — оружие опускается
        if (PlayerController.Instance != null && PlayerController.Instance.IsDodging)
        {
            pos = DODGE_POS;
            rot = Quaternion.Euler(DODGE_ROT);
            return;
        }

        switch (_wi)
        {
            case 1:
                pos = SP_REST; rot = Quaternion.Euler(SPR_REST);
                break;
            case 2:
                pos = _isAiming ? B_AIM : B_REST;
                rot = Quaternion.Euler(_isAiming ? BR_AIM : BR_REST);
                break;
            default:
                pos = S_REST; rot = Quaternion.Euler(SR_REST);
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // АТАКИ
    // ══════════════════════════════════════════════════════════════════════

    IEnumerator MeleeSwing()
    {
        _swinging         = true;
        _lastSwing        = Time.time;
        _swingFlash       = 0.2f;
        GameAudioManager.Instance?.Play(GameAudioManager.SFX.Swing, 0.5f);
        _comboResetTimer  = COMBO_RESET;

        bool isHeavy = _comboStep == 2;

        // Выпад: рывок в направлении камеры при каждом ударе
        PlayerController.Instance?.AddLunge(transform.forward, isHeavy ? 4.5f : 2.8f);

        // Кулдаун текущего удара
        _effectiveSwordCd = isHeavy ? 0.50f : 0.19f;

        // Kickback по фазе комбо
        switch (_comboStep)
        {
            case 0: // горизонтальный слева направо
                _kickPos = new Vector3(-0.10f, 0.10f, -0.10f);
                _kickRot = new Vector3(45f, 15f, -22f);
                _fovKick = 3f;
                FeedbackManager.Instance?.PlayWhoosh();
                break;
            case 1: // диагональный сверху-слева
                _kickPos = new Vector3(0.08f, 0.12f, -0.08f);
                _kickRot = new Vector3(48f, -12f, 14f);
                _fovKick = 3.5f;
                FeedbackManager.Instance?.PlayWhoosh();
                break;
            case 2: // тяжёлый вертикальный
                _kickPos = new Vector3(0f, 0.22f, -0.18f);
                _kickRot = new Vector3(70f, 0f, -5f);
                _fovKick = 7f;
                FeedbackManager.Instance?.PlayCrit();
                break;
        }

        DoMeleeHit(isHeavy);

        _comboStep = (_comboStep + 1) % 3;

        yield return new WaitForSeconds(isHeavy ? 0.16f : 0.11f);
        _swinging = false;
    }

    void DoMeleeHit(bool isHeavy = false)
    {
        Vector3 origin = _playerT != null
            ? _playerT.position + Vector3.up * 1.0f
            : transform.position;

        Vector3 camFwd = transform.forward; // WeaponController на камере

        // Веер: собираем ВСЕХ врагов в конусе, не только ближайшего
        EnemyAI primary = null;
        float bestD = MELEE_RANGE;
        var inCone = new System.Collections.Generic.List<EnemyAI>();

        var all = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (e == null || e.state == EnemyAI.State.Dead) continue;

            Vector3 toEnemy = e.transform.position + Vector3.up * 0.8f - origin;
            float d         = toEnemy.magnitude;
            if (d >= MELEE_RANGE) continue;

            // Конус: враг должен быть перед игроком (dot > 0.30 ≈ 106° конус)
            if (Vector3.Dot(camFwd, toEnemy.normalized) < CONE_DOT) continue;

            inCone.Add(e);
            if (d < bestD) { bestD = d; primary = e; }
        }

        if (primary == null) return;

        float dmgMult = isHeavy ? 1.65f : 0.85f;
        bool  isCrit  = Random.value < 0.15f;
        int   baseDmg = Mathf.RoundToInt((_cur.dmg + bonusSwordDamage) * dmgMult);
        int   primaryDmg = isCrit ? Mathf.RoundToInt(baseDmg * 1.5f) : baseDmg;

        if (isCrit) primary.SetNextHitBonus(RunScoreManager.KillBonus.CriticalHit);
        primary.TakeDamage(primaryDmg);

        // Вторичные враги в конусе — 65% урона, без крита
        foreach (var e in inCone)
        {
            if (e == primary) continue;
            e.TakeDamage(Mathf.RoundToInt(baseDmg * 0.65f));
        }

        GameAudioManager.Instance?.Play(isCrit ? GameAudioManager.SFX.CritHit : GameAudioManager.SFX.Hit, 0.6f);

        _hitFlash   = 0.7f;
        _crossScale = isCrit ? 3.5f : isHeavy ? 2.8f : 2.2f;

        float shakeStrength = isCrit ? 0.28f : isHeavy ? 0.22f : 0.12f;
        float shakeDur      = isCrit ? 0.30f : isHeavy ? 0.26f : 0.15f;
        if (inCone.Count > 1) { shakeStrength *= 1.25f; shakeDur *= 1.15f; }
        _shake?.Shake(shakeStrength, shakeDur);
        _fovKick += isCrit ? 5f : isHeavy ? 4f : 3f;

        if (_mouseLook) _mouseLook.ApplyRecoil(isCrit ? -0.7f : isHeavy ? -0.5f : -0.3f, 0.15f);
        _kickPos += new Vector3(0f, 0.03f, -0.06f);

        string multi = inCone.Count > 1 ? $" ×{inCone.Count}" : "";
        string msg = isCrit  ? $"★ КРИТ! -{primaryDmg}!"
                   : isHeavy ? $"⚡ -{primaryDmg}!{multi}"
                   :           $"⚔ -{primaryDmg}!{multi}";
        GameManager.Instance?.ShowMessage(msg, 0.6f);

        // Hit-freeze через единую систему HitStop
        float freezeDur = isCrit ? 0.050f : isHeavy ? 0.055f : 0.030f;
        HitStop.Freeze(freezeDur);

        // Stagger: NavMeshAgent останавливается на время
        float staggerDur = isHeavy ? 0.45f : 0.22f;
        if (isCrit) staggerDur += 0.15f;
        var nav = primary.GetComponent<NavMeshAgent>();
        if (nav != null) StartCoroutine(StaggerNav(nav, staggerDur));

        // Knockback: тяжёлый удар и крит отбрасывают врага
        if (isHeavy || isCrit)
        {
            Vector3 knockDir = (primary.transform.position - origin).normalized;
            float knockForce = isHeavy ? 3.5f : 2f;
            if (isCrit) knockForce *= 1.4f;
            primary.ApplyKnockback(knockDir, knockForce);
        }
    }

    IEnumerator StaggerNav(NavMeshAgent nav, float duration)
    {
        if (nav == null) yield break;
        float saved = nav.speed;
        nav.speed = 0f;
        yield return new WaitForSeconds(duration);
        if (nav != null) nav.speed = Mathf.Max(saved, nav.speed);
    }

    IEnumerator SpearThrow()
    {
        _swinging  = true;
        _lastSwing = Time.time;

        // Замах — тяжёлый отвод назад
        _kickPos = new Vector3(0.08f, 0.22f, -0.35f);
        _kickRot = new Vector3(-30f, -10f, 15f);
        _fovKick = -5f;
        FeedbackManager.Instance?.PlayWhoosh();
        _shake?.Shake(0.06f, 0.08f);   // лёгкая дрожь при замахе

        yield return new WaitForSeconds(0.14f);

        // Выброс — резкий возврат вперёд
        _kickPos = new Vector3(-0.05f, -0.08f, 0.18f);
        _kickRot = new Vector3(35f, 6f, -10f);
        _fovKick = -6f;

        var dir = transform.forward;
        var org = transform.position + dir * 1.0f;
        SpearProjectile.Fire(org, dir, _spear.dmg + bonusSpearDamage, gameObject);
        GameAudioManager.Instance?.Play(GameAudioManager.SFX.SpearThrow, 0.55f);
        if (_mouseLook) _mouseLook.ApplyRecoil(-1.1f, 0.28f);
        _shake?.Shake(0.18f, 0.18f);   // сильный толчок при броске

        yield return new WaitForSeconds(0.15f);
        _swinging = false;
    }

    IEnumerator BowShoot()
    {
        _swinging  = true;
        _lastSwing = Time.time;

        // Мгновенный выстрел — стреляем сразу, без долгого замаха (фил бластера).
        FireArrow();
        GameAudioManager.Instance?.Play(GameAudioManager.SFX.BowShoot, 0.45f);
        FeedbackManager.Instance?.PlayWhoosh();

        _bowFireFlash = 1f;                       // быстрый откат тетивы
        _kickPos = new Vector3(0.02f, 0.05f, -0.10f);
        _kickRot = new Vector3(-6f, 2f, 2f);
        _fovKick = 2.5f;
        if (_mouseLook) _mouseLook.ApplyRecoil(-0.5f, 0.05f);   // лёгкая, чтобы не мешала темпу
        _shake?.Shake(0.05f, 0.07f);
        _crossScale = 1.8f;

        yield return new WaitForSeconds(0.05f);
        _swinging = false;
    }

    void FireArrow()
    {
        // Направление = ТОЧНО в прицел. Без aim-assist, без заряда.
        var dir = transform.forward;

        // Совсем небольшой разброс — почти лазер, но не идеально стерильно.
        const float spread = 0.006f;
        dir += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread));
        dir.Normalize();

        var org = transform.position + dir * 0.8f;
        var go  = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Arrow";
        go.transform.position  = org;
        go.transform.rotation  = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
        go.transform.localScale = new Vector3(0.04f, 0.35f, 0.04f);

        // URP-safe материал (был Standard → розовый в URP)
        var mat = new Material(LitShader()) { color = new Color(0.88f, 0.65f, 0.18f) };
        go.GetComponent<Renderer>().material = mat;

        var rb = go.AddComponent<Rigidbody>();
        // Очень быстро и БЕЗ падения — попадание почти мгновенное (фил бластера).
        rb.linearVelocity = dir * 140f;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var ap = go.AddComponent<ArrowProjectile>();
        ap.damage = _bow.dmg + bonusBowDamage;
        ap.pierce = false;
        ap.shooterPosition = _playerT != null ? _playerT.position : transform.position;

        var trail = go.AddComponent<TrailRenderer>();
        trail.time        = 0.10f;
        trail.startWidth  = 0.025f;
        trail.endWidth    = 0f;
        trail.material    = new Material(LitShader());
        trail.startColor  = new Color(0.95f, 0.80f, 0.35f, 0.9f);
        trail.endColor    = new Color(0.95f, 0.80f, 0.35f, 0f);

        Destroy(go, 5f);
    }

    void UpdateBowString()
    {
        if (_bowStringLR == null) return;
        // Нок резко уходит назад (-Z) в момент выстрела и быстро возвращается вперёд.
        float z = Mathf.Lerp(0.04f, -0.06f, _bowFireFlash);
        _bowStringLR.SetPosition(1, new Vector3(0f, 0f, z));
    }

    void Equip(int idx)
    {
        _wi  = idx;
        _cur = idx == 0 ? _sword : idx == 1 ? _spear : _bow;
        if (_swordGO) _swordGO.SetActive(idx == 0);
        if (_spearGO) _spearGO.SetActive(idx == 1);
        if (_bowGO)   _bowGO.SetActive(idx == 2);

        _comboStep        = 0;
        _comboResetTimer  = 0f;
        _effectiveSwordCd = _sword.cd;
        _bowChargeStart   = -1f;

        _kickPos = new Vector3(0f, -0.15f, 0f);
        _kickRot = new Vector3(-10f, 0f, 0f);

        GameManager.Instance?.ShowMessage($"{_cur.icon} {_cur.name}", 0.8f);
    }

    // ══════════════════════════════════════════════════════════════════════
    // GUI
    // ══════════════════════════════════════════════════════════════════════

    void OnGUI()
    {
        if (!_built) return;
        int sw = Screen.width, sh = Screen.height;
        float cx = sw * 0.5f, cy = sh * 0.5f;

        if (PlayerController.DodgeFlashTimer > 0f)
        {
            PlayerController.DodgeFlashTimer -= Time.unscaledDeltaTime;
            float a = (PlayerController.DodgeFlashTimer / 0.12f) * 0.22f;
            GUI.color = new Color(0.35f, 0.65f, 1f, a);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Флеш попадания
        if (_hitFlash > 0)
        {
            GUI.color = new Color(1f, 0.12f, 0.03f, _hitFlash * 0.35f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        }
        if (_swingFlash > 0)
        {
            GUI.color = new Color(1f, 1f, 1f, _swingFlash * 0.12f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;

        // Прицелы
        if      (_wi == 2 && _isAiming) DrawBowAimSight(cx, cy);
        else if (_wi == 1)               DrawSpearCross(cx, cy);
        else if (_wi == 2)               DrawBowHipCross(cx, cy);
        else                             DrawCross(cx, cy);

        // Кулдаун
        float activeCd = _wi == 0 ? _effectiveSwordCd : _cur.cd;
        float prog = Mathf.Clamp01((Time.time - _lastSwing) / activeCd);
        if (prog < 1f) DrawCooldown(cx, cy, prog);

        // Комбо-индикатор (только для меча)
        if (_wi == 0) DrawComboIndicator(sw, sh);

        // Панель оружия
        DrawWeaponPanel(sw, sh, prog);

        // Уклонение
        DrawDodgeIndicator(sw, sh);
    }

    void DrawCross(float cx, float cy)
    {
        float s = _crossScale, ln = 7f * s, gp = 4f * s;
        Color c = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.1f), _hitFlash);
        c.a = 0.9f;
        GUI.color = c;
        GUI.DrawTexture(new Rect(cx - gp - ln, cy - 1, ln, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx + gp, cy - 1, ln, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - gp - ln, 2, ln), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy + gp, 2, ln), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1.5f, cy - 1.5f, 3f, 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawSpearCross(float cx, float cy)
    {
        float s = _crossScale;
        Color c = Color.Lerp(new Color(1f, 0.85f, 0.2f, 0.85f), new Color(1f, 0.3f, 0.1f), _hitFlash);
        GUI.color = c;
        GUI.DrawTexture(new Rect(cx - 1, cy - 14f * s, 2, 10f * s), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy + 4f * s, 2, 10f * s), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 3, cy - 16f * s, 6, 4), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 8f * s, cy - 1, 5f * s, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx + 3f * s, cy - 1, 5f * s, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawBowHipCross(float cx, float cy)
    {
        float s = _crossScale;
        GUI.color = new Color(1, 1, 1, 0.6f);
        float gp = 6f * s, ln = 5f * s;
        GUI.DrawTexture(new Rect(cx - gp - ln, cy - 1, ln, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx + gp, cy - 1, ln, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - gp - ln, 2, ln), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy + gp, 2, ln), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - 1, 2, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawBowAimSight(float cx, float cy)
    {
        GUI.color = new Color(1, 1, 1, 0.9f);
        float r = 20f;
        for (int i = 0; i < 32; i++)
        {
            float a   = i / 32f * Mathf.PI * 2f;
            float deg = i * 360f / 32f;
            if (deg % 90f < 18f || deg % 90f > 72f) continue;
            GUI.DrawTexture(
                new Rect(cx + Mathf.Cos(a) * r - 1.5f, cy + Mathf.Sin(a) * r - 1.5f, 3f, 3f),
                Texture2D.whiteTexture);
        }
        GUI.DrawTexture(new Rect(cx - 2, cy - 2, 4, 4), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 28f, cy - 0.5f, 8f, 1), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx + 20f, cy - 0.5f, 8f, 1), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 0.5f, cy - 28f, 1, 8f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 0.5f, cy + 20f, 1, 8f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawCooldown(float cx, float cy, float prog)
    {
        for (int i = 0; i < 20; i++)
        {
            float a = i / 20f * Mathf.PI * 2f - Mathf.PI * 0.5f;
            GUI.color = new Color(1, 1, 1, i < prog * 20 ? 0.8f : 0.12f);
            GUI.DrawTexture(
                new Rect(cx + Mathf.Cos(a) * 16 - 1.5f, cy + Mathf.Sin(a) * 16 - 1.5f, 3f, 3f),
                Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }

    void DrawComboIndicator(int sw, int sh)
    {
        if (_comboStep == 0 && _comboResetTimer <= 0f) return;

        // Выше dodge-бара (dodge at sh-108, combo at sh-126)
        float bx = sw - 195f, by = sh - 126f;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };

        if (_comboStep == 2)
        {
            style.normal.textColor = new Color(1f, 0.45f, 0.05f);
            GUI.Label(new Rect(bx + 58, by, 130f, 18f), "⚡ ТЯЖЁЛЫЙ ГОТОВ!", style);
        }
        else if (_comboStep > 0)
        {
            style.normal.textColor = new Color(1f, 0.88f, 0.2f);
            GUI.Label(new Rect(bx + 58, by, 130f, 18f), $"КОМБО x{_comboStep + 1}", style);
        }
    }

    void DrawWeaponPanel(int sw, int sh, float prog)
    {
        float bx = sw - 195f, by = sh - 74f;
        GUI.color = new Color(0, 0, 0, 0.62f);
        GUI.DrawTexture(new Rect(bx, by, 188f, 66f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var ic = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
        ic.normal.textColor = Color.white;
        GUI.Label(new Rect(bx + 2, by + 4, 52f, 56f), _cur.icon, ic);

        var nm = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        nm.normal.textColor = Color.white;
        GUI.Label(new Rect(bx + 58, by + 5, 130f, 26f), _cur.name, nm);

        GUI.color = new Color(0.25f, 0.25f, 0.25f);
        GUI.DrawTexture(new Rect(bx + 58, by + 34, 124f, 10f), Texture2D.whiteTexture);
        GUI.color = prog >= 1f ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.9f, 0.65f, 0.1f);
        GUI.DrawTexture(new Rect(bx + 58, by + 34, 124f * prog, 10f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var st = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(bx + 58, by + 46, 124f, 18f), prog >= 1f ? "ГОТОВО" : $"{prog * 100:0}%", st);

        var ht = new GUIStyle(GUI.skin.label) { fontSize = 11 };
        ht.normal.textColor = new Color(0.6f, 0.6f, 0.65f);
        GUI.Label(new Rect(bx, by - 20f, 190f, 18f), "1-Меч  2-Копьё  3/Tab-Лук", ht);
    }

    void DrawDodgeIndicator(int sw, int sh)
    {
        if (PlayerController.Instance == null) return;
        float prog  = PlayerController.Instance.DodgeCooldownProgress;
        bool  ready = prog >= 1f;

        float bx = sw - 195f, by = sh - 108f;
        GUI.color = new Color(0, 0, 0, 0.55f);
        GUI.DrawTexture(new Rect(bx, by, 188f, 14f), Texture2D.whiteTexture);
        GUI.color = ready
            ? new Color(0.3f, 0.6f, 1f, 0.9f)
            : new Color(0.15f, 0.32f, 0.7f, 0.65f);
        GUI.DrawTexture(new Rect(bx, by, 188f * prog, 14f), Texture2D.whiteTexture);

        var st = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = Color.white;
        GUI.color = Color.white;
        GUI.Label(new Rect(bx, by, 188f, 14f),
            ready ? "УКЛОНЕНИЕ [Q/E]" : "УКЛОНЕНИЕ...", st);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ВИЗУАЛ ОРУЖИЯ
    // ══════════════════════════════════════════════════════════════════════

    void Build()
    {
        _root = new GameObject("WeaponRoot");
        _root.transform.SetParent(transform);
        _root.transform.localPosition = S_REST;
        _root.transform.localRotation = Quaternion.Euler(SR_REST);
        _root.transform.localScale    = Vector3.one;

        _swordGO = MakeSword(_root);
        _spearGO = MakeSpearView(_root); _spearGO.SetActive(false);
        _bowGO   = MakeBow(_root);       _bowGO.SetActive(false);
    }

    GameObject MakeSword(GameObject p)
    {
        var r = TryLoadWeaponAsset(SwordPrefab, p.transform, "Sword",
            new Vector3(0, 0.02f, 0.02f), Quaternion.Euler(0, 0, 0), Vector3.one * 0.45f);
        if (r != null) return r;

        r = new GameObject("Sword");
        r.transform.SetParent(p.transform);
        r.transform.localPosition = new Vector3(0, .07f, 0);
        r.transform.localScale    = Vector3.one;
        r.transform.localRotation = Quaternion.identity;
        Color st = new Color(.82f, .87f, .94f);
        Color gd = new Color(.83f, .68f, .12f);
        Color wd = new Color(.44f, .27f, .08f);
        Sph(r, new Vector3(0, -.24f, 0), .033f, gd);
        Cyl(r, new Vector3(0, -.13f, 0), new Vector3(.03f, .18f, .03f), wd);
        Box(r, new Vector3(0, 0, 0), new Vector3(.185f, .028f, .038f), gd);
        Box(r, new Vector3(0, 0, 0), new Vector3(.028f, .028f, .105f), gd);
        Box(r, new Vector3(0, .23f, 0), new Vector3(.033f, .40f, .014f), st);
        Box(r, new Vector3(0, .21f, 0), new Vector3(.01f, .32f, .004f), new Color(.65f, .70f, .80f));
        Box(r, new Vector3(0, .51f, 0), new Vector3(.018f, .11f, .010f), st);
        return r;
    }

    GameObject MakeSpearView(GameObject p)
    {
        var r = TryLoadWeaponAsset(SpearPrefab, p.transform, "SpearView",
            new Vector3(0.02f, -0.08f, 0.04f), Quaternion.Euler(0, 0, 5f), Vector3.one * 0.4f);
        if (r != null) return r;

        r = new GameObject("SpearView");
        r.transform.SetParent(p.transform);
        r.transform.localPosition = new Vector3(0.02f, -0.1f, 0.04f);
        r.transform.localScale    = Vector3.one;
        r.transform.localRotation = Quaternion.Euler(0, 0, 5f);

        Color shaft = new Color(0.45f, 0.30f, 0.15f);
        Color sdark = new Color(0.30f, 0.18f, 0.08f);
        Color tipGd = new Color(1f, 0.82f, 0.12f);
        Color tipEd = new Color(0.85f, 0.75f, 0.25f);

        Cyl(r, new Vector3(0, 0.15f, 0), new Vector3(0.028f, 0.65f, 0.028f), shaft);
        Cyl(r, new Vector3(0, -0.15f, 0), new Vector3(0.035f, 0.12f, 0.035f), sdark);
        Cyl(r, new Vector3(0, -0.05f, 0), new Vector3(0.032f, 0.06f, 0.032f), sdark);
        Cyl(r, new Vector3(0, 0.56f, 0), new Vector3(0.042f, 0.02f, 0.042f), tipGd);
        Box(r, new Vector3(0, 0.68f, 0), new Vector3(0.05f, 0.18f, 0.015f), tipGd);
        Box(r, new Vector3(0, 0.68f, 0), new Vector3(0.015f, 0.18f, 0.05f), tipEd);
        Box(r, new Vector3(0, 0.80f, 0), new Vector3(0.025f, 0.08f, 0.008f), tipGd);
        Cyl(r, new Vector3(0, -0.45f, 0), new Vector3(0.025f, 0.04f, 0.025f), sdark);
        Sph(r, new Vector3(0, -0.49f, 0), 0.018f, shaft);
        return r;
    }

    GameObject TryLoadWeaponAsset(string path, Transform parent, string name,
        Vector3 localPos, Quaternion localRot, Vector3 localScale)
    {
        var prefab = L0Util.LoadPrefab(path);
        if (prefab == null) return null;
        var go = Instantiate(prefab, parent);
        go.name = name;
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale    = localScale;
        foreach (var col in go.GetComponentsInChildren<Collider>()) Destroy(col);
        L0Util.FixMaterials(go);
        return go;
    }

    GameObject MakeBow(GameObject p)
    {
        var r = new GameObject("Bow");
        r.transform.SetParent(p.transform);
        r.transform.localPosition = new Vector3(-.02f, .02f, 0);
        r.transform.localScale    = Vector3.one;
        r.transform.localRotation = Quaternion.identity;

        Color wood  = new Color(.46f, .27f, .10f);
        Color wdark = new Color(.30f, .17f, .06f);
        Color grip  = new Color(.16f, .09f, .04f);
        Color str   = new Color(.92f, .89f, .78f);

        // Рукоять (грип) + кожаная обмотка
        Box(r, new Vector3(0, 0, 0), new Vector3(.045f, .24f, .055f), grip);
        Box(r, new Vector3(0, 0, .016f), new Vector3(.035f, .10f, .03f), new Color(.32f, .18f, .07f));

        // Дуга лука — гладкий рекурсивный изгиб из тонких сегментов.
        // sign +1 = верхняя дуга, -1 = нижняя. Изгиб назад (-Z), кончик-рекурв вперёд (+Z).
        void Limb(int sign)
        {
            Vector3[] pts = {
                new Vector3(0, sign * 0.12f, 0f),
                new Vector3(0, sign * 0.30f, -0.05f),
                new Vector3(0, sign * 0.46f, -0.03f),
                new Vector3(0, sign * 0.58f,  0.07f),  // рекурв-кончик вперёд
            };
            for (int i = 0; i < pts.Length - 1; i++)
            {
                Vector3 a = pts[i], b = pts[i + 1];
                Vector3 d = b - a;
                float len = d.magnitude;
                float thick = Mathf.Lerp(.040f, .020f, i / (float)(pts.Length - 1));
                var seg = Cyl(r, (a + b) * 0.5f, new Vector3(thick, len * 0.5f, thick), i < 2 ? wood : wdark);
                seg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
            }
        }
        Limb(1);
        Limb(-1);

        // Тетива: верхний кончик → нок → нижний кончик
        var sg = new GameObject("BowString");
        sg.transform.SetParent(r.transform);
        sg.transform.localPosition = Vector3.zero;
        var lr = sg.AddComponent<LineRenderer>();
        lr.positionCount  = 3;
        lr.useWorldSpace  = false;
        lr.startWidth = lr.endWidth = 0.006f;
        lr.numCapVertices = 2;
        lr.SetPosition(0, new Vector3(0, .58f, .07f));
        lr.SetPosition(1, new Vector3(0, 0f, .04f));   // нок (анимируется в UpdateBowString по Z)
        lr.SetPosition(2, new Vector3(0, -.58f, .07f));
        lr.material = new Material(LitShader()) { color = str };
        _bowStringLR = lr;

        // Наложенная стрела — вдоль +Z (в прицел). Ось Y цилиндра → Z поворотом X=90°.
        Color shaft = new Color(.55f, .35f, .12f);
        var arw = Cyl(r, new Vector3(.012f, 0, .20f), new Vector3(.011f, .26f, .011f), shaft);
        arw.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // Наконечник
        var tip = Sph(r, new Vector3(.012f, 0, .46f), .03f, new Color(.70f, .73f, .78f));
        tip.transform.localScale = new Vector3(.03f, .03f, .06f);
        // Оперение
        Box(r, new Vector3(.012f, .018f, -.02f), new Vector3(.004f, .045f, .05f), new Color(.86f, .30f, .20f));
        Box(r, new Vector3(.012f, -.018f, -.02f), new Vector3(.004f, .045f, .05f), new Color(.86f, .30f, .20f));
        return r;
    }

    // ── Хелперы примитивов ────────────────────────────────────────────────

    GameObject Box(GameObject p, Vector3 pos, Vector3 scl, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Init(g, p, pos, scl, c); return g;
    }
    GameObject Cyl(GameObject p, Vector3 pos, Vector3 scl, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Init(g, p, pos, scl, c); return g;
    }
    GameObject Sph(GameObject p, Vector3 pos, float rad, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Init(g, p, pos, Vector3.one * rad * 2, c); return g;
    }
    void Init(GameObject g, GameObject p, Vector3 pos, Vector3 scl, Color c)
    {
        g.transform.SetParent(p.transform);
        g.transform.localPosition = pos;
        g.transform.localScale    = scl;
        g.transform.localRotation = Quaternion.identity;
        Destroy(g.GetComponent<Collider>());
        // URP-safe (был Standard → розовый в URP)
        var m = new Material(LitShader());
        m.color = c;
        m.SetFloat("_Metallic",    0.3f);
        m.SetFloat("_Smoothness",  0.5f);
        g.GetComponent<Renderer>().material = m;
    }

    static Shader LitShader() =>
        Shader.Find("Universal Render Pipeline/Lit")
        ?? Shader.Find("Standard")
        ?? Shader.Find("Diffuse");
}
