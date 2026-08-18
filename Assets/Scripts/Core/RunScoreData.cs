using UnityEngine;

/// Все данные одного забега. Plain C# — никаких MonoBehaviour.
public class RunScoreData
{
    // ── Очки ──
    public int score;

    // ── Убийства ──
    public int kills;
    public int orcWarriorKills;
    public int orcArcherKills;
    public int eliteKills;
    public int bossKills;

    // ── Бонусные метрики ──
    public int   headshotKills;
    public int   criticalHits;
    public int   stompKills;
    public float longestKillDistance;

    // ── Kill streak ──
    public int   currentStreak;
    public int   bestStreak;
    public float lastKillTime;   // Time.time последнего убийства

    // ── Прогресс ──
    public float runTime;
    public bool  level0Cleared;
    public bool  sceneTwoCleared;
    public bool  bossDefeated;

    // ── Зоны без урона (для будущего no-hit bonus) ──
    public bool gateRaidNoHit;
    public bool orcVillageNoHit;
    public bool finalZoneNoHit;

    public void Reset()
    {
        score              = 0;
        kills              = 0;
        orcWarriorKills    = 0;
        orcArcherKills     = 0;
        eliteKills         = 0;
        bossKills          = 0;
        headshotKills      = 0;
        criticalHits       = 0;
        stompKills         = 0;
        longestKillDistance = 0f;
        currentStreak      = 0;
        bestStreak         = 0;
        lastKillTime       = -999f;
        runTime            = 0f;
        level0Cleared      = false;
        sceneTwoCleared    = false;
        bossDefeated       = false;
        gateRaidNoHit      = true;
        orcVillageNoHit    = true;
        finalZoneNoHit     = true;
    }
}
