using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Лаба №7(1) «Контроль территории», переосмысленная как УДЕРЖАНИЕ ВОРОТ.
///
/// Это инвертированный CaptureZone: вместо захвата — оборона.
///   • Зона ворот вокруг gateCenter (0,0.2,-25).
///   • Считаем живых орков в зоне (orcsInZone) и живых стражников (aliveGuards).
///   • Шкала hold (1→0): если орков больше стражи — шкала «удержания» падает
///     пропорционально перевесу; если орков мало — медленно восстанавливается.
///   • Когда hold обнуляется — гибнет один стражник (строй прогибается), шкала
///     откатывается частично.
///   • Если все стражники погибли — гибнет игрок («без строя ворота не удержать»).
///   • Когда рейд очищен (castleGateRaidCleared) — фаза победно завершается.
///
/// Additive-компонент: НЕ переписывает логику Level0OrcRaidManager, только читает
/// EnemyAI (tag Enemy) и L0GateAllySoldier. Бутстрапится из рейд-менеджера.
/// </summary>
public class L0GateDefense : MonoBehaviour
{
    public static L0GateDefense Instance { get; private set; }

    [Header("Зона ворот")]
    public Vector3 gateCenter = new Vector3(0f, 0.2f, -25f);
    public float zoneRadius = 12f;

    [Header("Баланс удержания")]
    // R2: давление сильнее, реген слабее — без игрока стража медленно проигрывает.
    public float drainPerOrc = 0.085f;  // скорость падения шкалы за каждого «лишнего» орка
    public float regenRate = 0.14f;     // восстановление, когда давления нет
    public float holdResetOnGuardLoss = 0.40f; // куда откатывается шкала при гибели стража

    // ── Рантайм-состояние ──
    float _hold = 1f;
    int _orcsInZone;
    int _aliveGuards;
    bool _guardsEverSpawned;
    bool _phaseDone;
    float _scanTimer;
    const float ScanInterval = 0.3f;

    readonly List<L0GateAllySoldier> _guards = new List<L0GateAllySoldier>();

    // HUD
    Texture2D _bg, _fillGreen, _fillRed, _border;

    /// R4: активна ли фаза удержания (для подавления дублирующих HUD, напр. верхней
    /// строки цели в L0ObjectiveGuide). Один чистый HUD — шкала удержания.
    public bool HoldPhaseActive
    {
        get
        {
            if (!_guardsEverSpawned || _phaseDone) return false;
            GameProgressManager p = GameProgressManager.Instance;
            return p != null && p.castleGateRaidStarted && !p.castleGateRaidCleared;
        }
    }

    public static L0GateDefense Ensure()
    {
        if (Instance != null) return Instance;
        L0GateDefense found = FindFirstObjectByType<L0GateDefense>();
        if (found != null) { Instance = found; return found; }

        GameObject go = new GameObject("L0_GATE_DEFENSE");
        return go.AddComponent<L0GateDefense>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _bg        = MakeTex(new Color(0f, 0f, 0f, 0.7f));
        _fillGreen = MakeTex(new Color(0.20f, 0.78f, 0.28f, 1f));
        _fillRed   = MakeTex(new Color(0.88f, 0.16f, 0.08f, 1f));
        _border    = MakeTex(new Color(0.88f, 0.66f, 0.08f, 0.85f));
    }

    void Update()
    {
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress == null) return;

        // Победное завершение фазы — когда рейд очищен.
        if (progress.castleGateRaidCleared)
        {
            if (!_phaseDone) PhaseWin();
            return;
        }

        bool active = progress.castleGateRaidStarted && !progress.castleGateRaidCleared;
        if (!active) return;

        // Периодический пересчёт орков/стражи.
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = ScanInterval;
            RescanZone();
        }

        // Пока не появились стражники (1-я волна) — давление не считаем.
        if (!_guardsEverSpawned) return;

        // Все стражники пали → гибель игрока.
        if (_aliveGuards <= 0)
        {
            LoseGate();
            return;
        }

        // Давление: перевес орков над стражей.
        int pressure = _orcsInZone - _aliveGuards;
        if (pressure > 0)
            _hold -= pressure * drainPerOrc * Time.deltaTime;
        else
            _hold += regenRate * Time.deltaTime;

        _hold = Mathf.Clamp01(_hold);

        // Шкала обнулилась — строй прогибается, гибнет крайний стражник.
        if (_hold <= 0f)
        {
            KillFrontGuard();
            _hold = holdResetOnGuardLoss;
        }
    }

    void RescanZone()
    {
        // Орки в зоне (tag Enemy + живой EnemyAI в радиусе).
        _orcsInZone = 0;
        float r2 = zoneRadius * zoneRadius;
        EnemyAI[] all = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            EnemyAI ai = all[i];
            if (ai == null || ai.state == EnemyAI.State.Dead) continue;
            Vector3 d = ai.transform.position - gateCenter;
            d.y = 0f;
            if (d.sqrMagnitude <= r2) _orcsInZone++;
        }

        // Живые стражники.
        _guards.Clear();
        L0GateAllySoldier[] soldiers = FindObjectsByType<L0GateAllySoldier>(FindObjectsSortMode.None);
        for (int i = 0; i < soldiers.Length; i++)
        {
            if (soldiers[i] != null && !soldiers[i].IsDead)
                _guards.Add(soldiers[i]);
        }
        _aliveGuards = _guards.Count;
        if (_aliveGuards > 0) _guardsEverSpawned = true;
    }

    /// Убить «крайнего» стражника — того, кто ближе всего к наступающим оркам (фронт).
    void KillFrontGuard()
    {
        if (_guards.Count == 0) return;

        L0GateAllySoldier front = null;
        float bestZ = float.PositiveInfinity; // орки идут с -Z, фронт = наименьший Z
        for (int i = 0; i < _guards.Count; i++)
        {
            L0GateAllySoldier g = _guards[i];
            if (g == null || g.IsDead) continue;
            float z = g.transform.position.z;
            if (z < bestZ) { bestZ = z; front = g; }
        }

        if (front == null) front = _guards[0];

        front.Kill();
        _guards.Remove(front);
        _aliveGuards = Mathf.Max(0, _aliveGuards - 1);

        GameManager.Instance?.ShowMessage(
            _aliveGuards > 0
                ? $"Стражник пал! Осталось: {_aliveGuards}"
                : "Последний стражник пал!",
            GameManager.MsgType.Damage, 2.2f);
    }

    void LoseGate()
    {
        if (_phaseDone) return;
        _phaseDone = true;

        GameManager.Instance?.ShowMessage("ВОРОТА ПАЛИ! Стражники погибли...", GameManager.MsgType.Damage, 4f);

        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null) ph.KillFromEvent();
    }

    void PhaseWin()
    {
        _phaseDone = true;
        _hold = 1f;
        GameManager.Instance?.ShowMessage("Ворота удержаны! Союзники держат строй.", GameManager.MsgType.Kill, 4f);
        RunScoreManager.Instance?.AddScore(500, "gate_held");
    }

    void OnGUI()
    {
        if (_phaseDone || !_guardsEverSpawned) return;

        GameProgressManager progress = GameProgressManager.Instance;
        if (progress == null || !progress.castleGateRaidStarted || progress.castleGateRaidCleared)
            return;

        int sw = Screen.width;

        float barW = 420f;
        float barH = 20f;
        float x = sw * 0.5f - barW * 0.5f;
        float y = 48f; // ниже boss-бара (тот на y=18), чтобы не перекрывать

        // Фон
        GUI.DrawTexture(new Rect(x - 2, y - 2, barW + 4, barH + 4), _bg);

        // Заливка (зелёный→красный по мере падения)
        Texture2D fill = _hold > 0.45f ? _fillGreen : _fillRed;
        GUI.DrawTexture(new Rect(x, y, barW * _hold, barH), fill);

        // Рамка
        GUI.DrawTexture(new Rect(x - 2, y - 2, barW + 4, 2), _border);
        GUI.DrawTexture(new Rect(x - 2, y + barH, barW + 4, 2), _border);
        GUI.DrawTexture(new Rect(x - 2, y, 2, barH), _border);
        GUI.DrawTexture(new Rect(x + barW, y, 2, barH), _border);

        // Подпись
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = _hold > 0.45f
            ? new Color(1f, 0.92f, 0.4f)
            : new Color(1f, 0.4f, 0.2f);
        GUI.Label(new Rect(x, y - 20, barW, 18), "⚔ УДЕРЖАНИЕ ВОРОТ ⚔", title);

        // Счётчики
        GUIStyle info = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        info.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y + barH + 2, barW, 18),
            $"Стражники: {_aliveGuards}    Орков в зоне: {_orcsInZone}", info);
    }

    static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
