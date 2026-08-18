using UnityEngine;
using UnityEngine.SceneManagement;

/// Главное меню — без Canvas. Start / Records / Controls / Quit.
public class MainMenu : MonoBehaviour
{
    public string gameScene = "Level0_Castl";

    float _pulse;
    enum Page { Main, Records, Controls }
    Page _page = Page.Main;
    bool _confirmReset;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 1f;

        if (RunScoreManager.Instance != null)
            RunScoreManager.Instance.StopRun();

        if (SceneFader.Instance == null)
            new GameObject("SceneFader").AddComponent<SceneFader>();
        SceneFader.FadeIn();
    }

    void Update() => _pulse += Time.deltaTime * 2.2f;

    void OnGUI()
    {
        int sw = Screen.width, sh = Screen.height;
        DrawBackground(sw, sh);

        switch (_page)
        {
            case Page.Main:     DrawMain(sw, sh);     break;
            case Page.Records:  DrawRecords(sw, sh);  break;
            case Page.Controls: DrawControls(sw, sh); break;
        }

        GUIStyle ver = new GUIStyle(GUI.skin.label) { fontSize = 12 };
        ver.normal.textColor = new Color(0.35f, 0.3f, 0.4f);
        GUI.Label(new Rect(12, sh - 22, 400, 18), "ArenaEscape v5", ver);
    }

    void DrawBackground(int sw, int sh)
    {
        GUI.color = new Color(0.04f, 0.03f, 0.08f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = new Color(0.35f, 0.05f, 0.05f, 0.4f);
        GUI.DrawTexture(new Rect(0, sh * 0.6f, sw, sh * 0.4f), Texture2D.whiteTexture);
        GUI.color = new Color(0.5f, 0.35f, 0.02f, 0.25f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh * 0.35f), Texture2D.whiteTexture);
        GUI.color = new Color(0.85f, 0.7f, 0.1f, 0.7f);
        GUI.DrawTexture(new Rect(0, 8, sw, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0, sh - 11, sw, 3), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // ══════════════════════════════════════════════════════════════════════
    // ГЛАВНАЯ СТРАНИЦА
    // ══════════════════════════════════════════════════════════════════════

    void DrawMain(int sw, int sh)
    {
        float cy = sh * 0.5f;

        float pulse = 1f + Mathf.Sin(_pulse) * 0.025f;
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize  = (int)(64 * pulse),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        title.normal.textColor = new Color(0f, 0f, 0f, 0.7f);
        GUI.Label(new Rect(sw / 2f - 302, cy - 195, 604, 90), "ARENA ESCAPE", title);
        title.normal.textColor = new Color(1f, 0.88f, 0.12f);
        GUI.Label(new Rect(sw / 2f - 300, cy - 197, 600, 90), "ARENA ESCAPE", title);

        GUIStyle sub = new GUIStyle(GUI.skin.label)
            { fontSize = 17, alignment = TextAnchor.MiddleCenter };
        sub.normal.textColor = new Color(0.75f, 0.6f, 0.3f);
        GUI.Label(new Rect(sw / 2f - 260, cy - 108, 520, 28),
            "Сражайся  ·  Выживай  ·  Одолей всех врагов", sub);

        GUI.color = new Color(0.85f, 0.7f, 0.1f, 0.4f);
        GUI.DrawTexture(new Rect(sw / 2f - 160, cy - 75, 320, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float bx = sw / 2f - 130, bw = 260, bh = 52;
        float by = cy - 50;

        DrawButton(new Rect(bx, by, bw, bh), "НОВЫЙ ЗАБЕГ",
            new Color(0.15f, 0.65f, 0.25f), new Color(0.2f, 0.85f, 0.35f), () =>
        { SaveSystem.Delete(); SceneFader.LoadScene(gameScene); });

        DrawButton(new Rect(bx, by + 62, bw, bh), "РЕКОРДЫ",
            new Color(0.5f, 0.38f, 0.05f), new Color(0.75f, 0.58f, 0.1f), () =>
        { _page = Page.Records; _confirmReset = false; });

        DrawButton(new Rect(bx, by + 124, bw, bh), "УПРАВЛЕНИЕ",
            new Color(0.25f, 0.3f, 0.5f), new Color(0.35f, 0.45f, 0.7f), () =>
        { _page = Page.Controls; });

        DrawButton(new Rect(bx, by + 186, bw, bh), "ВЫЙТИ",
            new Color(0.45f, 0.06f, 0.06f), new Color(0.65f, 0.1f, 0.1f), () =>
        { Application.Quit(); });

        int best = RecordsManager.BestScore;
        if (best > 0)
        {
            GUIStyle bs = new GUIStyle(GUI.skin.label)
                { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            bs.normal.textColor = new Color(0.7f, 0.6f, 0.3f);
            GUI.Label(new Rect(bx, by + 250, bw, 24), $"Лучший счёт: {best}", bs);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // РЕКОРДЫ — таблица top-5 + общая статистика
    // ══════════════════════════════════════════════════════════════════════

    void DrawRecords(int sw, int sh)
    {
        float pw = 520f, ph = 580f;
        float px = sw * 0.5f - pw * 0.5f;
        float py = sh * 0.5f - ph * 0.5f;

        // Рамка
        GUI.color = new Color(0.85f, 0.7f, 0.1f, 0.8f);
        GUI.DrawTexture(new Rect(px - 3, py - 3, pw + 6, ph + 6), Texture2D.whiteTexture);
        GUI.color = new Color(0.06f, 0.04f, 0.08f, 0.97f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Заголовок
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(1f, 0.88f, 0.16f);
        GUI.Label(new Rect(px, py + 14, pw, 40), "РЕКОРДЫ", title);

        // ── Top-5 таблица ──
        float tx = px + 20;
        float tw = pw - 40;
        float ty = py + 60;

        // Заголовки колонок
        GUIStyle header = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        header.normal.textColor = new Color(0.6f, 0.55f, 0.45f);

        GUI.Label(new Rect(tx, ty, 30, 22), "#", header);
        header.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(tx + 35, ty, 100, 22), "ОЧКИ", header);
        GUI.Label(new Rect(tx + 140, ty, 80, 22), "УБИЙСТВА", header);
        GUI.Label(new Rect(tx + 230, ty, 70, 22), "СЕРИЯ", header);
        GUI.Label(new Rect(tx + 310, ty, 70, 22), "ВРЕМЯ", header);
        GUI.Label(new Rect(tx + 390, ty, 80, 22), "БОСС", header);

        // Разделитель
        GUI.color = new Color(0.85f, 0.7f, 0.1f, 0.3f);
        GUI.DrawTexture(new Rect(tx, ty + 24, tw, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var runs = RecordsManager.GetTop5();
        float rowH = 28f;

        GUIStyle rowNum = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        GUIStyle rowText = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft
        };

        for (int i = 0; i < 5; i++)
        {
            float ry = ty + 28 + i * rowH;

            if (i < runs.Count)
            {
                var r = runs[i];
                bool isFirst = (i == 0);
                Color rc = isFirst
                    ? new Color(1f, 0.88f, 0.2f)
                    : new Color(0.85f, 0.8f, 0.65f);

                rowNum.normal.textColor = isFirst ? new Color(1f, 0.85f, 0.1f) : new Color(0.5f, 0.45f, 0.35f);
                string medal = i == 0 ? "★" : i == 1 ? "2" : i == 2 ? "3" : $"{i + 1}";
                GUI.Label(new Rect(tx, ry, 30, rowH), medal, rowNum);

                rowText.normal.textColor = rc;
                GUI.Label(new Rect(tx + 35, ry, 100, rowH), $"{r.score}", rowText);
                GUI.Label(new Rect(tx + 140, ry, 80, rowH), $"{r.kills}", rowText);
                GUI.Label(new Rect(tx + 230, ry, 70, rowH), $"{r.streak}", rowText);
                GUI.Label(new Rect(tx + 310, ry, 70, rowH), RunScoreManager.FormatTime(r.time), rowText);

                rowText.normal.textColor = r.boss ? new Color(0.3f, 1f, 0.4f) : new Color(0.5f, 0.35f, 0.3f);
                GUI.Label(new Rect(tx + 390, ry, 80, rowH), r.boss ? "Победа" : "Смерть", rowText);
            }
            else
            {
                rowNum.normal.textColor = new Color(0.3f, 0.28f, 0.25f);
                GUI.Label(new Rect(tx, ry, 30, rowH), $"{i + 1}", rowNum);
                rowText.normal.textColor = new Color(0.3f, 0.28f, 0.25f);
                GUI.Label(new Rect(tx + 35, ry, 200, rowH), "—", rowText);
            }
        }

        // ── Разделитель ──
        float statY = ty + 28 + 5 * rowH + 8;
        GUI.color = new Color(0.85f, 0.7f, 0.1f, 0.25f);
        GUI.DrawTexture(new Rect(tx, statY, tw, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Общая статистика ──
        GUIStyle statL = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15, alignment = TextAnchor.MiddleLeft
        };
        statL.normal.textColor = new Color(0.55f, 0.50f, 0.40f);
        GUIStyle statR = new GUIStyle(statL) { alignment = TextAnchor.MiddleRight };

        float sy = statY + 6;
        float sH = 24f;
        DrawLine(tx + 10, sy, tw - 20, sH, "Всего забегов", $"{RecordsManager.TotalRuns}", statL, statR);
        DrawLine(tx + 10, sy + sH, tw - 20, sH, "Всего убийств", $"{RecordsManager.TotalKills}", statL, statR);
        DrawLine(tx + 10, sy + sH * 2, tw - 20, sH, "Побед над боссом", $"{RecordsManager.BossDefeats}", statL, statR);

        float bt = RecordsManager.BestTime;
        DrawLine(tx + 10, sy + sH * 3, tw - 20, sH, "Лучшее время", bt > 0f ? RunScoreManager.FormatTime(bt) : "—", statL, statR);

        int bs = RecordsManager.BestStreak;
        DrawLine(tx + 10, sy + sH * 4, tw - 20, sH, "Лучшая серия", bs > 0 ? $"{bs}" : "—", statL, statR);

        float bd = RecordsManager.BestDistance;
        DrawLine(tx + 10, sy + sH * 5, tw - 20, sH, "Дальний убой", bd > 0f ? $"{bd:F0}м" : "—", statL, statR);

        // ── Кнопки ──
        float btnY = py + ph - 62;
        float btnW = 140f;

        DrawButton(new Rect(px + 30, btnY, btnW, 44), "НАЗАД",
            new Color(0.35f, 0.3f, 0.25f), new Color(0.55f, 0.45f, 0.3f), () =>
        { _page = Page.Main; _confirmReset = false; });

        if (!_confirmReset)
        {
            DrawButton(new Rect(px + pw - btnW - 30, btnY, btnW, 44), "СБРОС",
                new Color(0.5f, 0.08f, 0.08f), new Color(0.7f, 0.12f, 0.12f), () =>
            { _confirmReset = true; });
        }
        else
        {
            DrawButton(new Rect(px + pw - btnW - 30, btnY, btnW, 44), "ТОЧНО?",
                new Color(0.8f, 0.1f, 0.1f), new Color(1f, 0.15f, 0.15f), () =>
            {
                RecordsManager.ClearAll();
                _confirmReset = false;
            });
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // УПРАВЛЕНИЕ
    // ══════════════════════════════════════════════════════════════════════

    void DrawControls(int sw, int sh)
    {
        float pw = 440f, ph = 470f;
        float px = sw * 0.5f - pw * 0.5f;
        float py = sh * 0.5f - ph * 0.5f;

        GUI.color = new Color(0.3f, 0.4f, 0.7f, 0.8f);
        GUI.DrawTexture(new Rect(px - 3, py - 3, pw + 6, ph + 6), Texture2D.whiteTexture);
        GUI.color = new Color(0.06f, 0.04f, 0.08f, 0.97f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(0.6f, 0.75f, 1f);
        GUI.Label(new Rect(px, py + 14, pw, 40), "УПРАВЛЕНИЕ", title);

        GUIStyle line = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft
        };
        line.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
        GUIStyle keyStyle = new GUIStyle(line) { alignment = TextAnchor.MiddleRight };
        keyStyle.normal.textColor = new Color(0.6f, 0.75f, 1f);

        float lx = px + 40, rw = pw - 80, ly = py + 68, lh = 28f;

        DrawLine(lx, ly,          rw, lh, "Движение",          "WASD",      line, keyStyle);
        DrawLine(lx, ly + lh,     rw, lh, "Камера",            "Мышь",      line, keyStyle);
        DrawLine(lx, ly + lh * 2, rw, lh, "Атака",             "ЛКМ",       line, keyStyle);
        DrawLine(lx, ly + lh * 3, rw, lh, "Прицел (лук)",      "ПКМ",       line, keyStyle);
        DrawLine(lx, ly + lh * 4, rw, lh, "Меч / Копьё / Лук", "1 / 2 / 3", line, keyStyle);
        DrawLine(lx, ly + lh * 5, rw, lh, "Смена оружия",      "Tab",       line, keyStyle);
        DrawLine(lx, ly + lh * 6, rw, lh, "Взаимодействие",    "E / F",     line, keyStyle);
        DrawLine(lx, ly + lh * 7, rw, lh, "Прыжок",            "Пробел",    line, keyStyle);
        DrawLine(lx, ly + lh * 8, rw, lh, "Уклон",             "Пробел+WASD", line, keyStyle);
        DrawLine(lx, ly + lh * 9, rw, lh, "Бег",               "Shift",     line, keyStyle);
        DrawLine(lx, ly + lh * 10, rw, lh, "Огненный шар",     "Q",         line, keyStyle);
        DrawLine(lx, ly + lh * 11, rw, lh, "Пауза",            "Esc",       line, keyStyle);

        DrawButton(new Rect(px + 100, py + ph - 62, pw - 200, 46), "НАЗАД",
            new Color(0.35f, 0.3f, 0.25f), new Color(0.55f, 0.45f, 0.3f), () =>
        { _page = Page.Main; });
    }

    // ══════════════════════════════════════════════════════════════════════
    // ХЕЛПЕРЫ
    // ══════════════════════════════════════════════════════════════════════

    void DrawLine(float x, float y, float w, float h, string left, string right, GUIStyle ls, GUIStyle rs)
    {
        GUI.Label(new Rect(x, y, w * 0.6f, h), left, ls);
        GUI.Label(new Rect(x + w * 0.6f, y, w * 0.4f, h), right, rs);
    }

    void DrawButton(Rect r, string label, Color normalC, Color hoverC, System.Action onClick)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        Color c = hover ? hoverC : normalC;

        GUI.color = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 0.9f);
        GUI.DrawTexture(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), Texture2D.whiteTexture);
        GUI.color = new Color(c.r, c.g, c.b, hover ? 0.95f : 0.8f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle s = new GUIStyle(GUI.skin.label)
            { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        s.normal.textColor = hover ? Color.white : new Color(0.95f, 0.95f, 0.9f);
        GUI.Label(r, label, s);

        GUI.color = Color.clear;
        if (GUI.Button(r, GUIContent.none, GUIStyle.none)) onClick?.Invoke();
        GUI.color = Color.white;
    }
}
