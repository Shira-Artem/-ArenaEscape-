using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// SceneBuilder v26 — Four Zones Final Polish.
/// Рабочая версия карты на 4 зоны:
/// 1) холмы, 2) крепость, 3) храм, 4) объединённая кузня + тронный собор + финальный босс.
/// Исправлены недостающие методы Zone4ForgeTower/Zone4BridgeSupports и добавлена безопасная полировка переходов.
public class SceneBuilder : MonoBehaviour
{
    public static SceneBuilder Instance { get; private set; }
    bool _restartInProgress;

    Material _grass, _dirt, _fort, _temple, _arena, _lava, _gold, _wood, _dark, _blood, _water;
    Transform _worldRoot;
    Transform _walkRoot;
    Transform _decorRoot;
    Transform _itemsRoot;
    Transform _enemiesRoot;
    Transform _waypointsRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        StartCoroutine(BuildRoutine());
    }

    public static void RestartCurrentRun()
    {
        Time.timeScale = 1f;
        SaveSystem.Delete();
        MouseLook.Unlock();

        SceneBuilder builder = Instance;
        if (builder == null)
        {
            builder = Object.FindFirstObjectByType<SceneBuilder>();
            if (builder == null)
                builder = new GameObject("SceneBuilder_Runtime_Restarter").AddComponent<SceneBuilder>();
            Instance = builder;
        }

        builder.RestartRun();
    }

    public void RestartRun()
    {
        if (_restartInProgress) return;
        StopAllCoroutines();
        StartCoroutine(RestartRunRoutine());
    }

    IEnumerator RestartRunRoutine()
    {
        _restartInProgress = true;
        Time.timeScale = 1f;
        SaveSystem.Delete();
        MouseLook.Unlock();

        CleanupGeneratedRunObjects();

        var gm = GameManager.Instance;
        if (gm != null) gm.ResetForNewRun();

        // Даём Unity один кадр физически удалить старого игрока/камеру/мир.
        // После этого строим карту заново без LoadScene — так не пропадает камера.
        yield return null;

        yield return BuildRoutine();
        _restartInProgress = false;
    }

    void CleanupGeneratedRunObjects()
    {
        if (_worldRoot != null) Destroy(_worldRoot.gameObject);
        _worldRoot = _walkRoot = _decorRoot = _itemsRoot = _enemiesRoot = _waypointsRoot = null;

        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (t == null || t.parent != null) continue;
            if (t.name.StartsWith("Generated_World") || t.name == "Runtime_NavMeshSurface")
                Destroy(t.gameObject);
        }

        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
            if (p != null) Destroy(p);

        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
            if (e != null) Destroy(e);

        // На всякий случай удаляем старые камеры, если они отцепились от игрока.
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (cam != null && cam.transform.parent == null) Destroy(cam.gameObject);
    }

    IEnumerator BuildRoutine()
    {
        CleanupDefaultSceneObjects();
        // v9: игра теперь про один честный забег. Старые сохранения/чекпоинты сбрасываем сразу.
        SaveSystem.Delete();
        SetupMaterials();

        _worldRoot = new GameObject("Generated_World_v24_MergedForgeCathedral").transform;
        _walkRoot = NewChild(_worldRoot, "01_Walkable_NavMesh");
        _decorRoot = NewChild(_worldRoot, "02_Decor_And_Hazards");
        _itemsRoot = NewChild(_worldRoot, "03_Items_No_Checkpoints");
        _enemiesRoot = NewChild(_worldRoot, "04_Enemies");
        _waypointsRoot = NewChild(_worldRoot, "05_Enemy_Waypoints_WorldSpace");

        Sky();
        Player();
        Managers();

        BuildZone1_Hills();
        Level2Zone1Dressing.Build(_decorRoot); // L2-2: зелёная растительность Z1
        BuildZone2_Fort();
        BuildZone3_Temple();
        BuildZone4_LavaArena(); // v24: объединённая большая зона 4 = кузня + тронный собор + финальный босс
        BuildMainRouteConnectors();
        AddV24Zone3ExtraSpace();
        AddV26LongTransitionPolish();
        AddOuterBorders();

        if (GameManager.Instance != null)
            GameManager.Instance.RefreshCoinTotal();

        yield return BakeNavMesh();

        GameManager.Instance?.ShowZoneTitle(
            "Царство Орков",
            "Убей вождя. Освободи жителей.", 5f);

        Debug.Log("✅ SceneBuilder v26: рабочая карта из 4 зон собрана — финальная зона объединяет кузню, тронный собор и босса; переходы усилены мостами, площадками отдыха и светом.");
    }

    Transform NewChild(Transform parent, string name)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent);
        return t;
    }

    void CleanupDefaultSceneObjects()
    {
        // Убираем стандартную камеру/слушатель из пустой сцены, чтобы не было предупреждения "2 audio listeners".
        // Рабочего игрока всё равно создаёт SceneBuilder ниже.
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam != null && cam.transform.parent == null)
                Destroy(cam.gameObject);
        }

        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light != null && light.type == LightType.Directional && light.gameObject.name == "Directional Light")
                Destroy(light.gameObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // VISUAL SETUP
    // ─────────────────────────────────────────────────────────────────────
    void SetupMaterials()
    {
        _grass = MakeMat("M_Grass", new Color(0.24f, 0.46f, 0.16f), new Color(0.34f, 0.62f, 0.24f));
        _dirt = MakeMat("M_Dirt", new Color(0.36f, 0.27f, 0.17f), new Color(0.48f, 0.36f, 0.23f));
        _fort = MakeMat("M_FortStone", new Color(0.38f, 0.34f, 0.28f), new Color(0.52f, 0.47f, 0.38f));
        _temple = MakeMat("M_TempleStone", new Color(0.66f, 0.61f, 0.47f), new Color(0.82f, 0.76f, 0.58f));
        _arena = MakeMat("M_DarkArenaStone", new Color(0.18f, 0.15f, 0.14f), new Color(0.30f, 0.25f, 0.22f));
        _wood = MakeMat("M_Wood", new Color(0.35f, 0.20f, 0.10f), new Color(0.52f, 0.32f, 0.16f));
        _gold = MakeMat("M_Gold", new Color(0.92f, 0.68f, 0.12f), new Color(1.00f, 0.88f, 0.28f), true, new Color(1f, 0.65f, 0.05f), 0.35f);
        _lava = MakeMat("M_Lava", new Color(0.80f, 0.10f, 0.02f), new Color(1.00f, 0.42f, 0.02f), true, new Color(1f, 0.12f, 0f), 1.7f);
        _dark = MakeMat("M_Abyss", new Color(0.03f, 0.02f, 0.015f), new Color(0.10f, 0.04f, 0.02f));
        _blood = MakeMat("M_BloodStone", new Color(0.32f, 0.05f, 0.04f), new Color(0.48f, 0.08f, 0.05f));
        _water = MakeMat("M_MoatDark", new Color(0.03f, 0.08f, 0.10f), new Color(0.05f, 0.18f, 0.22f), true, new Color(0f, 0.18f, 0.25f), 0.35f);
    }

    static Shader LitShader() =>
        Shader.Find("Universal Render Pipeline/Lit") ??
        Shader.Find("Standard") ??
        Shader.Find("Diffuse");

    Material MakeMat(string name, Color a, Color b, bool emission = false, Color emissionColor = default, float emissionPower = 0f)
    {
        // v8: убрали процедурную шахматную текстуру.
        // Для текущего проекта лучше чистый low-poly цвет: ничего не "плывёт" и не мерцает на больших плоскостях.
        var m = new Material(LitShader()) { name = name };
        m.SetColor("_BaseColor", Color.Lerp(a, b, 0.35f));
        m.color = Color.Lerp(a, b, 0.35f);
        m.SetFloat("_Smoothness", 0.08f);
        if (emission)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emissionColor * emissionPower);
        }
        return m;
    }

    void Sky()
    {
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.045f, 0.035f, 0.08f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.06f, 0.045f, 0.08f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 165f;
        RenderSettings.ambientLight = new Color(0.16f, 0.12f, 0.18f);

        var sun = new GameObject("Sun_Warm_KeyLight");
        sun.transform.SetParent(_worldRoot);
        var dl = sun.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.color = new Color(1f, 0.74f, 0.42f);
        dl.intensity = 0.78f;
        sun.transform.rotation = Quaternion.Euler(42f, -38f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────
    // LEGO BLOCKS
    // ─────────────────────────────────────────────────────────────────────
    GameObject Block(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent, bool navMesh = true, bool collider = true)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.transform.SetParent(parent);
        ApplyMaterial(g, mat, scale);

        if (!collider)
        {
            var c = g.GetComponent<Collider>();
            if (c) Destroy(c);
        }

        if (navMesh) g.isStatic = true;
        else IgnoreNavMesh(g);

        return g;
    }

    GameObject VisualBlock(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        return Block(name, pos, scale, mat, _decorRoot, false, false);
    }

    void ApplyMaterial(GameObject g, Material src, Vector3 scale)
    {
        var r = g.GetComponent<Renderer>();
        if (!r || src == null) return;
        var m = new Material(src);
        float tx = Mathf.Max(1f, Mathf.Abs(scale.x) / 2f);
        float ty = Mathf.Max(1f, Mathf.Abs(scale.z) / 2f);
        // На вертикальных тонких объектах берём высоту, чтобы рисунок не растягивался.
        if (Mathf.Abs(scale.z) < 0.5f) ty = Mathf.Max(1f, Mathf.Abs(scale.y) / 2f);
        if (Mathf.Abs(scale.x) < 0.5f) tx = Mathf.Max(1f, Mathf.Abs(scale.y) / 2f);
        m.mainTextureScale = new Vector2(tx, ty);
        r.material = m;
    }

    void IgnoreNavMesh(GameObject g)
    {
        g.isStatic = false;
        var mod = g.GetComponent<NavMeshModifier>();
        if (mod == null) mod = g.AddComponent<NavMeshModifier>();
        mod.ignoreFromBuild = true;
    }

    GameObject Ramp(string name, Vector3 pos, Vector3 scale, Material mat, float angleX, Transform parent)
    {
        // v8: вместо наклонённого гигантского куба делаем аккуратные низкие ступени.
        // Так не появляются "поехавшие" стены/плиты и игроку понятнее, куда подниматься.
        var root = new GameObject(name + "_CleanTerraceRamp");
        root.transform.SetParent(parent);

        int steps = 5;
        float length = Mathf.Max(1f, Mathf.Abs(scale.z));
        float stepD = length / steps;
        float totalH = Mathf.Max(0.7f, Mathf.Abs(scale.y) * 1.35f);
        float startZ = pos.z - length / 2f;
        float baseY = pos.y - totalH / 2f;
        bool reverseHeight = angleX < 0f;

        for (int i = 0; i < steps; i++)
        {
            int hIndex = reverseHeight ? (steps - 1 - i) : i;
            float topY = baseY + ((hIndex + 1f) / steps) * totalH;
            float h = 0.28f + (hIndex + 1f) * 0.04f;
            Vector3 stepPos = new Vector3(pos.x, topY - h / 2f, startZ + i * stepD + stepD / 2f);
            Block(name + "_Step_" + (i + 1), stepPos, new Vector3(scale.x, h, stepD + 0.03f), mat, root.transform, true, true);
        }

        return root;
    }

    void Stairs(string name, Vector3 startPos, int steps, float width, float stepH, float stepD, Material mat, Transform parent)
    {
        for (int i = 0; i < steps; i++)
        {
            var pos = startPos + new Vector3(0f, i * stepH + stepH / 2f, i * stepD + stepD / 2f);
            Block($"{name}_Step_{i + 1}", pos, new Vector3(width, stepH, Mathf.Abs(stepD)), mat, parent, true, true);
        }
    }

    void RailPair(string name, float xHalf, float y, float z, float len, Material mat, Transform parent)
    {
        Block(name + "_Rail_L", new Vector3(-xHalf, y, z), new Vector3(0.20f, 1.0f, len), mat, parent, true, true);
        Block(name + "_Rail_R", new Vector3(xHalf, y, z), new Vector3(0.20f, 1.0f, len), mat, parent, true, true);
    }

    void Column(string name, Vector3 basePos, float height, float radius, Material mat, Transform parent, bool broken = false)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        c.name = name;
        c.transform.position = basePos + Vector3.up * (height / 2f);
        c.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        c.transform.SetParent(parent);
        ApplyMaterial(c, mat, new Vector3(radius * 2f, height, radius * 2f));
        c.isStatic = true;

        Block(name + "_Base", basePos + Vector3.up * 0.15f, new Vector3(radius * 2.8f, 0.3f, radius * 2.8f), mat, parent, true, true);
        Block(name + "_Cap", basePos + Vector3.up * (height + 0.2f), new Vector3(radius * 2.8f, 0.4f, radius * 2.8f), mat, parent, true, true);

        if (broken)
        {
            Block(name + "_Broken_Chunk", basePos + new Vector3(radius * 2.2f, 0.25f, radius * 1.6f), new Vector3(1.0f, 0.5f, 0.7f), mat, parent, true, true)
                .transform.rotation = Quaternion.Euler(0f, 25f, 0f);
        }
    }

    void Tower(string name, Vector3 basePos, float height, float size, Material mat)
    {
        Block(name + "_Body", basePos + Vector3.up * height / 2f, new Vector3(size, height, size), mat, _decorRoot, true, true);
        float topY = basePos.y + height + 0.45f;
        for (int i = 0; i < 4; i++)
        {
            float sx = (i < 2 ? -1f : 1f) * size * 0.33f;
            float sz = (i % 2 == 0 ? -1f : 1f) * size * 0.33f;
            Block(name + "_Merlon_" + i, new Vector3(basePos.x + sx, topY, basePos.z + sz), new Vector3(0.8f, 0.9f, 0.8f), mat, _decorRoot, true, true);
        }
    }

    void LightAt(string name, Vector3 pos, Color color, float intensity, float range)
    {
        var g = new GameObject(name);
        g.transform.position = pos;
        g.transform.SetParent(_decorRoot);
        var l = g.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
    }

    void Fire(Vector3 pos)
    {
        var root = new GameObject("Fire_Torch");
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);
        var l = root.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.46f, 0.08f);
        l.intensity = 1.8f;
        l.range = 8f;
        var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.transform.SetParent(root.transform);
        flame.transform.localPosition = Vector3.up * 0.35f;
        flame.transform.localScale = Vector3.one * 0.34f;
        flame.GetComponent<Renderer>().material = new Material(_lava);
        var col = flame.GetComponent<Collider>();
        if (col) Destroy(col);
        flame.AddComponent<FireFlicker>();
        IgnoreNavMesh(flame);
    }

    // ─────────────────────────────────────────────────────────────────────
    // MAP ZONES
    // ─────────────────────────────────────────────────────────────────────
    void BuildZone1_Hills()
    {
        var zroot = NewChild(_walkRoot, "Zone_1_Hilly_Plain");

        // Модульная земля вместо одной огромной плиты — текстуры не плывут.
        for (int x = -2; x <= 2; x++)
            for (int z = 0; z < 4; z++)
                Block("Z1_Grass_Tile", new Vector3(x * 10f, -0.5f, z * 10f + 5f), new Vector3(10f, 1f, 10f), _grass, zroot, true, true);

        // Два читаемых холма + нормальные пандусы.
        Block("Z1_LowHill_Base", new Vector3(-8f, 0.25f, 13f), new Vector3(8f, 1.5f, 9f), _dirt, zroot, true, true);
        Block("Z1_LowHill_Top", new Vector3(-8f, 1.35f, 14f), new Vector3(5.8f, 0.7f, 5.6f), _grass, zroot, true, true);
        Ramp("Z1_LowHill_Ramp", new Vector3(-8f, 0.30f, 8.1f), new Vector3(6f, 0.6f, 5f), _dirt, 14f, zroot);

        Block("Z1_HighHill_Base", new Vector3(8f, 0.60f, 26f), new Vector3(9f, 2.2f, 10f), _dirt, zroot, true, true);
        Block("Z1_HighHill_Top", new Vector3(8f, 2.15f, 27f), new Vector3(5.8f, 0.9f, 5.8f), _grass, zroot, true, true);
        Ramp("Z1_HighHill_Ramp", new Vector3(8f, 0.70f, 19.8f), new Vector3(5f, 0.9f, 8f), _dirt, 19f, zroot);

        // Овраг: тёмное дно не входит в NavMesh, мост — единственная нормальная дорога.
        VisualBlock("Z1_Ravine_Dark_Left", new Vector3(-8f, -2.4f, 34f), new Vector3(10f, 3.5f, 9f), _dark);
        VisualBlock("Z1_Ravine_Dark_Right", new Vector3(8f, -2.4f, 34f), new Vector3(10f, 3.5f, 9f), _dark);
        Block("Z1_Narrow_Bridge", new Vector3(0f, 0.10f, 34f), new Vector3(3.5f, 0.25f, 10f), _wood, zroot, true, true);
        RailPair("Z1_Bridge", 1.85f, 0.75f, 34f, 10f, _wood, _decorRoot);
        LightAt("Z1_Ravine_LowLight", new Vector3(0f, -1.4f, 34f), new Color(0.25f, 0.09f, 0.03f), 1.1f, 16f);

        // Декор/укрытия.
        Rock(new Vector3(-4f, 0.25f, 5f), 1.4f);
        Rock(new Vector3(5f, 0.25f, 12f), 1.1f);
        Rock(new Vector3(-10f, 0.25f, 28f), 1.6f);
        Fire(new Vector3(-4.5f, 0.1f, 2.5f));
        Fire(new Vector3(4.5f, 0.1f, 2.5f));

        // v9: чекпоинты убраны — смерть возвращает игрока к началу карты.

        // Checkpoint(new Vector3(0f, 0.25f, 3f));
        Coin(new Vector3(0f, 0.75f, 10f));
        Coin(new Vector3(-8f, 2.25f, 14f));
        Coin(new Vector3(0f, 0.75f, 34f));
        Coin(new Vector3(8f, 3.1f, 27f));
        HealthPack(new Vector3(-3f, 0.75f, 23f));

        var z1e0 = OrcGrunt("Z1_Grunt_A", new Vector3(-4f, 0.6f, 16f), new[] { new Vector3(-7f, 0.6f, 10f), new Vector3(0f, 0.6f, 20f) });
        var z1e1 = OrcGrunt("Z1_Grunt_B", new Vector3(5f, 0.6f, 30f), new[] { new Vector3(3f, 0.6f, 24f), new Vector3(8f, 0.6f, 36f) });
        var z1e2 = OrcBerserker("Z1_Berserker_Hill", new Vector3(-6f, 1.4f, 22f), new[] { new Vector3(-8f, 1.4f, 18f), new Vector3(-4f, 1.4f, 26f) });
        var z1e3 = OrcArcher("Z1_Archer_Bridge", new Vector3(0f, 0.9f, 40f), new[] { new Vector3(-2f, 0.9f, 38f), new Vector3(2f, 0.9f, 40f) });

        RitualGateWithTotems("RitualGate_Z1_to_Z2", new Vector3(0f, 0f, 40.5f), 7.5f, 4.5f, new[] { z1e0, z1e1, z1e2, z1e3 });

        ZoneSign("ЗОНА 1", new Vector3(0f, 3.0f, 1.2f), new Color(0.25f, 0.85f, 0.20f));
        LightAt("Z1_Green_Ambient", new Vector3(0f, 7f, 22f), new Color(0.28f, 0.65f, 0.25f), 1.4f, 40f);

        // ── ОРОЧЬЯ АТМОСФЕРА ЗОНЫ 1 ──────────────────────────────────
        var boneMat1 = MakeMat("M_Z1_Bone", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var woodPole = MakeMat("M_Z1_Pole", new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);
        // Черепа на палках у моста через овраг
        VisualBlock("Z1_SkullPole_L_pole", new Vector3(-3.5f, 1.2f, 30f), new Vector3(0.15f, 2.4f, 0.15f), woodPole);
        VisualBlock("Z1_SkullPole_L_skull", new Vector3(-3.5f, 2.6f, 30f), new Vector3(0.42f, 0.42f, 0.35f), boneMat1);
        VisualBlock("Z1_SkullPole_R_pole", new Vector3(3.5f, 1.2f, 38f), new Vector3(0.15f, 2.4f, 0.15f), woodPole);
        VisualBlock("Z1_SkullPole_R_skull", new Vector3(3.5f, 2.6f, 38f), new Vector3(0.42f, 0.42f, 0.35f), boneMat1);
        // Орочьи знаки-флажки
        VisualBlock("Z1_OrcFlag_A", new Vector3(-6f, 1.8f, 8f), new Vector3(0.6f, 1.0f, 0.05f), _blood);
        VisualBlock("Z1_OrcFlag_B", new Vector3(7f, 1.8f, 20f), new Vector3(0.6f, 1.0f, 0.05f), _blood);
        // Предупреждающие костры перед оврагом
        Fire(new Vector3(-2.5f, 0.1f, 29f));
        Fire(new Vector3(2.5f, 0.1f, 39f));
        // Путевые факелы — ведут игрока вперёд
        Fire(new Vector3(-2f, 0.1f, 15f));
        Fire(new Vector3( 3f, 0.1f, 25f));
    }

    void BuildZone2_Fort()
    {
        var zroot = NewChild(_walkRoot, "Zone_2_v17_Real_Fortress");
        const float z0 = 42f;

        // v17: зона 2 теперь читается как настоящий форт: ров -> мост -> открытая арка -> двор -> стены -> выход.
        // Главный путь всегда открыт, но вокруг появились стены, башни, верхние проходы и места для лучников.

        // Подход к форту и боковые берега рва.
        Block("Z2_Approach_Main_Road", new Vector3(0f, -0.5f, z0 + 0.5f), new Vector3(10f, 1f, 5f), _dirt, zroot, true, true);
        Block("Z2_Moat_Left_Bank", new Vector3(-10.5f, -0.5f, z0 + 7f), new Vector3(9f, 1f, 10f), _fort, zroot, true, true);
        Block("Z2_Moat_Right_Bank", new Vector3(10.5f, -0.5f, z0 + 7f), new Vector3(9f, 1f, 10f), _fort, zroot, true, true);
        VisualBlock("Z2_Wide_Moat_Dark_Water", new Vector3(0f, -1.35f, z0 + 7f), new Vector3(22f, 0.35f, 10f), _water);

        // Мост через ров — широкий и очевидный.
        Block("Z2_Main_Drawbridge_Walkable", new Vector3(0f, 0.05f, z0 + 7f), new Vector3(5.4f, 0.32f, 10f), _wood, zroot, true, true);
        RailPair("Z2_Drawbridge_Rails", 2.95f, 0.72f, z0 + 7f, 9.4f, _wood, _decorRoot);

        // Большой внутренний двор. Центральная ось остаётся чистой.
        Block("Z2_Main_Courtyard", new Vector3(0f, -0.5f, z0 + 31f), new Vector3(28f, 1f, 38f), _fort, zroot, true, true);
        Block("Z2_Center_Route_Gold_Line", new Vector3(0f, 0.08f, z0 + 31f), new Vector3(1.2f, 0.06f, 34f), _gold, _decorRoot, false, false);

        // Главная стена с ОТКРЫТОЙ аркой: двери больше нет на обязательном пути.
        Block("Z2_Gatehouse_LeftMass", new Vector3(-8.7f, 3.35f, z0 + 14f), new Vector3(9.5f, 6.7f, 2f), _fort, _decorRoot, true, true);
        Block("Z2_Gatehouse_RightMass", new Vector3(8.7f, 3.35f, z0 + 14f), new Vector3(9.5f, 6.7f, 2f), _fort, _decorRoot, true, true);
        Block("Z2_Gatehouse_ArchTop", new Vector3(0f, 6.65f, z0 + 14f), new Vector3(8f, 1.35f, 2f), _fort, _decorRoot, true, true);
        Block("Z2_Raised_Portcullis", new Vector3(0f, 4.55f, z0 + 13.05f), new Vector3(4.5f, 0.45f, 0.28f), _arena, _decorRoot, false, false);

        // Распахнутые створки как декор без коллайдера, чтобы игрок не думал, что дверь закрыта.
        DoorVisual(new Vector3(0f, 2.1f, z0 + 13.0f), new Vector3(4.8f, 4.2f, 0.35f));

        // Четыре башни + силуэт крепости.
        Tower("Z2_Front_Tower_Left", new Vector3(-14.5f, -0.05f, z0 + 14f), 9.2f, 3.4f, _fort);
        Tower("Z2_Front_Tower_Right", new Vector3(14.5f, -0.05f, z0 + 14f), 9.2f, 3.4f, _fort);
        Tower("Z2_Back_Tower_Left", new Vector3(-14.5f, -0.05f, z0 + 48f), 7.4f, 3.0f, _fort);
        Tower("Z2_Back_Tower_Right", new Vector3(14.5f, -0.05f, z0 + 48f), 7.4f, 3.0f, _fort);

        // Боковые стены и верхние проходы для лучников.
        Block("Z2_Left_Long_Wall", new Vector3(-14.8f, 3.0f, z0 + 31f), new Vector3(2.1f, 6.0f, 35f), _fort, _decorRoot, true, true);
        Block("Z2_Right_Long_Wall", new Vector3(14.8f, 3.0f, z0 + 31f), new Vector3(2.1f, 6.0f, 35f), _fort, _decorRoot, true, true);

        Block("Z2_Left_Rampart_Walk", new Vector3(-12.7f, 6.25f, z0 + 31f), new Vector3(3.2f, 0.5f, 35f), _fort, zroot, true, true);
        Block("Z2_Right_Rampart_Walk", new Vector3(12.7f, 6.25f, z0 + 31f), new Vector3(3.2f, 0.5f, 35f), _fort, zroot, true, true);
        Block("Z2_Gate_Top_Rampart", new Vector3(0f, 7.15f, z0 + 14f), new Vector3(17f, 0.45f, 3.2f), _fort, zroot, true, true);

        // Зубцы не сплошной стеной — силуэт есть, но обзор и путь читаются.
        for (float z = z0 + 17f; z <= z0 + 47f; z += 4f)
        {
            Block("Z2_Left_Wall_Merlon", new Vector3(-12.7f, 6.95f, z), new Vector3(0.85f, 0.9f, 0.85f), _fort, _decorRoot, true, true);
            Block("Z2_Right_Wall_Merlon", new Vector3(12.7f, 6.95f, z), new Vector3(0.85f, 0.9f, 0.85f), _fort, _decorRoot, true, true);
        }
        Battlements(z0 + 14f, -8f, 8f, 7.85f, _fort);

        // Лестницы на стены: игрок видит, что наверх можно подняться.
        Stairs("Z2_Left_Rampart_Stairs", new Vector3(-10.6f, 0f, z0 + 19f), 12, 2.6f, 0.42f, 0.82f, _fort, zroot);
        Stairs("Z2_Right_Rampart_Stairs", new Vector3(10.6f, 0f, z0 + 36f), 12, 2.6f, 0.42f, -0.82f, _fort, zroot);

        // Приподнятая левая площадка и низкая правая яма — разные уровни, но без тупика.
        Block("Z2_Left_Command_Platform", new Vector3(-6.6f, 0.6f, z0 + 29f), new Vector3(8.4f, 2.0f, 14f), _fort, zroot, true, true);
        Ramp("Z2_Left_Command_Ramp", new Vector3(-2.8f, 0.35f, z0 + 22f), new Vector3(4f, 0.85f, 6f), _fort, 15f, zroot);

        VisualBlock("Z2_Right_Low_Pit_Ember", new Vector3(7.6f, -0.95f, z0 + 29f), new Vector3(9f, 0.25f, 14f), _lava);
        Block("Z2_Right_Low_Pit_Floor", new Vector3(7.6f, -0.45f, z0 + 29f), new Vector3(9f, 0.35f, 14f), _arena, zroot, true, true);
        Ramp("Z2_Right_Pit_Down", new Vector3(3.3f, -0.20f, z0 + 23f), new Vector3(3.4f, 0.65f, 5f), _fort, -12f, zroot);
        Ramp("Z2_Right_Pit_Up", new Vector3(3.3f, -0.20f, z0 + 35f), new Vector3(3.4f, 0.65f, 5f), _fort, 12f, zroot);
        LightAt("Z2_Pit_Ember_Light", new Vector3(7.5f, 1.1f, z0 + 29f), new Color(1f, 0.18f, 0.05f), 1.9f, 17f);

        // Внутренний разрушенный бастион перед выходом в храм.
        Block("Z2_Inner_Bastion_Left", new Vector3(-7.6f, 2.45f, z0 + 51f), new Vector3(8f, 4.9f, 1.4f), _fort, _decorRoot, true, true);
        Block("Z2_Inner_Bastion_Right", new Vector3(7.6f, 2.45f, z0 + 51f), new Vector3(8f, 4.9f, 1.4f), _fort, _decorRoot, true, true);
        Block("Z2_Inner_Bastion_BrokenTop", new Vector3(-1.0f, 4.95f, z0 + 51f), new Vector3(4f, 0.8f, 1.4f), _fort, _decorRoot, true, true);
        Block("Z2_Rubble_Through_Breach_A", new Vector3(-2.5f, 0.25f, z0 + 51.2f), new Vector3(2.8f, 0.5f, 1.3f), _fort, zroot, true, true);
        Block("Z2_Rubble_Through_Breach_B", new Vector3(2.4f, 0.20f, z0 + 50.7f), new Vector3(2.0f, 0.4f, 1.1f), _fort, zroot, true, true);

        // Укрытия/баррикады внутри двора — нужны для боя с дальниками.
        Barricade(new Vector3(-4.7f, 0.55f, z0 + 20f), 8f);
        Barricade(new Vector3(4.2f, 0.55f, z0 + 25f), -12f);
        Barricade(new Vector3(-1.2f, 0.55f, z0 + 38f), 20f);
        Barricade(new Vector3(5.5f, -0.05f, z0 + 32f), -8f);

        Block("Z2_Crate_Cover_A", new Vector3(-2.5f, 0.25f, z0 + 30f), new Vector3(1.2f, 1.0f, 1.2f), _wood, _decorRoot, true, true);
        Block("Z2_Crate_Cover_B", new Vector3(2.3f, 0.25f, z0 + 40f), new Vector3(1.2f, 1.0f, 1.2f), _wood, _decorRoot, true, true);
        Block("Z2_Crate_Cover_C", new Vector3(-9.5f, 1.25f, z0 + 31f), new Vector3(1.2f, 1.0f, 1.2f), _wood, _decorRoot, true, true);
        Rock(new Vector3(8.5f, -0.15f, z0 + 24f), 1.2f);
        Rock(new Vector3(-9.5f, 1.15f, z0 + 25f), 1.0f);

        // Факелы и баннеры — дешёвый, но заметный визуал.
        Fire(new Vector3(-5.2f, 0.15f, z0 + 13f));
        Fire(new Vector3(5.2f, 0.15f, z0 + 13f));
        Fire(new Vector3(-12.7f, 6.65f, z0 + 22f));
        Fire(new Vector3(12.7f, 6.65f, z0 + 39f));
        Fire(new Vector3(-4.4f, 0.15f, z0 + 50f));
        Fire(new Vector3(4.4f, 0.15f, z0 + 50f));

        VisualBlock("Z2_Banner_Left", new Vector3(-6.2f, 4.0f, z0 + 13.0f), new Vector3(1.0f, 3.2f, 0.08f), _blood);
        VisualBlock("Z2_Banner_Right", new Vector3(6.2f, 4.0f, z0 + 13.0f), new Vector3(1.0f, 3.2f, 0.08f), _blood);
        VisualBlock("Z2_Back_Banner_Left", new Vector3(-6.2f, 3.0f, z0 + 50.2f), new Vector3(1.0f, 2.4f, 0.08f), _blood);
        VisualBlock("Z2_Back_Banner_Right", new Vector3(6.2f, 3.0f, z0 + 50.2f), new Vector3(1.0f, 2.4f, 0.08f), _blood);

        // Лут ведёт по основному маршруту.
        CoinLine(new Vector3(0f, 0.75f, z0 + 6f), 5, 0f, 4.0f);
        Coin(new Vector3(-6.6f, 1.85f, z0 + 29f));
        Coin(new Vector3(7.6f, 0.12f, z0 + 29f));
        Coin(new Vector3(-12.7f, 7.0f, z0 + 27f));
        Coin(new Vector3(12.7f, 7.0f, z0 + 36f));
        HealthPack(new Vector3(-9.2f, 1.75f, z0 + 35f));

        // Ловушки теперь как охрана входа/ямы, но путь не перекрывают полностью.
        Trap_("Z2_Trap_Gate_Left", new Vector3(-2.6f, 0.62f, z0 + 18f));
        Trap_("Z2_Trap_Pit_Right", new Vector3(7.6f, -0.15f, z0 + 31f));

        // v17: гарнизон форта. Дальники стоят на стенах и реально стреляют, рукопашники держат двор.
        var z2e0 = OrcShieldGuard("Z2_Gate_Shield_L", new Vector3(-3.8f, 0.6f, z0 + 19f), new[] { new Vector3(-5.8f, 0.6f, z0 + 18f), new Vector3(-2.0f, 0.6f, z0 + 22f) });
        var z2e1 = OrcShieldGuard("Z2_Gate_Shield_R", new Vector3(3.8f, 0.6f, z0 + 19f), new[] { new Vector3(2.0f, 0.6f, z0 + 18f), new Vector3(5.8f, 0.6f, z0 + 22f) });

        var z2e2 = OrcArcher("Z2_Ranged_Left_Rampart_A", new Vector3(-12.7f, 6.7f, z0 + 22f), new[] { new Vector3(-12.7f, 6.7f, z0 + 19f), new Vector3(-12.7f, 6.7f, z0 + 31f) });
        var z2e3 = OrcArcher("Z2_Ranged_Right_Rampart_A", new Vector3(12.7f, 6.7f, z0 + 36f), new[] { new Vector3(12.7f, 6.7f, z0 + 28f), new Vector3(12.7f, 6.7f, z0 + 45f) });
        var z2e4 = OrcArcher("Z2_Ranged_Gate_Top", new Vector3(0f, 7.65f, z0 + 14f), new[] { new Vector3(-4f, 7.65f, z0 + 14f), new Vector3(4f, 7.65f, z0 + 14f) });

        var z2e5 = OrcBerserker("Z2_Courtyard_Berserker", new Vector3(-6.2f, 1.45f, z0 + 31f), new[] { new Vector3(-8.5f, 1.45f, z0 + 24f), new Vector3(-3.0f, 1.45f, z0 + 38f) });
        var z2e6 = OrcShieldGuard("Z2_Inner_Captain", new Vector3(3.0f, 0.6f, z0 + 45f), new[] { new Vector3(-3f, 0.6f, z0 + 42f), new Vector3(5.5f, 0.6f, z0 + 48f) });
        var z2ambush0 = OrcGrunt("Z2_Ambush_L", new Vector3(-5.5f, 0.6f, z0 + 20f), new[] { new Vector3(-6f, 0.6f, z0 + 18f), new Vector3(-5f, 0.6f, z0 + 22f) });
        var z2ambush1 = OrcGrunt("Z2_Ambush_R", new Vector3(5.5f, 0.6f, z0 + 20f), new[] { new Vector3(6f, 0.6f, z0 + 18f), new Vector3(5f, 0.6f, z0 + 22f) });

        var trig = new GameObject("Z2_EntryTrigger");
        trig.transform.position = new Vector3(0f, 1f, z0 + 14f);
        var tc = trig.AddComponent<BoxCollider>();
        tc.isTrigger = true; tc.size = new Vector3(14f, 3f, 2f);
        trig.AddComponent<SimpleMessageTrigger>().message = "⚠ ЗАСАДА!";
        trig.transform.SetParent(_decorRoot);

        RitualGateWithTotems("RitualGate_Z2_to_Z3", new Vector3(0f, 0f, 93.5f), 8.5f, 5.0f,
            new[] { z2e0, z2e1, z2e2, z2e3, z2e4, z2e5, z2e6, z2ambush0, z2ambush1 });

        ZoneSign("ЗОНА 2 — КРЕПОСТЬ", new Vector3(0f, 3.7f, z0 + 1.5f), new Color(0.95f, 0.52f, 0.10f));
        LightAt("Z2_v17_Warm_Fort_Ambient", new Vector3(0f, 9f, z0 + 30f), new Color(0.95f, 0.40f, 0.08f), 2.6f, 56f);

        // ── ОРОЧЬЯ АТМОСФЕРА ЗОНЫ 2 ──────────────────────────────────
        var z2bone = MakeMat("M_Z2_Bone", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var z2pole = MakeMat("M_Z2_Pole", new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);
        // Боевые флаги на стенах — по 3 на каждую сторону
        for (int i = 0; i < 3; i++)
        {
            float fz = z0 + 20f + i * 10f;
            HangingBanner(new Vector3(-14.0f, 5.5f, fz), false);
            HangingBanner(new Vector3(14.0f, 5.5f, fz), true);
        }
        // Черепа на зубцах стены
        VisualBlock("Z2_MerlonSkull_L1", new Vector3(-12.7f, 7.6f, z0 + 21f), new Vector3(0.35f, 0.35f, 0.28f), z2bone);
        VisualBlock("Z2_MerlonSkull_L2", new Vector3(-12.7f, 7.6f, z0 + 37f), new Vector3(0.35f, 0.35f, 0.28f), z2bone);
        VisualBlock("Z2_MerlonSkull_R1", new Vector3(12.7f, 7.6f, z0 + 25f), new Vector3(0.35f, 0.35f, 0.28f), z2bone);
        VisualBlock("Z2_MerlonSkull_R2", new Vector3(12.7f, 7.6f, z0 + 41f), new Vector3(0.35f, 0.35f, 0.28f), z2bone);
        // Тотемы-указатели у входа в крепость
        VisualBlock("Z2_EntryTotem_L_pole", new Vector3(-4.5f, 1.5f, z0 + 4f), new Vector3(0.2f, 3.0f, 0.2f), z2pole);
        VisualBlock("Z2_EntryTotem_L_skull", new Vector3(-4.5f, 3.2f, z0 + 4f), new Vector3(0.45f, 0.45f, 0.38f), z2bone);
        VisualBlock("Z2_EntryTotem_R_pole", new Vector3(4.5f, 1.5f, z0 + 4f), new Vector3(0.2f, 3.0f, 0.2f), z2pole);
        VisualBlock("Z2_EntryTotem_R_skull", new Vector3(4.5f, 3.2f, z0 + 4f), new Vector3(0.45f, 0.45f, 0.38f), z2bone);
        // Ритуальные чаши внутри двора
        RitualBowl(new Vector3(-9f, 0.55f, z0 + 22f));
        RitualBowl(new Vector3(9f, 0.55f, z0 + 38f));
        // Подвесные цепи у стен
        ForgeChain(new Vector3(-8f, 5.5f, z0 + 28f));
        ForgeChain(new Vector3(8f, 5.5f, z0 + 34f));
        // Оружейные стойки и ящики — жизнь в крепости
        Block("Z2_WeaponRack_L",   new Vector3(-8f,  0.7f, z0 + 26f), new Vector3(0.3f, 1.4f, 1.2f), _wood, _decorRoot, false, true);
        Block("Z2_WeaponRack_R",   new Vector3( 8f,  0.7f, z0 + 36f), new Vector3(0.3f, 1.4f, 1.2f), _wood, _decorRoot, false, true);
        Block("Z2_SupplyCrate_A",  new Vector3(-10f, 0.5f, z0 + 42f), new Vector3(1.0f, 1.0f, 1.0f), _wood, _decorRoot, false, true);
        Block("Z2_SupplyCrate_B",  new Vector3(-10f, 1.0f, z0 + 42f), new Vector3(0.8f, 0.6f, 0.8f), _wood, _decorRoot, false, true);
    }

    void BuildZone3_Temple()
    {
        var zroot = NewChild(_walkRoot, "Zone_3_Grand_Temple_Ruins_v18");
        const float z0 = 94f;

        // v18: зона 3 теперь не просто прямоугольная площадка, а большой храмовый комплекс.
        // Главный путь всё равно читаемый: нижний двор -> широкая лестница -> алтарная платформа -> выход к арене.

        // ── НИЖНИЙ ДВОР / ПОДХОД К ХРАМУ ────────────────────────────────
        for (int x = -1; x <= 1; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                Block("Z3_v18_Lower_Courtyard_Tile", new Vector3(x * 8f, -0.5f, z0 + z * 8f + 4f),
                    new Vector3(8f, 1f, 8f), _temple, zroot, true, true);
            }
        }

        // Боковые провалы делают храм выше и опаснее, но центральный путь не трогаем.
        VisualBlock("Z3_v18_Left_Deep_Cliff", new Vector3(-17f, -3.2f, z0 + 29f), new Vector3(12f, 5.8f, 66f), _dark);
        VisualBlock("Z3_v18_Right_Deep_Cliff", new Vector3(17f, -3.2f, z0 + 29f), new Vector3(12f, 5.8f, 66f), _dark);
        LightAt("Z3_v18_Left_Abyss_Glow", new Vector3(-13f, -1.0f, z0 + 30f), new Color(0.28f, 0.08f, 0.02f), 1.3f, 28f);
        LightAt("Z3_v18_Right_Abyss_Glow", new Vector3(13f, -1.0f, z0 + 30f), new Color(0.28f, 0.08f, 0.02f), 1.3f, 28f);

        // Большой входной пилон: игрок сразу понимает, что начинается храм.
        Block("Z3_v18_Entrance_Pylon_L", new Vector3(-6.8f, 3.5f, z0 + 5f), new Vector3(1.2f, 7.0f, 1.2f), _temple, _decorRoot, true, true);
        Block("Z3_v18_Entrance_Pylon_R", new Vector3(6.8f, 3.5f, z0 + 5f), new Vector3(1.2f, 7.0f, 1.2f), _temple, _decorRoot, true, true);
        Block("Z3_v18_Entrance_Broken_Top_L", new Vector3(-2.7f, 7.2f, z0 + 5f), new Vector3(5.2f, 0.55f, 0.8f), _temple, _decorRoot, true, true)
            .transform.rotation = Quaternion.Euler(0f, 0f, -7f);
        Block("Z3_v18_Entrance_Broken_Top_R", new Vector3(2.7f, 7.2f, z0 + 5f), new Vector3(5.2f, 0.55f, 0.8f), _temple, _decorRoot, true, true)
            .transform.rotation = Quaternion.Euler(0f, 0f, 7f);
        Fire(new Vector3(-7.8f, 0.1f, z0 + 6.5f));
        Fire(new Vector3(7.8f, 0.1f, z0 + 6.5f));

        // Золотой маршрут по полу — не UI, а визуальная подсказка.
        Block("Z3_v18_Gold_Path_Lower", new Vector3(0f, 0.08f, z0 + 14f), new Vector3(2.0f, 0.06f, 18f), _gold, _decorRoot, false, false);

        // ── ГЛАВНАЯ ЛЕСТНИЦА И ТЕРРАСЫ ──────────────────────────────────
        Stairs("Z3_v18_Monumental_Stairs", new Vector3(0f, 0f, z0 + 18f), 10, 12.5f, 0.28f, 1.10f, _temple, zroot);
        RailPair("Z3_v18_Grand_Stair_Rails", 6.55f, 1.60f, z0 + 23.7f, 13.2f, _fort, _decorRoot);

        // Нижняя аллея с обломками и колоннами по бокам.
        for (float z = z0 + 12f; z <= z0 + 28f; z += 8f)
        {
            Column("Z3_v18_Lower_Column_L_" + z, new Vector3(-8.8f, 0f, z), 4.6f, 0.43f, _temple, _decorRoot, false);
            Column("Z3_v18_Lower_Column_R_" + z, new Vector3(8.8f, 0f, z), 4.6f, 0.43f, _temple, _decorRoot, false);
            LowRubble(new Vector3(-5.6f, 0.08f, z + 1.4f), _temple);
            LowRubble(new Vector3(5.6f, 0.08f, z - 1.4f), _temple);
        }

        // Главная верхняя площадка. Широкая, чтобы было место для боя с шаманами.
        Block("Z3_v18_Main_Temple_Platform", new Vector3(0f, 2.35f, z0 + 39f), new Vector3(25f, 1.0f, 23f), _temple, zroot, true, true);
        Block("Z3_v18_Gold_Path_Upper", new Vector3(0f, 2.93f, z0 + 40f), new Vector3(2.1f, 0.06f, 22f), _gold, _decorRoot, false, false);

        // Боковые галереи для дальников. Они дают объём, но не заставляют игрока туда идти.
        Block("Z3_v18_Left_Side_Gallery", new Vector3(-11.8f, 3.25f, z0 + 39f), new Vector3(3.2f, 0.5f, 24f), _temple, zroot, true, true);
        Block("Z3_v18_Right_Side_Gallery", new Vector3(11.8f, 3.25f, z0 + 39f), new Vector3(3.2f, 0.5f, 24f), _temple, zroot, true, true);
        Block("Z3_v18_Left_Gallery_Rail", new Vector3(-9.8f, 4.0f, z0 + 39f), new Vector3(0.25f, 1.0f, 22f), _fort, _decorRoot, true, true);
        Block("Z3_v18_Right_Gallery_Rail", new Vector3(9.8f, 4.0f, z0 + 39f), new Vector3(0.25f, 1.0f, 22f), _fort, _decorRoot, true, true);

        // Катакомбный ритм: ряды колонн и поломанные арки над головой.
        for (float z = z0 + 31f; z <= z0 + 51f; z += 6.5f)
        {
            Column("Z3_v18_Inner_Column_L_" + z, new Vector3(-7.6f, 2.85f, z), 6.2f, 0.48f, _temple, _decorRoot, false);
            Column("Z3_v18_Inner_Column_R_" + z, new Vector3(7.6f, 2.85f, z), 6.2f, 0.48f, _temple, _decorRoot, false);
            var beam = Block("Z3_v18_Broken_Ceiling_Beam_" + z, new Vector3(0f, 8.8f, z), new Vector3(16.5f, 0.35f, 0.48f), _temple, _decorRoot, false, false);
            beam.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-6f, 6f));
        }

        // ── АЛТАРНАЯ ЗОНА ───────────────────────────────────────────────
        Block("Z3_v18_Altar_Dais", new Vector3(0f, 3.05f, z0 + 44f), new Vector3(8.8f, 0.55f, 6.2f), _fort, zroot, true, true);
        Block("Z3_v18_Altar_Base", new Vector3(0f, 3.55f, z0 + 44f), new Vector3(5.2f, 0.62f, 3.6f), _gold, _decorRoot, true, true);
        Block("Z3_v18_Altar_Top", new Vector3(0f, 4.08f, z0 + 44f), new Vector3(3.2f, 0.40f, 2.2f), _gold, _decorRoot, true, true);
        RelicCrystal(new Vector3(0f, 4.95f, z0 + 44f), new Color(1f, 0.82f, 0.12f));
        RelicCrystal(new Vector3(-4.3f, 3.75f, z0 + 41f), new Color(0.95f, 0.55f, 1f));
        RelicCrystal(new Vector3(4.3f, 3.75f, z0 + 47f), new Color(0.95f, 0.55f, 1f));
        RitualBowl(new Vector3(-5.8f, 3.15f, z0 + 42f));
        RitualBowl(new Vector3(5.8f, 3.15f, z0 + 46f));
        LightAt("Z3_v18_Altar_Strong_Light", new Vector3(0f, 6.6f, z0 + 44f), new Color(1f, 0.78f, 0.18f), 4.0f, 34f);

        // Небольшой верхний проход-награда сбоку: красиво, но не ломает основной маршрут.
        Block("Z3_v18_Upper_Left_Relic_Walk", new Vector3(-11.8f, 5.15f, z0 + 50f), new Vector3(3.2f, 0.42f, 13f), _temple, zroot, true, true);
        Stairs("Z3_v18_Left_Relic_Stairs", new Vector3(-11.8f, 3.45f, z0 + 43f), 6, 3.2f, 0.28f, 0.78f, _temple, zroot);
        RelicCrystal(new Vector3(-11.8f, 5.95f, z0 + 54f), new Color(0.75f, 0.95f, 1f));

        // Выход из храма к зоне 4. Оставляем широким и понятным.
        Block("Z3_v18_Exit_Platform", new Vector3(0f, 2.45f, z0 + 58f), new Vector3(12f, 1f, 12f), _temple, zroot, true, true);
        Block("Z3_v18_Exit_Gold_Line", new Vector3(0f, 3.03f, z0 + 57f), new Vector3(2.0f, 0.06f, 12f), _gold, _decorRoot, false, false);
        Block("Z3_v18_Exit_Arch_L", new Vector3(-5.2f, 5.6f, z0 + 61f), new Vector3(0.9f, 6.2f, 0.9f), _temple, _decorRoot, true, true);
        Block("Z3_v18_Exit_Arch_R", new Vector3(5.2f, 5.6f, z0 + 61f), new Vector3(0.9f, 6.2f, 0.9f), _temple, _decorRoot, true, true);
        Block("Z3_v18_Exit_Arch_Top", new Vector3(0f, 8.5f, z0 + 61f), new Vector3(11.5f, 0.65f, 0.9f), _temple, _decorRoot, true, true);
        Fire(new Vector3(-5.8f, 3.1f, z0 + 58f));
        Fire(new Vector3(5.8f, 3.1f, z0 + 58f));

        // ── ЛУТ / ЛОВУШКИ / ВРАГИ ──────────────────────────────────────
        CoinLine(new Vector3(0f, 0.78f, z0 + 8f), 4, 0f, 4.7f);
        Coin(new Vector3(-1.0f, 4.75f, z0 + 44f));
        Coin(new Vector3(0.0f, 5.05f, z0 + 44f));
        Coin(new Vector3(1.0f, 4.75f, z0 + 44f));
        Coin(new Vector3(-11.8f, 5.85f, z0 + 54f));
        HealthPack(new Vector3(7.8f, 3.35f, z0 + 36f));
        HealthPack(new Vector3(0f, 3.25f, z0 + 58f));

        // Ловушки видимые и боковые: они создают опасность, но не закрывают весь путь.
        Trap_("Z3_v18_Trap_Stairs_Left", new Vector3(-4.7f, 1.9f, z0 + 25f));
        Trap_("Z3_v18_Trap_Altar_Right", new Vector3(4.9f, 3.05f, z0 + 40f));
        Trap_("Z3_v18_Trap_Gallery", new Vector3(-11.8f, 3.95f, z0 + 36f));

        // Расстановка врагов: защитники на лестнице, дальники на галереях, шаманы у алтаря.
        var z3e0 = OrcShieldGuard("Z3_v18_Stair_Guard_Left", new Vector3(-4.2f, 0.8f, z0 + 20f),
            new[] { new Vector3(-5.2f, 0.8f, z0 + 17f), new Vector3(1.5f, 1.45f, z0 + 27f) });
        var z3e1 = OrcShieldGuard("Z3_v18_Stair_Guard_Right", new Vector3(4.2f, 1.35f, z0 + 26f),
            new[] { new Vector3(5.2f, 1.35f, z0 + 22f), new Vector3(-1.5f, 2.25f, z0 + 31f) });
        var z3e2 = OrcArcher("Z3_v18_Left_Gallery_Archer", new Vector3(-11.8f, 3.95f, z0 + 34f),
            new[] { new Vector3(-11.8f, 3.95f, z0 + 30f), new Vector3(-11.8f, 3.95f, z0 + 47f) });
        var z3e3 = OrcArcher("Z3_v18_Right_Gallery_Archer", new Vector3(11.8f, 3.95f, z0 + 47f),
            new[] { new Vector3(11.8f, 3.95f, z0 + 33f), new Vector3(11.8f, 3.95f, z0 + 50f) });
        var z3e4 = OrcShaman("Z3_v18_Altar_Shaman_Left", new Vector3(-3.8f, 3.15f, z0 + 42f),
            new[] { new Vector3(-5.5f, 3.15f, z0 + 38f), new Vector3(1.5f, 3.15f, z0 + 46f) });
        var z3e5 = OrcShaman("Z3_v18_Altar_Shaman_Right", new Vector3(3.8f, 3.15f, z0 + 46f),
            new[] { new Vector3(5.5f, 3.15f, z0 + 40f), new Vector3(-1.5f, 3.15f, z0 + 50f) });

        RitualGateWithTotems("RitualGate_Z3_to_Z4", new Vector3(0f, 0f, 149.5f), 9.0f, 5.5f,
            new[] { z3e0, z3e1, z3e2, z3e3, z3e4, z3e5 });

        ZoneSign("ЗОНА 3 — ХРАМ", new Vector3(0f, 4.2f, z0 + 4f), new Color(1f, 0.82f, 0.22f));
        LightAt("Z3_v18_Temple_Ambient_Gold", new Vector3(0f, 11f, z0 + 40f), new Color(0.95f, 0.68f, 0.22f), 3.8f, 68f);
        LightAt("Z3_v18_Purple_Ritual_Ambient", new Vector3(0f, 6.2f, z0 + 45f), new Color(0.72f, 0.15f, 1f), 1.8f, 34f);

        // ── ОРОЧЬЯ АТМОСФЕРА ЗОНЫ 3 ──────────────────────────────────
        var z3bone = MakeMat("M_Z3_Bone", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var z3rune = MakeMat("M_Z3_Rune", new Color(0.72f, 0.15f, 0.8f), new Color(0.85f, 0.2f, 0.95f), true, new Color(0.8f, 0.15f, 1f), 0.7f);
        // Руны на полу у алтаря — пурпурные
        for (int i = 0; i < 8; i++)
        {
            float rang = i * 45f;
            float rr = 4.5f;
            Vector3 rp = new Vector3(
                Mathf.Cos(rang * Mathf.Deg2Rad) * rr,
                2.92f,
                Mathf.Sin(rang * Mathf.Deg2Rad) * rr + z0 + 40f);
            var runeObj = VisualBlock("Z3_FloorRune_" + i, rp, new Vector3(0.16f, 0.04f, 0.85f), z3rune);
            runeObj.transform.rotation = Quaternion.Euler(0f, -rang, 0f);
            runeObj.AddComponent<LavaPulse>().pulseScale = 0.02f;
        }
        // Черепа на колоннах нижней аллеи
        VisualBlock("Z3_ColSkull_L1", new Vector3(-8.8f, 5.0f, z0 + 12f), new Vector3(0.38f, 0.38f, 0.3f), z3bone);
        VisualBlock("Z3_ColSkull_R1", new Vector3(8.8f, 5.0f, z0 + 20f), new Vector3(0.38f, 0.38f, 0.3f), z3bone);
        VisualBlock("Z3_ColSkull_L2", new Vector3(-8.8f, 5.0f, z0 + 28f), new Vector3(0.38f, 0.38f, 0.3f), z3bone);
        VisualBlock("Z3_ColSkull_R2", new Vector3(8.8f, 5.0f, z0 + 28f), new Vector3(0.38f, 0.38f, 0.3f), z3bone);
        // Дополнительные ритуальные чаши у входных пилонов
        RitualBowl(new Vector3(-6.8f, 0.1f, z0 + 8f));
        RitualBowl(new Vector3(6.8f, 0.1f, z0 + 8f));
        // Орочьи флаги на внутренних колоннах
        VisualBlock("Z3_InnerFlag_L1", new Vector3(-7.6f, 6.5f, z0 + 37.5f), new Vector3(0.75f, 2.0f, 0.05f), _blood);
        VisualBlock("Z3_InnerFlag_R1", new Vector3(7.6f, 6.5f, z0 + 44f), new Vector3(0.75f, 2.0f, 0.05f), _blood);
        // Тотемы у выходной арки
        var z3pole = MakeMat("M_Z3_Pole", new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);
        VisualBlock("Z3_ExitTotem_L_pole", new Vector3(-6.5f, 4.2f, z0 + 61f), new Vector3(0.2f, 3.4f, 0.2f), z3pole);
        VisualBlock("Z3_ExitTotem_L_skull", new Vector3(-6.5f, 6.1f, z0 + 61f), new Vector3(0.48f, 0.48f, 0.4f), z3bone);
        VisualBlock("Z3_ExitTotem_R_pole", new Vector3(6.5f, 4.2f, z0 + 61f), new Vector3(0.2f, 3.4f, 0.2f), z3pole);
        VisualBlock("Z3_ExitTotem_R_skull", new Vector3(6.5f, 6.1f, z0 + 61f), new Vector3(0.48f, 0.48f, 0.4f), z3bone);
        Fire(new Vector3(-6.5f, 6.3f, z0 + 61f));
        Fire(new Vector3(6.5f, 6.3f, z0 + 61f));
    }

    void BuildZone4_LavaArena()
    {
        var zroot = NewChild(_walkRoot, "Zone_4_Merged_Forge_Throne_Cathedral_v24");
        const float z0 = 148f;

        // v24: объединяем прежние зоны 4 и 5 в один большой финальный уровень.
        // Теперь это не отдельная лава-арена + отдельный тронный зал, а цельный маршрут:
        // вход в кузню -> лавовые мосты -> центральная кузня -> тёмный тронный собор -> большая арена босса.

        // ── ОБЩИЙ ФОН: ЛАВА НИЖЕ, ХОДИМЫЕ ОБЪЕКТЫ СВЕРХУ ─────────────
        VisualBlock("Z4_v24_Lava_Underworld_Base", new Vector3(0f, -2.35f, z0 + 57f), new Vector3(76f, 1.1f, 126f), _lava);
        LavaSheet("Z4_v24_Lava_River_Left", new Vector3(-13f, -1.48f, z0 + 43f), new Vector3(14f, 0.08f, 86f), 0.4f);
        LavaSheet("Z4_v24_Lava_River_Right", new Vector3(13f, -1.46f, z0 + 43f), new Vector3(14f, 0.08f, 86f), 1.2f);
        LavaSheet("Z4_v24_Lava_River_Back", new Vector3(0f, -1.44f, z0 + 88f), new Vector3(44f, 0.08f, 42f), 1.9f);
        LightAt("Z4_v24_Lava_Low_Controlled_Glow", new Vector3(0f, -0.25f, z0 + 55f), new Color(1f, 0.13f, 0.02f), 4.5f, 86f);
        LightAt("Z4_v24_Cool_Dark_Cathedral_Fill", new Vector3(0f, 10f, z0 + 74f), new Color(0.16f, 0.12f, 0.25f), 1.2f, 92f);

        for (int i = 0; i < 12; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            float x = side * Random.Range(10f, 18f);
            float z = z0 + 10f + i * 7.2f;
            LavaVent(new Vector3(x, -1.05f, z));
        }

        // ── ЧАСТЬ 1: ВХОД В КУЗНЮ ───────────────────────────────────
        Block("Z4_v24_Forge_Entry_Platform", new Vector3(0f, 0.45f, z0 + 7f), new Vector3(24f, 1f, 10f), _arena, zroot, true, true);
        Block("Z4_v24_Forge_Entry_Dark_Overlay", new Vector3(0f, 1.02f, z0 + 7f), new Vector3(21f, 0.05f, 8.2f), _arena, _decorRoot, false, false);
        Zone4ForgeTower("Z4_v24_Entry_Tower_L", new Vector3(-11.5f, 0f, z0 + 8.5f));
        Zone4ForgeTower("Z4_v24_Entry_Tower_R", new Vector3(11.5f, 0f, z0 + 8.5f));
        ForgeAnvil(new Vector3(-8.5f, 0.55f, z0 + 5f));
        ForgeAnvil(new Vector3(8.5f, 0.55f, z0 + 8.5f));
        Fire(new Vector3(-6.5f, 0.15f, z0 + 10.5f));
        Fire(new Vector3(6.5f, 0.15f, z0 + 10.5f));
        VisualBlock("Z4_v24_Entry_Banner_L", new Vector3(-6.6f, 4.4f, z0 + 12f), new Vector3(1.0f, 3.3f, 0.08f), _blood);
        VisualBlock("Z4_v24_Entry_Banner_R", new Vector3(6.6f, 4.4f, z0 + 12f), new Vector3(1.0f, 3.3f, 0.08f), _blood);

        // ── ЧАСТЬ 2: ЛАВОВЫЕ МОСТЫ И ЦЕНТРАЛЬНАЯ КУЗНЯ ───────────────
        Block("Z4_v24_Front_Bridge", new Vector3(0f, 1.18f, z0 + 21f), new Vector3(6.2f, 0.45f, 22f), _arena, zroot, true, true);
        RailPair("Z4_v24_Front_Bridge_Rails", 3.35f, 1.95f, z0 + 21f, 22f, _fort, _decorRoot);
        Zone4BridgeSupports("Z4_v24_Front_Bridge_Supports", new Vector3(0f, 0.1f, z0 + 21f), 6, 4.8f, 20f);
        Block("Z4_v24_Front_Gold_Path", new Vector3(0f, 1.50f, z0 + 21f), new Vector3(1.5f, 0.055f, 18f), _gold, _decorRoot, false, false);

        Block("Z4_v24_Central_Forge_Island", new Vector3(0f, 1.85f, z0 + 36f), new Vector3(22f, 1.2f, 20f), _arena, zroot, true, true);
        Block("Z4_v24_Central_Forge_Dais", new Vector3(0f, 2.65f, z0 + 36f), new Vector3(10.5f, 0.55f, 9.5f), _fort, zroot, true, true);
        Block("Z4_v24_Central_Hot_Core", new Vector3(0f, 3.04f, z0 + 36f), new Vector3(3.8f, 0.12f, 3.8f), _lava, _decorRoot, false, false);
        BossAuraRing(new Vector3(0f, 3.10f, z0 + 36f));
        LightAt("Z4_v24_Central_Forge_Core_Light", new Vector3(0f, 4.0f, z0 + 36f), new Color(1f, 0.25f, 0.03f), 2.5f, 18f);
        ForgeAnvil(new Vector3(-6.3f, 2.55f, z0 + 35f));
        ForgeAnvil(new Vector3(6.3f, 2.55f, z0 + 37f));
        RitualBowl(new Vector3(-7.2f, 2.55f, z0 + 29f));
        RitualBowl(new Vector3(7.2f, 2.55f, z0 + 43f));
        CoverWall("Z4_v24_Forge_Cover_L", new Vector3(-5.0f, 2.55f, z0 + 31f), 14f, 3.4f);
        CoverWall("Z4_v24_Forge_Cover_R", new Vector3(5.0f, 2.55f, z0 + 41f), -14f, 3.4f);

        Block("Z4_v24_Back_Bridge", new Vector3(0f, 1.18f, z0 + 53f), new Vector3(6.2f, 0.45f, 24f), _arena, zroot, true, true);
        RailPair("Z4_v24_Back_Bridge_Rails", 3.35f, 1.95f, z0 + 53f, 24f, _fort, _decorRoot);
        Zone4BridgeSupports("Z4_v24_Back_Bridge_Supports", new Vector3(0f, 0.1f, z0 + 53f), 6, 4.8f, 22f);
        Block("Z4_v24_Back_Gold_Path", new Vector3(0f, 1.50f, z0 + 53f), new Vector3(1.5f, 0.055f, 20f), _gold, _decorRoot, false, false);

        // Боковые обходы и балконы — это ещё всё тот же уровень 4, но он уже ощущается большим.
        Block("Z4_v24_Left_Ring_Walk", new Vector3(-19.5f, 0.45f, z0 + 38f), new Vector3(4.2f, 1f, 62f), _arena, zroot, true, true);
        Block("Z4_v24_Right_Ring_Walk", new Vector3(19.5f, 0.45f, z0 + 38f), new Vector3(4.2f, 1f, 62f), _arena, zroot, true, true);
        Block("Z4_v24_Left_Link_Front", new Vector3(-10f, 0.45f, z0 + 25f), new Vector3(19f, 1f, 3.4f), _arena, zroot, true, true);
        Block("Z4_v24_Right_Link_Front", new Vector3(10f, 0.45f, z0 + 25f), new Vector3(19f, 1f, 3.4f), _arena, zroot, true, true);
        Block("Z4_v24_Left_Link_Back", new Vector3(-10f, 0.45f, z0 + 56f), new Vector3(19f, 1f, 3.4f), _arena, zroot, true, true);
        Block("Z4_v24_Right_Link_Back", new Vector3(10f, 0.45f, z0 + 56f), new Vector3(19f, 1f, 3.4f), _arena, zroot, true, true);
        Block("Z4_v24_Upper_Left_Balcony", new Vector3(-19.5f, 3.2f, z0 + 27f), new Vector3(4.0f, 0.45f, 20f), _fort, zroot, true, true);
        Block("Z4_v24_Upper_Right_Balcony", new Vector3(19.5f, 3.2f, z0 + 50f), new Vector3(4.0f, 0.45f, 20f), _fort, zroot, true, true);
        Stairs("Z4_v24_Left_Balcony_Stairs", new Vector3(-19.5f, 0.9f, z0 + 14f), 7, 4.0f, 0.34f, 0.85f, _fort, zroot);
        Stairs("Z4_v24_Right_Balcony_Stairs", new Vector3(19.5f, 0.9f, z0 + 63f), 7, 4.0f, 0.34f, -0.85f, _fort, zroot);

        // Большие силуэты кузни: башни, цепи, колонны.
        Zone4ForgeTower("Z4_v24_Tower_FL", new Vector3(-25f, 0f, z0 + 18f));
        Zone4ForgeTower("Z4_v24_Tower_FR", new Vector3(25f, 0f, z0 + 18f));
        Zone4ForgeTower("Z4_v24_Tower_BL", new Vector3(-25f, 0f, z0 + 58f));
        Zone4ForgeTower("Z4_v24_Tower_BR", new Vector3(25f, 0f, z0 + 58f));
        for (int i = 0; i < 5; i++)
        {
            float z = z0 + 18f + i * 10f;
            ForgeChain(new Vector3(-5.2f, 4.0f, z));
            ForgeChain(new Vector3(5.2f, 4.0f, z + 4f));
        }

        // ── ЧАСТЬ 3: ПЕРЕХОД В ТРОННЫЙ СОБОР ВНУТРИ ТОГО ЖЕ УРОВНЯ ───
        TransitionArch("Z4_v24_Internal_Cathedral_Gate", new Vector3(0f, 1.0f, z0 + 68f), 13.5f, 7.0f, _fort, new Color(1f, 0.55f, 0.12f));
        Block("Z4_v24_Cathedral_Nave_A", new Vector3(0f, 1.05f, z0 + 78f), new Vector3(18f, 1.1f, 18f), _arena, zroot, true, true);
        Block("Z4_v24_Cathedral_Nave_B", new Vector3(0f, 1.05f, z0 + 94f), new Vector3(18f, 1.1f, 18f), _arena, zroot, true, true);
        Block("Z4_v24_Cathedral_Carpet_A", new Vector3(0f, 1.64f, z0 + 86f), new Vector3(5.2f, 0.06f, 34f), _blood, _decorRoot, false, false);
        Block("Z4_v24_Cathedral_Gold_Line_L", new Vector3(-3.05f, 1.70f, z0 + 86f), new Vector3(0.22f, 0.06f, 34f), _gold, _decorRoot, false, false);
        Block("Z4_v24_Cathedral_Gold_Line_R", new Vector3(3.05f, 1.70f, z0 + 86f), new Vector3(0.22f, 0.06f, 34f), _gold, _decorRoot, false, false);

        for (int i = 0; i < 5; i++)
        {
            float z = z0 + 73f + i * 7.2f;
            TallCathedralRib("Z4_v24_Cathedral_Rib_" + i, z);
            HangingBanner(new Vector3(-8.2f, 6.3f, z + 1f), false);
            HangingBanner(new Vector3(8.2f, 6.3f, z + 1f), true);
            RitualBowl(new Vector3(-8.6f, 1.6f, z));
            RitualBowl(new Vector3(8.6f, 1.6f, z));
        }
        LightAt("Z4_v24_Cathedral_Gold_Fill", new Vector3(0f, 9.5f, z0 + 88f), new Color(1f, 0.64f, 0.16f), 2.0f, 54f);

        // ── ЧАСТЬ 4: БОЛЬШАЯ АРЕНА БОССА И ТРОН КАК ФОН ──────────────
        Stairs("Z4_v24_Boss_Arena_Stairs", new Vector3(0f, 1.05f, z0 + 101f), 8, 11.5f, 0.28f, 1.05f, _fort, zroot);
        Block("Z4_v24_Boss_Arena_Main", new Vector3(0f, 3.05f, z0 + 120f), new Vector3(50f, 1.15f, 40f), _arena, zroot, true, true);

        // Внешнее кольцо — приподнятые боковые площадки
        Block("Z4_BA_Side_Platform_L", new Vector3(-28f, 3.55f, z0 + 120f), new Vector3(6f, 0.65f, 36f), _fort, zroot, true, true);
        Block("Z4_BA_Side_Platform_R", new Vector3(28f, 3.55f, z0 + 120f), new Vector3(6f, 0.65f, 36f), _fort, zroot, true, true);
        Block("Z4_BA_Back_Platform", new Vector3(0f, 3.55f, z0 + 142f), new Vector3(50f, 0.65f, 4f), _fort, zroot, true, true);
        // Ступени на боковые площадки
        Stairs("Z4_BA_Stairs_L", new Vector3(-25.5f, 3.05f, z0 + 108f), 3, 4f, 0.18f, 1.2f, _fort, zroot);
        Stairs("Z4_BA_Stairs_R", new Vector3(25.5f, 3.05f, z0 + 108f), 3, 4f, 0.18f, 1.2f, _fort, zroot);

        // Центральный подиум босса — 3 ступени
        Block("Z4_BA_Podium_Step1", new Vector3(0f, 3.72f, z0 + 120f), new Vector3(14f, 0.22f, 14f), _fort, zroot, true, true);
        Block("Z4_BA_Podium_Step2", new Vector3(0f, 3.98f, z0 + 120f), new Vector3(10f, 0.22f, 10f), _fort, zroot, true, true);
        Block("Z4_BA_Podium_Step3", new Vector3(0f, 4.22f, z0 + 120f), new Vector3(7f, 0.22f, 7f), _arena, zroot, true, true);

        // Ковёр и золотые линии
        Block("Z4_v24_Boss_Arena_Carpet", new Vector3(0f, 3.66f, z0 + 115f), new Vector3(8f, 0.06f, 30f), _blood, _decorRoot, false, false);
        Block("Z4_v24_Boss_Arena_Gold_Line_L", new Vector3(-4.2f, 3.72f, z0 + 115f), new Vector3(0.24f, 0.06f, 30f), _gold, _decorRoot, false, false);
        Block("Z4_v24_Boss_Arena_Gold_Line_R", new Vector3(4.2f, 3.72f, z0 + 115f), new Vector3(0.24f, 0.06f, 30f), _gold, _decorRoot, false, false);

        // Руническая окружность вокруг подиума
        BossAuraRing(new Vector3(0f, 3.82f, z0 + 118f));
        BossAuraRing(new Vector3(0f, 4.0f, z0 + 122f));
        V24RunicCircle(new Vector3(0f, 3.84f, z0 + 120f));

        // 6 обелисков по кругу
        for (int i = 0; i < 6; i++)
        {
            float ang = i * Mathf.PI * 2f / 6f;
            float x = Mathf.Cos(ang) * 16f;
            float z = Mathf.Sin(ang) * 12f + z0 + 120f;
            Column("Z4_v24_Boss_Obelisk_" + i, new Vector3(x, 3.65f, z), 7.4f, 0.55f, _fort, _decorRoot, i % 2 == 0);
            RelicCrystal(new Vector3(x, 11.35f, z), i % 2 == 0 ? new Color(1f, 0.22f, 0.05f) : new Color(1f, 0.75f, 0.12f));
            if (i % 2 == 0) CoverWall("Z4_v24_Boss_Cover_" + i, new Vector3(x * 0.85f, 3.9f, z), 14f, 3.2f);
        }

        // 4 больших угловых тотема с огнём
        float arenaLeft = -23f, arenaRight = 23f;
        float arenaFront = z0 + 103f, arenaBack = z0 + 139f;
        Vector3[] cornerPos = {
            new Vector3(arenaLeft, 3.65f, arenaFront),
            new Vector3(arenaRight, 3.65f, arenaFront),
            new Vector3(arenaLeft, 3.65f, arenaBack),
            new Vector3(arenaRight, 3.65f, arenaBack)
        };
        for (int i = 0; i < 4; i++)
        {
            var cp = cornerPos[i];
            Block("Z4_BA_CornerTotem_Base_" + i, cp + new Vector3(0f, 0.5f, 0f), new Vector3(2.0f, 1.0f, 2.0f), _fort, _decorRoot, true, true);
            Column("Z4_BA_CornerTotem_" + i, cp + new Vector3(0f, 0.8f, 0f), 9.5f, 0.7f, _fort, _decorRoot, true);
            // Черепа на тотемах
            VisualBlock("Z4_BA_TotemSkull_" + i, cp + new Vector3(0f, 5.5f, 0.6f), new Vector3(0.5f, 0.5f, 0.42f),
                MakeMat("M_BA_Bone_" + i, new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f));
            Fire(cp + new Vector3(0f, 10.5f, 0f));
            LightAt("Z4_BA_CornerLight_" + i, cp + new Vector3(0f, 11f, 0f), new Color(1f, 0.45f, 0.08f), 2.0f, 18f);
        }

        // 8 костровых чаш по периметру арены
        for (int i = 0; i < 8; i++)
        {
            float ang2 = i * Mathf.PI * 2f / 8f;
            float bx = Mathf.Cos(ang2) * 21f;
            float bz = Mathf.Sin(ang2) * 16f + z0 + 120f;
            RitualBowl(new Vector3(bx, 3.65f, bz));
        }

        // Дополнительные укрытия — разбитые каменные блоки по краям
        CoverWall("Z4_BA_Cover_FrontL", new Vector3(-15f, 3.9f, z0 + 108f), 25f, 2.8f);
        CoverWall("Z4_BA_Cover_FrontR", new Vector3(15f, 3.9f, z0 + 108f), -25f, 2.8f);
        CoverWall("Z4_BA_Cover_MidL", new Vector3(-18f, 3.9f, z0 + 125f), 0f, 3.0f);
        CoverWall("Z4_BA_Cover_MidR", new Vector3(18f, 3.9f, z0 + 125f), 0f, 3.0f);
        Block("Z4_BA_Rubble_1", new Vector3(-20f, 3.9f, z0 + 115f), new Vector3(2.2f, 1.5f, 1.8f), _fort, _decorRoot, true, true);
        Block("Z4_BA_Rubble_2", new Vector3(20f, 3.9f, z0 + 130f), new Vector3(1.8f, 1.2f, 2.2f), _fort, _decorRoot, true, true);
        Block("Z4_BA_Rubble_3", new Vector3(-12f, 3.9f, z0 + 135f), new Vector3(2.5f, 1.0f, 1.5f), _fort, _decorRoot, true, true);

        // Висящие баннеры вдоль арены
        for (int i = 0; i < 4; i++)
        {
            float bz2 = z0 + 106f + i * 10f;
            HangingBanner(new Vector3(-24.5f, 7.0f, bz2), false);
            HangingBanner(new Vector3(24.5f, 7.0f, bz2), true);
        }

        // Цепи свисают с потолка по бокам
        for (int i = 0; i < 4; i++)
        {
            float cz = z0 + 108f + i * 9f;
            ForgeChain(new Vector3(-22f, 7.5f, cz));
            ForgeChain(new Vector3(22f, 7.5f, cz + 3f));
        }

        // Руны на полу вокруг подиума — пульсирующие
        var runeBossMat = MakeMat("M_BA_Rune", new Color(0.9f, 0.15f, 0.03f), new Color(1f, 0.2f, 0.05f), true, new Color(1f, 0.2f, 0.04f), 0.9f);
        for (int i = 0; i < 12; i++)
        {
            float rang = i * 30f;
            float rr = 9f;
            Vector3 rp = new Vector3(
                Mathf.Cos(rang * Mathf.Deg2Rad) * rr,
                3.68f,
                Mathf.Sin(rang * Mathf.Deg2Rad) * rr + z0 + 120f);
            var runeObj = VisualBlock("Z4_BA_Rune_" + i, rp, new Vector3(0.2f, 0.04f, 1.1f), runeBossMat);
            runeObj.transform.rotation = Quaternion.Euler(0f, -rang, 0f);
            runeObj.AddComponent<LavaPulse>().pulseScale = 0.025f;
        }

        // Трон — гигантский фон арены босса.
        Block("Z4_v24_Throne_Podium", new Vector3(0f, 5.25f, z0 + 145f), new Vector3(15f, 1.0f, 9f), _gold, zroot, true, true);
        Stairs("Z4_BA_Throne_Stairs", new Vector3(0f, 3.65f, z0 + 141f), 5, 8f, 0.32f, 1.0f, _gold, zroot);
        Vector3 seat = new Vector3(0f, 5.95f, z0 + 147f);
        Block("Z4_v24_Throne_Seat", seat, new Vector3(5f, 0.65f, 3.8f), _gold, _decorRoot, true, true);
        Block("Z4_v24_Throne_High_Back", seat + new Vector3(0f, 3.1f, 1.65f), new Vector3(5.4f, 5.9f, 0.7f), _gold, _decorRoot, true, true);
        Block("Z4_v24_Throne_Arm_L", seat + new Vector3(-2.65f, 1.0f, 0f), new Vector3(0.7f, 1.8f, 3.4f), _gold, _decorRoot, true, true);
        Block("Z4_v24_Throne_Arm_R", seat + new Vector3(2.65f, 1.0f, 0f), new Vector3(0.7f, 1.8f, 3.4f), _gold, _decorRoot, true, true);
        Block("Z4_v24_Throne_Crown", seat + new Vector3(0f, 6.25f, 1.65f), new Vector3(6.5f, 0.75f, 0.85f), _gold, _decorRoot, true, true);
        // Тотемы по бокам трона
        Column("Z4_BA_Throne_Totem_L", seat + new Vector3(-5.5f, -0.3f, 1.0f), 8f, 0.6f, _fort, _decorRoot, true);
        Column("Z4_BA_Throne_Totem_R", seat + new Vector3(5.5f, -0.3f, 1.0f), 8f, 0.6f, _fort, _decorRoot, true);
        Fire(seat + new Vector3(-5.5f, 8.0f, 1.0f));
        Fire(seat + new Vector3(5.5f, 8.0f, 1.0f));

        // Задняя стена — руины + арка
        Block("Z4_v24_Back_Ruin_L", new Vector3(-8f, 8.3f, z0 + 136f), new Vector3(2.4f, 13f, 0.9f), _fort, _decorRoot, true, true);
        Block("Z4_v24_Back_Ruin_R", new Vector3(8f, 8.3f, z0 + 136f), new Vector3(2.4f, 13f, 0.9f), _fort, _decorRoot, true, true);
        Block("Z4_v24_Back_Ruin_Top", new Vector3(0f, 14.0f, z0 + 136f), new Vector3(13f, 0.7f, 0.9f), _fort, _decorRoot, true, true);
        // Дополнительные руины/колонны по бокам задней стены
        Block("Z4_BA_Back_Wing_L", new Vector3(-14f, 6.5f, z0 + 138f), new Vector3(1.8f, 8f, 0.9f), _fort, _decorRoot, true, true);
        Block("Z4_BA_Back_Wing_R", new Vector3(14f, 6.5f, z0 + 138f), new Vector3(1.8f, 8f, 0.9f), _fort, _decorRoot, true, true);
        // Черепа на стене
        VisualBlock("Z4_BA_WallSkull_L", new Vector3(-8f, 12.5f, z0 + 135.5f), new Vector3(0.8f, 0.8f, 0.6f),
            MakeMat("M_BA_WSkull_L", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f));
        VisualBlock("Z4_BA_WallSkull_R", new Vector3(8f, 12.5f, z0 + 135.5f), new Vector3(0.8f, 0.8f, 0.6f),
            MakeMat("M_BA_WSkull_R", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f));

        // Усиленное освещение арены
        LightAt("Z4_v24_Final_Boss_Red_Aura", new Vector3(0f, 7.6f, z0 + 120f), new Color(1f, 0.12f, 0.02f), 5.5f, 65f);
        LightAt("Z4_v24_Throne_Gold_Backlight", new Vector3(0f, 10.5f, z0 + 140f), new Color(1f, 0.72f, 0.08f), 4.5f, 55f);
        LightAt("Z4_BA_Boss_Podium_Glow", new Vector3(0f, 5.0f, z0 + 120f), new Color(1f, 0.08f, 0.02f), 3.0f, 20f);
        LightAt("Z4_BA_Arena_Side_L", new Vector3(-22f, 6.5f, z0 + 120f), new Color(1f, 0.35f, 0.06f), 1.8f, 25f);
        LightAt("Z4_BA_Arena_Side_R", new Vector3(22f, 6.5f, z0 + 120f), new Color(1f, 0.35f, 0.06f), 1.8f, 25f);
        LightAt("Z4_BA_Arena_Front_Glow", new Vector3(0f, 5.0f, z0 + 105f), new Color(0.8f, 0.15f, 0.03f), 1.5f, 18f);
        V24AshEmitter(new Vector3(0f, 5.0f, z0 + 120f));
        V24AshEmitter(new Vector3(-15f, 5.0f, z0 + 115f));
        V24AshEmitter(new Vector3(15f, 5.0f, z0 + 125f));

        // ── ПРЕДМЕТЫ / ЛОВУШКИ / ВРАГИ ───────────────────────────────
        CoinLine(new Vector3(0f, 1.9f, z0 + 12f), 4, 0f, 5.0f);
        Coin(new Vector3(-2f, 2.8f, z0 + 36f));
        Coin(new Vector3(0f, 3.0f, z0 + 36f));
        Coin(new Vector3(2f, 2.8f, z0 + 36f));
        CoinLine(new Vector3(0f, 1.8f, z0 + 78f), 4, 0f, 6.2f);
        Coin(new Vector3(-3f, 4.0f, z0 + 114f));
        Coin(new Vector3(0f, 4.1f, z0 + 114f));
        Coin(new Vector3(3f, 4.0f, z0 + 114f));
        Coin(new Vector3(0f, 6.9f, z0 + 134f));
        HealthPack(new Vector3(-5.8f, 2.85f, z0 + 34f));
        HealthPack(new Vector3(5.8f, 2.85f, z0 + 39f));
        HealthPack(new Vector3(-5.0f, 1.65f, z0 + 91f));
        HealthPack(new Vector3(5.2f, 3.95f, z0 + 112f));
        HealthPack(new Vector3(0f, 6.0f, z0 + 130f));

        Trap_("Z4_v24_Trap_Forge_Left", new Vector3(-5.5f, 2.55f, z0 + 42f));
        Trap_("Z4_v24_Trap_Forge_Right", new Vector3(5.5f, 2.55f, z0 + 30f));
        Trap_("Z4_v24_Trap_Cathedral_Left", new Vector3(-5.5f, 1.65f, z0 + 87f));
        Trap_("Z4_v24_Trap_Cathedral_Right", new Vector3(5.5f, 1.65f, z0 + 94f));
        Trap_("Z4_v24_Trap_Boss_Left", new Vector3(-7.2f, 3.95f, z0 + 118f));
        Trap_("Z4_v24_Trap_Boss_Right", new Vector3(7.2f, 3.95f, z0 + 110f));

        // Враги расставлены как постановка: вход -> кузня -> собор -> босс.
        var z4e0 = OrcBerserker("Z4_v24_Forge_Entry_Berserker", new Vector3(-4.2f, 1.1f, z0 + 9f),
            new[] { new Vector3(-5.5f, 1.1f, z0 + 6f), new Vector3(4.5f, 1.1f, z0 + 12f) });
        var z4e1 = OrcShieldGuard("Z4_v24_Forge_Entry_Guard", new Vector3(4.4f, 1.1f, z0 + 11f),
            new[] { new Vector3(2.5f, 1.1f, z0 + 7f), new Vector3(6.0f, 1.1f, z0 + 14f) });
        var z4e2 = OrcArcher("Z4_v24_Left_Balcony_Archer", new Vector3(-19.5f, 3.95f, z0 + 25f),
            new[] { new Vector3(-19.5f, 3.95f, z0 + 17f), new Vector3(-19.5f, 3.95f, z0 + 35f) });
        var z4e3 = OrcArcher("Z4_v24_Right_Balcony_Archer", new Vector3(19.5f, 3.95f, z0 + 52f),
            new[] { new Vector3(19.5f, 3.95f, z0 + 44f), new Vector3(19.5f, 3.95f, z0 + 62f) });
        var z4e4 = OrcForgeMaster("Z4_v24_Forge_Master", new Vector3(0f, 3.22f, z0 + 36f),
            new[] { new Vector3(-3f, 3.22f, z0 + 32f), new Vector3(3f, 3.22f, z0 + 40f), new Vector3(0f, 3.22f, z0 + 36f) });

        // Ритуальные ворота кузня → собор (отслеживаем врагов кузни)
        RitualGateWithTotems("RitualGate_Forge_to_Cathedral", new Vector3(0f, 1.0f, z0 + 67f), 12.0f, 6.5f,
            new[] { z4e0, z4e1, z4e2, z4e3, z4e4 });

        OrcShieldGuard("Z4_v24_Cathedral_Guard_L", new Vector3(-4.5f, 1.65f, z0 + 84f),
            new[] { new Vector3(-5f, 1.65f, z0 + 78f), new Vector3(1f, 1.65f, z0 + 91f) });
        OrcShieldGuard("Z4_v24_Cathedral_Guard_R", new Vector3(4.5f, 1.65f, z0 + 92f),
            new[] { new Vector3(5f, 1.65f, z0 + 84f), new Vector3(-1f, 1.65f, z0 + 98f) });
        OrcBerserker("Z4_v24_Cathedral_Berserker", new Vector3(-3f, 1.65f, z0 + 88f),  // засада из колонны
            new[] { new Vector3(-7f, 1.65f, z0 + 85f), new Vector3(4f, 1.65f, z0 + 95f) });
        OrcShaman("Z4_v24_Cathedral_Shaman", new Vector3(0f, 1.65f, z0 + 98f),
            new[] { new Vector3(-3f, 1.65f, z0 + 95f), new Vector3(3f, 1.65f, z0 + 101f) });
        OrcShaman("Z4_v24_Cathedral_Shaman_Stairs", new Vector3(5f, 2.8f, z0 + 104f),  // шаман на ступенях к арене
            new[] { new Vector3(4f, 2.5f, z0 + 102f), new Vector3(6f, 3.0f, z0 + 106f) });
        OrcArcher("Z4_v24_Boss_Left_Balcony_Archer", new Vector3(-10.0f, 4.0f, z0 + 112f),
            new[] { new Vector3(-10f, 4.0f, z0 + 105f), new Vector3(-10f, 4.0f, z0 + 122f) });
        OrcArcher("Z4_v24_Boss_Right_Balcony_Archer", new Vector3(10.0f, 4.0f, z0 + 116f),
            new[] { new Vector3(10f, 4.0f, z0 + 106f), new Vector3(10f, 4.0f, z0 + 124f) });
        OrcWarlord("Z4_v24_Throne_Guard_L", new Vector3(-4.8f, 5.95f, z0 + 130f),
            new[] { new Vector3(-5f, 5.95f, z0 + 128f), new Vector3(-2.5f, 5.95f, z0 + 134f) });
        OrcWarlord("Z4_v24_Throne_Guard_R", new Vector3(4.8f, 5.95f, z0 + 130f),
            new[] { new Vector3(5f, 5.95f, z0 + 128f), new Vector3(2.5f, 5.95f, z0 + 134f) });

        // ── 20 дополнительных орков у арены босса ──
        // Собор — засада (4 орка)
        OrcGrunt("Z4_Cath_Extra_Grunt_L", new Vector3(-7f, 1.65f, z0 + 76f),
            new[] { new Vector3(-4f, 1.65f, z0 + 80f), new Vector3(-9f, 1.65f, z0 + 72f) });
        OrcGrunt("Z4_Cath_Extra_Grunt_R", new Vector3(7f, 1.65f, z0 + 78f),
            new[] { new Vector3(4f, 1.65f, z0 + 82f), new Vector3(9f, 1.65f, z0 + 74f) });
        OrcArcher("Z4_Cath_Extra_Archer_L", new Vector3(-12f, 1.65f, z0 + 82f),
            new[] { new Vector3(-14f, 1.65f, z0 + 78f), new Vector3(-10f, 1.65f, z0 + 86f) });
        OrcBerserker("Z4_Cath_Extra_Berserk", new Vector3(6f, 1.65f, z0 + 74f),
            new[] { new Vector3(3f, 1.65f, z0 + 70f), new Vector3(8f, 1.65f, z0 + 80f) });
        // Ступени к арене (4 орка)
        OrcShieldGuard("Z4_Stairs_Guard_L", new Vector3(-6f, 2.8f, z0 + 102f),
            new[] { new Vector3(-8f, 2.8f, z0 + 99f), new Vector3(-3f, 3.2f, z0 + 106f) });
        OrcShieldGuard("Z4_Stairs_Guard_R", new Vector3(6f, 2.8f, z0 + 103f),
            new[] { new Vector3(8f, 2.8f, z0 + 100f), new Vector3(3f, 3.2f, z0 + 107f) });
        OrcArcher("Z4_Stairs_Archer_Far_L", new Vector3(-14f, 3.95f, z0 + 106f),
            new[] { new Vector3(-16f, 3.95f, z0 + 102f), new Vector3(-12f, 3.95f, z0 + 110f) });
        OrcArcher("Z4_Stairs_Archer_Far_R", new Vector3(14f, 3.95f, z0 + 108f),
            new[] { new Vector3(16f, 3.95f, z0 + 104f), new Vector3(12f, 3.95f, z0 + 112f) });
        // Арена босса — левый фланг (4 орка)
        OrcBerserker("Z4_Arena_Berserk_FL", new Vector3(-14f, 3.95f, z0 + 112f),
            new[] { new Vector3(-10f, 3.95f, z0 + 116f), new Vector3(-16f, 3.95f, z0 + 108f) });
        OrcGrunt("Z4_Arena_Grunt_ML", new Vector3(-12f, 3.95f, z0 + 120f),
            new[] { new Vector3(-8f, 3.95f, z0 + 118f), new Vector3(-14f, 3.95f, z0 + 124f) });
        OrcShaman("Z4_Arena_Shaman_BL", new Vector3(-16f, 3.95f, z0 + 128f),
            new[] { new Vector3(-14f, 3.95f, z0 + 125f), new Vector3(-18f, 3.95f, z0 + 132f) });
        OrcArcher("Z4_Arena_Archer_BL", new Vector3(-10f, 3.95f, z0 + 134f),
            new[] { new Vector3(-12f, 3.95f, z0 + 130f), new Vector3(-8f, 3.95f, z0 + 138f) });
        // Арена босса — правый фланг (4 орка)
        OrcBerserker("Z4_Arena_Berserk_FR", new Vector3(14f, 3.95f, z0 + 114f),
            new[] { new Vector3(10f, 3.95f, z0 + 118f), new Vector3(16f, 3.95f, z0 + 110f) });
        OrcGrunt("Z4_Arena_Grunt_MR", new Vector3(12f, 3.95f, z0 + 122f),
            new[] { new Vector3(8f, 3.95f, z0 + 120f), new Vector3(14f, 3.95f, z0 + 126f) });
        OrcShaman("Z4_Arena_Shaman_BR", new Vector3(16f, 3.95f, z0 + 130f),
            new[] { new Vector3(14f, 3.95f, z0 + 127f), new Vector3(18f, 3.95f, z0 + 134f) });
        OrcArcher("Z4_Arena_Archer_BR", new Vector3(10f, 3.95f, z0 + 136f),
            new[] { new Vector3(12f, 3.95f, z0 + 132f), new Vector3(8f, 3.95f, z0 + 140f) });
        // Арена босса — тыл/центр (4 орка)
        OrcWarlord("Z4_Arena_Warlord_Back", new Vector3(0f, 3.95f, z0 + 138f),
            new[] { new Vector3(-5f, 3.95f, z0 + 135f), new Vector3(5f, 3.95f, z0 + 135f) });
        OrcShieldGuard("Z4_Arena_Guard_Back_L", new Vector3(-6f, 3.95f, z0 + 136f),
            new[] { new Vector3(-8f, 3.95f, z0 + 133f), new Vector3(-3f, 3.95f, z0 + 139f) });
        OrcShieldGuard("Z4_Arena_Guard_Back_R", new Vector3(6f, 3.95f, z0 + 136f),
            new[] { new Vector3(8f, 3.95f, z0 + 133f), new Vector3(3f, 3.95f, z0 + 139f) });
        OrcForgeMaster("Z4_Arena_ForgeMaster_Rear", new Vector3(0f, 3.95f, z0 + 132f),
            new[] { new Vector3(-4f, 3.95f, z0 + 128f), new Vector3(4f, 3.95f, z0 + 136f), new Vector3(0f, 3.95f, z0 + 130f) });

        var boss = OrcFinalBoss("FINAL_BOSS_ORC_WARCHIEF_v24_MERGED", new Vector3(0f, 3.95f, z0 + 120f),
            new[] { new Vector3(-12f, 3.95f, z0 + 105f), new Vector3(12f, 3.95f, z0 + 105f), new Vector3(0f, 3.95f, z0 + 135f), new Vector3(-8f, 3.95f, z0 + 120f), new Vector3(8f, 3.95f, z0 + 120f), new Vector3(0f, 3.95f, z0 + 120f) });
        if (boss != null) AddV24MergedBossPresentation(boss.transform);

        // BossArenaController — триггер и запирание арены босса
        var bossArenaObj = new GameObject("Z4_BossArenaController");
        bossArenaObj.transform.position = new Vector3(0f, 3.5f, z0 + 100f);
        bossArenaObj.transform.SetParent(_decorRoot);
        var bossArenaBox = bossArenaObj.AddComponent<BoxCollider>();
        bossArenaBox.size = new Vector3(20f, 8f, 4f);
        bossArenaBox.isTrigger = true;
        IgnoreNavMesh(bossArenaObj);

        var bossBarrier = Block("Z4_BossArena_Entrance_Barrier", new Vector3(0f, 5.0f, z0 + 100f),
            new Vector3(14f, 8f, 0.5f), _dark, _decorRoot, true, true);
        bossBarrier.SetActive(false);
        IgnoreNavMesh(bossBarrier);

        var bac = bossArenaObj.AddComponent<BossArenaController>();
        bac.boss = boss != null ? boss.GetComponent<EnemyAI>() : null;
        bac.entranceBarrier = bossBarrier;
        bac.enemiesRoot = _enemiesRoot;
        bac.waypointsRoot = _waypointsRoot;
        // Phase3Wave вычисляет позиции сама от transform; эти поля — fallback
        bac.minionSpawnPoints = new[] {
            new Vector3(-16f, 3.95f, z0 + 108f),
            new Vector3( 16f, 3.95f, z0 + 108f),
            new Vector3(-16f, 3.95f, z0 + 130f),
            new Vector3( 16f, 3.95f, z0 + 130f),
        };
        bac.archerSpawnPoints = new[] {
            new Vector3(-10f, 3.95f, z0 + 137f),
            new Vector3( 10f, 3.95f, z0 + 137f),
        };

        ZoneSign("ЗОНА 4 — КУЗНЯ И ТРОННЫЙ СОБОР", new Vector3(0f, 4.4f, z0 + 5f), new Color(1f, 0.45f, 0.08f));

        // ── ДЕКОРАЦИИ ЗОНЫ 4: КУЗНЯ И СОБОР ──────────────────────────
        var z4bone = MakeMat("M_Z4_Decor_Bone", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var z4pole = MakeMat("M_Z4_Decor_Pole", new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);
        // Черепа на палках вдоль входной платформы кузни
        VisualBlock("Z4_SkullPole_Entry_L_pole", new Vector3(-10f, 1.8f, z0 + 5f), new Vector3(0.15f, 2.6f, 0.15f), z4pole);
        VisualBlock("Z4_SkullPole_Entry_L_skull", new Vector3(-10f, 3.3f, z0 + 5f), new Vector3(0.45f, 0.45f, 0.38f), z4bone);
        VisualBlock("Z4_SkullPole_Entry_R_pole", new Vector3(10f, 1.8f, z0 + 10f), new Vector3(0.15f, 2.6f, 0.15f), z4pole);
        VisualBlock("Z4_SkullPole_Entry_R_skull", new Vector3(10f, 3.3f, z0 + 10f), new Vector3(0.45f, 0.45f, 0.38f), z4bone);
        // Флаги вдоль боковых обходов
        VisualBlock("Z4_SideFlag_L1", new Vector3(-19.5f, 2.5f, z0 + 28f), new Vector3(0.8f, 1.8f, 0.06f), _blood);
        VisualBlock("Z4_SideFlag_L2", new Vector3(-19.5f, 2.5f, z0 + 48f), new Vector3(0.8f, 1.8f, 0.06f), _blood);
        VisualBlock("Z4_SideFlag_R1", new Vector3(19.5f, 2.5f, z0 + 32f), new Vector3(0.8f, 1.8f, 0.06f), _blood);
        VisualBlock("Z4_SideFlag_R2", new Vector3(19.5f, 2.5f, z0 + 52f), new Vector3(0.8f, 1.8f, 0.06f), _blood);
        // Черепа и знаки вдоль маршрута в соборе
        VisualBlock("Z4_CathSkull_L", new Vector3(-8.2f, 3.6f, z0 + 80f), new Vector3(0.38f, 0.38f, 0.3f), z4bone);
        VisualBlock("Z4_CathSkull_R", new Vector3(8.2f, 3.6f, z0 + 90f), new Vector3(0.38f, 0.38f, 0.3f), z4bone);
        // Ритуальные чаши дополнительные у входа
        RitualBowl(new Vector3(-10f, 0.55f, z0 + 12f));
        RitualBowl(new Vector3(10f, 0.55f, z0 + 12f));
        // Факелы у собора
        Fire(new Vector3(-8.5f, 1.65f, z0 + 73f));
        Fire(new Vector3(8.5f, 1.65f, z0 + 73f));
    }


    void Zone4ForgeTower(string name, Vector3 basePos)
    {
        // v24/v26: башни кузни для объединённой зоны 4.
        // Это только геометрия, свет и атмосфера. Игровую логику не трогаем.
        var root = NewChild(_decorRoot, name);

        Block(name + "_Foundation",
            basePos + new Vector3(0f, 0.22f, 0f),
            new Vector3(4.9f, 0.44f, 4.9f),
            _arena, root, true, true);

        Block(name + "_Lower_Body",
            basePos + new Vector3(0f, 2.05f, 0f),
            new Vector3(3.25f, 4.1f, 3.25f),
            _fort, root, true, true);

        Block(name + "_Upper_Body",
            basePos + new Vector3(0f, 5.15f, 0f),
            new Vector3(2.35f, 2.2f, 2.35f),
            _arena, root, true, true);

        Block(name + "_Top_Plate",
            basePos + new Vector3(0f, 6.45f, 0f),
            new Vector3(4.25f, 0.42f, 4.25f),
            _fort, root, true, true);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                Block(name + "_Merlon_" + x + "_" + z,
                    basePos + new Vector3(x * 1.45f, 7.05f, z * 1.45f),
                    new Vector3(0.62f, 0.92f, 0.62f),
                    _fort, root, true, true);
            }
        }

        // Диагональные металлические подпорки — дают ощущение настоящей кузни.
        var braceL = Block(name + "_Brace_L",
            basePos + new Vector3(-1.85f, 3.35f, 0f),
            new Vector3(0.28f, 4.4f, 0.28f),
            _dark, root, true, true);
        braceL.transform.rotation = Quaternion.Euler(0f, 0f, 18f);

        var braceR = Block(name + "_Brace_R",
            basePos + new Vector3(1.85f, 3.35f, 0f),
            new Vector3(0.28f, 4.4f, 0.28f),
            _dark, root, true, true);
        braceR.transform.rotation = Quaternion.Euler(0f, 0f, -18f);

        Fire(basePos + new Vector3(0f, 7.25f, 0f));
        LightAt(name + "_Forge_Tower_Glow",
            basePos + new Vector3(0f, 5.35f, 0f),
            new Color(1f, 0.32f, 0.06f),
            1.65f,
            15f);
    }

    void Zone4BridgeSupports(string name, Vector3 center, int count, float spanX, float spanZ)
    {
        // v24/v26: опоры лавовых мостов. Только визуальная опора, путь игрока не перекрывается.
        var root = NewChild(_decorRoot, name);
        int safeCount = Mathf.Max(2, count);

        for (int i = 0; i < safeCount; i++)
        {
            float t = i / (float)(safeCount - 1);
            float z = center.z - spanZ * 0.5f + spanZ * t;

            Block(name + "_CrossBeam_" + i,
                new Vector3(center.x, center.y + 0.72f, z),
                new Vector3(spanX + 1.35f, 0.24f, 0.42f),
                _fort, root, true, true);

            var left = Block(name + "_LeftPillar_" + i,
                new Vector3(center.x - spanX * 0.5f, center.y - 1.15f, z),
                new Vector3(0.42f, 3.15f, 0.42f),
                _arena, root, true, true);
            left.transform.rotation = Quaternion.Euler(0f, 0f, 7f);

            var right = Block(name + "_RightPillar_" + i,
                new Vector3(center.x + spanX * 0.5f, center.y - 1.15f, z),
                new Vector3(0.42f, 3.15f, 0.42f),
                _arena, root, true, true);
            right.transform.rotation = Quaternion.Euler(0f, 0f, -7f);

            if (i % 2 == 0)
            {
                LightAt(name + "_Low_Lava_Glow_" + i,
                    new Vector3(center.x, center.y - 0.92f, z),
                    new Color(1f, 0.18f, 0.03f),
                    0.75f,
                    9f);
            }
        }
    }

    void BuildZone5_ThroneRoom()
    {
        // v24: отдельной зоны 5 больше нет.
        // Финал встроен в большой объединённый уровень 4: кузня -> тронный собор -> босс.
    }


    void AddV26LongTransitionPolish()
    {
        // v26: безопасная полировка переходов между 4 зонами.
        // Не удаляет существующие зоны, не трогает врагов/босса/лук/ловушки/смерть/рестарт.
        var root = NewChild(_decorRoot, "V26_Long_Bridges_RestPlatforms_LightFX");

        // Между зоной 1 и крепостью — деревянно-каменный мост после оврага.
        V26RestPlatform("V26_Rest_After_Hills", new Vector3(0f, 0.18f, 39.8f), _dirt, new Color(0.38f, 0.85f, 0.26f));
        V26BridgeRailsAndLamps("V26_Hills_To_Fort_Bridge", 39.0f, 45.0f, 4.6f, 0.65f, _wood, root);

        // Между крепостью и храмом — длинный каменный переход с паузой перед храмом.
        V26RestPlatform("V26_Rest_After_Fort", new Vector3(0f, 0.2f, 91.8f), _fort, new Color(1f, 0.55f, 0.14f));
        V26BridgeRailsAndLamps("V26_Fort_To_Temple_Ceremony_Bridge", 88.5f, 97.0f, 5.2f, 0.78f, _fort, root);

        // Между храмом и финальной кузней — главный драматичный мост над тьмой/лавой.
        VisualBlock("V26_Final_Approach_Abyss_Depth", new Vector3(0f, -3.35f, 148.2f), new Vector3(24f, 4.4f, 17f), _dark);
        V26RestPlatform("V26_Rest_Before_Final_Forge", new Vector3(0f, 2.78f, 144.6f), _temple, new Color(1f, 0.78f, 0.16f));
        V26BridgeRailsAndLamps("V26_Temple_To_Final_Forge_Long_Bridge", 143.0f, 153.4f, 6.4f, 3.02f, _temple, root);
        V26SuspendedChains("V26_Final_Bridge_Chains", 148.0f, 10, 3.85f, 6.1f, root);

        // Внутри финальной зоны — промежуточная площадка отдыха между кузней и собором.
        V26RestPlatform("V26_Rest_Inside_Final_Forge", new Vector3(0f, 2.85f, 206f), _arena, new Color(1f, 0.22f, 0.04f));
        V26BridgeRailsAndLamps("V26_Forge_To_Cathedral_Internal_Bridge", 205.5f, 217.2f, 6.1f, 2.95f, _arena, root);

        // Мягкие атмосферные частицы/свет на переходах.
        V26LightMist("V26_Green_Mist_Hills", new Vector3(0f, 0.8f, 40f), new Color(0.32f, 0.9f, 0.26f, 0.18f), 6f);
        V26LightMist("V26_Gold_Mist_Temple", new Vector3(0f, 2.9f, 144f), new Color(1f, 0.75f, 0.15f, 0.16f), 7f);
        V26LightMist("V26_Red_Mist_Final", new Vector3(0f, 3.0f, 210f), new Color(1f, 0.20f, 0.04f, 0.20f), 8f);
    }

    void V26BridgeRailsAndLamps(string name, float zFrom, float zTo, float width, float y, Material mat, Transform parent)
    {
        float len = zTo - zFrom;
        float mid = (zFrom + zTo) * 0.5f;

        Block(name + "_Extra_Walkable_Spine",
            new Vector3(0f, y - 0.16f, mid),
            new Vector3(width, 0.28f, len),
            mat, _walkRoot, true, true);

        Block(name + "_Gold_Guide_Line",
            new Vector3(0f, y + 0.02f, mid),
            new Vector3(0.72f, 0.05f, len * 0.84f),
            _gold, parent, false, false);

        SideWallPair(name + "_Side_Rails", width * 0.58f, y + 0.55f, mid, len, _wood, parent);

        int lamps = Mathf.Max(2, Mathf.RoundToInt(len / 2.4f));
        for (int i = 0; i <= lamps; i++)
        {
            float z = Mathf.Lerp(zFrom + 0.75f, zTo - 0.75f, i / (float)lamps);
            if (i % 2 == 0)
            {
                Fire(new Vector3(-width * 0.72f, y + 0.38f, z));
                Fire(new Vector3(width * 0.72f, y + 0.38f, z));
            }
            else
            {
                Block(name + "_Small_Stone_L_" + i, new Vector3(-width * 0.72f, y + 0.12f, z), new Vector3(0.55f, 0.28f, 0.55f), mat, parent, true, true);
                Block(name + "_Small_Stone_R_" + i, new Vector3(width * 0.72f, y + 0.12f, z), new Vector3(0.55f, 0.28f, 0.55f), mat, parent, true, true);
            }
        }
    }

    void V26RestPlatform(string name, Vector3 pos, Material floorMat, Color lightColor)
    {
        var root = NewChild(_walkRoot, name);
        Block(name + "_Octagon_Base_A", pos, new Vector3(9.5f, 0.42f, 6.0f), floorMat, root, true, true);
        Block(name + "_Octagon_Base_B", pos + new Vector3(0f, 0.03f, 0f), new Vector3(6.0f, 0.48f, 9.5f), floorMat, root, true, true);
        Block(name + "_Rune_Center", pos + new Vector3(0f, 0.30f, 0f), new Vector3(3.2f, 0.05f, 3.2f), _gold, _decorRoot, false, false);
        Block(name + "_Bench_L", pos + new Vector3(-3.8f, 0.55f, 0f), new Vector3(0.55f, 0.45f, 3.4f), _wood, _decorRoot, true, true);
        Block(name + "_Bench_R", pos + new Vector3(3.8f, 0.55f, 0f), new Vector3(0.55f, 0.45f, 3.4f), _wood, _decorRoot, true, true);
        Fire(pos + new Vector3(-4.7f, 0.42f, -2.8f));
        Fire(pos + new Vector3(4.7f, 0.42f, 2.8f));
        LightAt(name + "_Soft_Rest_Light", pos + new Vector3(0f, 3.1f, 0f), lightColor, 1.55f, 18f);
    }

    void V26SuspendedChains(string name, float centerZ, int count, float xHalf, float topY, Transform parent)
    {
        for (int i = 0; i < count; i++)
        {
            float z = centerZ - 5.0f + i * 1.1f;
            Block(name + "_Chain_L_" + i, new Vector3(-xHalf, topY - 1.3f, z), new Vector3(0.16f, 2.6f, 0.16f), _dark, parent, false, false);
            Block(name + "_Chain_R_" + i, new Vector3(xHalf, topY - 1.3f, z), new Vector3(0.16f, 2.6f, 0.16f), _dark, parent, false, false);
            if (i % 3 == 0)
            {
                Block(name + "_Overhead_Beam_" + i, new Vector3(0f, topY, z), new Vector3(xHalf * 2.25f, 0.18f, 0.18f), _dark, parent, false, false);
            }
        }
    }

    void V26LightMist(string name, Vector3 pos, Color color, float radius)
    {
        var root = new GameObject(name);
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);

        var ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 2.8f;
        main.startSpeed = 0.28f;
        main.startSize = 0.16f;
        main.maxParticles = 36;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new ParticleSystem.MinMaxGradient(color, new Color(color.r, color.g, color.b, color.a * 0.45f));

        var emission = ps.emission;
        emission.rateOverTime = 7f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        if (rend != null)
        {
            // URP/Lit на ParticleSystemRenderer рендерит эмиттер как гигантский твёрдый шар.
            // Используем только шейдеры для частиц.
            string[] particleShaders = {
                "Universal Render Pipeline/Particles/Unlit",
                "Particles/Standard Unlit",
                "Legacy Shaders/Particles/Alpha Blended",
                "Sprites/Default",
            };
            Shader ps2 = null;
            foreach (var sn in particleShaders)
            {
                ps2 = Shader.Find(sn);
                if (ps2 != null) break;
            }
            if (ps2 != null)
            {
                var pm = new Material(ps2);
                pm.SetColor("_BaseColor", color);
                pm.color = color;
                rend.material = pm;
            }
        }
    }

    void BuildMainRouteConnectors()
    {
        var r = NewChild(_walkRoot, "00_V24_ROUTE_CONNECTORS_4_LEVELS");

        // v24: теперь всего 4 уровня. Последний переход ведёт не в отдельную зону 5,
        // а в большой объединённый финальный уровень 4.
        ModularCauseway("V24_Z1_To_Fort_Causeway", 38.2f, 44.2f, 7.8f, -0.32f, _dirt, _wood, r);
        TransitionArch("V24_Fort_Outer_Arch", new Vector3(0f, 0f, 45.0f), 8.5f, 4.8f, _fort, new Color(1f, 0.48f, 0.10f));

        ModularCauseway("V24_Fort_To_Temple_StoneCorridor", 89.2f, 96.2f, 8.8f, -0.40f, _fort, _temple, r);
        TransitionArch("V24_Temple_Entrance_Arch", new Vector3(0f, 0f, 96.2f), 9.2f, 5.2f, _temple, new Color(1f, 0.76f, 0.18f));

        TransitionStairs("V24_Temple_To_FinalForge_Descent", new Vector3(0f, 143.0f, 0f), 9, 10.5f, 2.95f, 0.88f, 9.6f, _temple, r);
        SideWallPair("V24_Temple_To_FinalForge_Borders", 5.8f, 1.45f, 147.8f, 10.5f, _fort, r);
        TransitionArch("V24_FinalForge_Entry_Arch", new Vector3(0f, 0f, 151.6f), 10.2f, 5.5f, _arena, new Color(1f, 0.20f, 0.04f));

        // Визуальные ориентиры по 4-уровневой структуре.
        GuideBeacon("GO_TO_BRIDGE", new Vector3(0f, 1.0f, 31.5f), new Color(1f, 0.86f, 0.18f));
        GuideBeacon("GO_TO_OPEN_GATE", new Vector3(0f, 1.1f, 54.5f), new Color(1f, 0.54f, 0.12f));
        GuideBeacon("GO_TO_TEMPLE_STAIRS", new Vector3(0f, 1.0f, 111.0f), new Color(1f, 0.78f, 0.18f));
        GuideBeacon("GO_TO_FINAL_FORGE", new Vector3(0f, 1.2f, 151.5f), new Color(1f, 0.20f, 0.05f));
        GuideBeacon("GO_TO_FINAL_BOSS", new Vector3(0f, 3.8f, 260.0f), new Color(1f, 0.80f, 0.16f));

        // ── ДЕКОРАЦИИ ПЕРЕХОДОВ ──────────────────────────────────────
        var transBone = MakeMat("M_Trans_Bone", new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var transPole = MakeMat("M_Trans_Pole", new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);

        // Переход Z1→Z2: черепа-предупреждения по бокам дорожки
        VisualBlock("Trans_Z1Z2_SkullPole_L", new Vector3(-3.5f, 0.9f, 41f), new Vector3(0.12f, 1.8f, 0.12f), transPole);
        VisualBlock("Trans_Z1Z2_Skull_L", new Vector3(-3.5f, 2.0f, 41f), new Vector3(0.38f, 0.38f, 0.32f), transBone);
        VisualBlock("Trans_Z1Z2_SkullPole_R", new Vector3(3.5f, 0.9f, 41f), new Vector3(0.12f, 1.8f, 0.12f), transPole);
        VisualBlock("Trans_Z1Z2_Skull_R", new Vector3(3.5f, 2.0f, 41f), new Vector3(0.38f, 0.38f, 0.32f), transBone);

        // Переход Z2→Z3: факелы и флаги у коридора
        Fire(new Vector3(-4.0f, 0.1f, 91f));
        Fire(new Vector3(4.0f, 0.1f, 91f));
        VisualBlock("Trans_Z2Z3_Flag_L", new Vector3(-4.2f, 2.5f, 93f), new Vector3(0.7f, 1.5f, 0.05f), _blood);
        VisualBlock("Trans_Z2Z3_Flag_R", new Vector3(4.2f, 2.5f, 93f), new Vector3(0.7f, 1.5f, 0.05f), _blood);

        // Переход Z3→Z4: ритуальные знаки на земле перед спуском в кузню
        var transRune = MakeMat("M_Trans_Rune", new Color(0.9f, 0.12f, 0.03f), new Color(1f, 0.18f, 0.05f), true, new Color(1f, 0.15f, 0.03f), 0.6f);
        for (int i = 0; i < 4; i++)
        {
            float rx = Mathf.Lerp(-3f, 3f, i / 3f);
            var tr = VisualBlock("Trans_Z3Z4_Rune_" + i, new Vector3(rx, 0.06f, 147f + i * 0.5f), new Vector3(0.15f, 0.03f, 0.7f), transRune);
            tr.transform.rotation = Quaternion.Euler(0f, i * 22f, 0f);
        }
        Fire(new Vector3(-5f, 0.1f, 148f));
        Fire(new Vector3(5f, 0.1f, 148f));
    }

    void AddV24Zone3ExtraSpace()
    {
        // Небольшое сохранение удачного фидбека v21: зона 3 получает больше места,
        // но без старых наложений и без вмешательства в финальный уровень.
        RemoveObjectsContaining("Z3_v18_Left_Deep_Cliff", "Z3_v18_Right_Deep_Cliff", "Z3_v18_Left_Abyss_Glow", "Z3_v18_Right_Abyss_Glow");
        VisualBlock("Z3_v24_Left_Cliff_Pushed_Out", new Vector3(-27f, -3.4f, 124f), new Vector3(13f, 6f, 74f), _dark);
        VisualBlock("Z3_v24_Right_Cliff_Pushed_Out", new Vector3(27f, -3.4f, 124f), new Vector3(13f, 6f, 74f), _dark);
        var z3 = NewChild(_walkRoot, "V24_Zone3_Extra_Space_No_Overlap");
        Block("Z3_v24_Lower_Left_Wing", new Vector3(-13f, -0.5f, 108f), new Vector3(8f, 1f, 22f), _temple, z3, true, true);
        Block("Z3_v24_Lower_Right_Wing", new Vector3(13f, -0.5f, 108f), new Vector3(8f, 1f, 22f), _temple, z3, true, true);
        Block("Z3_v24_Upper_Left_Combat_Wing", new Vector3(-16f, 2.35f, 135f), new Vector3(7f, 1f, 20f), _temple, z3, true, true);
        Block("Z3_v24_Upper_Right_Combat_Wing", new Vector3(16f, 2.35f, 135f), new Vector3(7f, 1f, 20f), _temple, z3, true, true);
        CoverWall("Z3_v24_Lower_Cover_L", new Vector3(-11.5f, 0.2f, 112f), 14f, 3.8f);
        CoverWall("Z3_v24_Lower_Cover_R", new Vector3(11.5f, 0.2f, 118f), -14f, 3.8f);
        HealthPack(new Vector3(-15.5f, 3.25f, 136f));
        Coin(new Vector3(15.5f, 3.25f, 136f));
    }

    void V24RunicCircle(Vector3 pos)
    {
        var red = MakeMat("M_V24_Rune_Red", new Color(0.7f, 0.03f, 0.02f), new Color(1f, 0.16f, 0.04f), true, new Color(1f, 0.08f, 0.02f), 1.1f);
        var gold = MakeMat("M_V24_Rune_Gold", new Color(0.85f, 0.55f, 0.05f), new Color(1f, 0.82f, 0.12f), true, new Color(1f, 0.55f, 0.04f), 0.7f);
        for (int i = 0; i < 12; i++)
        {
            float ang = i * 30f;
            float rad = (i % 2 == 0) ? 5.2f : 6.8f;
            Vector3 p = pos + new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad) * rad, 0f, Mathf.Sin(ang * Mathf.Deg2Rad) * rad);
            var r = Block("V24_Rune_Mark_" + i, p, new Vector3(0.22f, 0.05f, 1.25f), i % 2 == 0 ? red : gold, _decorRoot, false, false);
            r.transform.rotation = Quaternion.Euler(0f, -ang, 0f);
            r.AddComponent<LavaPulse>().pulseScale = 0.025f;
        }
    }

    void V24AshEmitter(Vector3 pos)
    {
        var root = new GameObject("V24_Final_Ash_And_Embers_Lightweight");
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);
        var ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 3.2f;
        main.startSpeed = 0.55f;
        main.startSize = 0.08f;
        main.maxParticles = 42;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.18f, 0.02f, 0.34f), new Color(1f, 0.75f, 0.16f, 0.25f));
        var emission = ps.emission;
        emission.rateOverTime = 7f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(24f, 5f, 20f);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null) renderer.material = new Material(_lava);
    }

    void AddV24MergedBossPresentation(Transform boss)
    {
        if (boss == null) return;
        Color red = new Color(1f, 0.05f, 0.02f);
        Color gold = new Color(1f, 0.74f, 0.12f);
        Color black = new Color(0.04f, 0.02f, 0.02f);
        // Визуально усиливаем босса без изменения EnemyAI: спинные шипы, корона-аура и большой задний силуэт.
        Prim(PrimitiveType.Cube, boss, "V24_Boss_Back_Spike_Center", new Vector3(0f, 2.4f, -0.35f), new Vector3(0.28f, 1.6f, 0.28f), Quaternion.Euler(-24f, 0f, 0f), red);
        Prim(PrimitiveType.Cube, boss, "V24_Boss_Back_Spike_L", new Vector3(-0.55f, 2.15f, -0.35f), new Vector3(0.22f, 1.2f, 0.22f), Quaternion.Euler(-22f, 0f, -22f), red);
        Prim(PrimitiveType.Cube, boss, "V24_Boss_Back_Spike_R", new Vector3(0.55f, 2.15f, -0.35f), new Vector3(0.22f, 1.2f, 0.22f), Quaternion.Euler(-22f, 0f, 22f), red);
        GlowOrb(boss, "V24_Boss_Outer_Aura", new Vector3(0f, 0.25f, 0f), new Vector3(3.2f, 0.08f, 3.2f), red, 1.4f).AddComponent<LavaPulse>();
        GlowOrb(boss, "V24_Boss_Crown_Aura", new Vector3(0f, 2.45f, 0.05f), new Vector3(1.2f, 0.10f, 1.2f), gold, 1.2f).AddComponent<LavaPulse>();
        Prim(PrimitiveType.Cube, boss, "V24_Boss_Dark_Backplate", new Vector3(0f, 1.15f, -0.52f), new Vector3(1.6f, 1.7f, 0.12f), Quaternion.identity, black);
    }

    void ModularCauseway(string name, float zFrom, float zTo, float width, float y, Material floorMat, Material accentMat, Transform parent)
    {
        float z = zFrom;
        int i = 0;
        while (z < zTo - 0.05f)
        {
            float len = Mathf.Min(2.2f, zTo - z);
            float cz = z + len * 0.5f;
            Block(name + "_Tile_" + i, new Vector3(0f, y, cz), new Vector3(width, 0.55f, len), floorMat, parent, true, true);
            if (i % 2 == 0)
                Block(name + "_CenterAccent_" + i, new Vector3(0f, y + 0.31f, cz), new Vector3(1.25f, 0.05f, len * 0.78f), accentMat, _decorRoot, false, false);
            z += len;
            i++;
        }
        SideWallPair(name + "_LowRails", width * 0.55f, y + 0.58f, (zFrom + zTo) * 0.5f, zTo - zFrom, accentMat, parent);
    }

    void SideWallPair(string name, float xHalf, float y, float z, float len, Material mat, Transform parent)
    {
        Block(name + "_L", new Vector3(-xHalf, y, z), new Vector3(0.35f, 0.65f, len), mat, parent, true, true);
        Block(name + "_R", new Vector3(xHalf, y, z), new Vector3(0.35f, 0.65f, len), mat, parent, true, true);
    }

    void TransitionArch(string name, Vector3 basePos, float width, float height, Material mat, Color lightColor)
    {
        Block(name + "_Pillar_L", basePos + new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(0.75f, height, 0.8f), mat, _decorRoot, true, true);
        Block(name + "_Pillar_R", basePos + new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(0.75f, height, 0.8f), mat, _decorRoot, true, true);
        Block(name + "_Top", basePos + new Vector3(0f, height + 0.25f, 0f), new Vector3(width + 1.2f, 0.55f, 0.85f), mat, _decorRoot, true, true);
        Fire(basePos + new Vector3(-width * 0.5f - 0.6f, 0.2f, -0.4f));
        Fire(basePos + new Vector3(width * 0.5f + 0.6f, 0.2f, -0.4f));
        LightAt(name + "_SoftLight", basePos + new Vector3(0f, height * 0.62f, 0f), lightColor, 1.45f, 18f);
    }

    void TransitionStairs(string name, Vector3 start, int steps, float width, float startTopY, float endTopY, float totalDepth, Material mat, Transform parent)
    {
        float stepD = totalDepth / steps;
        for (int i = 0; i < steps; i++)
        {
            float t = (i + 0.5f) / steps;
            float topY = Mathf.Lerp(startTopY, endTopY, t);
            float z = start.y + i * stepD + stepD / 2f;
            Block(name + "_Step_" + (i + 1), new Vector3(start.x, topY - 0.18f, z), new Vector3(width, 0.36f, stepD + 0.04f), mat, parent, true, true);
        }
    }

    void GuideBeacon(string name, Vector3 pos, Color color)
    {
        var mat = MakeMat("M_" + name, color * 0.7f, color, true, color, 0.3f);
        Block(name + "_Base", pos + Vector3.up * 0.05f, new Vector3(1.0f, 0.1f, 1.0f), mat, _decorRoot, false, false);
        var pillar = Block(name + "_Vertical_Glow", pos + Vector3.up * 1.45f, new Vector3(0.18f, 2.8f, 0.18f), mat, _decorRoot, false, false);
        pillar.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        LightAt(name + "_Light", pos + Vector3.up * 1.4f, color, 0.6f, 8f);
    }



    void AddV21LayoutPolish()
    {
        // v21: правка по фидбеку пользователя. Не трогаем системы, только композицию/геометрию/свет.

        // 1) Расширяем зону 3: старые пропасти были слишком близко, поэтому убираем их визуал и даём больше места для боя.
        RemoveObjectsContaining("Z3_v18_Left_Deep_Cliff", "Z3_v18_Right_Deep_Cliff", "Z3_v18_Left_Abyss_Glow", "Z3_v18_Right_Abyss_Glow");
        VisualBlock("Z3_v21_Left_Cliff_Pushed_Out", new Vector3(-27f, -3.4f, 124f), new Vector3(13f, 6f, 74f), _dark);
        VisualBlock("Z3_v21_Right_Cliff_Pushed_Out", new Vector3(27f, -3.4f, 124f), new Vector3(13f, 6f, 74f), _dark);
        LightAt("Z3_v21_Left_Abyss_Subtle_Glow", new Vector3(-22f, -1f, 124f), new Color(0.18f, 0.06f, 0.025f), 0.9f, 26f);
        LightAt("Z3_v21_Right_Abyss_Subtle_Glow", new Vector3(22f, -1f, 124f), new Color(0.18f, 0.06f, 0.025f), 0.9f, 26f);

        var z3 = NewChild(_walkRoot, "V21_Zone3_Wider_Combat_Space");
        // Нижний двор шире: больше места для манёвра перед лестницей.
        Block("Z3_v21_Lower_Left_Wing", new Vector3(-13f, -0.5f, 108f), new Vector3(8f, 1f, 22f), _temple, z3, true, true);
        Block("Z3_v21_Lower_Right_Wing", new Vector3(13f, -0.5f, 108f), new Vector3(8f, 1f, 22f), _temple, z3, true, true);
        CoverWall("Z3_v21_Lower_Cover_L", new Vector3(-11.5f, 0.2f, 112f), 14f, 3.8f);
        CoverWall("Z3_v21_Lower_Cover_R", new Vector3(11.5f, 0.2f, 118f), -14f, 3.8f);
        LowRubble(new Vector3(-15f, 0.05f, 103f), _temple);
        LowRubble(new Vector3(15f, 0.05f, 121f), _temple);

        // Верхняя площадка получает боковые крылья и карманы, чтобы не было ощущения тесного коридора.
        Block("Z3_v21_Upper_Left_Combat_Wing", new Vector3(-16f, 2.35f, 135f), new Vector3(7f, 1f, 20f), _temple, z3, true, true);
        Block("Z3_v21_Upper_Right_Combat_Wing", new Vector3(16f, 2.35f, 135f), new Vector3(7f, 1f, 20f), _temple, z3, true, true);
        Block("Z3_v21_Left_Wing_Link", new Vector3(-12f, 2.35f, 132f), new Vector3(3.8f, 0.55f, 9f), _temple, z3, true, true);
        Block("Z3_v21_Right_Wing_Link", new Vector3(12f, 2.35f, 132f), new Vector3(3.8f, 0.55f, 9f), _temple, z3, true, true);
        Column("Z3_v21_Wide_Column_L_A", new Vector3(-17.5f, 2.85f, 130f), 5.4f, 0.44f, _temple, _decorRoot, false);
        Column("Z3_v21_Wide_Column_L_B", new Vector3(-17.5f, 2.85f, 142f), 5.4f, 0.44f, _temple, _decorRoot, true);
        Column("Z3_v21_Wide_Column_R_A", new Vector3(17.5f, 2.85f, 130f), 5.4f, 0.44f, _temple, _decorRoot, false);
        Column("Z3_v21_Wide_Column_R_B", new Vector3(17.5f, 2.85f, 142f), 5.4f, 0.44f, _temple, _decorRoot, true);
        HealthPack(new Vector3(-15.5f, 3.25f, 136f));
        Coin(new Vector3(15.5f, 3.25f, 136f));

        // 2) Зона 4: убираем красную кашу. Старые слишком сильные красные огни гасим и добавляем контраст: тёмный камень + лава как акцент.
        RemoveObjectsContaining("Z4_v19_Global_Lava_Glow", "Z4_v19_Global_Forge_Ambient", "Z4_Strong_Lava_Light", "Z4_Clear_Red_Ambient", "V14_Forge_Extra_Lava_Glow", "V15_Forge_Portal_Red");
        var z4 = NewChild(_walkRoot, "V21_Zone4_Readability_DarkBasalt");
        // Тонкие тёмные накладки на ходовые поверхности визуально отделяют пол от лавы, но не мешают коллайдерам.
        VisualBlock("Z4_v21_Dark_Entry_Floor_Readable", new Vector3(0f, 1.02f, 154f), new Vector3(18f, 0.05f, 7f), _arena);
        VisualBlock("Z4_v21_Dark_Front_Bridge_Readable", new Vector3(0f, 1.50f, 166f), new Vector3(5.2f, 0.05f, 17f), _arena);
        VisualBlock("Z4_v21_Dark_Island_Readable", new Vector3(0f, 2.53f, 178f), new Vector3(17f, 0.05f, 17f), _arena);
        VisualBlock("Z4_v21_Dark_Back_Bridge_Readable", new Vector3(0f, 1.50f, 190f), new Vector3(5.2f, 0.05f, 17f), _arena);
        VisualBlock("Z4_v21_Dark_Exit_Floor_Readable", new Vector3(0f, 1.02f, 204f), new Vector3(18f, 0.05f, 7f), _arena);
        Block("Z4_v21_Clean_Gold_Path_Entry", new Vector3(0f, 1.58f, 166f), new Vector3(1.45f, 0.055f, 15f), _gold, _decorRoot, false, false);
        Block("Z4_v21_Clean_Gold_Path_Center", new Vector3(0f, 2.62f, 178f), new Vector3(1.45f, 0.055f, 13f), _gold, _decorRoot, false, false);
        Block("Z4_v21_Clean_Gold_Path_Exit", new Vector3(0f, 1.58f, 190f), new Vector3(1.45f, 0.055f, 15f), _gold, _decorRoot, false, false);
        LightAt("Z4_v21_Controlled_Lava_LowGlow", new Vector3(0f, -0.2f, 178f), new Color(1f, 0.16f, 0.03f), 2.6f, 54f);
        LightAt("Z4_v21_Warm_Forge_KeyLight", new Vector3(0f, 7.5f, 178f), new Color(1f, 0.48f, 0.15f), 2.0f, 42f);
        LightAt("Z4_v21_Cool_Dark_Contrast", new Vector3(0f, 8.5f, 178f), new Color(0.24f, 0.18f, 0.36f), 0.75f, 52f);
        // Большие читаемые силуэты по бокам вместо мелкой каши.
        Column("Z4_v21_Big_Basalt_Pillar_L1", new Vector3(-23f, 0f, 162f), 8.5f, 0.75f, _arena, _decorRoot, false);
        Column("Z4_v21_Big_Basalt_Pillar_L2", new Vector3(-23f, 0f, 194f), 8.5f, 0.75f, _arena, _decorRoot, false);
        Column("Z4_v21_Big_Basalt_Pillar_R1", new Vector3(23f, 0f, 162f), 8.5f, 0.75f, _arena, _decorRoot, false);
        Column("Z4_v21_Big_Basalt_Pillar_R2", new Vector3(23f, 0f, 194f), 8.5f, 0.75f, _arena, _decorRoot, false);
        RitualBowl(new Vector3(-23f, 8.7f, 162f));
        RitualBowl(new Vector3(23f, 8.7f, 194f));

        // 3) Зона 5: композиция финала. Делаем свет не просто красным, а красный низ + золото на троне + тёмный зал.
        RemoveObjectsContaining("Z5_v20_Boss_Aura_Light", "Z5_v20_Throne_Gold_Backlight", "Z5_Final_Ambient", "V15_Boss_Final_Aura_Light", "V14_Final_Throne_Fire_Aura");
        var z5 = NewChild(_walkRoot, "V21_Zone5_Final_Composition_Polish");
        // Тёмные края и контрастный центральный путь, чтобы финал читался издалека.
        VisualBlock("Z5_v21_Dark_Nave_Overlay", new Vector3(0f, 1.01f, 232f), new Vector3(12f, 0.05f, 38f), _arena);
        Block("Z5_v21_Final_Carpet_Clean", new Vector3(0f, 1.08f, 232f), new Vector3(4.4f, 0.055f, 38f), _blood, _decorRoot, false, false);
        Block("Z5_v21_Final_Gold_Line_L", new Vector3(-2.55f, 1.14f, 232f), new Vector3(0.22f, 0.055f, 38f), _gold, _decorRoot, false, false);
        Block("Z5_v21_Final_Gold_Line_R", new Vector3(2.55f, 1.14f, 232f), new Vector3(0.22f, 0.055f, 38f), _gold, _decorRoot, false, false);
        // Немного расширяем ощущение босc-арены и даём укрытия, но не ставим новую толпу врагов.
        Block("Z5_v21_BossArena_Left_Extension", new Vector3(-14f, 3.25f, 268f), new Vector3(5.5f, 0.75f, 15f), _arena, z5, true, true);
        Block("Z5_v21_BossArena_Right_Extension", new Vector3(14f, 3.25f, 268f), new Vector3(5.5f, 0.75f, 15f), _arena, z5, true, true);
        CoverWall("Z5_v21_Boss_Extension_Cover_L", new Vector3(-14f, 3.85f, 266f), 0f, 3.3f);
        CoverWall("Z5_v21_Boss_Extension_Cover_R", new Vector3(14f, 3.85f, 270f), 0f, 3.3f);
        LightAt("Z5_v21_Gold_Throne_Key", new Vector3(0f, 10f, 277f), new Color(1f, 0.72f, 0.12f), 4.8f, 44f);
        LightAt("Z5_v21_Red_Boss_Floor_Glow", new Vector3(0f, 4.2f, 268f), new Color(1f, 0.12f, 0.035f), 3.0f, 38f);
        LightAt("Z5_v21_Cool_Back_Contrast", new Vector3(0f, 9f, 246f), new Color(0.22f, 0.15f, 0.34f), 1.1f, 58f);
        TransitionArch("Z5_v21_BossArena_Inner_Arch", new Vector3(0f, 3.3f, 257f), 15f, 7.0f, _fort, new Color(1f, 0.55f, 0.08f));
    }

    void RemoveObjectsContaining(params string[] tokens)
    {
        var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var t in all)
        {
            if (t == null || t.gameObject == null) continue;
            foreach (var token in tokens)
            {
                if (!string.IsNullOrEmpty(token) && t.name.Contains(token))
                {
                    Destroy(t.gameObject);
                    break;
                }
            }
        }
    }

    void AddV22FinalZoneRebuild()
    {
        // v22: зона 5 полностью пересобирается после всех старых добавок v14/v15/v20/v21.
        // Так мы убираем оранжевую кашу, наложенные плоскости и старый маленький пятачок босса.
        RemoveZone5OldContent(206.5f);

        var zroot = NewChild(_walkRoot, "Zone_5_v22_Final_Dark_Throne_Cathedral");
        const float z0 = 214f;

        // ── ПЕРЕХОД И ВХОД В ФИНАЛ ───────────────────────────────────
        // Переход собран из коротких модулей, без огромной растянутой плиты.
        for (int i = 0; i < 5; i++)
        {
            float z = 205.8f + i * 2.2f;
            Block("Z5_v22_Modular_Entry_Bridge_" + i, new Vector3(0f, 0.45f, z), new Vector3(10.5f, 0.65f, 2.05f), _arena, zroot, true, true);
            if (i % 2 == 0)
                Block("Z5_v22_Entry_Bridge_Trim_" + i, new Vector3(0f, 0.82f, z), new Vector3(1.4f, 0.05f, 1.55f), _gold, _decorRoot, false, false);
        }
        SideWallPair("Z5_v22_Entry_Bridge_Rails", 5.5f, 1.05f, 210.2f, 10.8f, _fort, zroot);
        TransitionArch("Z5_v22_Dark_Cathedral_Gate", new Vector3(0f, 0.2f, z0), 13.0f, 7.2f, _fort, new Color(0.85f, 0.32f, 0.08f));
        VisualBlock("Z5_v22_Left_Abyss", new Vector3(-23f, -3.2f, z0 + 42f), new Vector3(18f, 6.2f, 94f), _dark);
        VisualBlock("Z5_v22_Right_Abyss", new Vector3(23f, -3.2f, z0 + 42f), new Vector3(18f, 6.2f, 94f), _dark);
        LightAt("Z5_v22_Entry_Subtle_Warm_Light", new Vector3(0f, 4.0f, z0 + 2f), new Color(0.85f, 0.26f, 0.06f), 1.25f, 26f);

        // ── ДЛИННЫЙ ТЁМНЫЙ НЕФ ───────────────────────────────────────
        // Главный зал длинный и широкий, но визуально тёмный: золото только тонкими линиями.
        for (int i = 0; i < 5; i++)
        {
            float z = z0 + 6f + i * 8.2f;
            Block("Z5_v22_Nave_DarkFloor_" + i, new Vector3(0f, 0.50f, z), new Vector3(16f, 0.9f, 8.0f), _arena, zroot, true, true);
        }
        Block("Z5_v22_Nave_Red_Carpet", new Vector3(0f, 0.98f, z0 + 22.0f), new Vector3(4.0f, 0.045f, 38f), _blood, _decorRoot, false, false);
        Block("Z5_v22_Nave_Gold_Line_L", new Vector3(-2.35f, 1.03f, z0 + 22.0f), new Vector3(0.18f, 0.045f, 38f), _gold, _decorRoot, false, false);
        Block("Z5_v22_Nave_Gold_Line_R", new Vector3(2.35f, 1.03f, z0 + 22.0f), new Vector3(0.18f, 0.045f, 38f), _gold, _decorRoot, false, false);

        for (int i = 0; i < 6; i++)
        {
            float z = z0 + 5f + i * 7.2f;
            Column("Z5_v22_Nave_Column_L_" + i, new Vector3(-8.2f, 0.95f, z), 7.4f, 0.46f, _fort, _decorRoot, false);
            Column("Z5_v22_Nave_Column_R_" + i, new Vector3(8.2f, 0.95f, z), 7.4f, 0.46f, _fort, _decorRoot, false);
            V22DarkRib("Z5_v22_Nave_Rib_" + i, z, 8.2f, 8.6f);
            if (i % 2 == 0)
            {
                RitualBowl(new Vector3(-8.2f, 8.55f, z));
                RitualBowl(new Vector3(8.2f, 8.55f, z));
            }
            else
            {
                HangingBanner(new Vector3(-7.1f, 5.1f, z), false);
                HangingBanner(new Vector3(7.1f, 5.1f, z), true);
            }
        }

        // Боковые проходы дают масштаб и простор, но основной путь остаётся по центру.
        Block("Z5_v22_Left_Side_Aisle", new Vector3(-13.8f, 0.75f, z0 + 23f), new Vector3(4.5f, 0.75f, 34f), _fort, zroot, true, true);
        Block("Z5_v22_Right_Side_Aisle", new Vector3(13.8f, 0.75f, z0 + 23f), new Vector3(4.5f, 0.75f, 34f), _fort, zroot, true, true);
        CoverWall("Z5_v22_Left_Aisle_Cover_A", new Vector3(-13.8f, 1.22f, z0 + 17f), 0f, 3.4f);
        CoverWall("Z5_v22_Right_Aisle_Cover_A", new Vector3(13.8f, 1.22f, z0 + 27f), 0f, 3.4f);
        HealthPack(new Vector3(-13.8f, 1.25f, z0 + 32f));
        Coin(new Vector3(13.8f, 1.25f, z0 + 19f));
        LightAt("Z5_v22_Nave_Cool_Shadow_Fill", new Vector3(0f, 7.0f, z0 + 23f), new Color(0.18f, 0.14f, 0.28f), 0.9f, 54f);
        LightAt("Z5_v22_Nave_Subtle_Red_Fill", new Vector3(0f, 4.2f, z0 + 34f), new Color(0.75f, 0.10f, 0.035f), 1.05f, 40f);

        // ── ЗАЛ ГВАРДИИ ──────────────────────────────────────────────
        Block("Z5_v22_Guard_Hall", new Vector3(0f, 0.70f, z0 + 49f), new Vector3(21f, 1.0f, 13f), _arena, zroot, true, true);
        Block("Z5_v22_Guard_Hall_Carpet", new Vector3(0f, 1.23f, z0 + 49f), new Vector3(4.6f, 0.045f, 11.5f), _blood, _decorRoot, false, false);
        V22Obelisk("Z5_v22_Guard_Obelisk_L", new Vector3(-8.6f, 1.1f, z0 + 48f), 5.5f);
        V22Obelisk("Z5_v22_Guard_Obelisk_R", new Vector3(8.6f, 1.1f, z0 + 48f), 5.5f);
        RitualBowl(new Vector3(-6.3f, 1.25f, z0 + 44f));
        RitualBowl(new Vector3(6.3f, 1.25f, z0 + 44f));
        Trap_("Z5_v22_GuardTrap_L", new Vector3(-4.7f, 1.25f, z0 + 51f));
        Trap_("Z5_v22_GuardTrap_R", new Vector3(4.7f, 1.25f, z0 + 47f));

        // ── ПОДЪЁМ К БОССУ ───────────────────────────────────────────
        // Широкий подъём без огромной золотой заливки: ступени тёмные, золото только как кант.
        Stairs("Z5_v22_Boss_Approach_Stairs", new Vector3(0f, 1.05f, z0 + 56f), 10, 13.5f, 0.20f, 1.05f, _fort, zroot);
        Block("Z5_v22_Stairs_Gold_Center_Line", new Vector3(0f, 2.23f, z0 + 61.4f), new Vector3(1.5f, 0.05f, 10.0f), _gold, _decorRoot, false, false);
        SideWallPair("Z5_v22_Boss_Stair_SideWalls", 7.0f, 2.0f, z0 + 61.3f, 10.8f, _fort, zroot);

        // ── БОЛЬШАЯ АРЕНА БОССА ──────────────────────────────────────
        Block("Z5_v22_Boss_Arena_Main", new Vector3(0f, 3.05f, z0 + 77f), new Vector3(34f, 1.0f, 34f), _arena, zroot, true, true);
        // В центре только малый ритуальный знак, не огромный жёлтый ковёр.
        BossAuraRing(new Vector3(0f, 3.62f, z0 + 77f));
        Block("Z5_v22_Arena_Blood_Sigil", new Vector3(0f, 3.60f, z0 + 77f), new Vector3(8f, 0.04f, 8f), _blood, _decorRoot, false, false);
        Block("Z5_v22_Arena_Gold_Sigil_Line", new Vector3(0f, 3.66f, z0 + 77f), new Vector3(1.2f, 0.04f, 10f), _gold, _decorRoot, false, false);

        // Обелиски/укрытия по краям арены: красиво и функционально против дальних атак.
        V22Obelisk("Z5_v22_Arena_Obelisk_FL", new Vector3(-12.5f, 3.55f, z0 + 65f), 7.5f);
        V22Obelisk("Z5_v22_Arena_Obelisk_FR", new Vector3(12.5f, 3.55f, z0 + 65f), 7.5f);
        V22Obelisk("Z5_v22_Arena_Obelisk_BL", new Vector3(-12.5f, 3.55f, z0 + 89f), 7.5f);
        V22Obelisk("Z5_v22_Arena_Obelisk_BR", new Vector3(12.5f, 3.55f, z0 + 89f), 7.5f);
        CoverWall("Z5_v22_Arena_Cover_L", new Vector3(-7.2f, 3.65f, z0 + 77f), 90f, 4.0f);
        CoverWall("Z5_v22_Arena_Cover_R", new Vector3(7.2f, 3.65f, z0 + 77f), 90f, 4.0f);
        Trap_("Z5_v22_BossArena_Trap_L", new Vector3(-8.8f, 3.65f, z0 + 84f));
        Trap_("Z5_v22_BossArena_Trap_R", new Vector3(8.8f, 3.65f, z0 + 70f));
        HealthPack(new Vector3(-10.5f, 3.85f, z0 + 77f));
        HealthPack(new Vector3(10.5f, 3.85f, z0 + 77f));

        // Боковые балконы — дальники видны, но не превращают зал в кашу.
        Block("Z5_v22_Left_Boss_Balcony", new Vector3(-18.0f, 5.1f, z0 + 77f), new Vector3(4.0f, 0.55f, 24f), _fort, zroot, true, true);
        Block("Z5_v22_Right_Boss_Balcony", new Vector3(18.0f, 5.1f, z0 + 77f), new Vector3(4.0f, 0.55f, 24f), _fort, zroot, true, true);
        Block("Z5_v22_Left_Balcony_Rail", new Vector3(-15.7f, 5.95f, z0 + 77f), new Vector3(0.25f, 1.0f, 22f), _fort, _decorRoot, true, true);
        Block("Z5_v22_Right_Balcony_Rail", new Vector3(15.7f, 5.95f, z0 + 77f), new Vector3(0.25f, 1.0f, 22f), _fort, _decorRoot, true, true);
        Stairs("Z5_v22_Left_Balcony_Stairs", new Vector3(-18f, 3.15f, z0 + 62f), 7, 4.0f, 0.28f, 0.9f, _fort, zroot);
        Stairs("Z5_v22_Right_Balcony_Stairs", new Vector3(18f, 3.15f, z0 + 88f), 7, 4.0f, 0.28f, -0.9f, _fort, zroot);

        // ── ТРОН И ФОН БОССА ─────────────────────────────────────────
        // Трон — большой фон за ареной, а не плоская золотая стена перед лицом игрока.
        Block("Z5_v22_Throne_Back_Platform", new Vector3(0f, 4.0f, z0 + 99f), new Vector3(20f, 1.2f, 12f), _fort, zroot, true, true);
        Stairs("Z5_v22_Throne_Dark_Steps", new Vector3(0f, 3.35f, z0 + 91f), 6, 12f, 0.16f, 0.9f, _fort, zroot);
        Block("Z5_v22_Throne_Gold_Trim", new Vector3(0f, 4.66f, z0 + 98.5f), new Vector3(9f, 0.08f, 1.0f), _gold, _decorRoot, false, false);
        Vector3 throne = new Vector3(0f, 5.05f, z0 + 102f);
        Block("Z5_v22_Throne_Dark_Seat", throne, new Vector3(5.2f, 0.7f, 4.2f), _fort, _decorRoot, true, true);
        Block("Z5_v22_Throne_Dark_Back", throne + new Vector3(0f, 3.5f, 1.7f), new Vector3(6.0f, 6.8f, 0.8f), _fort, _decorRoot, true, true);
        Block("Z5_v22_Throne_Gold_Center", throne + new Vector3(0f, 3.6f, 2.12f), new Vector3(2.2f, 5.6f, 0.18f), _gold, _decorRoot, false, false);
        Block("Z5_v22_Throne_Arm_L", throne + new Vector3(-2.8f, 1.0f, 0f), new Vector3(0.7f, 1.8f, 3.8f), _fort, _decorRoot, true, true);
        Block("Z5_v22_Throne_Arm_R", throne + new Vector3(2.8f, 1.0f, 0f), new Vector3(0.7f, 1.8f, 3.8f), _fort, _decorRoot, true, true);
        Block("Z5_v22_Throne_Horn_L", throne + new Vector3(-3.25f, 7.1f, 1.7f), new Vector3(0.45f, 2.2f, 0.45f), _blood, _decorRoot, false, false)
            .transform.rotation = Quaternion.Euler(0f, 0f, -28f);
        Block("Z5_v22_Throne_Horn_R", throne + new Vector3(3.25f, 7.1f, 1.7f), new Vector3(0.45f, 2.2f, 0.45f), _blood, _decorRoot, false, false)
            .transform.rotation = Quaternion.Euler(0f, 0f, 28f);
        Block("Z5_v22_Back_Ruin_L", new Vector3(-9.0f, 9.8f, z0 + 105f), new Vector3(2.0f, 13f, 1.0f), _dark, _decorRoot, true, true);
        Block("Z5_v22_Back_Ruin_R", new Vector3(9.0f, 9.8f, z0 + 105f), new Vector3(2.0f, 13f, 1.0f), _dark, _decorRoot, true, true);
        Block("Z5_v22_Back_Ruin_Top", new Vector3(0f, 15.8f, z0 + 105f), new Vector3(16f, 0.85f, 1.0f), _dark, _decorRoot, true, true)
            .transform.rotation = Quaternion.Euler(0f, 0f, 2.5f);
        LightAt("Z5_v22_Boss_Key_Red", new Vector3(0f, 7.0f, z0 + 78f), new Color(0.95f, 0.08f, 0.025f), 2.4f, 42f);
        LightAt("Z5_v22_Throne_Subtle_Gold_Backlight", new Vector3(0f, 11.0f, z0 + 102f), new Color(1f, 0.55f, 0.08f), 2.2f, 38f);
        LightAt("Z5_v22_Cool_Back_Silhouette", new Vector3(0f, 11.0f, z0 + 88f), new Color(0.20f, 0.12f, 0.34f), 1.2f, 56f);

        // ── ЛУТ / МАРШРУТ ────────────────────────────────────────────
        CoinLine(new Vector3(0f, 1.18f, z0 + 6f), 5, 0f, 5.4f);
        Coin(new Vector3(-2.2f, 3.85f, z0 + 77f));
        Coin(new Vector3(0f, 3.95f, z0 + 77f));
        Coin(new Vector3(2.2f, 3.85f, z0 + 77f));
        Coin(new Vector3(0f, 5.95f, z0 + 102f));
        GuideBeacon("GO_TO_FINAL_BOSS_V22", new Vector3(0f, 3.7f, z0 + 77f), new Color(1f, 0.18f, 0.04f));
        ZoneSign("ФИНАЛ — ТРОННЫЙ СОБОР", new Vector3(0f, 4.8f, z0 + 2f), new Color(1f, 0.62f, 0.12f));

        // ── ВРАГИ И БОСС ─────────────────────────────────────────────
        OrcShieldGuard("Z5_v22_Nave_Elite_L", new Vector3(-4.6f, 1.1f, z0 + 18f),
            new[] { new Vector3(-5.5f, 1.1f, z0 + 13f), new Vector3(4.5f, 1.1f, z0 + 24f) });
        OrcShieldGuard("Z5_v22_Nave_Elite_R", new Vector3(4.6f, 1.1f, z0 + 27f),
            new[] { new Vector3(5.5f, 1.1f, z0 + 20f), new Vector3(-4.5f, 1.1f, z0 + 31f) });
        OrcBerserker("Z5_v22_Guard_Hall_Brute", new Vector3(0f, 1.35f, z0 + 49f),
            new[] { new Vector3(-5.5f, 1.35f, z0 + 46f), new Vector3(5.5f, 1.35f, z0 + 52f) });
        OrcArcher("Z5_v22_Left_Balcony_Archer", new Vector3(-18f, 5.9f, z0 + 75f),
            new[] { new Vector3(-18f, 5.9f, z0 + 67f), new Vector3(-18f, 5.9f, z0 + 86f) });
        OrcArcher("Z5_v22_Right_Balcony_Archer", new Vector3(18f, 5.9f, z0 + 79f),
            new[] { new Vector3(18f, 5.9f, z0 + 68f), new Vector3(18f, 5.9f, z0 + 88f) });
        OrcShaman("Z5_v22_Throne_Dark_Shaman", new Vector3(0f, 4.75f, z0 + 97f),
            new[] { new Vector3(-3.5f, 4.75f, z0 + 96f), new Vector3(3.5f, 4.75f, z0 + 101f) });
        OrcWarlord("Z5_v22_Throne_Guard_L", new Vector3(-5.2f, 4.75f, z0 + 96f),
            new[] { new Vector3(-6f, 4.75f, z0 + 94f), new Vector3(-3f, 4.75f, z0 + 101f) });
        OrcWarlord("Z5_v22_Throne_Guard_R", new Vector3(5.2f, 4.75f, z0 + 96f),
            new[] { new Vector3(6f, 4.75f, z0 + 94f), new Vector3(3f, 4.75f, z0 + 101f) });

        var boss = OrcFinalBoss("FINAL_BOSS_ORC_WARCHIEF_v22", new Vector3(0f, 3.85f, z0 + 78f),
            new[] { new Vector3(-8f, 3.85f, z0 + 72f), new Vector3(8f, 3.85f, z0 + 72f), new Vector3(0f, 3.85f, z0 + 84f), new Vector3(0f, 3.85f, z0 + 76f) });
        if (boss != null)
            AddV22BossScaryExtras(boss.transform);

        // Задняя граница нового финала, потому что старый Border_End удалён вместе со старым финалом.
        Block("Z5_v22_Final_Back_Boundary", new Vector3(0f, 7f, z0 + 113f), new Vector3(54f, 14f, 1.2f), _dark, _decorRoot, true, true);
    }

    void RemoveZone5OldContent(float zMin)
    {
        Transform[] roots = { _walkRoot, _decorRoot, _itemsRoot, _enemiesRoot, _waypointsRoot };
        var list = new System.Collections.Generic.List<GameObject>();
        foreach (var root in roots)
        {
            if (root == null) continue;
            var all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t == null || t == root) continue;
                bool byPos = t.position.z >= zMin;
                bool byName = t.name.Contains("Z5_") || t.name.Contains("FINAL_BOSS") ||
                              t.name.Contains("V14_Final") || t.name.Contains("V14_Throne") ||
                              t.name.Contains("V15_Final") || t.name.Contains("V15_Cathedral") ||
                              t.name.Contains("V15_Boss") || t.name.Contains("V21_Zone5") ||
                              t.name.Contains("GO_TO_THRONE") || t.name.Contains("V21_Final_Hall") ||
                              t.name.Contains("V21_Forge_To_Throne") || t.name.Contains("BOSS_PERSONAL_RED_LIGHT");
                if (byPos || byName) list.Add(t.gameObject);
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                Object.Destroy(list[i]);
        }
    }

    void V22DarkRib(string name, float z, float xHalf, float y)
    {
        var l = Block(name + "_L", new Vector3(-xHalf * 0.48f, y, z), new Vector3(xHalf, 0.35f, 0.55f), _fort, _decorRoot, false, false);
        l.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
        var r = Block(name + "_R", new Vector3(xHalf * 0.48f, y, z), new Vector3(xHalf, 0.35f, 0.55f), _fort, _decorRoot, false, false);
        r.transform.rotation = Quaternion.Euler(0f, 0f, 9f);
    }

    void V22Obelisk(string name, Vector3 basePos, float height)
    {
        Block(name + "_Base", basePos + Vector3.up * 0.15f, new Vector3(1.6f, 0.3f, 1.6f), _fort, _decorRoot, true, true);
        Block(name + "_Body", basePos + Vector3.up * (height * 0.48f), new Vector3(0.95f, height, 0.95f), _dark, _decorRoot, true, true);
        var cap = Block(name + "_Cap", basePos + Vector3.up * (height + 0.2f), new Vector3(1.35f, 0.4f, 1.35f), _fort, _decorRoot, true, true);
        cap.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        RelicCrystal(basePos + Vector3.up * (height + 0.85f), new Color(1f, 0.20f, 0.04f));
    }

    void AddV22BossScaryExtras(Transform boss)
    {
        if (boss == null) return;
        Color bone = new Color(0.86f, 0.78f, 0.58f);
        Color black = new Color(0.025f, 0.018f, 0.015f);
        Color red = new Color(1f, 0.04f, 0.015f);
        Color gold = new Color(0.82f, 0.56f, 0.08f);

        // Дополнительный силуэт: большой шлем, кости на плечах и второй контур ауры.
        Prim(PrimitiveType.Cube, boss, "V22_BOSS_Dark_Helmet_Plate", new Vector3(0f, 2.05f, 0.10f), new Vector3(1.35f, 0.22f, 0.95f), Quaternion.identity, black);
        Prim(PrimitiveType.Cylinder, boss, "V22_BOSS_Bone_Horn_L", new Vector3(-0.62f, 2.20f, 0.20f), new Vector3(0.13f, 0.65f, 0.13f), Quaternion.Euler(0f, 0f, -38f), bone);
        Prim(PrimitiveType.Cylinder, boss, "V22_BOSS_Bone_Horn_R", new Vector3(0.62f, 2.20f, 0.20f), new Vector3(0.13f, 0.65f, 0.13f), Quaternion.Euler(0f, 0f, 38f), bone);
        Prim(PrimitiveType.Cube, boss, "V22_BOSS_Spiked_Shoulder_L", new Vector3(1.18f, 1.22f, 0.18f), new Vector3(0.26f, 0.70f, 0.26f), Quaternion.Euler(0f, 0f, -28f), bone);
        Prim(PrimitiveType.Cube, boss, "V22_BOSS_Spiked_Shoulder_R", new Vector3(-1.18f, 1.22f, 0.18f), new Vector3(0.26f, 0.70f, 0.26f), Quaternion.Euler(0f, 0f, 28f), bone);
        Prim(PrimitiveType.Cube, boss, "V22_BOSS_Dark_Banner_Back", new Vector3(0f, 0.78f, -0.45f), new Vector3(1.7f, 2.2f, 0.08f), Quaternion.Euler(-8f, 0f, 0f), black);
        var ring = Prim(PrimitiveType.Cylinder, boss, "V22_BOSS_Large_Red_Aura", new Vector3(0f, -0.55f, 0f), new Vector3(2.9f, 0.035f, 2.9f), Quaternion.identity, red);
        ring.AddComponent<LavaPulse>();
        Prim(PrimitiveType.Cube, boss, "V22_BOSS_Gold_Belt", new Vector3(0f, 0.45f, 0.58f), new Vector3(1.35f, 0.18f, 0.12f), Quaternion.identity, gold);
    }


    void AddV23FinalAtmosphereFX()
    {
        // v23: НЕ ломаем форму зоны 5 из v22. Только добавляем атмосферу, динамику и сильные силуэты.
        const float z0 = 214f;
        var fxRoot = NewChild(_decorRoot, "V23_Zone5_Atmosphere_FX_And_Geometry");

        // ── РУНИЧЕСКАЯ АРЕНА БОССА ──────────────────────────────────
        // Маленькие сегменты вместо огромной плоскости: ничего не накладывается и не "плывёт".
        V23RuneCircle("Z5_v23_Boss_Rune_Outer", new Vector3(0f, 3.72f, z0 + 77f), 8.8f, 24, new Color(1f, 0.18f, 0.04f), fxRoot);
        V23RuneCircle("Z5_v23_Boss_Rune_Inner", new Vector3(0f, 3.78f, z0 + 77f), 4.2f, 16, new Color(1f, 0.68f, 0.10f), fxRoot);
        V23RuneSpokes("Z5_v23_Boss_Rune_Spokes", new Vector3(0f, 3.83f, z0 + 77f), 7.2f, 8, fxRoot);
        V23PulsingLight("Z5_v23_Rune_Pulse_Core_Light", new Vector3(0f, 4.5f, z0 + 77f), new Color(1f, 0.08f, 0.03f), 1.4f, 3.2f, 26f, 1.35f, fxRoot);

        // ── БОЛЬШИЕ ГЕОМЕТРИЧЕСКИЕ СИЛУЭТЫ ───────────────────────────
        // Высокие шипы и "корона" вокруг арены создают ощущение финального ритуального места.
        V23SpikePylon("Z5_v23_SpikePylon_FL", new Vector3(-15.5f, 3.55f, z0 + 64f), 9.0f, fxRoot);
        V23SpikePylon("Z5_v23_SpikePylon_FR", new Vector3(15.5f, 3.55f, z0 + 64f), 9.0f, fxRoot);
        V23SpikePylon("Z5_v23_SpikePylon_BL", new Vector3(-15.5f, 3.55f, z0 + 90f), 9.5f, fxRoot);
        V23SpikePylon("Z5_v23_SpikePylon_BR", new Vector3(15.5f, 3.55f, z0 + 90f), 9.5f, fxRoot);
        V23OverheadCrown("Z5_v23_Boss_Arena_Crown", z0 + 77f, 13.5f, 12.8f, fxRoot);

        // Цепи и висящие балки над ареной. Это дешёвая геометрия, но визуально зона становится живой.
        for (int i = 0; i < 5; i++)
        {
            float z = z0 + 61f + i * 7.8f;
            ForgeChain(new Vector3(-6.5f, 9.0f, z));
            ForgeChain(new Vector3(6.5f, 9.0f, z + 2.2f));
        }

        // ── ПАРЯЩИЕ ОСКОЛКИ / РИТУАЛЬНЫЕ КРИСТАЛЛЫ ───────────────────
        for (int i = 0; i < 12; i++)
        {
            float a = (Mathf.PI * 2f / 12f) * i;
            float r = (i % 2 == 0) ? 6.2f : 9.5f;
            Vector3 pos = new Vector3(Mathf.Cos(a) * r, 6.0f + (i % 3) * 0.9f, z0 + 77f + Mathf.Sin(a) * r);
            V23FloatingShard("Z5_v23_Floating_RuneShard_" + i, pos, a * Mathf.Rad2Deg, i * 0.55f, fxRoot);
        }

        // ── ДЫМ / ИСКРЫ / АУРА: ЛЁГКИЕ PARTICLES ─────────────────────
        V23SmallAshEmitter("Z5_v23_Arena_Ash", new Vector3(0f, 4.1f, z0 + 77f), fxRoot, new Color(1f, 0.30f, 0.06f), 12f, 15f, 5.5f);
        V23SmallAshEmitter("Z5_v23_Throne_Smoke_Ash", new Vector3(0f, 6.2f, z0 + 101f), fxRoot, new Color(1f, 0.18f, 0.04f), 9f, 11f, 4.8f);
        V23VerticalBeam("Z5_v23_Back_Blood_Beam_L", new Vector3(-4.2f, 7.5f, z0 + 101f), 9.5f, new Color(1f, 0.05f, 0.02f), fxRoot);
        V23VerticalBeam("Z5_v23_Back_Blood_Beam_R", new Vector3(4.2f, 7.5f, z0 + 101f), 9.5f, new Color(1f, 0.05f, 0.02f), fxRoot);
        V23VerticalBeam("Z5_v23_Throne_Gold_Beam", new Vector3(0f, 8.0f, z0 + 103f), 11.5f, new Color(1f, 0.68f, 0.10f), fxRoot);

        // Дополнительные чаши по краям арены: больше жизни, но без сотни огней.
        RitualBowl(new Vector3(-10.5f, 3.7f, z0 + 70f));
        RitualBowl(new Vector3(10.5f, 3.7f, z0 + 84f));
        RitualBowl(new Vector3(-10.5f, 3.7f, z0 + 84f));
        RitualBowl(new Vector3(10.5f, 3.7f, z0 + 70f));

        // ── БОСС-ПОДАЧА ─────────────────────────────────────────────
        Transform boss = FindFinalBossTransform();
        if (boss != null)
            AddV23BossPresentation(boss);
    }

    Transform FindFinalBossTransform()
    {
        if (_enemiesRoot == null) return null;
        var all = _enemiesRoot.GetComponentsInChildren<EnemyAI>(true);
        foreach (var ai in all)
            if (ai != null && ai.isFinalBoss)
                return ai.transform;
        return null;
    }

    void V23RuneCircle(string name, Vector3 center, float radius, int segments, Color color, Transform parent)
    {
        var mat = MakeMat("M_" + name, color * 0.7f, color, true, color, 0.65f);
        for (int i = 0; i < segments; i++)
        {
            float a = (Mathf.PI * 2f / segments) * i;
            Vector3 pos = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            var seg = Block(name + "_Seg_" + i, pos, new Vector3(1.05f, 0.035f, 0.13f), mat, parent, false, false);
            seg.transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
            var pulse = seg.AddComponent<LavaPulse>();
            pulse.phase = i * 0.23f;
            pulse.pulseScale = 0.018f;
        }
    }

    void V23RuneSpokes(string name, Vector3 center, float radius, int count, Transform parent)
    {
        var mat = MakeMat("M_" + name, new Color(0.65f, 0.03f, 0.02f), new Color(1f, 0.23f, 0.04f), true, new Color(1f, 0.08f, 0.02f), 0.55f);
        for (int i = 0; i < count; i++)
        {
            float a = (360f / count) * i;
            var spoke = Block(name + "_Spoke_" + i, center, new Vector3(0.16f, 0.035f, radius), mat, parent, false, false);
            spoke.transform.rotation = Quaternion.Euler(0f, a, 0f);
        }
    }

    void V23SpikePylon(string name, Vector3 basePos, float height, Transform parent)
    {
        Block(name + "_Base", basePos + Vector3.up * 0.22f, new Vector3(2.4f, 0.45f, 2.4f), _fort, parent, true, true);
        Block(name + "_Dark_Core", basePos + Vector3.up * (height * 0.45f), new Vector3(1.05f, height, 1.05f), _dark, parent, true, true);
        var top = Block(name + "_Slanted_Spike", basePos + Vector3.up * (height + 0.7f), new Vector3(0.55f, 2.0f, 0.55f), _blood, parent, false, false);
        top.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
        RelicCrystal(basePos + Vector3.up * (height + 1.75f), new Color(1f, 0.14f, 0.04f));
    }

    void V23OverheadCrown(string name, float zCenter, float xHalf, float y, Transform parent)
    {
        for (int i = 0; i < 6; i++)
        {
            float z = zCenter - 13f + i * 5.2f;
            var l = Block(name + "_Rib_L_" + i, new Vector3(-xHalf * 0.48f, y, z), new Vector3(xHalf, 0.28f, 0.45f), _dark, parent, false, false);
            l.transform.rotation = Quaternion.Euler(0f, 0f, -10f);
            var r = Block(name + "_Rib_R_" + i, new Vector3(xHalf * 0.48f, y, z), new Vector3(xHalf, 0.28f, 0.45f), _dark, parent, false, false);
            r.transform.rotation = Quaternion.Euler(0f, 0f, 10f);
        }
    }

    void V23FloatingShard(string name, Vector3 pos, float rotY, float phase, Transform parent)
    {
        var shard = Block(name, pos, new Vector3(0.38f, 1.25f, 0.18f), _gold, parent, false, false);
        shard.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), rotY, Random.Range(-18f, 18f));
        var bob = shard.AddComponent<V23BobSpin>();
        bob.phase = phase;
        bob.bobAmount = 0.35f;
        bob.spinSpeed = 22f;
        LightAt(name + "_TinyGlow", pos, new Color(1f, 0.42f, 0.05f), 0.35f, 5f);
    }

    void V23VerticalBeam(string name, Vector3 pos, float height, Color color, Transform parent)
    {
        var mat = MakeMat("M_" + name, color * 0.55f, color, true, color, 0.85f);
        var beam = Block(name, pos, new Vector3(0.22f, height, 0.22f), mat, parent, false, false);
        beam.AddComponent<V23BobSpin>().Setup(0.08f, 8f, Random.Range(0f, 6f));
    }

    void V23SmallAshEmitter(string name, Vector3 pos, Transform parent, Color color, float rate, float radius, float lifetime)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.SetParent(parent);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = lifetime;
        main.startSpeed = 0.55f;
        main.startSize = 0.12f;
        main.maxParticles = 80;
        main.startColor = new Color(color.r, color.g, color.b, 0.55f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.rotation = new Vector3(90f, 0f, 0f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = 0.45f;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 1;
        }
    }

    void V23PulsingLight(string name, Vector3 pos, Color color, float minIntensity, float maxIntensity, float range, float speed, Transform parent)
    {
        var g = new GameObject(name);
        g.transform.position = pos;
        g.transform.SetParent(parent);
        var l = g.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = maxIntensity;
        l.range = range;
        g.AddComponent<V23PulseLight>().Setup(minIntensity, maxIntensity, speed);
    }

    void AddV23BossPresentation(Transform boss)
    {
        Color bone = new Color(0.88f, 0.80f, 0.60f);
        Color black = new Color(0.018f, 0.012f, 0.010f);
        Color red = new Color(1f, 0.03f, 0.01f);
        Color gold = new Color(1f, 0.66f, 0.08f);

        // Ещё сильнее силуэт: корона, задние шипы, огромное оружие и личный пульс света.
        Prim(PrimitiveType.Cube, boss, "V23_BOSS_Crown_Base", new Vector3(0f, 2.42f, 0.12f), new Vector3(1.65f, 0.22f, 0.85f), Quaternion.identity, gold);
        for (int i = -2; i <= 2; i++)
        {
            float h = 0.45f + (2 - Mathf.Abs(i)) * 0.18f;
            Prim(PrimitiveType.Cube, boss, "V23_BOSS_Crown_Spike_" + i, new Vector3(i * 0.33f, 2.72f + h * 0.18f, 0.12f), new Vector3(0.15f, h, 0.15f), Quaternion.Euler(0f, 0f, i * -7f), gold);
        }
        Prim(PrimitiveType.Cube, boss, "V23_BOSS_Back_Spike_L", new Vector3(-0.95f, 1.55f, -0.52f), new Vector3(0.22f, 1.55f, 0.22f), Quaternion.Euler(-25f, 0f, -22f), bone);
        Prim(PrimitiveType.Cube, boss, "V23_BOSS_Back_Spike_R", new Vector3(0.95f, 1.55f, -0.52f), new Vector3(0.22f, 1.55f, 0.22f), Quaternion.Euler(-25f, 0f, 22f), bone);
        Prim(PrimitiveType.Cube, boss, "V23_BOSS_Giant_Hammer_Handle", new Vector3(1.35f, 0.68f, 0.18f), new Vector3(0.16f, 2.65f, 0.16f), Quaternion.Euler(0f, 0f, -22f), black);
        Prim(PrimitiveType.Cube, boss, "V23_BOSS_Giant_Hammer_Head", new Vector3(1.75f, 1.75f, 0.20f), new Vector3(0.95f, 0.45f, 0.55f), Quaternion.Euler(0f, 0f, -22f), bone);
        var aura = Prim(PrimitiveType.Cylinder, boss, "V23_BOSS_Outer_Red_Aura", new Vector3(0f, -0.68f, 0f), new Vector3(4.0f, 0.025f, 4.0f), Quaternion.identity, red);
        var pulse = aura.AddComponent<LavaPulse>();
        pulse.pulseScale = 0.045f;
        V23SmallAshEmitter("V23_BOSS_Personal_Ash", boss.position + Vector3.up * 1.6f, boss, red, 10f, 2.6f, 2.6f);
        var light = new GameObject("V23_BOSS_PERSONAL_PULSE_LIGHT");
        light.transform.SetParent(boss);
        light.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        var l = light.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = red;
        l.range = 13f;
        l.intensity = 1.6f;
        light.AddComponent<V23PulseLight>().Setup(1.0f, 2.6f, 1.8f);
    }

    void AddOrcCitadelAtmosphere()
    {
        // v14: берём лучшие идеи из присланного скрипта, но НЕ ломаем текущую техническую базу.
        // Никаких чекпоинтов, никаких закрытых дверей, Player/Managers/Restart остаются из рабочей версии v12.

        // Зона 1 — мрачный лес и орочий лагерь по бокам от основного маршрута.
        for (int i = 0; i < 10; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            float x = side * Random.Range(12.5f, 18.5f);
            float z = Random.Range(6f, 38f);
            Tree(new Vector3(x, 0f, z), Random.Range(0.75f, 1.15f));
        }
        OrcTent(new Vector3(-13.5f, 0.2f, 18f), 22f);
        OrcTent(new Vector3(13.5f, 0.2f, 27f), -18f);
        CampfireSet(new Vector3(-10.5f, 0.15f, 30f));
        CampfireSet(new Vector3(10.5f, 0.15f, 14f));

        // Зона 2 — форт становится более похожим на цитадель: зубцы, башенный декор, факелы, ящики.
        for (float x = -14f; x <= 14f; x += 3.5f)
            Block("V14_Fort_Extra_Merlon", new Vector3(x, 7.4f, 54f), new Vector3(1.1f, 1.0f, 1.1f), _fort, _decorRoot, true, true);
        Block("V14_Fort_Overhead_Arch", new Vector3(0f, 8.2f, 54f), new Vector3(10.5f, 0.75f, 1.1f), _fort, _decorRoot, true, true);
        CrateStack(new Vector3(-14f, 0.2f, 84f), 3);
        CrateStack(new Vector3(14f, 0.2f, 64f), 2);
        CampfireSet(new Vector3(-15f, 0.2f, 55f));
        CampfireSet(new Vector3(15f, 0.2f, 55f));

        // Зона 3 — храм/катакомбы: больше ритма колонн и разрушенных балок, но проход по центру не перекрываем.
        for (float z = 99f; z <= 139f; z += 10f)
        {
            Column("V14_Catacomb_Column_L_" + z, new Vector3(-11.8f, 0f, z), 6.2f, 0.42f, _temple, _decorRoot, false);
            Column("V14_Catacomb_Column_R_" + z, new Vector3(11.8f, 0f, z), 6.2f, 0.42f, _temple, _decorRoot, false);
            Block("V14_Catacomb_Broken_Beam_" + z, new Vector3(0f, 6.6f, z), new Vector3(18f, 0.35f, 0.5f), _temple, _decorRoot, true, true)
                .transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f));
        }
        RitualBowl(new Vector3(-9.2f, 3.2f, 130f));
        RitualBowl(new Vector3(9.2f, 3.2f, 130f));
        BrokenColumnPile(new Vector3(-10.5f, 0.15f, 103f));
        BrokenColumnPile(new Vector3(10.5f, 0.15f, 137f));

        // Зона 4 — кузня: наковальни, раскалённые чаши, лавовые объекты по бокам. Основной мост остаётся понятным.
        ForgeAnvil(new Vector3(-22f, 0.8f, 160f));
        ForgeAnvil(new Vector3(22f, 0.8f, 188f));
        ForgeAnvil(new Vector3(-21f, 0.8f, 198f));
        RitualBowl(new Vector3(-11f, 2.4f, 178f));
        RitualBowl(new Vector3(11f, 2.4f, 178f));
        LightAt("V14_Forge_Extra_Lava_Glow", new Vector3(0f, 4.5f, 178f), new Color(1f, 0.18f, 0.02f), 2.2f, 46f);

        // Зона 5 — финальный тронный собор: готические арки и огненные чаши у трона.
        for (int i = 0; i < 4; i++)
        {
            float z = 222f + i * 12f;
            GothicArch("V14_Final_Gothic_Arch_" + i, z);
        }
        RitualBowl(new Vector3(-5.2f, 4.85f, 273f));
        RitualBowl(new Vector3(5.2f, 4.85f, 273f));
        Block("V14_Throne_Back_Ruin_L", new Vector3(-6.8f, 7.0f, 278f), new Vector3(2.4f, 10f, 0.8f), _fort, _decorRoot, true, true);
        Block("V14_Throne_Back_Ruin_R", new Vector3(6.8f, 7.0f, 278f), new Vector3(2.4f, 10f, 0.8f), _fort, _decorRoot, true, true);
        LightAt("V14_Final_Throne_Fire_Aura", new Vector3(0f, 8.0f, 274f), new Color(1f, 0.38f, 0.04f), 3.6f, 36f);
    }

    void AddLateGameSpectacleLightweight()
    {
        // v15: усиливаем только зоны 3-5. Всё лёгкое: кубы, несколько точечных огней, маленькие particle-системы.
        // Никаких жидкостных симуляций, Rigidbody-хаоса и тяжёлых ассетов — ноут не душим.

        var z3ExtraWalk = NewChild(_walkRoot, "V15_Zone3_Extra_Walkable_Ruins");
        var z4ExtraWalk = NewChild(_walkRoot, "V15_Zone4_Extra_Walkable_Arena");
        var z5ExtraWalk = NewChild(_walkRoot, "V15_Zone5_Extra_Walkable_Cathedral");

        // ЗОНА 3 — храм перестаёт быть прямоугольной площадкой: добавляем боковые галереи, алтарную арку и обломки.
        Block("V15_Temple_Left_Gallery", new Vector3(-10.7f, 3.85f, 124f), new Vector3(3.0f, 0.45f, 34f), _temple, z3ExtraWalk, true, true);
        Block("V15_Temple_Right_Gallery", new Vector3(10.7f, 3.85f, 124f), new Vector3(3.0f, 0.45f, 34f), _temple, z3ExtraWalk, true, true);
        Stairs("V15_Temple_Left_Gallery_Stairs", new Vector3(-10.7f, 0.85f, 106f), 7, 3.0f, 0.43f, 1.05f, _temple, z3ExtraWalk);
        Stairs("V15_Temple_Right_Gallery_Stairs", new Vector3(10.7f, 0.85f, 136f), 7, 3.0f, 0.43f, -1.05f, _temple, z3ExtraWalk);

        for (float z = 104f; z <= 144f; z += 8f)
        {
            BrokenArch("V15_Temple_Broken_Arch_" + z, z, _temple);
            LowRubble(new Vector3(-6.8f, 3.05f, z + 1.8f), _temple);
            LowRubble(new Vector3(6.8f, 3.05f, z - 1.8f), _temple);
        }
        RelicCrystal(new Vector3(0f, 4.6f, 136.5f), new Color(1f, 0.82f, 0.18f));
        RelicCrystal(new Vector3(-10.7f, 4.45f, 130f), new Color(0.9f, 0.72f, 0.18f));
        RelicCrystal(new Vector3(10.7f, 4.45f, 118f), new Color(0.9f, 0.72f, 0.18f));
        LightAt("V15_Temple_High_Gold_Rays", new Vector3(0f, 8.8f, 134f), new Color(1f, 0.78f, 0.22f), 2.2f, 36f);

        // Пара монет на новых галереях, чтобы они были не просто декором, а маленьким риск/награда маршрутом.
        Coin(new Vector3(-10.7f, 4.55f, 128f));
        Coin(new Vector3(10.7f, 4.55f, 120f));

        // ЗОНА 4 — лава теперь "живая": пульсация, пузырьки, струйки пара/искр. Всё с малым числом частиц.
        LavaSheet("V15_Lava_Current_Left_A", new Vector3(-9.5f, -1.42f, 166f), new Vector3(10f, 0.08f, 24f), 0.8f);
        LavaSheet("V15_Lava_Current_Left_B", new Vector3(-9.5f, -1.40f, 190f), new Vector3(10f, 0.08f, 24f), 1.4f);
        LavaSheet("V15_Lava_Current_Right_A", new Vector3(9.5f, -1.41f, 166f), new Vector3(10f, 0.08f, 24f), 1.1f);
        LavaSheet("V15_Lava_Current_Right_B", new Vector3(9.5f, -1.39f, 190f), new Vector3(10f, 0.08f, 24f), 1.8f);

        for (int i = 0; i < 8; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            float z = 154f + i * 6.2f;
            LavaVent(new Vector3(side * Random.Range(7.0f, 13.5f), -0.95f, z));
        }

        // Арена получает силуэт: цепи/арки/порталы по углам, но центральный мост не трогаем.
        for (int i = 0; i < 4; i++)
        {
            float z = 158f + i * 13f;
            ForgeChain(new Vector3(-5.2f, 3.0f, z));
            ForgeChain(new Vector3(5.2f, 3.0f, z + 5f));
        }
        Block("V15_Forge_Exit_Arch_L", new Vector3(-5.2f, 4.6f, 204f), new Vector3(0.8f, 7.0f, 0.8f), _arena, _decorRoot, true, true);
        Block("V15_Forge_Exit_Arch_R", new Vector3(5.2f, 4.6f, 204f), new Vector3(0.8f, 7.0f, 0.8f), _arena, _decorRoot, true, true);
        Block("V15_Forge_Exit_Arch_Top", new Vector3(0f, 8.2f, 204f), new Vector3(11.5f, 0.65f, 0.8f), _arena, _decorRoot, true, true);
        LightAt("V15_Forge_Portal_Red", new Vector3(0f, 4.0f, 204f), new Color(1f, 0.12f, 0.02f), 2.5f, 28f);

        // ЗОНА 5 — тронный зал делаем похожим на финальный собор: неф, боковые алтари, banners, аура босса.
        for (int i = 0; i < 5; i++)
        {
            float z = 220f + i * 10.5f;
            TallCathedralRib("V15_Cathedral_Rib_" + i, z);
            HangingBanner(new Vector3(-7.2f, 6.0f, z + 1f), false);
            HangingBanner(new Vector3(7.2f, 6.0f, z + 1f), true);
        }

        Block("V15_Final_Left_Side_Altar", new Vector3(-8.2f, 1.2f, 252f), new Vector3(3.0f, 1.3f, 3.4f), _fort, z5ExtraWalk, true, true);
        Block("V15_Final_Right_Side_Altar", new Vector3(8.2f, 1.2f, 252f), new Vector3(3.0f, 1.3f, 3.4f), _fort, z5ExtraWalk, true, true);
        RitualBowl(new Vector3(-8.2f, 2.0f, 252f));
        RitualBowl(new Vector3(8.2f, 2.0f, 252f));
        BossAuraRing(new Vector3(0f, 5.5f, 272.6f));
        LightAt("V15_Boss_Final_Aura_Light", new Vector3(0f, 7.6f, 272.6f), new Color(1f, 0.25f, 0.02f), 5.0f, 42f);
    }

    void LavaSheet(string name, Vector3 pos, Vector3 scale, float phase)
    {
        var g = Block(name, pos, scale, _lava, _decorRoot, false, false);
        var pulse = g.AddComponent<LavaPulse>();
        pulse.phase = phase;
        pulse.pulseScale = 0.035f;
        pulse.colorA = new Color(1f, 0.12f, 0.01f);
        pulse.colorB = new Color(1f, 0.48f, 0.02f);
    }

    void LavaVent(Vector3 pos)
    {
        var root = new GameObject("V15_Lightweight_LavaVent");
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);
        var ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 0.7f;
        main.startSpeed = 1.25f;
        main.startSize = 0.18f;
        main.maxParticles = 18;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.32f, 0.02f, 0.75f), new Color(1f, 0.72f, 0.08f, 0.55f));
        var emission = ps.emission;
        emission.rateOverTime = 2.0f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.18f;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null) renderer.material = new Material(_lava);

        var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = "V15_LavaVent_Glow";
        glow.transform.SetParent(root.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 0.42f;
        ApplyMaterial(glow, _lava, Vector3.one);
        var c = glow.GetComponent<Collider>();
        if (c) Destroy(c);
        glow.AddComponent<LavaPulse>();
    }

    void BrokenArch(string name, float z, Material mat)
    {
        Block(name + "_Left", new Vector3(-7.4f, 6.0f, z), new Vector3(0.7f, 6.2f, 0.6f), mat, _decorRoot, true, true);
        Block(name + "_Right", new Vector3(7.4f, 6.0f, z), new Vector3(0.7f, 6.2f, 0.6f), mat, _decorRoot, true, true);
        var topA = Block(name + "_Top_A", new Vector3(-3.3f, 9.0f, z), new Vector3(6.8f, 0.42f, 0.55f), mat, _decorRoot, true, true);
        topA.transform.rotation = Quaternion.Euler(0f, 0f, -6f);
        var topB = Block(name + "_Top_B", new Vector3(3.3f, 9.0f, z), new Vector3(6.8f, 0.42f, 0.55f), mat, _decorRoot, true, true);
        topB.transform.rotation = Quaternion.Euler(0f, 0f, 6f);
    }

    void LowRubble(Vector3 pos, Material mat)
    {
        for (int i = 0; i < 3; i++)
        {
            var g = Block("V15_Low_Rubble", pos + new Vector3(Random.Range(-0.7f, 0.7f), 0.12f, Random.Range(-0.7f, 0.7f)),
                new Vector3(Random.Range(0.5f, 1.1f), Random.Range(0.18f, 0.35f), Random.Range(0.4f, 0.9f)), mat, _decorRoot, true, true);
            g.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
        }
    }

    void RelicCrystal(Vector3 pos, Color color)
    {
        var mat = MakeMat("M_V15_Relic", color * 0.65f, color, true, color, 0.75f);
        var g = Block("V15_Relic_Crystal", pos, new Vector3(0.55f, 1.7f, 0.55f), mat, _decorRoot, false, false);
        g.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        g.AddComponent<LavaPulse>();
        LightAt("V15_Relic_Light", pos + Vector3.up * 0.8f, color, 1.3f, 12f);
    }

    void ForgeChain(Vector3 pos)
    {
        for (int i = 0; i < 4; i++)
        {
            var link = Block("V15_Forge_Chain_Link", pos + Vector3.down * (i * 0.55f), new Vector3(0.18f, 0.5f, 0.18f), _fort, _decorRoot, false, false);
            link.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? 0f : 90f, 0f);
        }
    }

    void TallCathedralRib(string name, float z)
    {
        Block(name + "_L", new Vector3(-9.2f, 6.2f, z), new Vector3(0.65f, 10.5f, 0.65f), _fort, _decorRoot, true, true);
        Block(name + "_R", new Vector3(9.2f, 6.2f, z), new Vector3(0.65f, 10.5f, 0.65f), _fort, _decorRoot, true, true);
        var topL = Block(name + "_TopL", new Vector3(-4.8f, 11.2f, z), new Vector3(9.0f, 0.42f, 0.55f), _fort, _decorRoot, true, true);
        topL.transform.rotation = Quaternion.Euler(0f, 0f, -10f);
        var topR = Block(name + "_TopR", new Vector3(4.8f, 11.2f, z), new Vector3(9.0f, 0.42f, 0.55f), _fort, _decorRoot, true, true);
        topR.transform.rotation = Quaternion.Euler(0f, 0f, 10f);
    }

    void HangingBanner(Vector3 pos, bool flip)
    {
        var mat = MakeMat("M_V15_Banner", new Color(0.28f, 0.02f, 0.02f), new Color(0.70f, 0.05f, 0.03f), true, new Color(0.55f, 0.02f, 0.01f), 0.12f);
        var bar = Block("V15_Banner_Bar", pos + Vector3.up * 0.65f, new Vector3(1.45f, 0.10f, 0.15f), _wood, _decorRoot, false, false);
        bar.transform.rotation = Quaternion.Euler(0f, flip ? 90f : -90f, 0f);
        var cloth = Block("V15_Hanging_Banner", pos, new Vector3(1.15f, 2.2f, 0.08f), mat, _decorRoot, false, false);
        cloth.transform.rotation = Quaternion.Euler(0f, flip ? 90f : -90f, 0f);
        cloth.AddComponent<BannerSway>();
    }

    void BossAuraRing(Vector3 pos)
    {
        for (int i = 0; i < 3; i++)
        {
            var ring = Block("V15_Boss_Aura_Ring_" + i, pos + Vector3.up * (i * 0.18f), new Vector3(5.2f + i * 0.8f, 0.05f, 5.2f + i * 0.8f), _lava, _decorRoot, false, false);
            ring.transform.rotation = Quaternion.Euler(0f, i * 35f, 0f);
            var pulse = ring.AddComponent<LavaPulse>();
            pulse.phase = i * 0.75f;
            pulse.pulseScale = 0.055f;
        }
    }

    void OrcTent(Vector3 pos, float yRot)
    {
        var root = new GameObject("V14_Orc_Tent");
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        root.transform.SetParent(_decorRoot);

        var left = Block("Tent_Lean_Left", pos + new Vector3(-0.9f, 1.0f, 0f), new Vector3(0.18f, 2.2f, 3.0f), _wood, root.transform, false, true);
        left.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        var right = Block("Tent_Lean_Right", pos + new Vector3(0.9f, 1.0f, 0f), new Vector3(0.18f, 2.2f, 3.0f), _wood, root.transform, false, true);
        right.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        Block("Tent_Back_Dark", pos + new Vector3(0f, 0.85f, 1.55f), new Vector3(2.0f, 1.6f, 0.18f), _dark, root.transform, false, true);
    }

    void CampfireSet(Vector3 pos)
    {
        Rock(pos + new Vector3(-0.7f, 0f, 0.2f), 0.45f);
        Rock(pos + new Vector3(0.6f, 0f, -0.15f), 0.42f);
        Block("V14_Campfire_Log_A", pos + new Vector3(0f, 0.18f, 0f), new Vector3(1.4f, 0.18f, 0.22f), _wood, _decorRoot, true, true)
            .transform.rotation = Quaternion.Euler(0f, 25f, 0f);
        Block("V14_Campfire_Log_B", pos + new Vector3(0f, 0.22f, 0f), new Vector3(1.4f, 0.18f, 0.22f), _wood, _decorRoot, true, true)
            .transform.rotation = Quaternion.Euler(0f, -25f, 0f);
        Fire(pos + Vector3.up * 0.2f);
    }

    void ForgeAnvil(Vector3 pos)
    {
        Block("V14_Forge_Anvil_Base", pos + Vector3.up * 0.45f, new Vector3(2.0f, 0.9f, 1.3f), _arena, _decorRoot, true, true);
        Block("V14_Forge_Anvil_Top", pos + Vector3.up * 1.05f, new Vector3(2.8f, 0.35f, 1.0f), _fort, _decorRoot, true, true);
        Block("V14_Forge_Hot_Iron", pos + Vector3.up * 1.32f, new Vector3(1.5f, 0.12f, 0.35f), _lava, _decorRoot, false, false);
        LightAt("V14_Anvil_Hot_Light", pos + Vector3.up * 1.6f, new Color(1f, 0.25f, 0.02f), 1.4f, 9f);
    }

    void RitualBowl(Vector3 pos)
    {
        var bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bowl.name = "V14_Ritual_Bowl";
        bowl.transform.position = pos + Vector3.up * 0.25f;
        bowl.transform.localScale = new Vector3(0.65f, 0.25f, 0.65f);
        bowl.transform.SetParent(_decorRoot);
        ApplyMaterial(bowl, _fort, Vector3.one);
        bowl.isStatic = true;

        var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fire.name = "V14_Ritual_Bowl_Fire";
        fire.transform.position = pos + Vector3.up * 0.65f;
        fire.transform.localScale = Vector3.one * 0.55f;
        fire.transform.SetParent(_decorRoot);
        ApplyMaterial(fire, _lava, Vector3.one);
        var c = fire.GetComponent<Collider>();
        if (c) Destroy(c);
        IgnoreNavMesh(fire);
        fire.AddComponent<FireFlicker>();
        LightAt("V14_Ritual_Bowl_Light", pos + Vector3.up * 0.85f, new Color(1f, 0.22f, 0.03f), 1.65f, 8f);
    }

    void GothicArch(string name, float z)
    {
        Block(name + "_Pillar_L", new Vector3(-8.5f, 5.2f, z), new Vector3(0.75f, 8.5f, 0.75f), _fort, _decorRoot, true, true);
        Block(name + "_Pillar_R", new Vector3(8.5f, 5.2f, z), new Vector3(0.75f, 8.5f, 0.75f), _fort, _decorRoot, true, true);
        var leftTop = Block(name + "_Top_Left", new Vector3(-4.4f, 9.2f, z), new Vector3(8.4f, 0.45f, 0.55f), _fort, _decorRoot, true, true);
        leftTop.transform.rotation = Quaternion.Euler(0f, 0f, -8f);
        var rightTop = Block(name + "_Top_Right", new Vector3(4.4f, 9.2f, z), new Vector3(8.4f, 0.45f, 0.55f), _fort, _decorRoot, true, true);
        rightTop.transform.rotation = Quaternion.Euler(0f, 0f, 8f);
    }

    void AddSurvivalDecor()
    {
        // v9: живость и укрытия без поломки основного маршрута.
        // Все объекты стоят по бокам или как низкие укрытия, чтобы игрок понимал путь и мог прятаться от врагов.
        // Зона 1 — деревья и камни вокруг холмов.
        Tree(new Vector3(-15f, 0.0f, 8f), 1.0f);
        Tree(new Vector3(-16f, 0.0f, 20f), 0.9f);
        Tree(new Vector3(15f, 0.0f, 12f), 1.1f);
        Tree(new Vector3(16f, 0.0f, 29f), 0.9f);
        CoverWall("Z1_Cover_Stone_Left", new Vector3(-4.5f, 0.35f, 23f), 0f, 3.2f);
        CoverWall("Z1_Cover_Stone_Right", new Vector3(4.5f, 0.35f, 25f), 12f, 3.0f);

        // Зона 2 — форт живее: ящики, низкие стены, бочки/укрытия во дворе.
        CrateStack(new Vector3(-11f, 0.2f, 58f), 2);
        CrateStack(new Vector3(11f, 0.2f, 70f), 3);
        CoverWall("Z2_Cover_BrokenWall_A", new Vector3(-1.8f, 0.45f, 66f), 90f, 3.8f);
        CoverWall("Z2_Cover_BrokenWall_B", new Vector3(3.5f, 0.45f, 78f), -18f, 4.2f);
        Barrel(new Vector3(-12f, 1.15f, 72f));
        Barrel(new Vector3(12f, 0.2f, 60f));

        // Зона 3 — руины: обломки колонн и каменные укрытия у боковых проходов.
        BrokenColumnPile(new Vector3(-8.5f, 0.15f, 112f));
        BrokenColumnPile(new Vector3(8.5f, 0.15f, 132f));
        CoverWall("Z3_Cover_Ruin_A", new Vector3(-4.5f, 3.1f, 121f), 10f, 3.5f);
        CoverWall("Z3_Cover_Ruin_B", new Vector3(4.5f, 3.1f, 129f), -10f, 3.5f);

        // Зона 4 — арена: укрытия не перекрывают мосты, но дают игроку места для манёвра.
        CoverWall("Z4_Island_Cover_A", new Vector3(-3.2f, 2.75f, 178f), 90f, 2.8f);
        CoverWall("Z4_Island_Cover_B", new Vector3(3.2f, 2.75f, 176f), 90f, 2.8f);
        CoverWall("Z4_Ring_Cover_L", new Vector3(-15.5f, 1.15f, 162f), 0f, 2.8f);
        CoverWall("Z4_Ring_Cover_R", new Vector3(15.5f, 1.15f, 186f), 0f, 2.8f);

        // Зона 5 — тронный зал: низкие барьеры/статуи по краям центрального пути.
        for (int i = 0; i < 4; i++)
        {
            float z = 222f + i * 11f;
            Statue(new Vector3(-6.0f, 1.0f, z));
            Statue(new Vector3(6.0f, 1.0f, z));
        }
        CoverWall("Z5_Last_Cover_L", new Vector3(-3.2f, 1.0f, 246f), 8f, 3.2f);
        CoverWall("Z5_Last_Cover_R", new Vector3(3.2f, 1.0f, 251f), -8f, 3.2f);
    }

    void CoverWall(string name, Vector3 pos, float yRot, float length)
    {
        var g = Block(name + "_LowBlock", pos, new Vector3(length, 0.75f, 0.55f), _fort, _decorRoot, true, true);
        g.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

    void Tree(Vector3 pos, float scale)
    {
        var root = new GameObject("Tree_LowPoly");
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Tree_Trunk";
        trunk.transform.SetParent(root.transform);
        trunk.transform.localPosition = Vector3.up * (1.15f * scale);
        trunk.transform.localScale = new Vector3(0.28f * scale, 1.15f * scale, 0.28f * scale);
        ApplyMaterial(trunk, _wood, Vector3.one * scale);
        trunk.isStatic = true;

        for (int i = 0; i < 3; i++)
        {
            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Tree_Crown";
            crown.transform.SetParent(root.transform);
            crown.transform.localPosition = new Vector3((i - 1) * 0.38f * scale, (2.25f + i * 0.18f) * scale, 0f);
            crown.transform.localScale = Vector3.one * (1.35f * scale);
            ApplyMaterial(crown, _grass, Vector3.one * scale);
            var c = crown.GetComponent<Collider>();
            if (c) Destroy(c);
            IgnoreNavMesh(crown);
        }
    }

    void CrateStack(Vector3 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = (i % 2) * 0.8f;
            float y = 0.35f + (i / 2) * 0.72f;
            Block("Fort_Crate", pos + new Vector3(x, y, 0f), new Vector3(0.75f, 0.7f, 0.75f), _wood, _decorRoot, true, true);
        }
    }

    void Barrel(Vector3 pos)
    {
        var b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        b.name = "Barrel_Cover";
        b.transform.position = pos + Vector3.up * 0.45f;
        b.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
        b.transform.SetParent(_decorRoot);
        ApplyMaterial(b, _wood, Vector3.one);
        b.isStatic = true;
    }

    void BrokenColumnPile(Vector3 pos)
    {
        Column("Broken_Ruin_Column", pos, 1.3f, 0.45f, _temple, _decorRoot, true);
        Rock(pos + new Vector3(1.1f, 0.1f, 0.6f), 0.9f);
        Rock(pos + new Vector3(-0.9f, 0.1f, -0.5f), 0.7f);
    }

    void Statue(Vector3 pos)
    {
        Block("Statue_Base", pos + Vector3.up * 0.15f, new Vector3(1.1f, 0.3f, 1.1f), _fort, _decorRoot, true, true);
        Block("Statue_Body", pos + Vector3.up * 1.0f, new Vector3(0.55f, 1.4f, 0.45f), _temple, _decorRoot, true, true);
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Statue_Head";
        head.transform.position = pos + Vector3.up * 1.9f;
        head.transform.localScale = Vector3.one * 0.45f;
        head.transform.SetParent(_decorRoot);
        ApplyMaterial(head, _temple, Vector3.one);
        head.isStatic = true;
    }

    GameObject RitualGateWithTotems(string name, Vector3 pos, float width, float height, GameObject[] enemies)
    {
        var root = new GameObject(name);
        root.transform.position = pos;
        root.transform.SetParent(_decorRoot);

        var stoneMat = MakeMat("M_RG_Stone_" + name, new Color(0.18f, 0.10f, 0.06f), new Color(0.22f, 0.13f, 0.08f), false, Color.black, 0f);
        var darkWood = MakeMat("M_RG_DarkWood_" + name, new Color(0.22f, 0.12f, 0.06f), new Color(0.28f, 0.15f, 0.08f), false, Color.black, 0f);
        var boneMat = MakeMat("M_RG_Bone_" + name, new Color(0.82f, 0.74f, 0.52f), new Color(0.9f, 0.82f, 0.6f), false, Color.black, 0f);
        var metalMat = MakeMat("M_RG_Metal_" + name, new Color(0.15f, 0.15f, 0.18f), new Color(0.2f, 0.2f, 0.25f), false, Color.black, 0f);
        var redCloth = MakeMat("M_RG_Cloth_" + name, new Color(0.6f, 0.05f, 0.02f), new Color(0.7f, 0.08f, 0.04f), false, Color.black, 0f);
        var runeMat = MakeMat("M_RG_Rune_" + name, new Color(0.8f, 0.15f, 0.03f), new Color(0.9f, 0.2f, 0.05f), true, new Color(1f, 0.2f, 0.04f), 0.8f);

        // Массивные столбы с каменным основанием
        float pillarW = 1.6f;
        Block(name + "_Base_L", pos + new Vector3(-width * 0.5f, 0.4f, 0f), new Vector3(pillarW + 0.6f, 0.8f, pillarW + 0.6f), stoneMat, _decorRoot, true, true);
        Block(name + "_Base_R", pos + new Vector3(width * 0.5f, 0.4f, 0f), new Vector3(pillarW + 0.6f, 0.8f, pillarW + 0.6f), stoneMat, _decorRoot, true, true);
        Block(name + "_Pillar_L", pos + new Vector3(-width * 0.5f, height * 0.5f + 0.4f, 0f), new Vector3(pillarW, height, pillarW), darkWood, _decorRoot, true, true);
        Block(name + "_Pillar_R", pos + new Vector3(width * 0.5f, height * 0.5f + 0.4f, 0f), new Vector3(pillarW, height, pillarW), darkWood, _decorRoot, true, true);

        // Перекладина с шипами
        Block(name + "_Lintel", pos + new Vector3(0f, height + 0.8f, 0f), new Vector3(width + 2.0f, 0.9f, 1.2f), darkWood, _decorRoot, true, true);
        Block(name + "_LintelTrim", pos + new Vector3(0f, height + 1.35f, 0f), new Vector3(width + 2.4f, 0.2f, 1.4f), metalMat, _decorRoot, true, true);
        // Шипы на перекладине
        for (int i = 0; i < 5; i++)
        {
            float sx = Mathf.Lerp(-width * 0.4f, width * 0.4f, i / 4f);
            var spike = VisualBlock(name + "_Spike_" + i, pos + new Vector3(sx, height + 1.8f, 0f), new Vector3(0.15f, 0.8f, 0.15f), metalMat);
            spike.transform.rotation = Quaternion.Euler(0f, 0f, (i - 2) * 6f);
        }

        // Боковые крылья/стены чтобы ворота не висели в воздухе
        float wingH = height * 0.6f;
        Block(name + "_Wing_L", pos + new Vector3(-width * 0.5f - pillarW * 0.5f - 2.2f, wingH * 0.5f, 0f), new Vector3(3.5f, wingH, 1.0f), stoneMat, _decorRoot, true, true);
        Block(name + "_Wing_R", pos + new Vector3(width * 0.5f + pillarW * 0.5f + 2.2f, wingH * 0.5f, 0f), new Vector3(3.5f, wingH, 1.0f), stoneMat, _decorRoot, true, true);

        // Черепа на столбах — крупные и заметные
        VisualBlock(name + "_Skull_L", pos + new Vector3(-width * 0.5f, height * 0.65f, pillarW * 0.55f), new Vector3(0.55f, 0.55f, 0.45f), boneMat);
        VisualBlock(name + "_Skull_R", pos + new Vector3(width * 0.5f, height * 0.65f, pillarW * 0.55f), new Vector3(0.55f, 0.55f, 0.45f), boneMat);
        // Дополнительные черепа ниже
        VisualBlock(name + "_Skull_L2", pos + new Vector3(-width * 0.5f, height * 0.35f, pillarW * 0.55f), new Vector3(0.45f, 0.45f, 0.38f), boneMat);
        VisualBlock(name + "_Skull_R2", pos + new Vector3(width * 0.5f, height * 0.35f, pillarW * 0.55f), new Vector3(0.45f, 0.45f, 0.38f), boneMat);

        // Висящие красные полотна/баннеры на столбах
        VisualBlock(name + "_Banner_L", pos + new Vector3(-width * 0.5f, height * 0.5f, -pillarW * 0.55f), new Vector3(0.9f, height * 0.5f, 0.06f), redCloth);
        VisualBlock(name + "_Banner_R", pos + new Vector3(width * 0.5f, height * 0.5f, -pillarW * 0.55f), new Vector3(0.9f, height * 0.5f, 0.06f), redCloth);

        // Руны на полу перед воротами
        for (int i = 0; i < 6; i++)
        {
            float ang = i * 60f;
            float r = 3.2f;
            Vector3 rp = pos + new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad) * r, 0.05f, Mathf.Sin(ang * Mathf.Deg2Rad) * r - 2.5f);
            var rm = VisualBlock(name + "_Rune_" + i, rp, new Vector3(0.18f, 0.04f, 0.9f), runeMat);
            rm.transform.rotation = Quaternion.Euler(0f, -ang, 0f);
            rm.AddComponent<LavaPulse>().pulseScale = 0.02f;
        }

        // Барьер (тёмная стена, блокирует проход)
        var barrier = Block(name + "_Barrier", pos + new Vector3(0f, height * 0.5f, 0f), new Vector3(width - 0.8f, height, 0.35f), _dark, _decorRoot, true, true);
        IgnoreNavMesh(barrier);

        // Тотемы — массивные, с несколькими секциями
        var totems = new GameObject[3];
        float[] totemX = { -width * 0.5f - 2.5f, width * 0.5f + 2.5f, 0f };
        float[] totemZ = { -2.0f, -2.0f, -4.0f };
        float totemH = height * 0.85f;

        for (int i = 0; i < 3; i++)
        {
            var totem = new GameObject(name + "_Totem_" + i);
            totem.transform.position = pos + new Vector3(totemX[i], 0f, totemZ[i]);
            totem.transform.SetParent(_decorRoot);
            IgnoreNavMesh(totem);

            // Каменное основание тотема
            var tBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tBase.name = "TotemPedestal";
            tBase.transform.SetParent(totem.transform);
            tBase.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            tBase.transform.localScale = new Vector3(1.1f, 0.7f, 1.1f);
            tBase.GetComponent<Renderer>().material = stoneMat;
            Destroy(tBase.GetComponent<Collider>());

            // Деревянный столб тотема — толстый
            var tPole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tPole.name = "TotemPole";
            tPole.transform.SetParent(totem.transform);
            tPole.transform.localPosition = new Vector3(0f, totemH * 0.5f + 0.5f, 0f);
            tPole.transform.localScale = new Vector3(0.75f, totemH, 0.75f);
            tPole.GetComponent<Renderer>().material = darkWood;
            Destroy(tPole.GetComponent<Collider>());

            // Декоративные кольца на тотеме
            for (int r2 = 0; r2 < 3; r2++)
            {
                float ry = 0.7f + r2 * (totemH * 0.3f);
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ring.name = "TotemRing_" + r2;
                ring.transform.SetParent(totem.transform);
                ring.transform.localPosition = new Vector3(0f, ry, 0f);
                ring.transform.localScale = new Vector3(0.95f, 0.15f, 0.95f);
                ring.GetComponent<Renderer>().material = r2 == 1 ? metalMat : boneMat;
                Destroy(ring.GetComponent<Collider>());
            }

            // Череп на тотеме
            var tSkull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tSkull.name = "TotemSkull";
            tSkull.transform.SetParent(totem.transform);
            tSkull.transform.localPosition = new Vector3(0f, totemH * 0.65f, 0.45f);
            tSkull.transform.localScale = new Vector3(0.42f, 0.42f, 0.35f);
            tSkull.GetComponent<Renderer>().material = boneMat;
            Destroy(tSkull.GetComponent<Collider>());

            // Светящийся орб наверху — крупный
            var orbObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbObj.name = "GlowOrb";
            orbObj.transform.SetParent(totem.transform);
            orbObj.transform.localPosition = new Vector3(0f, totemH + 0.6f, 0f);
            orbObj.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            Destroy(orbObj.GetComponent<Collider>());

            // Орб-подставка
            var orbBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            orbBase.name = "OrbCradle";
            orbBase.transform.SetParent(totem.transform);
            orbBase.transform.localPosition = new Vector3(0f, totemH + 0.1f, 0f);
            orbBase.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
            orbBase.GetComponent<Renderer>().material = metalMat;
            Destroy(orbBase.GetComponent<Collider>());

            totems[i] = totem;
        }

        // Костры по бокам — 4 штуки
        Fire(pos + new Vector3(-width * 0.5f - 1.0f, 0.1f, 0.8f));
        Fire(pos + new Vector3(width * 0.5f + 1.0f, 0.1f, 0.8f));
        Fire(pos + new Vector3(-width * 0.5f - 1.0f, 0.1f, -0.8f));
        Fire(pos + new Vector3(width * 0.5f + 1.0f, 0.1f, -0.8f));

        // Освещение — красное зловещее свечение
        LightAt(name + "_RedGlow_Top", pos + new Vector3(0f, height + 1.5f, 0f), new Color(1f, 0.10f, 0.02f), 2.5f, 22f);
        LightAt(name + "_RedGlow_Ground", pos + new Vector3(0f, 0.5f, -2.5f), new Color(1f, 0.15f, 0.04f), 1.5f, 12f);
        LightAt(name + "_WarmFire_L", pos + new Vector3(-width * 0.5f - 1.0f, 1.5f, 0f), new Color(1f, 0.55f, 0.12f), 1.2f, 8f);
        LightAt(name + "_WarmFire_R", pos + new Vector3(width * 0.5f + 1.0f, 1.5f, 0f), new Color(1f, 0.55f, 0.12f), 1.2f, 8f);

        var gate = root.AddComponent<RitualGate>();
        gate.trackedEnemies = enemies;
        gate.totems = totems;
        gate.barrier = barrier;

        return root;
    }

    void AddOuterBorders()
    {
        // Длинные борта не перекрывают путь, но не дают игроку случайно улететь за пределы карты.
        Block("Border_Left_Long", new Vector3(-30f, 3f, 145f), new Vector3(1.2f, 6f, 310f), _dark, _decorRoot, true, true);
        Block("Border_Right_Long", new Vector3(30f, 3f, 145f), new Vector3(1.2f, 6f, 310f), _dark, _decorRoot, true, true);
        Block("Border_Back", new Vector3(0f, 3f, -4f), new Vector3(60f, 6f, 1.2f), _dark, _decorRoot, true, true);
        Block("Border_End", new Vector3(0f, 5f, 300f), new Vector3(60f, 10f, 1.2f), _dark, _decorRoot, true, true);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ITEMS / DECOR / CHECKPOINTS
    // ─────────────────────────────────────────────────────────────────────
    void Rock(Vector3 pos, float size)
    {
        var g = Block("Rock", pos + Vector3.up * (size * 0.22f), new Vector3(size * 1.2f, size * 0.55f, size), _fort, _decorRoot, true, true);
        g.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
    }

    void Barricade(Vector3 pos, float yRot)
    {
        var root = new GameObject("Barricade");
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        root.transform.SetParent(_decorRoot);
        for (int i = -1; i <= 1; i++)
        {
            var plank = Block("Barricade_Plank", pos + new Vector3(i * 0.65f, i == 0 ? 0.15f : 0f, 0f), new Vector3(0.28f, 1.2f, 3.0f), _wood, root.transform, true, true);
            plank.transform.localRotation = Quaternion.Euler(0f, 0f, i * 12f);
        }
    }

    void DoorVisual(Vector3 pos, Vector3 scale)
    {
        // v8: обязательный путь НЕ закрываем. Это теперь открытая арка с распахнутыми створками.
        // Без коллайдера, без NavMesh-блокировки — игрок сразу видит, что надо идти внутрь.
        float leafW = scale.x * 0.36f;
        var left = Block("Fort_Gate_Open_Leaf_Left", pos + new Vector3(-scale.x * 0.62f, 0f, -0.25f), new Vector3(leafW, scale.y, 0.22f), _wood, _decorRoot, false, false);
        left.transform.rotation = Quaternion.Euler(0f, 58f, 0f);
        var right = Block("Fort_Gate_Open_Leaf_Right", pos + new Vector3(scale.x * 0.62f, 0f, -0.25f), new Vector3(leafW, scale.y, 0.22f), _wood, _decorRoot, false, false);
        right.transform.rotation = Quaternion.Euler(0f, -58f, 0f);

        Block("Fort_Gate_Gold_Path_Marker", pos + new Vector3(0f, -scale.y * 0.48f, -0.05f), new Vector3(scale.x * 0.85f, 0.08f, 2.6f), _gold, _decorRoot, false, false);
    }

    void Battlements(float z, float xFrom, float xTo, float y, Material mat)
    {
        int k = 0;
        for (float x = xFrom; x <= xTo; x += 1.6f)
        {
            Block("Battlement_" + k++, new Vector3(x, y, z), new Vector3(0.9f, 0.9f, 1.2f), mat, _decorRoot, true, true);
        }
    }

    void ZoneSign(string text, Vector3 pos, Color color)
    {
        var m = MakeMat("SignMat_" + text, color * 0.75f, color, true, color, 0.35f);
        Block("Sign_" + text, pos, new Vector3(Mathf.Max(3f, text.Length * 0.22f), 0.75f, 0.16f), m, _decorRoot, false, true);
        Block("SignPole_" + text, pos - Vector3.up * 1.45f, new Vector3(0.15f, 2.8f, 0.15f), _wood, _decorRoot, false, true);
    }

    void CoinLine(Vector3 start, int count, float xStep, float zStep)
    {
        for (int i = 0; i < count; i++)
            Coin(start + new Vector3(i * xStep, 0f, i * zStep));
    }

    void Coin(Vector3 pos)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        g.name = "Coin";
        g.transform.position = pos;
        g.transform.localScale = new Vector3(0.32f, 0.07f, 0.32f);
        g.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        g.transform.SetParent(_itemsRoot);
        ApplyMaterial(g, _gold, new Vector3(1f, 1f, 1f));
        var c = g.GetComponent<Collider>();
        if (c) c.isTrigger = true;
        g.AddComponent<Collectible>();

        // Тёплый PointLight — монета заметна издалека
        var glowGO = new GameObject("CoinGlow");
        glowGO.transform.SetParent(g.transform);
        glowGO.transform.localPosition = Vector3.up * 0.3f;
        var lt = glowGO.AddComponent<Light>();
        lt.type      = LightType.Point;
        lt.color     = new Color(1f, 0.88f, 0.25f);
        lt.intensity = 0.5f;
        lt.range     = 1.6f;

        IgnoreNavMesh(g);
    }

    void HealthPack(Vector3 pos)
    {
        var root = new GameObject("HealthPack");
        root.transform.position = pos;
        root.transform.SetParent(_itemsRoot);

        var mat  = MakeMat("HP_Mat",    new Color(0.08f, 0.72f, 0.22f), new Color(0.18f, 0.95f, 0.35f), true, new Color(0.06f, 1f, 0.18f), 0.40f);
        var wMat = MakeMat("HP_Cross",  new Color(0.90f, 0.95f, 0.90f), new Color(1f, 1f, 1f),           true, new Color(1f, 1f, 1f),       0.20f);

        // Основной куб
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "HP_Body";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one * 0.48f;
        ApplyMaterial(body, mat, Vector3.one);
        Object.Destroy(body.GetComponent<Collider>());
        IgnoreNavMesh(body);

        // Крест горизонталь
        var h = GameObject.CreatePrimitive(PrimitiveType.Cube);
        h.name = "HP_H";
        h.transform.SetParent(root.transform);
        h.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        h.transform.localScale = new Vector3(0.44f, 0.12f, 0.10f);
        ApplyMaterial(h, wMat, Vector3.one);
        Object.Destroy(h.GetComponent<Collider>());
        IgnoreNavMesh(h);

        // Крест вертикаль
        var v = GameObject.CreatePrimitive(PrimitiveType.Cube);
        v.name = "HP_V";
        v.transform.SetParent(root.transform);
        v.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        v.transform.localScale = new Vector3(0.10f, 0.12f, 0.44f);
        ApplyMaterial(v, wMat, Vector3.one);
        Object.Destroy(v.GetComponent<Collider>());
        IgnoreNavMesh(v);

        // Коллайдер на рут — триггер сбора
        var bc = root.AddComponent<BoxCollider>();
        bc.size = Vector3.one * 0.7f;
        bc.isTrigger = true;

        var col = root.AddComponent<Collectible>();
        col.isHealth = true;
        col.healAmt  = 30;
        IgnoreNavMesh(root);
    }

    void Trap_(string name, Vector3 pos)
    {
        // v10: ловушка стала очевидной: большая рамка, красный крест, шипы, фонарь-предупреждение.
        // Она всегда опасная, без непонятного режима "то работает, то нет".
        var root = new GameObject(name + "_READABLE_TRAP");
        root.transform.position = pos;
        root.transform.SetParent(_itemsRoot);
        IgnoreNavMesh(root);

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0.38f, 0f);
        trigger.size = new Vector3(3.35f, 0.9f, 3.35f);

        var trap = root.AddComponent<Trap>();
        trap.damage = 25;
        trap.tickDelay = 0.55f;
        trap.alwaysActive = true;

        // Основание и опасный красный крест.
        Block(name + "_BasePlate", pos + new Vector3(0f, 0.02f, 0f), new Vector3(3.4f, 0.12f, 3.4f), _dark, root.transform, false, false);
        Block(name + "_HotCross_A", pos + new Vector3(0f, 0.12f, 0f), new Vector3(3.05f, 0.06f, 0.34f), _lava, root.transform, false, false);
        Block(name + "_HotCross_B", pos + new Vector3(0f, 0.13f, 0f), new Vector3(0.34f, 0.06f, 3.05f), _lava, root.transform, false, false);

        // Золотая/жёлтая рамка по краю, чтобы ловушку было видно издалека.
        Block(name + "_WarningFrame_N", pos + new Vector3(0f, 0.22f, 1.75f), new Vector3(3.7f, 0.18f, 0.22f), _gold, root.transform, false, false);
        Block(name + "_WarningFrame_S", pos + new Vector3(0f, 0.22f, -1.75f), new Vector3(3.7f, 0.18f, 0.22f), _gold, root.transform, false, false);
        Block(name + "_WarningFrame_L", pos + new Vector3(-1.75f, 0.22f, 0f), new Vector3(0.22f, 0.18f, 3.7f), _gold, root.transform, false, false);
        Block(name + "_WarningFrame_R", pos + new Vector3(1.75f, 0.22f, 0f), new Vector3(0.22f, 0.18f, 3.7f), _gold, root.transform, false, false);

        // Шипы/колья — простые, но читаемые.
        for (int ix = -1; ix <= 1; ix++)
        {
            for (int iz = -1; iz <= 1; iz++)
            {
                if (ix == 0 && iz == 0) continue;
                var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spike.name = name + "_Spike";
                spike.transform.position = pos + new Vector3(ix * 0.72f, 0.46f, iz * 0.72f);
                spike.transform.localScale = new Vector3(0.16f, 0.42f, 0.16f);
                spike.transform.SetParent(root.transform);
                ApplyMaterial(spike, _blood, Vector3.one);
                var sc = spike.GetComponent<Collider>();
                if (sc) Destroy(sc);
                IgnoreNavMesh(spike);
            }
        }

        // Четыре маленьких предупреждающих фонаря.
        Vector3[] lamps = {
            new Vector3(-1.55f, 0.55f, -1.55f), new Vector3(1.55f, 0.55f, -1.55f),
            new Vector3(-1.55f, 0.55f,  1.55f), new Vector3(1.55f, 0.55f,  1.55f)
        };
        for (int i = 0; i < lamps.Length; i++)
        {
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = name + "_WarningLamp";
            lamp.transform.position = pos + lamps[i];
            lamp.transform.localScale = Vector3.one * 0.28f;
            lamp.transform.SetParent(root.transform);
            ApplyMaterial(lamp, _lava, Vector3.one);
            var lc = lamp.GetComponent<Collider>();
            if (lc) Destroy(lc);
            IgnoreNavMesh(lamp);
        }

        LightAt(name + "_Readable_Red_Warning_Light", pos + Vector3.up * 1.1f, new Color(1f, 0.10f, 0.02f), 1.65f, 7.5f);
    }

    void Checkpoint(Vector3 pos)
    {
        var root = new GameObject("Checkpoint");
        root.transform.position = pos;
        root.transform.SetParent(_itemsRoot);

        var trigger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trigger.name = "Checkpoint_Trigger";
        trigger.transform.SetParent(root.transform);
        trigger.transform.localPosition = Vector3.zero;
        trigger.transform.localScale = new Vector3(1.1f, 0.12f, 1.1f);
        ApplyMaterial(trigger, _gold, Vector3.one);
        var col = trigger.GetComponent<Collider>();
        if (col) col.isTrigger = true;
        IgnoreNavMesh(trigger);

        var pole = Block("Checkpoint_Pole", pos + Vector3.up * 1.1f, new Vector3(0.12f, 2.2f, 0.12f), _wood, _itemsRoot, false, true);
        var flag = Block("Checkpoint_Flag", pos + new Vector3(0.45f, 1.85f, 0f), new Vector3(0.9f, 0.55f, 0.08f), _gold, _itemsRoot, false, true);
        var cp = trigger.AddComponent<Checkpoint>();
        cp.flag = flag.GetComponent<Renderer>();
        LightAt("Checkpoint_Light", pos + Vector3.up * 1.4f, new Color(0.3f, 1f, 0.4f), 1.0f, 8f);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PLAYER / MANAGERS
    // ─────────────────────────────────────────────────────────────────────
    void Player()
    {
        var p = new GameObject("Player");
        p.tag = "Player";
        p.transform.position = new Vector3(0f, 1.2f, 2f);

        var cc = p.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.stepOffset = 0.55f;
        cc.slopeLimit = 52f;

        var ph = p.AddComponent<PlayerHealth>();
        p.AddComponent<PlayerController>();
        p.AddComponent<PauseMenu>();

        // v9: никаких загрузок с чекпоинта. Каждый запуск сцены начинается строго со старта.
        ph.respawnPoint = p.transform.position;

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(p.transform);
        camGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 80f;
        cam.nearClipPlane = 0.05f;
        cam.backgroundColor = new Color(0.045f, 0.035f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();
        var shake = camGO.AddComponent<CameraShake>();
        ph.shake = shake;
        camGO.AddComponent<MouseLook>();
        camGO.AddComponent<WeaponController>();
    }

    void Managers()
    {
        if (GameManager.Instance == null)
            new GameObject("GM").AddComponent<GameManager>();
        if (Object.FindFirstObjectByType<FeedbackManager>() == null)
            new GameObject("FB").AddComponent<FeedbackManager>();
        if (RunScoreManager.Instance == null)
        {
            var sm = new GameObject("RunScoreManager").AddComponent<RunScoreManager>();
            sm.StartRun();
        }
        if (SceneFader.Instance == null)
            new GameObject("SceneFader").AddComponent<SceneFader>();
        SceneFader.FadeIn();
        if (GameAudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<GameAudioManager>();
        if (KingAbility.Instance == null)
        {
            var ka = new GameObject("KingAbility").AddComponent<KingAbility>();
            ka.unlocked = true;
            ka.charge = 0.6f;
        }
        else if (!KingAbility.Instance.unlocked)
        {
            KingAbility.Instance.unlocked = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // NAVMESH
    // ─────────────────────────────────────────────────────────────────────
    IEnumerator BakeNavMesh()
    {
        yield return null;
        var surfGO = new GameObject("Runtime_NavMeshSurface");
        surfGO.transform.SetParent(_worldRoot);
        var surf = surfGO.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.All;
        surf.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surf.BuildNavMesh();

        yield return new WaitForSeconds(0.2f);

        foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            ai.OnNavMeshReady();
    }

    // ─────────────────────────────────────────────────────────────────────
    // ENEMIES — родные процедурные орки проекта
    // ─────────────────────────────────────────────────────────────────────
    GameObject OrcGrunt(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 3.0f, 10, 42, 13f, 1.85f);
        Color skin = new Color(0.22f, 0.58f, 0.18f);
        Color dark = new Color(0.13f, 0.36f, 0.10f);
        Color leather = new Color(0.34f, 0.22f, 0.12f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Torso_LeatherVest", new Vector3(0f, .58f, .06f), new Vector3(.82f, .62f, .56f), Quaternion.Euler(7f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Vest", new Vector3(0f, .58f, .36f), new Vector3(.62f, .45f, .08f), Quaternion.identity, leather);
        var head = Prim(PrimitiveType.Cube, root.transform, "Head_Blocky", new Vector3(0f, 1.08f, .05f), new Vector3(.62f, .56f, .58f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Heavy_Jaw", new Vector3(0f, .80f, .11f), new Vector3(.52f, .20f, .48f), Quaternion.identity, dark);
        AddTusks(root.transform, .86f, .29f, .075f);
        AddPointyEars(root.transform, skin, 1.12f, .38f);
        AddOrcEyes(root.transform, new Vector3(.16f, 1.11f, .33f), new Color(1f, .85f, .05f));
        AddOrcEyes(root.transform, new Vector3(-.16f, 1.11f, .33f), new Color(1f, .85f, .05f));
        Prim(PrimitiveType.Sphere, root.transform, "Shoulder_L", new Vector3(.52f, .76f, 0f), new Vector3(.30f, .28f, .28f), Quaternion.identity, leather);
        Prim(PrimitiveType.Sphere, root.transform, "Shoulder_R", new Vector3(-.52f, .76f, 0f), new Vector3(.30f, .28f, .28f), Quaternion.identity, leather);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(.62f, .35f, .05f), new Vector3(.14f, .34f, .14f), Quaternion.Euler(0f, 0f, -14f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-.62f, .35f, .05f), new Vector3(.14f, .34f, .14f), Quaternion.Euler(0f, 0f, 14f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.20f, -.35f, 0f), new Vector3(.19f, .38f, .19f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.20f, -.35f, 0f), new Vector3(.19f, .38f, .19f), Quaternion.identity, dark);
        AddClub(root.transform, new Vector3(-.78f, .20f, .22f), 0.75f, false);
        SetBodyParts(root, torso, head);
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.OrcWarrior;
        return root;
    }

    GameObject OrcShieldGuard(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 2.55f, 14, 80, 14f, 2.05f);
        root.transform.localScale = Vector3.one * 1.18f;
        Color skin = new Color(0.18f, 0.47f, 0.14f);
        Color armor = new Color(0.34f, 0.34f, 0.38f);
        Color gold = new Color(.74f, .58f, .12f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Guard_Torso_Armor", new Vector3(0f, .65f, .06f), new Vector3(.92f, .72f, .64f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Chest_Plate", new Vector3(0f, .66f, .39f), new Vector3(.82f, .62f, .11f), Quaternion.identity, armor);
        var head = Prim(PrimitiveType.Cube, root.transform, "Guard_Head", new Vector3(0f, 1.22f, .06f), new Vector3(.66f, .60f, .62f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Helmet_Brow", new Vector3(0f, 1.52f, .10f), new Vector3(.78f, .18f, .65f), Quaternion.identity, armor);
        AddTusks(root.transform, .97f, .34f, .085f);
        AddPointyEars(root.transform, skin, 1.23f, .41f);
        AddOrcEyes(root.transform, new Vector3(.18f, 1.25f, .36f), new Color(1f, .65f, .05f));
        AddOrcEyes(root.transform, new Vector3(-.18f, 1.25f, .36f), new Color(1f, .65f, .05f));
        Prim(PrimitiveType.Cube, root.transform, "Pauldron_L", new Vector3(.70f, .94f, .02f), new Vector3(.42f, .24f, .48f), Quaternion.Euler(0f, 0f, 16f), armor);
        Prim(PrimitiveType.Cube, root.transform, "Pauldron_R", new Vector3(-.70f, .94f, .02f), new Vector3(.42f, .24f, .48f), Quaternion.Euler(0f, 0f, -16f), armor);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(.78f, .42f, .08f), new Vector3(.18f, .42f, .18f), Quaternion.Euler(0f, 0f, -12f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-.78f, .42f, .08f), new Vector3(.18f, .42f, .18f), Quaternion.Euler(0f, 0f, 12f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.25f, -.42f, 0f), new Vector3(.23f, .44f, .23f), Quaternion.identity, new Color(.12f, .30f, .09f));
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.25f, -.42f, 0f), new Vector3(.23f, .44f, .23f), Quaternion.identity, new Color(.12f, .30f, .09f));
        AddShield(root.transform, new Vector3(.98f, .42f, .37f), armor, gold);
        AddSpear(root.transform, new Vector3(-.85f, .58f, .28f), 1.65f, gold);
        SetBodyParts(root, torso, head);
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.Elite;
        return root;
    }

    GameObject OrcArcher(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 3.25f, 9, 48, 22f, 1.75f);
        root.transform.localScale = Vector3.one * 0.92f;
        Color skin = new Color(0.20f, 0.54f, 0.17f);
        Color leather = new Color(0.30f, 0.18f, 0.09f);
        Color arrowGlow = new Color(1f, 0.72f, 0.12f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Scout_Torso", new Vector3(0f, .56f, .06f), new Vector3(.70f, .56f, .48f), Quaternion.Euler(8f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Quiver", new Vector3(.34f, .80f, -.28f), new Vector3(.22f, .80f, .18f), Quaternion.Euler(-18f, 0f, 20f), leather);
        var head = Prim(PrimitiveType.Cube, root.transform, "Scout_Head", new Vector3(0f, 1.02f, .06f), new Vector3(.55f, .50f, .52f), Quaternion.Euler(5f, 0f, 0f), skin);
        AddTusks(root.transform, .80f, .27f, .06f);
        AddPointyEars(root.transform, skin, 1.03f, .34f);
        AddOrcEyes(root.transform, new Vector3(.14f, 1.04f, .31f), arrowGlow);
        AddOrcEyes(root.transform, new Vector3(-.14f, 1.04f, .31f), arrowGlow);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.18f, -.34f, 0f), new Vector3(.16f, .34f, .16f), Quaternion.identity, new Color(.13f, .34f, .1f));
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.18f, -.34f, 0f), new Vector3(.16f, .34f, .16f), Quaternion.identity, new Color(.13f, .34f, .1f));
        AddBow(root.transform, new Vector3(-.70f, .55f, .25f), leather);
        AddDagger(root.transform, new Vector3(.54f, .30f, .28f));
        GlowOrb(root.transform, "Arrow_Ready_Glow", new Vector3(-.70f, .55f, .50f), new Vector3(.12f, .12f, .12f), arrowGlow, 0.8f);
        SetBodyParts(root, torso, head);
        ConfigureRanged(root, 8, 19f, 2.15f, 16.5f, arrowGlow, "ВЫСТРЕЛ!");
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.OrcArcher;
        return root;
    }

    GameObject OrcBerserker(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 3.75f, 20, 90, 15.5f, 2.15f);
        root.transform.localScale = Vector3.one * 1.28f;
        Color skin = new Color(.62f, .14f, .08f);
        Color dark = new Color(.38f, .06f, .04f);
        Color iron = new Color(.22f, .22f, .24f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Berserker_Torso", new Vector3(0f, .66f, .08f), new Vector3(1.02f, .74f, .64f), Quaternion.Euler(6f, 0f, 0f), skin);
        Prim(PrimitiveType.Sphere, root.transform, "Berserker_Belly", new Vector3(0f, .42f, .15f), new Vector3(.62f, .42f, .50f), Quaternion.identity, dark);
        var head = Prim(PrimitiveType.Cube, root.transform, "Berserker_Head", new Vector3(0f, 1.22f, .06f), new Vector3(.72f, .64f, .66f), Quaternion.Euler(6f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Metal_Brow", new Vector3(0f, 1.50f, .09f), new Vector3(.76f, .14f, .62f), Quaternion.identity, iron);
        AddTusks(root.transform, .98f, .35f, .10f);
        AddPointyEars(root.transform, skin, 1.24f, .43f);
        AddOrcEyes(root.transform, new Vector3(.19f, 1.25f, .37f), new Color(1f, .22f, 0f));
        AddOrcEyes(root.transform, new Vector3(-.19f, 1.25f, .37f), new Color(1f, .22f, 0f));
        AddBackSpikes(root.transform, iron, 4);
        Prim(PrimitiveType.Sphere, root.transform, "Shoulder_L", new Vector3(.68f, .94f, 0f), new Vector3(.38f, .34f, .32f), Quaternion.identity, iron);
        Prim(PrimitiveType.Sphere, root.transform, "Shoulder_R", new Vector3(-.68f, .94f, 0f), new Vector3(.38f, .34f, .32f), Quaternion.identity, iron);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(.78f, .42f, .08f), new Vector3(.20f, .42f, .20f), Quaternion.Euler(0f, 0f, -18f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-.78f, .42f, .08f), new Vector3(.20f, .42f, .20f), Quaternion.Euler(0f, 0f, 18f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.26f, -.44f, 0f), new Vector3(.25f, .45f, .25f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.26f, -.44f, 0f), new Vector3(.25f, .45f, .25f), Quaternion.identity, dark);
        AddAxe(root.transform, new Vector3(-.95f, .20f, .30f), 1.25f, true);
        SetBodyParts(root, torso, head);
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.Elite;
        return root;
    }

    GameObject OrcShaman(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 2.35f, 18, 95, 19f, 2.25f);
        root.transform.localScale = Vector3.one * 1.18f;
        Color skin = new Color(.16f, .46f, .18f);
        Color robe = new Color(.25f, .05f, .18f);
        Color glow = new Color(0.85f, 0.15f, 1f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Shaman_Robe", new Vector3(0f, .62f, .06f), new Vector3(.88f, .90f, .58f), Quaternion.Euler(4f, 0f, 0f), robe);
        var head = Prim(PrimitiveType.Cube, root.transform, "Shaman_Head", new Vector3(0f, 1.28f, .06f), new Vector3(.64f, .58f, .60f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Bone_Mask", new Vector3(0f, 1.28f, .40f), new Vector3(.46f, .26f, .08f), Quaternion.identity, new Color(.86f, .78f, .55f));
        AddTusks(root.transform, .99f, .34f, .075f);
        AddPointyEars(root.transform, skin, 1.28f, .40f);
        AddOrcEyes(root.transform, new Vector3(.17f, 1.31f, .44f), glow);
        AddOrcEyes(root.transform, new Vector3(-.17f, 1.31f, .44f), glow);
        AddTotemStaff(root.transform, new Vector3(-.82f, .72f, .22f), glow);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.22f, -.42f, 0f), new Vector3(.19f, .42f, .19f), Quaternion.identity, new Color(.10f, .25f, .10f));
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.22f, -.42f, 0f), new Vector3(.19f, .42f, .19f), Quaternion.identity, new Color(.10f, .25f, .10f));
        GlowOrb(root.transform, "Shaman_Aura", new Vector3(0f, .25f, .05f), new Vector3(1.4f, .04f, 1.4f), glow, 0.9f).AddComponent<LavaPulse>();
        SetBodyParts(root, torso, head);
        ConfigureRanged(root, 14, 17f, 2.75f, 10.5f, glow, "ШАР!");
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.Elite;
        return root;
    }

    GameObject OrcWarlord(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 2.85f, 26, 145, 18f, 2.55f);
        root.transform.localScale = Vector3.one * 1.55f;
        Color skin = new Color(.20f, .50f, .14f);
        Color dark = new Color(.12f, .30f, .08f);
        Color gold = new Color(.75f, .58f, .08f);
        Color iron = new Color(.25f, .25f, .30f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "Warlord_Torso", new Vector3(0f, .68f, .08f), new Vector3(1.08f, .84f, .74f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Golden_Chest", new Vector3(0f, .70f, .42f), new Vector3(.92f, .68f, .14f), Quaternion.identity, gold);
        var head = Prim(PrimitiveType.Cube, root.transform, "Warlord_Head", new Vector3(0f, 1.30f, .1f), new Vector3(.76f, .72f, .74f), Quaternion.Euler(7f, 0f, 0f), skin);
        AddCrownHelmet(root.transform, gold, iron, 1.66f);
        AddTusks(root.transform, 1.02f, .38f, .11f);
        AddPointyEars(root.transform, skin, 1.31f, .45f);
        AddOrcEyes(root.transform, new Vector3(.20f, 1.32f, .43f), new Color(1f, .05f, 0f));
        AddOrcEyes(root.transform, new Vector3(-.20f, 1.32f, .43f), new Color(1f, .05f, 0f));
        Prim(PrimitiveType.Cube, root.transform, "Pad_L", new Vector3(.72f, .98f, .05f), new Vector3(.42f, .26f, .46f), Quaternion.Euler(0f, 0f, 15f), gold);
        Prim(PrimitiveType.Cube, root.transform, "Pad_R", new Vector3(-.72f, .98f, .05f), new Vector3(.42f, .26f, .46f), Quaternion.Euler(0f, 0f, -15f), gold);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(.78f, .43f, .1f), new Vector3(.22f, .44f, .22f), Quaternion.Euler(0f, 0f, -16f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-.78f, .43f, .1f), new Vector3(.22f, .44f, .22f), Quaternion.Euler(0f, 0f, 16f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(.28f, -.47f, 0f), new Vector3(.28f, .49f, .28f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-.28f, -.47f, 0f), new Vector3(.28f, .49f, .28f), Quaternion.identity, dark);
        AddHammer(root.transform, new Vector3(-.98f, .15f, .34f), 1.35f, gold, iron);
        SetBodyParts(root, torso, head);
        root.GetComponent<EnemyAI>().enemyScoreType = RunScoreManager.EnemyType.Elite;
        return root;
    }

    GameObject OrcForgeMaster(string name, Vector3 pos, Vector3[] wps)
    {
        var root = OrcWarlord(name, pos, wps);
        root.name = name;
        var ai = root.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.maxHp = 180;
            ai.damage = 30;
            ai.chaseRange = 22f;
        }
        Color forge = new Color(1f, .22f, .02f);
        GlowOrb(root.transform, "Forge_Master_Core", new Vector3(0f, .78f, .52f), new Vector3(.45f, .45f, .08f), forge, 1.25f);
        ConfigureRanged(root, 18, 15.5f, 3.0f, 12f, forge, "ОГОНЬ!");
        return root;
    }

    GameObject OrcFinalBoss(string name, Vector3 pos, Vector3[] wps)
    {
        var root = MakeOrcRoot(name, pos, wps, 2.55f, 42, 550, 26f, 3.0f);
        root.transform.localScale = Vector3.one * 2.35f;
        Color skin = new Color(.18f, .42f, .12f);
        Color gold = new Color(.86f, .66f, .10f);
        Color black = new Color(.05f, .04f, .05f);
        Color red = new Color(1f, .05f, .02f);
        Color iron = new Color(.23f, .23f, .28f);
        var torso = Prim(PrimitiveType.Cube, root.transform, "BOSS_Massive_Torso", new Vector3(0f, .74f, .08f), new Vector3(1.28f, .98f, .84f), Quaternion.Euler(4f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "BOSS_Golden_Armor", new Vector3(0f, .78f, .50f), new Vector3(1.10f, .82f, .16f), Quaternion.identity, gold);
        var head = Prim(PrimitiveType.Cube, root.transform, "BOSS_Head", new Vector3(0f, 1.48f, .12f), new Vector3(.90f, .82f, .82f), Quaternion.Euler(6f, 0f, 0f), skin);
        AddCrownHelmet(root.transform, gold, iron, 1.92f);
        AddGiantHorns(root.transform, gold);
        AddTusks(root.transform, 1.18f, .48f, .15f);
        AddPointyEars(root.transform, skin, 1.50f, .55f);
        AddOrcEyes(root.transform, new Vector3(.25f, 1.52f, .50f), red);
        AddOrcEyes(root.transform, new Vector3(-.25f, 1.52f, .50f), red);
        Prim(PrimitiveType.Cube, root.transform, "BOSS_Shoulder_L", new Vector3(.92f, 1.08f, .04f), new Vector3(.62f, .34f, .58f), Quaternion.Euler(0f, 0f, 14f), gold);
        Prim(PrimitiveType.Cube, root.transform, "BOSS_Shoulder_R", new Vector3(-.92f, 1.08f, .04f), new Vector3(.62f, .34f, .58f), Quaternion.Euler(0f, 0f, -14f), gold);
        Prim(PrimitiveType.Cylinder, root.transform, "BOSS_Arm_L", new Vector3(.98f, .45f, .10f), new Vector3(.30f, .55f, .30f), Quaternion.Euler(0f, 0f, -14f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "BOSS_Arm_R", new Vector3(-.98f, .45f, .10f), new Vector3(.30f, .55f, .30f), Quaternion.Euler(0f, 0f, 14f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "BOSS_Leg_L", new Vector3(.36f, -.55f, 0f), new Vector3(.34f, .56f, .34f), Quaternion.identity, black);
        Prim(PrimitiveType.Cylinder, root.transform, "BOSS_Leg_R", new Vector3(-.36f, -.55f, 0f), new Vector3(.34f, .56f, .34f), Quaternion.identity, black);
        AddHammer(root.transform, new Vector3(-1.22f, .12f, .42f), 1.95f, gold, iron);
        AddBossCape(root.transform, black, red);
        GlowOrb(root.transform, "BOSS_Red_Aura_Ring", new Vector3(0f, .10f, 0f), new Vector3(2.4f, .06f, 2.4f), red, 1.2f).AddComponent<LavaPulse>();
        GlowOrb(root.transform, "BOSS_Chest_Core", new Vector3(0f, .93f, .61f), new Vector3(.45f, .45f, .10f), red, 1.8f).AddComponent<LavaPulse>();
        LightAt("BOSS_PERSONAL_RED_LIGHT", pos + Vector3.up * 4.1f, new Color(1f, .12f, .02f), 4.5f, 26f);

        var ai = root.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isFinalBoss = true;
            ai.enemyScoreType = RunScoreManager.EnemyType.Boss;
            ai.maxHp = 550;
            ai.damage = 42;
            ai.chaseRange = 28f;
            ai.attackRange = 3.2f;
            ai.cooldown = 1.25f;
            ai.useRangedAttack = true;
            ai.rangedDamage = 22;
            ai.rangedRange = 22f;
            ai.rangedCooldown = 3.0f;
            ai.rangedProjectileSpeed = 11f;
            ai.rangedProjectileColor = new Color(1f, 0.08f, 0.02f);
            ai.rangedAttackLabel = "ОГНЕННЫЙ УДАР!";
        }
        var nav = root.GetComponent<NavMeshAgent>();
        if (nav != null)
        {
            nav.height = 3.6f;
            nav.radius = 0.8f;
            nav.stoppingDistance = 1.0f;
            nav.avoidancePriority = 20;
        }
        var col = root.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.height = 2.2f;
            col.radius = 0.55f;
            col.center = new Vector3(0f, 1.05f, 0f);
        }
        SetBodyParts(root, torso, head);
        return root;
    }

    GameObject MakeOrcRoot(string name, Vector3 pos, Vector3[] wps, float speed, int dmg, int hp, float chase, float atk)
    {
        var g = new GameObject(name);
        g.tag = "Enemy";
        g.transform.position = pos;
        g.transform.SetParent(_enemiesRoot);
        IgnoreNavMesh(g);

        var col = g.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.45f;
        col.center = new Vector3(0f, 0.9f, 0f);

        var nav = g.AddComponent<NavMeshAgent>();
        nav.speed = speed;
        nav.angularSpeed = 300f;
        nav.acceleration = 18f;
        nav.stoppingDistance = 0.45f;
        nav.height = 1.8f;
        nav.radius = 0.4f;
        nav.avoidancePriority = Random.Range(30, 70);
        nav.autoRepath = true;

        var ai = g.AddComponent<EnemyAI>();
        ai.damage = dmg;
        ai.maxHp = hp;
        ai.chaseRange = chase;
        ai.attackRange = atk;
        g.AddComponent<EnemyHitDetector>();

        if (wps != null && wps.Length > 0)
        {
            ai.waypoints = new Transform[wps.Length];
            for (int i = 0; i < wps.Length; i++)
            {
                var wp = new GameObject(name + "_WP_" + i);
                wp.transform.position = wps[i];
                wp.transform.SetParent(_waypointsRoot);
                ai.waypoints[i] = wp.transform;
            }
        }
        else
        {
            ai.waypoints = new Transform[0];
        }

        return g;
    }


    void ConfigureRanged(GameObject root, int dmg, float range, float cd, float projectileSpeed, Color color, string label)
    {
        var ai = root.GetComponent<EnemyAI>();
        if (ai == null) return;

        ai.useRangedAttack = true;
        ai.rangedDamage = dmg;
        ai.rangedRange = range;
        ai.rangedCooldown = cd;
        ai.rangedProjectileSpeed = projectileSpeed;
        ai.rangedProjectileColor = color;
        ai.rangedAttackLabel = label;

        // Дальники не должны лезть в лицо игроку как обычные орки.
        ai.attackRange = Mathf.Max(ai.attackRange, 2.0f);
        ai.chaseRange = Mathf.Max(ai.chaseRange, range + 2f);

        var nav = root.GetComponent<NavMeshAgent>();
        if (nav != null)
        {
            nav.stoppingDistance = 0.8f;
            nav.avoidancePriority = Mathf.Min(nav.avoidancePriority, 35);
        }
    }

    void SetBodyParts(GameObject root, params GameObject[] parts)
    {
        var ai = root.GetComponent<EnemyAI>();
        if (ai != null) ai.bodyParts = parts;
    }

    void AddTusks(Transform root, float y, float z, float size)
    {
        Prim(PrimitiveType.Cube, root, "Tusk_L", new Vector3(.18f, y, z), new Vector3(size, size * 2.6f, size), Quaternion.Euler(-12f, 0f, 10f), Color.white);
        Prim(PrimitiveType.Cube, root, "Tusk_R", new Vector3(-.18f, y, z), new Vector3(size, size * 2.6f, size), Quaternion.Euler(-12f, 0f, -10f), Color.white);
    }

    void AddPointyEars(Transform root, Color color, float y, float x)
    {
        Prim(PrimitiveType.Cube, root, "Ear_L", new Vector3(x, y, .02f), new Vector3(.14f, .30f, .10f), Quaternion.Euler(0f, 0f, -35f), color);
        Prim(PrimitiveType.Cube, root, "Ear_R", new Vector3(-x, y, .02f), new Vector3(.14f, .30f, .10f), Quaternion.Euler(0f, 0f, 35f), color);
    }

    void AddOrcEyes(Transform root, Vector3 pos, Color color)
    {
        var e = GlowOrb(root, "Eye_Glow", pos, Vector3.one * .13f, color, 1.2f);
        Prim(PrimitiveType.Sphere, e.transform, "Pupil", new Vector3(0f, 0f, .55f), Vector3.one * .48f, Quaternion.identity, Color.black);
    }

    void AddBackSpikes(Transform root, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = -0.36f + i * (0.72f / Mathf.Max(1, count - 1));
            Prim(PrimitiveType.Cube, root, "Back_Spike_" + i, new Vector3(x, .98f, -.38f), new Vector3(.13f, .45f, .13f), Quaternion.Euler(-26f, 0f, 0f), color);
        }
    }

    void AddShield(Transform root, Vector3 pos, Color armor, Color emblem)
    {
        Prim(PrimitiveType.Cube, root, "Shield_Plate", pos, new Vector3(.12f, .72f, .56f), Quaternion.Euler(0f, 10f, 0f), armor);
        Prim(PrimitiveType.Cube, root, "Shield_Emblem", pos + new Vector3(.07f, 0f, .01f), new Vector3(.05f, .34f, .30f), Quaternion.Euler(0f, 10f, 0f), emblem);
    }

    void AddSpear(Transform root, Vector3 pos, float len, Color tip)
    {
        Prim(PrimitiveType.Cylinder, root, "Spear_Handle", pos, new Vector3(.055f, len * .5f, .055f), Quaternion.Euler(0f, 0f, -18f), new Color(.38f, .23f, .10f));
        Prim(PrimitiveType.Cube, root, "Spear_Tip", pos + new Vector3(-.22f, len * .42f, .05f), new Vector3(.16f, .34f, .16f), Quaternion.Euler(0f, 0f, -18f), tip);
    }

    void AddClub(Transform root, Vector3 pos, float len, bool heavy)
    {
        Prim(PrimitiveType.Cylinder, root, "Club_Handle", pos, new Vector3(.075f, len * .45f, .075f), Quaternion.Euler(35f, 0f, 24f), new Color(.34f, .20f, .09f));
        Prim(PrimitiveType.Cube, root, "Club_Head", pos + new Vector3(-.22f, -.36f, .16f), heavy ? new Vector3(.30f, .22f, .24f) : new Vector3(.22f, .16f, .18f), Quaternion.Euler(0f, 0f, 24f), new Color(.30f, .25f, .20f));
    }

    void AddAxe(Transform root, Vector3 pos, float len, bool twoHanded)
    {
        Prim(PrimitiveType.Cylinder, root, "Axe_Handle", pos, new Vector3(.075f, len * .50f, .075f), Quaternion.Euler(38f, 0f, 25f), new Color(.38f, .22f, .10f));
        Prim(PrimitiveType.Cube, root, "Axe_Blade", pos + new Vector3(-.28f, -.42f, .18f), new Vector3(twoHanded ? .42f : .30f, .18f, .18f), Quaternion.Euler(0f, 0f, 25f), new Color(.62f, .65f, .70f));
    }

    void AddHammer(Transform root, Vector3 pos, float len, Color accent, Color metal)
    {
        Prim(PrimitiveType.Cylinder, root, "Hammer_Handle", pos, new Vector3(.085f, len * .52f, .085f), Quaternion.Euler(40f, 0f, 28f), new Color(.45f, .28f, .12f));
        Prim(PrimitiveType.Cube, root, "Hammer_Head", pos + new Vector3(-.36f, -.48f, .24f), new Vector3(.52f, .30f, .28f), Quaternion.Euler(0f, 0f, 28f), metal);
        Prim(PrimitiveType.Cube, root, "Hammer_Gold_Band", pos + new Vector3(-.36f, -.48f, .24f), new Vector3(.12f, .36f, .32f), Quaternion.Euler(0f, 0f, 28f), accent);
    }

    void AddBow(Transform root, Vector3 pos, Color wood)
    {
        Prim(PrimitiveType.Cylinder, root, "Bow_Upper", pos + new Vector3(0f, .22f, 0f), new Vector3(.045f, .42f, .045f), Quaternion.Euler(18f, 0f, -20f), wood);
        Prim(PrimitiveType.Cylinder, root, "Bow_Lower", pos + new Vector3(0f, -.22f, 0f), new Vector3(.045f, .42f, .045f), Quaternion.Euler(-18f, 0f, -20f), wood);
        Prim(PrimitiveType.Cube, root, "Bow_String", pos, new Vector3(.025f, .86f, .025f), Quaternion.Euler(0f, 0f, -20f), new Color(.85f, .78f, .55f));
    }

    void AddDagger(Transform root, Vector3 pos)
    {
        Prim(PrimitiveType.Cube, root, "Dagger_Blade", pos, new Vector3(.08f, .34f, .07f), Quaternion.Euler(30f, 0f, -28f), new Color(.65f, .68f, .72f));
        Prim(PrimitiveType.Cube, root, "Dagger_Handle", pos + new Vector3(.08f, -.16f, 0f), new Vector3(.07f, .18f, .07f), Quaternion.Euler(30f, 0f, -28f), new Color(.35f, .21f, .08f));
    }

    void AddTotemStaff(Transform root, Vector3 pos, Color glow)
    {
        Prim(PrimitiveType.Cylinder, root, "Staff_Wood", pos, new Vector3(.06f, .78f, .06f), Quaternion.Euler(0f, 0f, -12f), new Color(.28f, .16f, .08f));
        GlowOrb(root, "Staff_Crystal", pos + new Vector3(-.14f, .72f, .03f), Vector3.one * .26f, glow, 1.6f);
        Prim(PrimitiveType.Cube, root, "Staff_Bone_A", pos + new Vector3(-.14f, .48f, .02f), new Vector3(.34f, .07f, .07f), Quaternion.Euler(0f, 0f, 18f), new Color(.85f, .78f, .60f));
    }

    void AddCrownHelmet(Transform root, Color gold, Color metal, float y)
    {
        Prim(PrimitiveType.Cube, root, "Helmet_Base", new Vector3(0f, y, .06f), new Vector3(.86f, .22f, .78f), Quaternion.identity, gold);
        for (int i = -1; i <= 1; i++)
            Prim(PrimitiveType.Cube, root, "Helmet_Spike_" + i, new Vector3(i * .29f, y + .30f, .06f), new Vector3(.12f, .34f, .12f), Quaternion.identity, metal);
    }

    void AddGiantHorns(Transform root, Color color)
    {
        Prim(PrimitiveType.Cylinder, root, "Boss_Horn_L", new Vector3(.48f, 2.04f, .10f), new Vector3(.13f, .55f, .13f), Quaternion.Euler(0f, 0f, -38f), color);
        Prim(PrimitiveType.Cylinder, root, "Boss_Horn_R", new Vector3(-.48f, 2.04f, .10f), new Vector3(.13f, .55f, .13f), Quaternion.Euler(0f, 0f, 38f), color);
        Prim(PrimitiveType.Cube, root, "Boss_Horn_Tip_L", new Vector3(.80f, 2.25f, .10f), new Vector3(.13f, .28f, .13f), Quaternion.Euler(0f, 0f, -38f), Color.white);
        Prim(PrimitiveType.Cube, root, "Boss_Horn_Tip_R", new Vector3(-.80f, 2.25f, .10f), new Vector3(.13f, .28f, .13f), Quaternion.Euler(0f, 0f, 38f), Color.white);
    }

    void AddBossCape(Transform root, Color dark, Color red)
    {
        Prim(PrimitiveType.Cube, root, "Boss_Cape_Dark", new Vector3(0f, .52f, -.42f), new Vector3(1.15f, 1.35f, .08f), Quaternion.Euler(8f, 0f, 0f), dark);
        Prim(PrimitiveType.Cube, root, "Boss_Cape_Red_Trim", new Vector3(0f, .18f, -.47f), new Vector3(1.05f, .14f, .06f), Quaternion.Euler(8f, 0f, 0f), red);
    }

    GameObject GlowOrb(Transform parent, string name, Vector3 pos, Vector3 scl, Color col, float power)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.name = name;
        g.transform.SetParent(parent);
        g.transform.localPosition = pos;
        g.transform.localScale = scl;
        g.transform.localRotation = Quaternion.identity;
        var c = g.GetComponent<Collider>();
        if (c) Destroy(c);
        var r = g.GetComponent<Renderer>();
        if (r != null) r.material = MakeMat("M_" + name, col * .55f, col, true, col, power);
        IgnoreNavMesh(g);
        return g;
    }

    void OrcEye(Transform parent, Vector3 pos, Color? col = null)
    {
        AddOrcEyes(parent, pos, col ?? new Color(1f, .85f, 0f));
    }

    GameObject Prim(PrimitiveType type, Transform parent, string nm, Vector3 pos, Vector3 scl, Quaternion rot, Color col)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = nm;
        g.transform.SetParent(parent);
        g.transform.localPosition = pos;
        g.transform.localScale = scl;
        g.transform.localRotation = rot;
        var c = g.GetComponent<Collider>();
        if (c) Destroy(c);
        var _vm = new Material(LitShader()); _vm.SetColor("_BaseColor", col); _vm.color = col;
        g.GetComponent<Renderer>().material = _vm;
        IgnoreNavMesh(g);
        return g;
    }
}


public class V23BobSpin : MonoBehaviour
{
    public float phase = 0f;
    public float bobAmount = 0.18f;
    public float spinSpeed = 14f;
    Vector3 _basePos;
    Quaternion _baseRot;

    public void Setup(float bob, float spin, float ph)
    {
        bobAmount = bob;
        spinSpeed = spin;
        phase = ph;
    }

    void Start()
    {
        _basePos = transform.localPosition;
        _baseRot = transform.localRotation;
    }

    void Update()
    {
        transform.localPosition = _basePos + Vector3.up * (Mathf.Sin(Time.time * 1.5f + phase) * bobAmount);
        transform.localRotation = _baseRot * Quaternion.Euler(0f, Time.time * spinSpeed, Mathf.Sin(Time.time + phase) * 4f);
    }
}

public class V23PulseLight : MonoBehaviour
{
    float _min = 0.8f;
    float _max = 2.0f;
    float _speed = 1.4f;
    Light _l;

    public void Setup(float minIntensity, float maxIntensity, float speed)
    {
        _min = minIntensity;
        _max = maxIntensity;
        _speed = speed;
    }

    void Start()
    {
        _l = GetComponent<Light>();
    }

    void Update()
    {
        if (_l == null) return;
        float t = (Mathf.Sin(Time.time * _speed) + 1f) * 0.5f;
        _l.intensity = Mathf.Lerp(_min, _max, t);
    }
}

public class LavaPulse : MonoBehaviour
{
    public float phase = 0f;
    public float pulseScale = 0.025f;
    public Color colorA = new Color(1f, 0.12f, 0.01f);
    public Color colorB = new Color(1f, 0.48f, 0.02f);

    Renderer _r;
    Vector3 _baseScale;
    Material _mat;

    void Start()
    {
        _baseScale = transform.localScale;
        _r = GetComponent<Renderer>();
        if (_r != null)
        {
            _mat = _r.material;
            _mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        float t = Time.time * 2.2f + phase;
        float s = 1f + Mathf.Sin(t) * pulseScale;
        transform.localScale = new Vector3(_baseScale.x * s, _baseScale.y, _baseScale.z * (1f + Mathf.Cos(t * 0.7f) * pulseScale));
        if (_mat != null)
        {
            Color c = Color.Lerp(colorA, colorB, (Mathf.Sin(t) + 1f) * 0.5f);
            _mat.color = c;
            _mat.SetColor("_EmissionColor", c * 1.35f);
        }
    }
}

public class BannerSway : MonoBehaviour
{
    Quaternion _baseRot;
    float _phase;

    void Start()
    {
        _baseRot = transform.localRotation;
        _phase = Random.Range(0f, 10f);
    }

    void Update()
    {
        float a = Mathf.Sin(Time.time * 1.25f + _phase) * 2.0f;
        transform.localRotation = _baseRot * Quaternion.Euler(0f, 0f, a);
    }
}

public class FireFlicker : MonoBehaviour
{
    float _t;
    Vector3 _base;

    void Start()
    {
        _base = transform.localScale;
    }

    void Update()
    {
        _t += Time.deltaTime * 4f;
        float s = 1f + Mathf.Sin(_t) * .15f + Mathf.Sin(_t * 2.3f) * .08f;
        transform.localScale = _base * s;
        var r = GetComponent<Renderer>();
        if (r) r.material.color = new Color(1f, 0.3f + Mathf.Sin(_t) * .15f, 0.02f);
    }
}

public class ZoneTitleTrigger : MonoBehaviour
{
    public string title;
    public string subtitle;
    public float duration = 4f;
    bool _fired;

    void OnTriggerEnter(Collider other)
    {
        if (_fired) return;
        if (!other.CompareTag("Player")) return;
        _fired = true;
        GameManager.Instance?.ShowZoneTitle(title, subtitle, duration);
    }
}
