using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float iFrames = 1.2f;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public Vector3 respawnPoint;
    [HideInInspector] public bool isDead;

    public CameraShake shake;
    public bool suppressLegacyHud;

    bool _inv;
    bool _gameOver;
    float _flashAlpha;

    Texture2D _darkTex;
    Texture2D _redTex;
    Texture2D _btnTex;
    Texture2D _btnHoverTex;

    // ── Damage Direction Indicators ──
    struct DmgDir { public float angle; public float life; }
    readonly System.Collections.Generic.List<DmgDir> _dmgDirs = new System.Collections.Generic.List<DmgDir>();
    const float DMG_DIR_DURATION = 1.2f;

    void Awake()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position;
        _darkTex = MakeTex(2, 2, new Color(0.02f, 0.01f, 0.01f, 0.94f));
        _redTex = MakeTex(2, 2, new Color(0.60f, 0.03f, 0.02f, 1f));
        _btnTex = MakeTex(2, 2, new Color(0.72f, 0.08f, 0.06f, 1f));
        _btnHoverTex = MakeTex(2, 2, new Color(0.95f, 0.16f, 0.12f, 1f));
    }

    void Update()
    {
        if (_flashAlpha > 0f)
            _flashAlpha = Mathf.Max(0f, _flashAlpha - Time.unscaledDeltaTime * 2.8f);

        for (int i = _dmgDirs.Count - 1; i >= 0; i--)
        {
            var d = _dmgDirs[i];
            d.life -= Time.unscaledDeltaTime;
            if (d.life <= 0f) { _dmgDirs.RemoveAt(i); continue; }
            _dmgDirs[i] = d;
        }

        // v11: если меню смерти открыто, курсор не должен снова захватываться камерой.
        // Дополнительно даём запасной рестарт с клавиатуры, если мышь в Editor ведёт себя странно.
        if (_gameOver)
        {
            MouseLook.Unlock();
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                RestartLevel();
            if (Input.GetKeyDown(KeyCode.Escape))
                GoMenu();
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector3.zero);
    }

    public void TakeDamage(int amount, Vector3 sourceWorldPos)
    {
        if (_inv || isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        _flashAlpha = 0.42f;
        shake?.Shake();
        FeedbackManager.Instance?.PlayDamage();

        if (sourceWorldPos != Vector3.zero)
            AddDamageDirection(sourceWorldPos);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(IFramesCo());
    }

    void AddDamageDirection(Vector3 sourceWorldPos)
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 toSource = sourceWorldPos - cam.transform.position;
        toSource.y = 0f;
        if (toSource.sqrMagnitude < 0.01f) return;

        Vector3 camFwd = cam.transform.forward;
        camFwd.y = 0f;
        camFwd.Normalize();

        float angle = Mathf.Atan2(
            Vector3.Dot(Vector3.Cross(camFwd, toSource.normalized), Vector3.up),
            Vector3.Dot(camFwd, toSource.normalized)) * Mathf.Rad2Deg;

        _dmgDirs.Add(new DmgDir { angle = angle, life = DMG_DIR_DURATION });
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    /// Мгновенная гибель по сюжетному событию (напр. ворота пали, лаба №7(1)).
    /// Игнорирует i-frames/неуязвимость.
    public void KillFromEvent()
    {
        if (isDead) return;
        _inv = false;
        currentHealth = 0;
        Die();
    }

    void Die()
    {
        if (isDead) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameWon) return;
        isDead = true;
        _gameOver = true;
        currentHealth = 0;
        Time.timeScale = 0f;
        MouseLook.Unlock();
        RunScoreManager.Instance?.FinalizeDeath();
    }

    public void Respawn()
    {
        // В режиме выживания не возрождаем на чекпоинте — только полный рестарт уровня.
        RestartLevel();
    }

    public void RestartLevel()
    {
        _gameOver = false;
        isDead = false;
        Time.timeScale = 1f;
        HitStop.CancelAll();
        SaveSystem.Delete();

        RunScoreManager.Instance?.StartRun();

        MouseLook.Unlock();

        string scene = SceneManager.GetActiveScene().name;
        if (scene == "two")
            SceneBuilder.RestartCurrentRun();
        else
            SceneFader.LoadScene("Level0_Castl");
    }

    public void GoMenu()
    {
        _gameOver = false;
        isDead = false;
        Time.timeScale = 1f;
        HitStop.CancelAll();
        MouseLook.Unlock();
        SceneFader.LoadScene("MainMenu");
    }

    void OnGUI()
    {
        int sw = Screen.width;
        int sh = Screen.height;

        if (!_gameOver && !suppressLegacyHud)
        {
            DrawHp();
        }

        if (_flashAlpha > 0f)
        {
            GUI.color = new Color(1f, 0f, 0f, _flashAlpha);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        DrawDamageDirections(sw, sh);

        if (_gameOver && !(GameManager.Instance != null && GameManager.Instance.IsGameWon))
        {
            MouseLook.Unlock();
            DrawSimpleDeathMenu(sw, sh);
        }
    }

    void DrawDamageDirections(int sw, int sh)
    {
        if (_dmgDirs.Count == 0) return;

        float cx = sw * 0.5f;
        float cy = sh * 0.5f;
        float radius = Mathf.Min(sw, sh) * 0.18f;
        float arcW = 42f;
        float arcH = 12f;

        var matrix = GUI.matrix;
        foreach (var d in _dmgDirs)
        {
            float alpha = Mathf.Clamp01(d.life / DMG_DIR_DURATION);
            alpha *= alpha;
            GUI.color = new Color(1f, 0.08f, 0.03f, alpha * 0.85f);

            float rad = d.angle * Mathf.Deg2Rad;
            float px = cx + Mathf.Sin(rad) * radius - arcW * 0.5f;
            float py = cy - Mathf.Cos(rad) * radius - arcH * 0.5f;

            GUIUtility.RotateAroundPivot(d.angle, new Vector2(cx, cy));
            GUI.DrawTexture(new Rect(cx - arcW * 0.5f, cy - radius - arcH * 0.5f, arcW, arcH), Texture2D.whiteTexture);
            GUI.matrix = matrix;
        }
        GUI.color = Color.white;
    }

    void DrawHp()
    {
        GUI.color = new Color(0.10f, 0.01f, 0.01f, 0.78f);
        GUI.DrawTexture(new Rect(14, 14, 206, 24), Texture2D.whiteTexture);
        GUI.color = new Color(0.95f, 0.12f, 0.10f, 1f);
        float fill = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        GUI.DrawTexture(new Rect(17, 17, 200 * fill, 18), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
        s.normal.textColor = Color.white;
        GUI.Label(new Rect(16, 40, 170, 22), $"HP: {currentHealth}/{maxHealth}", s);
    }

    void DrawSimpleDeathMenu(int sw, int sh)
    {
        var run = RunScoreManager.Instance?.Data;
        int score = run?.score ?? 0;
        int kills = run?.kills ?? 0;
        int bestStreak = run?.bestStreak ?? 0;
        string time = RunScoreManager.FormatTime(run?.runTime ?? 0f);
        int best = RecordsManager.BestScore;
        bool newBest = score > best && score > 0;

        GUI.color = new Color(0f, 0f, 0f, 0.88f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelW = 460f;
        float panelH = 380f;
        float px = sw * 0.5f - panelW * 0.5f;
        float py = sh * 0.5f - panelH * 0.5f;

        // Рамка — кроваво-красная
        GUI.color = new Color(0.65f, 0.04f, 0.03f, 1f);
        GUI.DrawTexture(new Rect(px - 4, py - 4, panelW + 8, panelH + 8), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(px, py, panelW, panelH), _darkTex);

        // Заголовок
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 42, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(1f, 0.12f, 0.08f);
        GUI.Label(new Rect(px, py + 18, panelW, 52), "ВЫ ПОГИБЛИ", title);

        // Разделитель
        GUI.color = new Color(0.65f, 0.04f, 0.03f, 0.6f);
        GUI.DrawTexture(new Rect(px + 40, py + 74, panelW - 80, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Статистика
        GUIStyle stat = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        stat.normal.textColor = new Color(0.95f, 0.82f, 0.65f);

        float sy = py + 86;
        GUI.Label(new Rect(px, sy, panelW, 28), $"Очки: {score}", stat);
        GUI.Label(new Rect(px, sy + 30, panelW, 28), $"Убийства: {kills}", stat);
        GUI.Label(new Rect(px, sy + 60, panelW, 28), $"Лучшая серия: {bestStreak}", stat);
        GUI.Label(new Rect(px, sy + 90, panelW, 28), $"Время: {time}", stat);

        // Best score
        GUIStyle bestStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17, alignment = TextAnchor.MiddleCenter
        };
        if (newBest)
        {
            bestStyle.normal.textColor = new Color(1f, 0.9f, 0.1f);
            bestStyle.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(px, sy + 126, panelW, 26), "★ НОВЫЙ РЕКОРД! ★", bestStyle);
        }
        else
        {
            bestStyle.normal.textColor = new Color(0.6f, 0.55f, 0.45f);
            GUI.Label(new Rect(px, sy + 126, panelW, 26), $"Лучший счёт: {best}", bestStyle);
        }

        // Кнопки
        GUIStyle btn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        btn.normal.textColor = Color.white;
        btn.hover.textColor = Color.white;
        btn.active.textColor = Color.white;

        btn.normal.background = _btnTex;
        btn.hover.background = _btnHoverTex;
        btn.active.background = _redTex;

        float btnY = py + panelH - 120;
        if (GUI.Button(new Rect(px + 60, btnY, panelW - 120, 48), "НАЧАТЬ СНАЧАЛА", btn))
            RestartLevel();

        btn.normal.background = _darkTex;
        btn.hover.background = _btnTex;
        if (GUI.Button(new Rect(px + 60, btnY + 56, panelW - 120, 42), "ГЛАВНОЕ МЕНЮ", btn))
            GoMenu();
    }

    public void SetInvincible(float duration)
    {
        StartCoroutine(InvincibleCo(duration));
    }

    IEnumerator InvincibleCo(float duration)
    {
        _inv = true;
        yield return new WaitForSeconds(duration);
        _inv = false;
    }

    IEnumerator IFramesCo()
    {
        _inv = true;
        yield return new WaitForSeconds(iFrames);
        _inv = false;
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
