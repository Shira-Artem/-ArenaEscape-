using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public int TotalCoins { get; private set; }

    public enum MsgType { Default, Coin, Heal, Checkpoint, Kill, Damage }

    public bool suppressLegacyHud;

    string _message;
    MsgType _msgType;
    float _msgTimer;
    float _msgMaxTimer;

    bool _gameWon;
    public bool IsGameWon => _gameWon;
    int _killedEnemies;

    // Boss HP bar
    EnemyAI _boss;
    bool _bossSearched;

    // Zone entry text
    string _zoneTitle;
    string _zoneSubtitle;
    float _zoneTitleTimer;

    Texture2D _panel;
    Texture2D _gold;
    Texture2D _green;
    Texture2D _dark;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _panel = MakeTex(2, 2, new Color(0.04f, 0.03f, 0.01f, 0.96f));
        _gold = MakeTex(2, 2, new Color(0.88f, 0.66f, 0.08f, 1f));
        _green = MakeTex(2, 2, new Color(0.12f, 0.58f, 0.20f, 1f));
        _dark = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.82f));
    }

    void Start()
    {
        ResetForNewRun();
        RefreshCoinTotal();
        SaveSystem.Delete();
    }

    void Update()
    {
        if (_msgTimer > 0f) _msgTimer -= Time.deltaTime;
        if (_zoneTitleTimer > 0f) _zoneTitleTimer -= Time.deltaTime;

        if (!_bossSearched)
        {
            foreach (var ai in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            {
                if (ai != null && ai.isFinalBoss) { _boss = ai; break; }
            }
            _bossSearched = true;
        }
    }

    public void AddScore(int value)
    {
        Score += value;
        Coins++;
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        return true;
    }

    public void SetScore(int value) => Score = value;
    public void RegisterEnemy() { }

    public void OnEnemyKilled(bool isFinalBoss)
    {
        _killedEnemies++;
        if (isFinalBoss) TriggerWin();
    }

    void TriggerWin()
    {
        if (_gameWon) return;
        _gameWon = true;
        HitStop.CancelAll();
        Time.timeScale = 0f;
        MouseLook.Unlock();
        RunScoreManager.Instance?.FinalizeVictory();
    }

    public void ShowMessage(string m, MsgType type = MsgType.Default, float t = 2f)
    {
        _message = m;
        _msgType = type;
        _msgTimer = t;
        _msgMaxTimer = t;
    }

    public void ShowMessage(string m, float t) => ShowMessage(m, MsgType.Default, t);

    public void ResetForNewRun()
    {
        Score = 0;
        Coins = 0;
        TotalCoins = 0;
        _message = null;
        _msgTimer = 0f;
        _msgMaxTimer = 0f;
        _gameWon = false;
        _killedEnemies = 0;
        _boss = null;
        _bossSearched = false;
        _zoneTitleTimer = 0f;
    }

    public void ShowZoneTitle(string title, string subtitle = "", float duration = 4f)
    {
        _zoneTitle = title;
        _zoneSubtitle = subtitle;
        _zoneTitleTimer = duration;
    }

    public void RefreshCoinTotal()
    {
        TotalCoins = 0;
        foreach (var item in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
        {
            if (item != null && !item.isHealth) TotalCoins++;
        }
    }

    void OnGUI()
    {
        int sw = Screen.width;
        int sh = Screen.height;

        if (!_gameWon && !suppressLegacyHud) DrawHud();
        if (!_gameWon) DrawBossHpBar(sw);
        if (_zoneTitleTimer > 0f) DrawZoneTitle(sw, sh);
        if (_msgTimer > 0f) DrawMessage(sw, sh);
        if (_gameWon) DrawWinScreen(sw, sh);
    }

    void DrawHud()
    {
        var run = RunScoreManager.Instance?.Data;
        int displayScore = run?.score ?? Score;
        int displayKills = run?.kills ?? _killedEnemies;

        GUI.color = new Color(0f, 0f, 0f, 0.50f);
        GUI.DrawTexture(new Rect(10, 56, 210, 54), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
        s.normal.textColor = new Color(1f, 0.92f, 0.25f);
        GUI.Label(new Rect(18, 62, 200, 25), $"★ Очки: {displayScore}", s);
        GUI.Label(new Rect(18, 86, 200, 25), $"☠ Убийств: {displayKills}", s);
    }

    void DrawMessage(int sw, int sh)
    {
        float alpha = Mathf.Clamp01(_msgTimer / Mathf.Max(0.01f, _msgMaxTimer));
        Color textColor = new Color(1f, 0.9f, 0.15f);
        if (_msgType == MsgType.Damage) textColor = new Color(1f, 0.15f, 0.08f);
        if (_msgType == MsgType.Heal) textColor = new Color(0.25f, 1f, 0.35f);

        GUIStyle ms = new GUIStyle(GUI.skin.label) { fontSize = 23, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        ms.normal.textColor = new Color(textColor.r, textColor.g, textColor.b, alpha);
        GUI.Label(new Rect(0, sh - 175f, sw, 40), _message, ms);
    }

    void DrawBossHpBar(int sw)
    {
        if (_boss == null || _boss.state == EnemyAI.State.Dead) return;

        var player = Camera.main != null ? Camera.main.transform : null;
        if (player == null) return;
        float dist = Vector3.Distance(player.position, _boss.transform.position);
        if (dist > _boss.chaseRange + 5f) return;

        float barW = 360f;
        float barH = 22f;
        float x = sw * 0.5f - barW * 0.5f;
        float y = 18f;

        float pct = Mathf.Clamp01((float)_boss.CurrentHp / _boss.maxHp);
        bool raging = pct <= 0.3f;

        // Background
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(x - 2, y - 2, barW + 4, barH + 4), Texture2D.whiteTexture);

        // HP fill
        Color hpColor = raging ? new Color(1f, 0.15f, 0.05f) : new Color(0.85f, 0.12f, 0.08f);
        GUI.color = hpColor;
        GUI.DrawTexture(new Rect(x, y, barW * pct, barH), Texture2D.whiteTexture);

        // Border
        GUI.color = new Color(0.88f, 0.66f, 0.08f, 0.8f);
        GUI.DrawTexture(new Rect(x - 2, y - 2, barW + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x - 2, y + barH, barW + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x - 2, y, 2, barH), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + barW, y, 2, barH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Name
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        nameStyle.normal.textColor = raging
            ? new Color(1f, 0.3f, 0.1f)
            : new Color(1f, 0.88f, 0.16f);
        string label = raging ? "⚡ ВОЖДЬ ОРКОВ — ЯРОСТЬ! ⚡" : "ВОЖДЬ ОРКОВ";
        GUI.Label(new Rect(x, y - 22, barW, 20), label, nameStyle);
    }

    void DrawZoneTitle(int sw, int sh)
    {
        float alpha = Mathf.Clamp01(_zoneTitleTimer / 1f);

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 38, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(1f, 0.88f, 0.16f, alpha);

        float y = sh * 0.25f;
        GUI.Label(new Rect(0, y, sw, 50), _zoneTitle, title);

        if (!string.IsNullOrEmpty(_zoneSubtitle))
        {
            GUIStyle sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, alignment = TextAnchor.MiddleCenter
            };
            sub.normal.textColor = new Color(0.9f, 0.85f, 0.7f, alpha);
            GUI.Label(new Rect(0, y + 48, sw, 30), _zoneSubtitle, sub);
        }
    }

    void DrawWinScreen(int sw, int sh)
    {
        var run = RunScoreManager.Instance?.Data;
        int finalScore = run?.score ?? Score;
        int totalKills = run?.kills ?? _killedEnemies;
        int bestStreak = run?.bestStreak ?? 0;
        int headshots  = run?.headshotKills ?? 0;
        float longestK = run?.longestKillDistance ?? 0f;
        string time    = RunScoreManager.FormatTime(run?.runTime ?? 0f);
        int best       = RecordsManager.BestScore;
        bool newBest   = finalScore > best && finalScore > 0;

        GUI.DrawTexture(new Rect(0, 0, sw, sh), _dark);

        float w = 480f;
        float h = 460f;
        float x = sw * 0.5f - w * 0.5f;
        float y = sh * 0.5f - h * 0.5f;

        // Золотая рамка
        GUI.DrawTexture(new Rect(x - 4, y - 4, w + 8, h + 8), _gold);
        GUI.DrawTexture(new Rect(x, y, w, h), _panel);

        // Заголовок
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 46, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
        };
        title.normal.textColor = new Color(1f, 0.88f, 0.16f);
        GUI.Label(new Rect(x, y + 16, w, 55), "ПОБЕДА!", title);

        // Разделитель
        GUI.color = new Color(1f, 0.88f, 0.16f, 0.4f);
        GUI.DrawTexture(new Rect(x + 40, y + 76, w - 80, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Статистика
        GUIStyle stat = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft
        };
        stat.normal.textColor = new Color(1f, 0.92f, 0.55f);

        GUIStyle val = new GUIStyle(stat) { alignment = TextAnchor.MiddleRight };

        float lx = x + 50;
        float rw = w - 100;
        float sy = y + 88;
        float lh = 30f;

        DrawStatLine(lx, sy,          rw, lh, "Итоговый счёт", $"{finalScore}", stat, val);
        DrawStatLine(lx, sy + lh,     rw, lh, "Убийства",      $"{totalKills}", stat, val);
        DrawStatLine(lx, sy + lh * 2, rw, lh, "Лучшая серия",  $"{bestStreak}", stat, val);
        DrawStatLine(lx, sy + lh * 3, rw, lh, "Хедшоты",       $"{headshots}", stat, val);

        if (longestK > 0f)
            DrawStatLine(lx, sy + lh * 4, rw, lh, "Дальний убой", $"{longestK:F0}м", stat, val);

        DrawStatLine(lx, sy + lh * 5, rw, lh, "Время", time, stat, val);

        // Boss defeated
        GUIStyle bossStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
        };
        bossStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
        GUI.Label(new Rect(x, sy + lh * 6 + 8, w, 26), "✦ БОСС ПОВЕРЖЕН ✦", bossStyle);

        // Новый рекорд
        if (newBest)
        {
            GUIStyle rec = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold
            };
            rec.normal.textColor = new Color(1f, 0.95f, 0.2f);
            GUI.Label(new Rect(x, sy + lh * 7 + 12, w, 30), "★ НОВЫЙ РЕКОРД! ★", rec);
        }
        else
        {
            GUIStyle rec = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, alignment = TextAnchor.MiddleCenter
            };
            rec.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
            GUI.Label(new Rect(x, sy + lh * 7 + 12, w, 26), $"Лучший счёт: {best}", rec);
        }

        // Кнопки
        GUIStyle btn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        btn.normal.textColor = Color.white;
        btn.hover.textColor = Color.white;

        btn.normal.background = _green;
        btn.hover.background = _green;

        float btnY = y + h - 88;
        if (GUI.Button(new Rect(x + 50, btnY, (w - 120) * 0.5f, 50), "ЗАНОВО", btn))
        {
            _gameWon = false;
            Time.timeScale = 1f;
            SaveSystem.Delete();
            MouseLook.Unlock();
            RunScoreManager.Instance?.StartRun();
            SceneFader.LoadScene("Level0_Castl");
        }

        btn.normal.background = _dark;
        btn.hover.background = _gold;
        if (GUI.Button(new Rect(x + 50 + (w - 120) * 0.5f + 20, btnY, (w - 120) * 0.5f, 50), "МЕНЮ", btn))
        {
            _gameWon = false;
            Time.timeScale = 1f;
            MouseLook.Unlock();
            SceneFader.LoadScene("MainMenu");
        }
    }

    void DrawStatLine(float x, float y, float w, float h, string label, string value, GUIStyle lStyle, GUIStyle rStyle)
    {
        GUI.Label(new Rect(x, y, w * 0.6f, h), label, lStyle);
        GUI.Label(new Rect(x + w * 0.6f, y, w * 0.4f, h), value, rStyle);
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
