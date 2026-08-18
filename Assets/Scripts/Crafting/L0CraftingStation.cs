using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Станция крафта у кузнеца Брока.
/// Единственный рецепт: 3 руды → 5 огненных стрел.
/// Руда в мире — красивые кристальные кластеры с оранжевым свечением.
/// </summary>
public class L0CraftingStation : MonoBehaviour
{
    public static L0CraftingStation Instance { get; private set; }

    // ── Рецепт ──────────────────────────────────────────────────────────────
    public struct Ingredient { public string name; public int amount; }

    public class CraftRecipe
    {
        public string       displayName;
        public Ingredient[] ingredients;
        public string       effectDescription;
        public System.Action onCraft;
    }

    public List<CraftRecipe> Recipes { get; private set; }

    static readonly Vector3 SmithPos = new Vector3(5.75f, 0.78f, -24.5f);

    // ── Ноды ресурсов ───────────────────────────────────────────────────────
    class OreNode
    {
        public GameObject go;
        public Light      light;
        public Transform  crystalRoot;
    }

    readonly List<OreNode> _nodes = new List<OreNode>();
    Transform _player;
    float     _pulseClock;

    // Квест руды: спавн после разговора с кузнецом, HUD-счётчик до первой ковки.
    bool _oresSpawned;
    bool _introShown;
    bool _everCrafted;
    const int ORE_NEEDED = 3;

    Texture2D _hudBg;
    GUIStyle  _hudStyle;

    // ── Singleton / Ensure ───────────────────────────────────────────────────
    public static L0CraftingStation Ensure()
    {
        if (Instance != null) return Instance;
        return new GameObject("L0CraftingStation").AddComponent<L0CraftingStation>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        L0Inventory.Ensure();
        BuildRecipe();
        // Руда спавнится не сразу, а после разговора с кузнецом (см. Update → TrySpawnOres).

        if (GetComponent<L0CraftingUI>() == null)
            gameObject.AddComponent<L0CraftingUI>();
    }

    // ── Рецепт ──────────────────────────────────────────────────────────────
    void BuildRecipe()
    {
        Recipes = new List<CraftRecipe>
        {
            new CraftRecipe
            {
                displayName       = "Огненные стрелы",
                ingredients       = new[] { new Ingredient { name = "Руда", amount = 3 } },
                effectDescription = "5 огненных стрел — поджигают врагов в области!",
                onCraft           = ApplyFireArrows
            }
        };
    }

    void ApplyFireArrows()
    {
        _everCrafted = true;
        FireArrowMode.Activate(5);
        GameManager.Instance?.ShowMessage(
            "⚡ Огненные стрелы готовы! (5 выстрелов)", GameManager.MsgType.Heal, 3.5f);
        FeedbackManager.Instance?.SpawnBurst(SmithPos,
            new Color(1f, 0.40f, 0.05f), new Color(1f, 0.15f, 0.0f), 28, 5f, 0.06f);
        FeedbackManager.Instance?.SpawnRing(SmithPos,
            new Color(1f, 0.50f, 0.10f), 22, 4f);
    }

    // ── CanCraft / Craft ─────────────────────────────────────────────────────
    public bool CanCraft(int idx)
    {
        var inv = L0Inventory.Instance;
        if (inv == null || idx < 0 || idx >= Recipes.Count) return false;
        foreach (var ing in Recipes[idx].ingredients)
            if (!inv.HasItem(ing.name, ing.amount)) return false;
        return true;
    }

    public void Craft(int idx)
    {
        if (!CanCraft(idx)) return;
        var inv = L0Inventory.Instance;
        foreach (var ing in Recipes[idx].ingredients)
            inv.RemoveItem(ing.name, ing.amount);
        Recipes[idx].onCraft?.Invoke();
    }

    // ── Гейтинг квеста: спавн после разговора с кузнецом ─────────────────────
    void TrySpawnOres()
    {
        if (_oresSpawned) return;

        var gp = GameProgressManager.Instance;
        if (gp == null || !gp.talkedToBlacksmith) return;

        _oresSpawned = true;
        SpawnOreNodes();

        if (!_introShown)
        {
            _introShown = true;
            CastlePrologueBuilder.Instance?.ShowDialogue(
                "Кузнец Брок",
                "Найди на дороге к логову 3 куска огненной руды и принеси мне — выкую огненные стрелы!",
                6f);
        }
    }

    // ── HUD: счётчик руды (до ковки) + остаток огненных стрел ─────────────────
    void OnGUI()
    {
        if (L0CraftingUI.IsOpen) return;

        if (_hudBg == null)
        {
            _hudBg = MakeSolidTex(new Color(0f, 0f, 0f, 0.55f));
            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft
            };
            _hudStyle.normal.textColor = new Color(1f, 0.72f, 0.20f);
        }

        // Счётчик собранной руды — до первой ковки.
        if (_oresSpawned && !_everCrafted)
        {
            var inv = L0Inventory.Instance;
            int have = inv != null ? inv.GetCount("Руда") : 0;
            string label = have >= ORE_NEEDED
                ? "Огненная руда: 3/3 — вернись к кузнецу [F]"
                : $"Огненная руда: {have}/{ORE_NEEDED}";

            const float x = 20f, y = 180f, w = 340f;
            GUI.DrawTexture(new Rect(x - 4, y - 2, w, 28), _hudBg);
            GUI.Label(new Rect(x, y, w, 24), "⛏ " + label, _hudStyle);
        }

        // Остаток огненных стрел — пока режим активен.
        if (FireArrowMode.Active && FireArrowMode.ArrowsLeft > 0)
        {
            float w = 240f, x = Screen.width - w - 20f, y = Screen.height - 230f;
            GUI.DrawTexture(new Rect(x - 4, y - 2, w, 28), _hudBg);
            GUI.Label(new Rect(x, y, w, 24),
                $"🔥 Огненные стрелы: {FireArrowMode.ArrowsLeft}", _hudStyle);
        }
    }

    static Texture2D MakeSolidTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    // ── Кристальная руда в мире ─────────────────────────────────────────────
    void SpawnOreNodes()
    {
        var root = new GameObject("CRAFTING_ORES");

        // 3 куска на дороге к оркам (Z=-50/-70/-90) — ровно столько, сколько нужно на рецепт.
        // Координаты совпадают с дизайн-доком (квест кузнеца).
        SpawnCrystal(root, new Vector3(4f, 0.35f, -50f));
        SpawnCrystal(root, new Vector3(2f, 0.35f, -70f));
        SpawnCrystal(root, new Vector3(6f, 0.35f, -90f));
    }

    void SpawnCrystal(GameObject parent, Vector3 pos)
    {
        var root = new GameObject("OreNode");
        root.transform.SetParent(parent.transform);
        root.transform.position = pos;

        // ── Основание — тёмный шершавый камень ──────────────────────────────
        var baseRock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseRock.name = "Base";
        baseRock.transform.SetParent(root.transform, false);
        baseRock.transform.localPosition = new Vector3(0f, -0.06f, 0f);
        baseRock.transform.localScale = new Vector3(0.45f, 0.16f, 0.42f);
        baseRock.transform.localRotation = Quaternion.Euler(0f, 28f, 0f);
        SetColor(baseRock, new Color(0.15f, 0.11f, 0.08f));
        DestroyImmediate(baseRock.GetComponent<Collider>());

        // ── Кристальные шарды (5 штук под разными углами) ───────────────────
        var crystalRoot = new GameObject("Crystals");
        crystalRoot.transform.SetParent(root.transform, false);

        SpawnShard(crystalRoot.transform, new Vector3( 0.00f, 0.22f,  0.00f), new Vector3(0.10f, 0.38f, 0.10f), Quaternion.Euler( 0f,   0f,  0f), 1.00f);
        SpawnShard(crystalRoot.transform, new Vector3( 0.10f, 0.18f, -0.06f), new Vector3(0.08f, 0.30f, 0.08f), Quaternion.Euler( 0f,  40f, 14f), 0.90f);
        SpawnShard(crystalRoot.transform, new Vector3(-0.10f, 0.16f,  0.06f), new Vector3(0.07f, 0.28f, 0.07f), Quaternion.Euler( 0f, -30f,-18f), 0.80f);
        SpawnShard(crystalRoot.transform, new Vector3( 0.06f, 0.14f,  0.10f), new Vector3(0.07f, 0.22f, 0.07f), Quaternion.Euler( 0f,  70f, 22f), 0.70f);
        SpawnShard(crystalRoot.transform, new Vector3(-0.08f, 0.12f, -0.08f), new Vector3(0.06f, 0.20f, 0.06f), Quaternion.Euler( 0f, -60f,-10f), 0.60f);

        // ── PointLight (оранжевый огонь) ─────────────────────────────────────
        var lgGo = new GameObject("Glow");
        lgGo.transform.SetParent(root.transform, false);
        lgGo.transform.localPosition = new Vector3(0, 0.55f, 0);
        var lt = lgGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.45f, 0.05f);
        lt.range = 5.5f;
        lt.intensity = 1.8f;

        // ── Тонкий столб-маркер ───────────────────────────────────────────────
        var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = "Beacon";
        beacon.transform.SetParent(root.transform, false);
        beacon.transform.localPosition = new Vector3(0, 2.2f, 0);
        beacon.transform.localScale = new Vector3(0.05f, 2.2f, 0.05f);
        SetColorTransparent(beacon, new Color(1f, 0.50f, 0.08f, 0.55f));
        DestroyImmediate(beacon.GetComponent<Collider>());

        // ── Лейбл ────────────────────────────────────────────────────────────
        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(root.transform, false);
        lblGo.transform.localPosition = new Vector3(0, 1.05f, 0);
        lblGo.AddComponent<SimpleBillboardLabel>();
        var tm = lblGo.AddComponent<TextMesh>();
        tm.text          = "Огненная руда [F]";
        tm.fontSize      = 22;
        tm.characterSize = 0.08f;
        tm.alignment     = TextAlignment.Center;
        tm.anchor        = TextAnchor.MiddleCenter;
        tm.color         = new Color(1f, 0.72f, 0.18f);

        _nodes.Add(new OreNode { go = root, light = lt,
                                 crystalRoot = crystalRoot.transform });
    }

    void SpawnShard(Transform parent, Vector3 localPos, Vector3 localScale,
                    Quaternion localRot, float brightness)
    {
        var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shard.name = "Shard";
        shard.transform.SetParent(parent, false);
        shard.transform.localPosition = localPos;
        shard.transform.localScale    = localScale;
        shard.transform.localRotation = localRot;
        // Заострённая форма — squish по X/Z
        shard.transform.localScale = new Vector3(
            localScale.x * 0.55f, localScale.y, localScale.z * 0.55f);

        float b = brightness;
        SetColor(shard, new Color(1f * b, 0.38f * b, 0.04f * b));
        DestroyImmediate(shard.GetComponent<Collider>());
    }

    // ── Update: пульс + подбор ───────────────────────────────────────────────
    void Update()
    {
        _pulseClock += Time.deltaTime;

        TrySpawnOres();

        // Пульсация кристаллов
        float pulse = 0.80f + Mathf.Sin(_pulseClock * 2.6f) * 0.40f;
        float bob   = Mathf.Sin(_pulseClock * 1.8f) * 0.04f;

        for (int i = _nodes.Count - 1; i >= 0; i--)
        {
            if (_nodes[i].go == null) { _nodes.RemoveAt(i); continue; }
            if (_nodes[i].light != null)
                _nodes[i].light.intensity = 1.4f + pulse * 0.8f;
            if (_nodes[i].crystalRoot != null)
                _nodes[i].crystalRoot.localPosition = new Vector3(0, bob, 0);
        }

        // Подбор по F (только когда окно крафта закрыто)
        if (L0CraftingUI.IsOpen) return;
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (_player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) _player = pc.transform;
            return;
        }

        for (int i = _nodes.Count - 1; i >= 0; i--)
        {
            if (_nodes[i].go == null) { _nodes.RemoveAt(i); continue; }
            if (Vector3.Distance(_player.position, _nodes[i].go.transform.position) < 2.8f)
            {
                PickupOre(i);
                break;
            }
        }
    }

    void PickupOre(int i)
    {
        Vector3 pos = _nodes[i].go.transform.position;
        L0Inventory.Instance?.AddItem("Руда", 1);

        FeedbackManager.Instance?.FloatText(pos, "+Руда", new Color(1f, 0.60f, 0.12f), 1.5f);
        FeedbackManager.Instance?.SpawnBurst(pos,
            new Color(1f, 0.45f, 0.05f), new Color(0.9f, 0.20f, 0.02f), 18, 3.5f, 0.05f);
        FeedbackManager.Instance?.PlayCoin();

        GameManager.Instance?.ShowMessage("Огненная руда подобрана!",
            GameManager.MsgType.Coin, 1.8f);

        Destroy(_nodes[i].go);
        _nodes.RemoveAt(i);
    }

    // ── Утилиты ──────────────────────────────────────────────────────────────
    static Shader _shader;
    static Shader LitShader() =>
        _shader ??= Shader.Find("Universal Render Pipeline/Lit")
                 ?? Shader.Find("Standard");

    static void SetColor(GameObject obj, Color col)
    {
        var r = obj.GetComponent<Renderer>();
        if (r) r.material = new Material(LitShader()) { color = col };
    }

    static void SetColorTransparent(GameObject obj, Color col)
    {
        var r = obj.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Standard") ?? LitShader());
        mat.color = col;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        r.material = mat;
    }
}
