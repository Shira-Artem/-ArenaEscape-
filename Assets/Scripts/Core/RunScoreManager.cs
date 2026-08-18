using UnityEngine;

/// Singleton-менеджер очков текущего забега.
/// DontDestroyOnLoad — живёт между сценами (Level0 → two).
public class RunScoreManager : MonoBehaviour
{
    public static RunScoreManager Instance { get; private set; }

    public RunScoreData Data { get; private set; } = new RunScoreData();

    // ── Типы врагов (используется при RegisterKill) ──
    public enum EnemyType { OrcWarrior, OrcArcher, Elite, Boss }

    // ── Контекст убийства (будет расширен в Phase 6) ──
    public enum KillBonus { None, Headshot, CriticalHit, Stomp }

    // ── Настройки streak ──
    const float StreakWindow = 5f;

    // ── Базовые очки ──
    const int PtsWarrior = 100;
    const int PtsArcher  = 100;
    const int PtsElite   = 250;
    const int PtsBoss    = 1000;

    // ── Бонусные очки ──
    const int PtsHeadshot = 75;
    const int PtsCrit     = 50;
    const int PtsStomp    = 50;
    const int PtsStreak3  = 100;
    const int PtsStreak5  = 250;
    const int PtsStreak10 = 500;

    // ── Long range (для Phase 6) ──
    const int PtsLongRange  = 50;   // 15м+
    const int PtsSniperKill = 100;  // 30м+

    bool _running;

    // ═══════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_running) Data.runTime += Time.deltaTime;
    }

    // ═══════════════════════════════════════
    // Run control
    // ═══════════════════════════════════════

    public void StartRun()
    {
        Data.Reset();
        _running = true;
    }

    public void StopRun()
    {
        _running = false;
    }

    // ═══════════════════════════════════════
    // Kill registration — главный метод
    // ═══════════════════════════════════════

    /// Зарегистрировать убийство. Возвращает сумму начисленных очков.
    public int RegisterKill(EnemyType type, KillBonus bonus = KillBonus.None, float killDistance = 0f)
    {
        int pts = 0;

        // 1. Базовые очки по типу
        switch (type)
        {
            case EnemyType.OrcWarrior: pts += PtsWarrior; Data.orcWarriorKills++; break;
            case EnemyType.OrcArcher:  pts += PtsArcher;  Data.orcArcherKills++;  break;
            case EnemyType.Elite:      pts += PtsElite;   Data.eliteKills++;      break;
            case EnemyType.Boss:       pts += PtsBoss;    Data.bossKills++;       Data.bossDefeated = true; break;
        }
        Data.kills++;

        // 2. Бонус за стиль
        switch (bonus)
        {
            case KillBonus.Headshot:    pts += PtsHeadshot; Data.headshotKills++; break;
            case KillBonus.CriticalHit: pts += PtsCrit;     Data.criticalHits++; break;
            case KillBonus.Stomp:       pts += PtsStomp;    Data.stompKills++;   break;
        }

        // 3. Бонус за дальность (для лука, Phase 6)
        if (killDistance >= 30f)       pts += PtsSniperKill;
        else if (killDistance >= 15f)  pts += PtsLongRange;

        if (killDistance > Data.longestKillDistance)
            Data.longestKillDistance = killDistance;

        // 4. Kill streak
        pts += ProcessStreak();

        // 5. Итого
        Data.score += pts;

        // 6. Debug-лог — потом уберём
        Debug.Log($"[Score] +{pts} ({type}{(bonus != KillBonus.None ? "/" + bonus : "")}) " +
                  $"| Total: {Data.score} | Kills: {Data.kills} | Streak: {Data.currentStreak}");

        return pts;
    }

    int ProcessStreak()
    {
        float now = Time.time;
        if (now - Data.lastKillTime <= StreakWindow)
            Data.currentStreak++;
        else
            Data.currentStreak = 1;

        Data.lastKillTime = now;

        if (Data.currentStreak > Data.bestStreak)
            Data.bestStreak = Data.currentStreak;

        // Бонус начисляется ровно при достижении порога
        switch (Data.currentStreak)
        {
            case 3:  return PtsStreak3;
            case 5:  return PtsStreak5;
            case 10: return PtsStreak10;
            default: return 0;
        }
    }

    // ═══════════════════════════════════════
    // Прямое начисление очков (для collectibles, событий)
    // ═══════════════════════════════════════

    public void AddScore(int points, string reason = "")
    {
        Data.score += points;
        if (!string.IsNullOrEmpty(reason))
            Debug.Log($"[Score] +{points} ({reason}) | Total: {Data.score}");
    }

    // ═══════════════════════════════════════
    // No-hit tracking
    // ═══════════════════════════════════════

    public void PlayerTookDamageInZone(string zone)
    {
        switch (zone)
        {
            case "gate_raid":     Data.gateRaidNoHit    = false; break;
            case "orc_village":   Data.orcVillageNoHit  = false; break;
            case "final_zone":    Data.finalZoneNoHit   = false; break;
        }
    }

    // ═══════════════════════════════════════
    // Time bonus (при победе)
    // ═══════════════════════════════════════

    public int CalcTimeBonus()
    {
        if (Data.runTime < 600f) return 1000;   // < 10 мин — мастер
        if (Data.runTime < 720f) return 500;    // < 12 мин — быстро
        if (Data.runTime < 900f) return 250;    // < 15 мин — норм
        return 0;
    }

    // ═══════════════════════════════════════
    // Финализация run
    // ═══════════════════════════════════════

    /// Вызвать при победе. Начисляет time bonus, сохраняет рекорды.
    public void FinalizeVictory()
    {
        StopRun();
        int tb = CalcTimeBonus();
        if (tb > 0)
        {
            Data.score += tb;
            Debug.Log($"[Score] Time Bonus +{tb} | Final: {Data.score}");
        }
        RecordsManager.SaveIfBest(Data);
        RecordsManager.AddRun(Data);
    }

    /// Вызвать при смерти. Сохраняет рекорды (если побиты).
    public void FinalizeDeath()
    {
        StopRun();
        RecordsManager.SaveIfBest(Data);
        RecordsManager.AddRun(Data);
    }

    // ═══════════════════════════════════════
    // Форматирование для UI
    // ═══════════════════════════════════════

    public static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
