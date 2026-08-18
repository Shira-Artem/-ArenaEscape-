using UnityEngine;

/// <summary>
/// Зона кузнеца Брока:
///   — поднятая каменная площадка (Y=0.58, давит траву/луга визуально)
///   — деревянный забор U-формой (открытый вход на севере = к воротам замка)
///   — горн с дымоходом и огнём
///   — наковальня с молотком
///   — NPC-визуал кузнеца, смотрящего на игрока
///   — вывеска "КУЗНЯ" над входом
///   — тёплое оранжевое освещение
///
/// Вызов: L0BlacksmithArea.Build(root) из Level0CastleDressingBuilder.
/// </summary>
public static class L0BlacksmithArea
{
    // ——— Мировые координаты ———
    const float CX = 5.75f;   // центр зоны X
    const float CZ = -24.5f;  // центр зоны Z

    // Платформа: поверхность PY=0.78 — высоко над лугами/травой
    const float PY = 0.78f;

    // Границы забора (мировые X/Z)
    const float XL = 2.95f;   // west edge
    const float XR = 8.55f;   // east edge
    const float ZN = -22.1f;  // north edge  ← ВХОД (без забора)
    const float ZS = -26.9f;  // south edge  ← глухая стена

    // ——— Палитра ———
    static readonly Color WOOD  = new Color(0.30f, 0.17f, 0.07f);
    static readonly Color WOODL = new Color(0.45f, 0.28f, 0.12f);
    static readonly Color STONE = new Color(0.40f, 0.36f, 0.30f);
    static readonly Color EARTH = new Color(0.27f, 0.22f, 0.16f);
    static readonly Color COAL  = new Color(0.14f, 0.11f, 0.09f);
    static readonly Color IRON  = new Color(0.22f, 0.22f, 0.26f);
    static readonly Color FLAME = new Color(1.00f, 0.42f, 0.05f);
    static readonly Color HOTM  = new Color(0.90f, 0.30f, 0.06f);
    static readonly Color SMOKE = new Color(0.32f, 0.30f, 0.30f);

    // ——— Точка входа ———
    public static GameObject Build(Transform parent)
    {
        var root = new GameObject("=== BLACKSMITH_AREA ===");
        root.transform.SetParent(parent, false);
        var t = root.transform;

        Platform(t);
        Fence(t);
        Forge(t);
        Anvil(t);
        Workbench(t);
        WeaponRack(t);
        WaterBarrel(t);
        Canopy(t);
        // NPC создаётся через CastlePrologueBuilder (интерактивный), не дублируем здесь
        Entrance(t);
        Lighting(t);

        return root;
    }

    // ——— Поднятая площадка ———
    static void Platform(Transform t)
    {
        // РАСЧИСТКА — большой тёмный пятак ПОВЕРХ лугов/травы (Y=0.18, 10×9 м)
        // Перекрывает любую зелень в радиусе ~1.5 м за забором
        Box("BS_Clearing", t, CX, 0.18f, CZ, 10.0f, 0.36f, 9.0f, EARTH);

        // Каменная подпорная стенка по периметру (видна как бордюр)
        float brdY = 0.48f;
        Box("BS_BorderN", t, CX, brdY, ZN + 0.60f, 7.0f, 0.60f, 0.14f, STONE);
        Box("BS_BorderS", t, CX, brdY, ZS - 0.60f, 7.0f, 0.60f, 0.14f, STONE);
        Box("BS_BorderW", t, XL - 0.60f, brdY, CZ, 0.14f, 0.60f, 6.2f, STONE);
        Box("BS_BorderE", t, XR + 0.60f, brdY, CZ, 0.14f, 0.60f, 6.2f, STONE);

        // Основная площадка: top=PY(0.78), толщина 0.40 → base=0.38
        Box("BS_PlatformEarth", t, CX, PY - 0.20f, CZ, 7.0f, 0.40f, 6.2f, EARTH);

        // Каменные плиты поверху
        Box("BS_Slab0", t, CX - 1.0f, PY + 0.02f, CZ + 0.50f, 0.90f, 0.03f, 0.70f, STONE);
        Box("BS_Slab1", t, CX + 0.3f, PY + 0.02f, CZ - 0.80f, 1.10f, 0.03f, 0.80f, STONE);
        Box("BS_Slab2", t, CX - 0.2f, PY + 0.02f, CZ + 0.10f, 1.00f, 0.03f, 0.65f, STONE);
        Box("BS_Slab3", t, CX + 1.6f, PY + 0.02f, CZ + 0.30f, 0.80f, 0.03f, 0.60f, STONE);

        // Ступени у входа (3 ступени для подъёма на 0.78)
        Box("BS_Step1", t, CX, 0.64f, ZN + 0.30f, 3.20f, 0.14f, 0.50f, STONE);
        Box("BS_Step2", t, CX, 0.46f, ZN + 0.72f, 3.20f, 0.14f, 0.50f, STONE);
        Box("BS_Step3", t, CX, 0.28f, ZN + 1.14f, 3.20f, 0.14f, 0.50f, STONE);
    }

    // ——— Деревянный забор U-формой ———
    static void Fence(Transform t)
    {
        const float postH     = 1.30f;
        const float postHalf  = postH * 0.5f;
        float postCY = PY + postHalf;  // центр столба по Y

        // Шаг между столбами
        float stepX = (XR - XL) / 4f;          // 1.4 м
        float stepZ = (ZS - ZN) / 3f;           // -1.6 м

        // — Столбы south wall (5 штук) —
        for (float x = XL; x <= XR + 0.01f; x += stepX)
            FencePost(t, x, postCY, ZS, postH);

        // — Столбы east wall (пропускаем ZN — там будут ворота 2м высотой) —
        for (float z = ZN + stepZ; z >= ZS - 0.01f; z += stepZ)
            FencePost(t, XR, postCY, z, postH);

        // — Столбы west wall —
        for (float z = ZN + stepZ; z >= ZS - 0.01f; z += stepZ)
            FencePost(t, XL, postCY, z, postH);

        // — Рейки south —
        float railY1 = PY + 0.40f;
        float railY2 = PY + 0.90f;
        float sW = XR - XL;
        Box("BS_RailS1", t, CX, railY1, ZS, sW, 0.07f, 0.07f, WOODL);
        Box("BS_RailS2", t, CX, railY2, ZS, sW, 0.07f, 0.07f, WOODL);

        // — Рейки east / west —
        float sideD  = Mathf.Abs(ZS - ZN);
        float sideCZ = (ZN + ZS) * 0.5f;
        Box("BS_RailE1", t, XR, railY1, sideCZ, 0.07f, 0.07f, sideD, WOODL);
        Box("BS_RailE2", t, XR, railY2, sideCZ, 0.07f, 0.07f, sideD, WOODL);
        Box("BS_RailW1", t, XL, railY1, sideCZ, 0.07f, 0.07f, sideD, WOODL);
        Box("BS_RailW2", t, XL, railY2, sideCZ, 0.07f, 0.07f, sideD, WOODL);
    }

    // ——— Горн ———
    static void Forge(Transform t)
    {
        float fx = CX + 1.90f;   // = 7.65
        float fz = CZ - 1.60f;  // = -26.1

        // Тёмная кирпичная стена позади горна (back-wall) — пламя читается на тёмном фоне
        Box("BS_ForgeBackWall", t, fx, PY + 0.90f, fz - 0.52f, 1.50f, 1.20f, 0.14f, COAL);
        Box("BS_ForgeBackTrim", t, fx, PY + 1.52f, fz - 0.52f, 1.56f, 0.08f, 0.16f, STONE);

        // Каменное тело горна
        Box("BS_ForgeBody",   t, fx,        PY + 0.40f, fz,        1.30f, 0.80f, 0.95f, STONE);
        Box("BS_ForgeFront",  t, fx,        PY + 0.80f, fz + 0.44f, 1.20f, 0.14f, 0.10f, STONE);

        // Боковые стенки горна (обнимают топку)
        Box("BS_ForgeSideL",  t, fx - 0.60f, PY + 0.90f, fz, 0.12f, 0.60f, 0.80f, STONE);
        Box("BS_ForgeSideR",  t, fx + 0.60f, PY + 0.90f, fz, 0.12f, 0.60f, 0.80f, STONE);

        // Угольная чаша (тлеет)
        var coalBed = Box("BS_CoalBed", t, fx, PY + 0.84f, fz - 0.05f, 0.72f, 0.10f, 0.55f, COAL);
        Emis(coalBed, new Color(1f, 0.30f, 0.04f), 0.8f);

        // Угли — мелкие кусочки
        Box("BS_Coal0",       t, fx - 0.18f, PY + 0.91f, fz - 0.10f, 0.10f, 0.08f, 0.10f, COAL);
        Box("BS_Coal1",       t, fx + 0.12f, PY + 0.91f, fz + 0.08f, 0.08f, 0.07f, 0.09f, COAL);
        Box("BS_Coal2",       t, fx - 0.05f, PY + 0.91f, fz - 0.18f, 0.12f, 0.08f, 0.08f, COAL);

        // Пламя — УВЕЛИЧЕНО для читаемости, ярко светится
        var flame = Box("BS_Flame", t, fx, PY + 1.04f, fz - 0.02f, 0.65f, 0.45f, 0.55f, FLAME);
        Emis(flame, new Color(1f, 0.45f, 0.08f), 2.2f);
        // Горячая деталь на огне
        var hotPiece = Box("BS_HotPiece", t, fx + 0.08f, PY + 1.10f, fz + 0.04f, 0.30f, 0.06f, 0.18f, HOTM);
        Emis(hotPiece, new Color(1f, 0.35f, 0.05f), 1.6f);

        // Дымоход — три секции, сужается кверху; СВЕТЛЕЕ (STONE) и ШИРЕ
        Box("BS_ChimLow",     t, fx - 0.10f, PY + 1.30f, fz - 0.36f, 0.90f, 0.60f, 0.56f, STONE);
        Box("BS_ChimMid",     t, fx - 0.10f, PY + 1.82f, fz - 0.36f, 0.70f, 0.50f, 0.48f, STONE);
        Box("BS_ChimTop",     t, fx - 0.10f, PY + 2.24f, fz - 0.36f, 0.56f, 0.20f, 0.40f, STONE);

        // Дымки над трубой (серые шарики, визуальный намёк)
        Sphere("BS_Smoke0",   t, new Vector3(fx - 0.10f, PY + 2.55f, fz - 0.36f), 0.18f, SMOKE);
        Sphere("BS_Smoke1",   t, new Vector3(fx - 0.06f, PY + 2.80f, fz - 0.40f), 0.14f, SMOKE);
        Sphere("BS_Smoke2",   t, new Vector3(fx - 0.14f, PY + 3.00f, fz - 0.34f), 0.11f, SMOKE);

        // Щипцы у горна
        Cyl("BS_Tongs", t,
            new Vector3(fx - 0.52f, PY + 0.78f, fz + 0.25f),
            new Vector3(0.04f, 0.48f, 0.04f),
            IRON, Quaternion.Euler(0f, 0f, 55f));
        Cyl("BS_Poker", t,
            new Vector3(fx - 0.42f, PY + 0.80f, fz + 0.10f),
            new Vector3(0.03f, 0.52f, 0.03f),
            IRON, Quaternion.Euler(0f, 25f, 52f));
    }

    // ——— Наковальня ———
    static void Anvil(Transform t)
    {
        float ax = CX - 0.50f;  // = 5.25
        float az = CZ - 0.30f;  // = -24.8

        // Деревянный пень-основание
        Cyl("BS_Stump", t, new Vector3(ax, PY + 0.26f, az), new Vector3(0.54f, 0.26f, 0.54f), WOODL);

        // Наковальня
        Box("BS_AnvilBase", t, ax,        PY + 0.64f, az, 0.33f, 0.22f, 0.64f, IRON);
        Box("BS_AnvilTop",  t, ax,        PY + 0.80f, az, 0.80f, 0.13f, 0.30f, IRON);
        Box("BS_AnvilHorn", t, ax + 0.50f, PY + 0.77f, az, 0.28f, 0.08f, 0.13f, IRON);

        // Молоток лежит на наковальне
        Cyl("BS_HamHdl", t,
            new Vector3(ax - 0.50f, PY + 0.86f, az + 0.12f),
            new Vector3(0.04f, 0.42f, 0.04f),
            WOODL, Quaternion.Euler(0f, 30f, 78f));
        Box("BS_HamHead", t, ax - 0.17f, PY + 0.91f, az + 0.12f, 0.19f, 0.10f, 0.09f, IRON);
    }

    // ——— NPC Кузнец ———
    static void Npc(Transform t)
    {
        // Стоит между входом и наковальней, смотрит на север (= на игрока входящего)
        CastleCharacterVisualFactory.CreateCharacter(
            CastleCharacterType.Blacksmith,
            "Кузнец Брок",
            new Vector3(CX - 0.5f, PY, CZ + 0.25f),
            Quaternion.Euler(0f, 0f, 0f),   // лицом на +Z = к воротам замка
            t);
    }

    // ——— Арка входа + вывеска ———
    static void Entrance(Transform t)
    {
        const float gateH     = 2.00f;
        const float gateHalf  = gateH * 0.5f;

        // Два высоких столба (тех же X, что края забора)
        Cyl("BS_GatePostL", t, new Vector3(XL, PY + gateHalf, ZN), new Vector3(0.20f, gateHalf, 0.20f), WOOD);
        Cyl("BS_GatePostR", t, new Vector3(XR, PY + gateHalf, ZN), new Vector3(0.20f, gateHalf, 0.20f), WOOD);

        // Горизонтальная перекладина
        float beamY = PY + gateH + 0.07f;
        Box("BS_GateBeam",    t, CX, beamY,          ZN, 5.60f, 0.14f, 0.14f, WOOD);
        Box("BS_GateBeamL",   t, CX, beamY + 0.08f,  ZN, 0.60f, 0.08f, 0.10f, WOODL);  // декор

        // Вывеска под перекладиной
        float signY = beamY - 0.15f;
        Box("BS_SignBoard",   t, CX, signY,           ZN + 0.04f, 1.85f, 0.38f, 0.09f, WOODL);
        Box("BS_SignBoardBdr", t, CX, signY,           ZN + 0.06f, 1.92f, 0.43f, 0.06f, WOOD);  // рамка

        // Текстовый лейбл "КУЗНЯ" над вывеской
        Label("КУЗНЯ", t, new Vector3(CX, beamY + 0.20f, ZN), new Color(1f, 0.88f, 0.50f));
    }

    // ——— Навес над горном ———
    static void Canopy(Transform t)
    {
        float fx = CX + 1.90f;
        float fz = CZ - 1.60f;
        float roofY = PY + 2.50f;

        // 3 вертикальных балки (столбы навеса)
        Box("BS_CanopyPole0", t, fx - 1.10f, PY + 1.54f, fz + 0.50f, 0.12f, 1.92f, 0.12f, WOOD);
        Box("BS_CanopyPole1", t, fx + 1.10f, PY + 1.54f, fz + 0.50f, 0.12f, 1.92f, 0.12f, WOOD);
        Box("BS_CanopyPole2", t, fx + 1.10f, PY + 1.40f, fz - 0.90f, 0.12f, 1.64f, 0.12f, WOOD);

        // Горизонтальные балки (каркас крыши)
        Box("BS_CanopyBeamF", t, fx, roofY, fz + 0.50f, 2.40f, 0.10f, 0.12f, WOOD);
        Box("BS_CanopyBeamB", t, fx, roofY - 0.28f, fz - 0.90f, 2.40f, 0.10f, 0.12f, WOOD);
        Box("BS_CanopyBeamL", t, fx - 1.10f, roofY - 0.14f, fz - 0.20f, 0.10f, 0.10f, 1.60f, WOOD);
        Box("BS_CanopyBeamR", t, fx + 1.10f, roofY - 0.14f, fz - 0.20f, 0.10f, 0.10f, 1.60f, WOOD);

        // Наклонная кровля (скат на юг = к back-wall, понижается к fz-0.90)
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "BS_CanopyRoof";
        roof.transform.SetParent(t, false);
        roof.transform.position = new Vector3(fx, roofY + 0.02f, fz - 0.20f);
        roof.transform.localScale = new Vector3(2.60f, 0.06f, 1.70f);
        roof.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        roof.GetComponent<Renderer>().material = M(WOOD);
        var c = roof.GetComponent<Collider>();
        if (c) Object.Destroy(c);
    }

    // ——— Верстак (слева от NPC) ———
    static void Workbench(Transform t)
    {
        float wx = CX - 2.00f;  // = 3.75 (слева)
        float wz = CZ - 0.10f;  // = -24.6

        // Ножки
        Box("BS_WBLeg0", t, wx - 0.55f, PY + 0.24f, wz - 0.22f, 0.10f, 0.48f, 0.10f, WOOD);
        Box("BS_WBLeg1", t, wx + 0.55f, PY + 0.24f, wz - 0.22f, 0.10f, 0.48f, 0.10f, WOOD);
        Box("BS_WBLeg2", t, wx - 0.55f, PY + 0.24f, wz + 0.22f, 0.10f, 0.48f, 0.10f, WOOD);
        Box("BS_WBLeg3", t, wx + 0.55f, PY + 0.24f, wz + 0.22f, 0.10f, 0.48f, 0.10f, WOOD);

        // Столешница
        Box("BS_WBTop", t, wx, PY + 0.50f, wz, 1.30f, 0.08f, 0.60f, WOODL);

        // Инструменты на столе
        Cyl("BS_WBHammer", t,
            new Vector3(wx - 0.30f, PY + 0.58f, wz + 0.10f),
            new Vector3(0.03f, 0.22f, 0.03f),
            WOODL, Quaternion.Euler(0f, 15f, 85f));
        Box("BS_WBHamHead", t, wx - 0.10f, PY + 0.58f, wz + 0.10f, 0.12f, 0.07f, 0.06f, IRON);

        // Зубило
        Cyl("BS_WBChisel", t,
            new Vector3(wx + 0.20f, PY + 0.56f, wz - 0.08f),
            new Vector3(0.02f, 0.20f, 0.02f),
            IRON, Quaternion.Euler(0f, -20f, 80f));

        // Заготовка (полоса металла)
        Box("BS_WBBlank", t, wx + 0.40f, PY + 0.55f, wz + 0.12f, 0.35f, 0.03f, 0.06f, IRON);
    }

    // ——— Стойка оружия (вертикальная A-рама у южной стены) ———
    static void WeaponRack(Transform t)
    {
        // Центр стойки — у южной стены, немного сдвинут влево от центра
        float rx = CX - 0.60f;   // = 5.15
        float rz = ZS + 0.22f;   // = -26.68
        float baseY = PY;         // = 0.78 (поверхность платформы)

        // ── Два вертикальных столба ──────────────────────────────────────────
        // Левый столб
        Cyl("WR_PostL", t,
            new Vector3(rx - 0.80f, baseY + 1.00f, rz),
            new Vector3(0.10f, 1.00f, 0.10f), WOOD);
        // Крышечка столба
        Box("WR_CapL", t, rx - 0.80f, baseY + 2.05f, rz, 0.14f, 0.05f, 0.14f, WOODL);

        // Правый столб
        Cyl("WR_PostR", t,
            new Vector3(rx + 0.80f, baseY + 1.00f, rz),
            new Vector3(0.10f, 1.00f, 0.10f), WOOD);
        Box("WR_CapR", t, rx + 0.80f, baseY + 2.05f, rz, 0.14f, 0.05f, 0.14f, WOODL);

        // ── Перекладины ─────────────────────────────────────────────────────
        // Верхняя балка
        Box("WR_BeamTop",  t, rx, baseY + 1.90f, rz, 1.72f, 0.09f, 0.10f, WOODL);
        // Средняя балка — главная "полка" для крепления
        Box("WR_BeamMid",  t, rx, baseY + 1.30f, rz, 1.72f, 0.07f, 0.10f, WOODL);
        // Нижняя балка
        Box("WR_BeamBot",  t, rx, baseY + 0.62f, rz, 1.72f, 0.06f, 0.10f, WOODL);

        // ── Диагональные распорки (для жёсткости) ───────────────────────────
        var bL = new GameObject("WR_DiagL"); bL.transform.SetParent(t, false);
        bL.transform.position = new Vector3(rx - 0.52f, baseY + 1.60f, rz);
        bL.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
        var bLc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bLc.name = "mesh"; bLc.transform.SetParent(bL.transform, false);
        bLc.transform.localScale = new Vector3(0.06f, 0.78f, 0.08f);
        bLc.GetComponent<Renderer>().material = M(WOOD);
        Object.DestroyImmediate(bLc.GetComponent<Collider>());

        var bR = new GameObject("WR_DiagR"); bR.transform.SetParent(t, false);
        bR.transform.position = new Vector3(rx + 0.52f, baseY + 1.60f, rz);
        bR.transform.localRotation = Quaternion.Euler(0f, 0f, -32f);
        var bRc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bRc.name = "mesh"; bRc.transform.SetParent(bR.transform, false);
        bRc.transform.localScale = new Vector3(0.06f, 0.78f, 0.08f);
        bRc.GetComponent<Renderer>().material = M(WOOD);
        Object.DestroyImmediate(bRc.GetComponent<Collider>());

        // ── Крючки на средней балке ──────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            float hx = rx - 0.55f + i * 0.55f;
            Box("WR_Hook" + i, t, hx, baseY + 1.27f, rz + 0.06f, 0.04f, 0.07f, 0.06f, IRON);
        }

        // ── Оружие (декоративные вертикальные силуэты) ──────────────────────
        // Меч слева — стоит вертикально
        Box("WR_SwordBlade", t, rx - 0.55f, baseY + 1.68f, rz - 0.02f, 0.06f, 0.72f, 0.04f, IRON);
        Box("WR_SwordGuard", t, rx - 0.55f, baseY + 1.34f, rz - 0.02f, 0.20f, 0.04f, 0.04f, IRON);
        Box("WR_SwordHilt",  t, rx - 0.55f, baseY + 1.22f, rz - 0.02f, 0.05f, 0.18f, 0.05f, WOODL);
        Box("WR_SwordPommel",t, rx - 0.55f, baseY + 1.12f, rz - 0.02f, 0.09f, 0.05f, 0.09f, IRON);

        // Копьё по центру — длиннее, тонкое
        Cyl("WR_SpearShaft", t,
            new Vector3(rx, baseY + 1.58f, rz),
            new Vector3(0.04f, 0.90f, 0.04f), WOODL);
        Box("WR_SpearHead", t, rx, baseY + 2.50f, rz, 0.06f, 0.28f, 0.04f, IRON);

        // Топор справа — наклонён
        var axeRoot = new GameObject("WR_AxeRoot"); axeRoot.transform.SetParent(t, false);
        axeRoot.transform.position = new Vector3(rx + 0.55f, baseY + 1.50f, rz);
        axeRoot.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
        var axeH = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axeH.name = "AxeHdl"; axeH.transform.SetParent(axeRoot.transform, false);
        axeH.transform.localScale = new Vector3(0.05f, 0.55f, 0.05f);
        axeH.GetComponent<Renderer>().material = M(WOODL);
        Object.DestroyImmediate(axeH.GetComponent<Collider>());
        var axeHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        axeHead.name = "AxeHead"; axeHead.transform.SetParent(axeRoot.transform, false);
        axeHead.transform.localPosition = new Vector3(0.10f, 0.56f, 0f);
        axeHead.transform.localScale = new Vector3(0.22f, 0.18f, 0.07f);
        axeHead.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        axeHead.GetComponent<Renderer>().material = M(IRON);
        Object.DestroyImmediate(axeHead.GetComponent<Collider>());

        // Щит — прислонён к правому столбу
        Cyl("WR_ShieldFace", t,
            new Vector3(rx + 0.80f, baseY + 1.12f, rz - 0.10f),
            new Vector3(0.42f, 0.04f, 0.42f),
            new Color(0.18f, 0.18f, 0.22f), Quaternion.Euler(80f, 0f, 0f));
        // Геральдический крест на щите
        Box("WR_CrossV", t, rx + 0.80f, baseY + 1.12f, rz - 0.14f, 0.04f, 0.30f, 0.03f, new Color(0.75f, 0.62f, 0.15f));
        Box("WR_CrossH", t, rx + 0.80f, baseY + 1.12f, rz - 0.14f, 0.22f, 0.04f, 0.03f, new Color(0.75f, 0.62f, 0.15f));

        // Небольшой факел на левом столбе
        Cyl("WR_TorchPost", t,
            new Vector3(rx - 0.80f, baseY + 1.72f, rz - 0.10f),
            new Vector3(0.04f, 0.18f, 0.04f), WOODL);
        Box("WR_TorchFlame", t, rx - 0.80f, baseY + 1.95f, rz - 0.10f,
            0.08f, 0.14f, 0.08f, new Color(1f, 0.42f, 0.05f));
        var torchLight = new GameObject("WR_TorchLight");
        torchLight.transform.SetParent(t, false);
        torchLight.transform.position = new Vector3(rx - 0.80f, baseY + 2.0f, rz - 0.10f);
        var lt = torchLight.AddComponent<Light>();
        lt.type = LightType.Point; lt.color = new Color(1f, 0.55f, 0.12f);
        lt.range = 4.5f; lt.intensity = 1.4f;
    }

    // ——— Бочка с водой (для закалки) ———
    static void WaterBarrel(Transform t)
    {
        float bx = CX + 0.30f;  // = 6.05 (справа от наковальни)
        float bz = CZ - 0.80f;  // = -25.3

        // Бочка (цилиндр)
        Cyl("BS_Barrel", t,
            new Vector3(bx, PY + 0.36f, bz),
            new Vector3(0.44f, 0.36f, 0.44f), WOODL);

        // Обручи
        Cyl("BS_BarrelBand0", t,
            new Vector3(bx, PY + 0.18f, bz),
            new Vector3(0.47f, 0.02f, 0.47f), IRON);
        Cyl("BS_BarrelBand1", t,
            new Vector3(bx, PY + 0.54f, bz),
            new Vector3(0.47f, 0.02f, 0.47f), IRON);

        // Вода (плоский синий диск внутри)
        Cyl("BS_Water", t,
            new Vector3(bx, PY + 0.70f, bz),
            new Vector3(0.38f, 0.01f, 0.38f),
            new Color(0.15f, 0.30f, 0.48f));
    }

    // ——— Освещение ———
    static void Lighting(Transform t)
    {
        // Оранжевое свечение от горна — ВНУТРИ зоны, не на краю (ярче, читается как огонь)
        AddLight(t, new Vector3(CX + 1.60f, PY + 1.6f, CZ - 1.6f),
                 new Color(1f, 0.46f, 0.10f), 10.0f, 3.4f);

        // Мягкий тёплый свет всей зоны
        AddLight(t, new Vector3(CX, PY + 2.8f, CZ),
                 new Color(1f, 0.82f, 0.60f), 7.0f, 1.0f);
    }

    // ——— Helpers ———

    static void FencePost(Transform t, float x, float cy, float z, float actualH)
    {
        Cyl("BS_Post", t, new Vector3(x, cy, z), new Vector3(0.14f, actualH * 0.5f, 0.14f), WOOD);
        Box("BS_PostCap", t, x, cy + actualH * 0.5f, z, 0.18f, 0.05f, 0.18f, WOODL);
    }

    static GameObject Box(string name, Transform t,
                    float x, float y, float z,
                    float sx, float sy, float sz,
                    Color c)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(t, false);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.localScale = new Vector3(sx, sy, sz);
        obj.GetComponent<Renderer>().material = M(c);
        var col = obj.GetComponent<Collider>();
        if (col) Object.Destroy(col);
        return obj;
    }

    // Свечение материала (горн, угли, раскалённый металл).
    static void Emis(GameObject go, Color c, float intensity)
    {
        if (go == null) return;
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var m = r.material;
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * intensity);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    static void Sphere(string name, Transform t, Vector3 pos, float radius, Color c)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = name;
        obj.transform.SetParent(t, false);
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * radius * 2f;
        obj.GetComponent<Renderer>().material = M(c);
        var col = obj.GetComponent<Collider>();
        if (col) Object.Destroy(col);
    }

    static void Cyl(string name, Transform t, Vector3 pos, Vector3 scale, Color c)
        => CylR(name, t, pos, scale, c, Quaternion.identity);

    static void Cyl(string name, Transform t, Vector3 pos, Vector3 scale, Color c, Quaternion rot)
        => CylR(name, t, pos, scale, c, rot);

    static void CylR(string name, Transform t, Vector3 pos, Vector3 scale, Color c, Quaternion rot)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(t, false);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.transform.rotation = rot;
        obj.GetComponent<Renderer>().material = M(c);
        var col = obj.GetComponent<Collider>();
        if (col) Object.Destroy(col);
    }

    static void AddLight(Transform t, Vector3 pos, Color c, float range, float intensity)
    {
        var go = new GameObject("BS_Light");
        go.transform.SetParent(t, false);
        go.transform.position = pos;
        var lt = go.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = c;
        lt.range = range;
        lt.intensity = intensity;
    }

    static void Label(string text, Transform t, Vector3 pos, Color c)
    {
        var go = new GameObject("BS_Lbl_" + text);
        go.transform.SetParent(t, false);
        go.transform.position = pos;
        go.AddComponent<SimpleBillboardLabel>();
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 28;
        tm.characterSize = 0.10f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = c;
    }

    static Material M(Color c)
    {
        var s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(s);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.color = c;
        return m;
    }
}
