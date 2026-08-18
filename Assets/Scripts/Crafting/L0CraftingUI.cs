using System.Collections;
using UnityEngine;

/// <summary>
/// Окно крафта у кузнеца (1 рецепт: руда → огненные стрелы).
/// Открывается по E вблизи кузнеца, только после завершения квеста Брока.
/// </summary>
public class L0CraftingUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    static readonly Vector3 SmithPos   = new Vector3(5.75f, 0.78f, -24.5f);
    const  float            OPEN_RANGE = 4.0f;

    Transform _player;
    bool      _forging;

    Texture2D _bgTex;
    Texture2D _goldTex;
    Texture2D _btnTex;
    Texture2D _btnHover;
    Texture2D _disabledTex;

    // ── Update ───────────────────────────────────────────────────────────────
    void Update()
    {
        if (IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F))
                Close();
            return;
        }

        if (!Input.GetKeyDown(KeyCode.F)) return;

        // Окно крафта доступно после разговора с кузнецом (тот же триггер, что выдаёт квест).
        var gp = GameProgressManager.Instance;
        if (gp == null || !gp.talkedToBlacksmith) return;

        if (_player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) _player = pc.transform;
            return;
        }

        if (Vector3.Distance(_player.position, SmithPos) <= OPEN_RANGE)
            Open();
    }

    void Open()  { IsOpen = true;  MouseLook.UiOpen = true;  MouseLook.Unlock(); }
    void Close() { IsOpen = false; MouseLook.UiOpen = false; MouseLook.Lock(); }

    // Страховка: если компонент выключат/уничтожат с открытым окном — не оставляем
    // камеру с разблокированной мышью навсегда.
    void OnDisable()
    {
        if (IsOpen)
        {
            IsOpen = false;
            MouseLook.UiOpen = false;
        }
    }

    // ── OnGUI ────────────────────────────────────────────────────────────────
    void OnGUI()
    {
        if (!IsOpen) return;
        EnsureTextures();

        int sw = Screen.width;
        int sh = Screen.height;

        float w = 480f, h = 460f;
        float x = sw * 0.5f - w * 0.5f;
        float y = sh * 0.5f - h * 0.5f;

        // Золотая рамка
        GUI.color = new Color(0.88f, 0.66f, 0.08f);
        GUI.DrawTexture(new Rect(x - 4, y - 4, w + 8, h + 8), Texture2D.whiteTexture);

        // Фон
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(x, y, w, h), _bgTex);

        // Шапка
        GUI.DrawTexture(new Rect(x, y, w, 54f), _goldTex);
        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = new Color(0.08f, 0.04f, 0.01f);
        GUI.Label(new Rect(x, y + 2, w, 50), "⚒  ГОРН БРОКА", titleStyle);

        float cy = y + 64f;

        Divider(x, cy, w); cy += 12f;

        // Рецепт — заголовок
        var rtStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        rtStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);
        GUI.Label(new Rect(x, cy, w, 30), "ОГНЕННЫЕ СТРЕЛЫ", rtStyle);
        cy += 34f;

        // Иконка огня
        var iconStyle = new GUIStyle(GUI.skin.label) { fontSize = 56, alignment = TextAnchor.MiddleCenter };
        iconStyle.normal.textColor = new Color(1f, 0.42f, 0.04f);
        GUI.Label(new Rect(x, cy, w, 70), "🔥", iconStyle);
        cy += 74f;

        // Описание
        var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        descStyle.normal.textColor = new Color(0.88f, 0.80f, 0.62f);
        GUI.Label(new Rect(x + 50, cy, w - 100, 44),
            "Поджигают врагов в области при попадании (r=4.5м, +30 доп. урона)", descStyle);
        cy += 50f;

        Divider(x, cy, w); cy += 10f;

        // Ингредиенты
        var inv      = L0Inventory.Instance;
        int oreCount = inv != null ? inv.GetCount("Руда") : 0;
        bool canCraft = L0CraftingStation.Instance != null
                     && L0CraftingStation.Instance.CanCraft(0)
                     && !_forging;

        var ingHeader = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        ingHeader.normal.textColor = new Color(0.75f, 0.70f, 0.52f);
        GUI.Label(new Rect(x, cy, w, 26), "Необходимо:", ingHeader);
        cy += 28f;

        Color oreCol = oreCount >= 3 ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.32f, 0.28f);
        var oreStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        oreStyle.normal.textColor = oreCol;
        GUI.Label(new Rect(x, cy, w, 34), $"⛏  Огненная руда:  {oreCount} / 3", oreStyle);
        cy += 38f;

        var resStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
        resStyle.normal.textColor = new Color(0.55f, 0.92f, 0.65f);
        GUI.Label(new Rect(x, cy, w, 24), "→  5 Огненных стрел", resStyle);
        cy += 28f;

        if (FireArrowMode.Active)
        {
            var arrowInfo = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            arrowInfo.normal.textColor = new Color(1f, 0.55f, 0.08f);
            GUI.Label(new Rect(x, cy, w, 22), $"🔥 Активно стрел: {FireArrowMode.ArrowsLeft}", arrowInfo);
            cy += 24f;
        }

        Divider(x, cy, w); cy += 10f;

        // Кнопка
        float btnW = 230f, btnH = 52f;
        float btnX = x + w * 0.5f - btnW * 0.5f;

        if (_forging)
        {
            var forgStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            forgStyle.normal.textColor = new Color(1f, 0.65f, 0.04f);
            GUI.Label(new Rect(btnX, cy, btnW, btnH), "⚒  Ковка...", forgStyle);
        }
        else
        {
            GUIStyle bStyle = new GUIStyle(GUI.skin.button) { fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            if (canCraft)
            {
                bStyle.normal.textColor  = new Color(0.06f, 0.03f, 0.00f);
                bStyle.hover.textColor   = Color.black;
                bStyle.normal.background = _btnTex;
                bStyle.hover.background  = _btnHover;
            }
            else
            {
                bStyle.normal.textColor  = new Color(0.45f, 0.45f, 0.45f);
                bStyle.normal.background = _disabledTex;
                bStyle.hover.background  = _disabledTex;
            }
            GUI.enabled = canCraft;
            if (GUI.Button(new Rect(btnX, cy, btnW, btnH), "⚒  СОЗДАТЬ", bStyle))
                StartCoroutine(DoForge());
            GUI.enabled = true;
        }
        cy += btnH + 14f;

        // Закрыть
        var closeHint = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
        closeHint.normal.textColor = new Color(0.50f, 0.46f, 0.36f);
        GUI.Label(new Rect(x, cy, w, 22), "[F] или [Esc] — закрыть", closeHint);
    }

    void Divider(float x, float y, float w)
    {
        GUI.color = new Color(0.88f, 0.66f, 0.08f, 0.45f);
        GUI.DrawTexture(new Rect(x + 28, y, w - 56, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    IEnumerator DoForge()
    {
        _forging = true;
        var fb = FeedbackManager.Instance;
        float t = 0f;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            if (fb != null && t % 0.30f < Time.deltaTime * 1.8f)
            {
                fb.SpawnBurst(SmithPos + Vector3.up * 0.9f,
                    new Color(1f, 0.58f, 0.04f), new Color(1f, 0.12f, 0f),
                    7, 2.8f, 0.05f);
            }
            yield return null;
        }
        L0CraftingStation.Instance?.Craft(0);
        _forging = false;
        Close();
    }

    void EnsureTextures()
    {
        if (_bgTex != null) return;
        _bgTex       = MakeTex(new Color(0.07f, 0.05f, 0.02f, 0.97f));
        _goldTex     = MakeTex(new Color(0.68f, 0.48f, 0.06f, 1.00f));
        _btnTex      = MakeTex(new Color(0.88f, 0.66f, 0.08f, 1.00f));
        _btnHover    = MakeTex(new Color(1.00f, 0.82f, 0.18f, 1.00f));
        _disabledTex = MakeTex(new Color(0.22f, 0.20f, 0.16f, 1.00f));
    }

    static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(2, 2);
        t.SetPixels(new[] { c, c, c, c });
        t.Apply();
        return t;
    }
}
