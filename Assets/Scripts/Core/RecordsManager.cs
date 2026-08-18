using UnityEngine;
using System.Collections.Generic;

/// Обёртка над PlayerPrefs для рекордов + top-5 история забегов.
public static class RecordsManager
{
    // ── Ключи PlayerPrefs ──
    const string K_BestScore    = "AE_BestScore";
    const string K_BestKills    = "AE_BestKills";
    const string K_BestTime     = "AE_BestTime";
    const string K_BestStreak   = "AE_BestStreak";
    const string K_BestDistance = "AE_BestDistance";
    const string K_TotalRuns    = "AE_TotalRuns";
    const string K_TotalKills   = "AE_TotalKills";
    const string K_BossDefeats  = "AE_BossDefeats";
    const string K_Top5         = "AE_Top5Runs";

    // ═══════════════════════════════════════
    // Top-5 entry
    // ═══════════════════════════════════════

    [System.Serializable]
    public class RunEntry
    {
        public int score;
        public int kills;
        public float time;
        public bool boss;
        public int streak;
    }

    [System.Serializable]
    class RunList
    {
        public List<RunEntry> runs = new List<RunEntry>();
    }

    static RunList _cache;

    static RunList LoadTop5()
    {
        if (_cache != null) return _cache;
        string json = PlayerPrefs.GetString(K_Top5, "");
        if (!string.IsNullOrEmpty(json))
        {
            try { _cache = JsonUtility.FromJson<RunList>(json); }
            catch { _cache = new RunList(); }
        }
        if (_cache == null) _cache = new RunList();
        return _cache;
    }

    static void SaveTop5()
    {
        var list = LoadTop5();
        PlayerPrefs.SetString(K_Top5, JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    public static List<RunEntry> GetTop5()
    {
        return LoadTop5().runs;
    }

    static void InsertRun(RunScoreData d)
    {
        var list = LoadTop5();
        var entry = new RunEntry
        {
            score = d.score,
            kills = d.kills,
            time = d.runTime,
            boss = d.bossDefeated,
            streak = d.bestStreak
        };

        list.runs.Add(entry);
        list.runs.Sort((a, b) => b.score.CompareTo(a.score));
        if (list.runs.Count > 5)
            list.runs.RemoveRange(5, list.runs.Count - 5);

        SaveTop5();
    }

    // ═══════════════════════════════════════
    // Чтение рекордов
    // ═══════════════════════════════════════

    public static int   BestScore    => PlayerPrefs.GetInt(K_BestScore, 0);
    public static int   BestKills    => PlayerPrefs.GetInt(K_BestKills, 0);
    public static float BestTime     => PlayerPrefs.GetFloat(K_BestTime, 0f);
    public static int   BestStreak   => PlayerPrefs.GetInt(K_BestStreak, 0);
    public static float BestDistance => PlayerPrefs.GetFloat(K_BestDistance, 0f);
    public static int   TotalRuns    => PlayerPrefs.GetInt(K_TotalRuns, 0);
    public static int   TotalKills   => PlayerPrefs.GetInt(K_TotalKills, 0);
    public static int   BossDefeats  => PlayerPrefs.GetInt(K_BossDefeats, 0);

    // ═══════════════════════════════════════
    // Сохранение рекордов (если побиты)
    // ═══════════════════════════════════════

    public static void SaveIfBest(RunScoreData d)
    {
        bool changed = false;

        if (d.score > BestScore)
        {
            PlayerPrefs.SetInt(K_BestScore, d.score);
            changed = true;
        }

        if (d.kills > BestKills)
        {
            PlayerPrefs.SetInt(K_BestKills, d.kills);
            changed = true;
        }

        if (d.bossDefeated && d.runTime > 0f)
        {
            float cur = BestTime;
            if (cur <= 0f || d.runTime < cur)
            {
                PlayerPrefs.SetFloat(K_BestTime, d.runTime);
                changed = true;
            }
        }

        if (d.bestStreak > BestStreak)
        {
            PlayerPrefs.SetInt(K_BestStreak, d.bestStreak);
            changed = true;
        }

        if (d.longestKillDistance > BestDistance)
        {
            PlayerPrefs.SetFloat(K_BestDistance, d.longestKillDistance);
            changed = true;
        }

        if (changed) PlayerPrefs.Save();
    }

    // ═══════════════════════════════════════
    // Накопительная статистика + top-5
    // ═══════════════════════════════════════

    public static void AddRun(RunScoreData d)
    {
        PlayerPrefs.SetInt(K_TotalRuns, TotalRuns + 1);
        PlayerPrefs.SetInt(K_TotalKills, TotalKills + d.kills);

        if (d.bossDefeated)
            PlayerPrefs.SetInt(K_BossDefeats, BossDefeats + 1);

        PlayerPrefs.Save();
        InsertRun(d);
    }

    // ═══════════════════════════════════════
    // Сброс
    // ═══════════════════════════════════════

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(K_BestScore);
        PlayerPrefs.DeleteKey(K_BestKills);
        PlayerPrefs.DeleteKey(K_BestTime);
        PlayerPrefs.DeleteKey(K_BestStreak);
        PlayerPrefs.DeleteKey(K_BestDistance);
        PlayerPrefs.DeleteKey(K_TotalRuns);
        PlayerPrefs.DeleteKey(K_TotalKills);
        PlayerPrefs.DeleteKey(K_BossDefeats);
        PlayerPrefs.DeleteKey(K_Top5);
        _cache = null;
        PlayerPrefs.Save();
    }

    public static bool IsNewBestScore(int score) => score > BestScore;
    public static bool IsNewBestKills(int kills) => kills > BestKills;
    public static bool IsNewBestStreak(int streak) => streak > BestStreak;
}
