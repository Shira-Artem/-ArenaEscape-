using UnityEngine;

/// <summary>
/// Castle Detail Upgrade v1.
/// Максимальная детализация ЛР4: внешний декор, двор, тронный зал и подземелье.
/// Этот файл запускается только в режиме Lab4_Assets.
/// </summary>
public static class CastleDecorBuilder
{
    public static void Build(CastleGenerator.CastleContext c)
    {
        GameObject parent = c.Child(c.Root, "02_CASTLE_DETAIL_UPGRADE_V1_DECOR");

        BuildFlagsAndBanners(c, parent);
        BuildTorchesAndLights(c, parent);
        BuildGateDecor(c, parent);
        BuildCourtyardProps(c, parent);
        BuildStableAndForgeDecor(c, parent);
        BuildWallDecor(c, parent);
        BuildThroneHallProps(c, parent);
        BuildDungeonProps(c, parent);
        BuildImportedAssetMarkers(c, parent);
        CastleTowerDetailBuilder.Build(c);
        CastleRoyalInteriorPolishBuilder.Build(c);
    }

    // ─────────────────────────────────────────────────────────
    // Флаги, гербы и баннеры
    // ─────────────────────────────────────────────────────────

    private static void BuildFlagsAndBanners(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject flags = c.Child(parent, "Flags_Banners_Coats_Of_Arms");

        Flag(c, flags, new Vector3(-4.6f, 8.35f, c.FrontZ - 0.2f), 1.15f);
        Flag(c, flags, new Vector3(4.6f, 8.35f, c.FrontZ - 0.2f), 1.15f);
        Flag(c, flags, new Vector3(c.LeftX, 8.65f, c.BackZ), 1.0f);
        Flag(c, flags, new Vector3(c.RightX, 8.65f, c.BackZ), 1.0f);

        Banner(c, flags, new Vector3(-3.8f, 5.4f, c.FrontZ - 0.92f), true);
        Banner(c, flags, new Vector3(3.8f, 5.4f, c.FrontZ - 0.92f), true);
        Banner(c, flags, new Vector3(-4.6f, 5.4f, 4.05f), true);
        Banner(c, flags, new Vector3(4.6f, 5.4f, 4.05f), true);

        Shield(c, flags, new Vector3(0f, 5.05f, c.FrontZ - 1.02f), true);
        Shield(c, flags, new Vector3(-6.9f, 4.7f, 14.0f), true);
        Shield(c, flags, new Vector3(6.9f, 4.7f, 14.0f), true);
    }

    // ─────────────────────────────────────────────────────────
    // Свет
    // ─────────────────────────────────────────────────────────

    private static void BuildTorchesAndLights(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject torches = c.Child(parent, "Torches_Lanterns_And_Lights");

        // Ворота.
        Torch(c, torches, new Vector3(-2.9f, 2.55f, c.FrontZ - 1.0f), true);
        Torch(c, torches, new Vector3(2.9f, 2.55f, c.FrontZ - 1.0f), true);

        // Внутренний двор.
        Torch(c, torches, new Vector3(-10.5f, 2.35f, 1.78f), false);
        Torch(c, torches, new Vector3(10.5f, 2.35f, 1.78f), false);
        Torch(c, torches, new Vector3(-7.85f, 3.1f, 7.2f), false);
        Torch(c, torches, new Vector3(7.85f, 3.1f, 7.2f), false);
        Torch(c, torches, new Vector3(-7.85f, 3.1f, 11.6f), false);
        Torch(c, torches, new Vector3(7.85f, 3.1f, 11.6f), false);

        // Подземелье и проход.
        Torch(c, torches, new Vector3(-12f, 1.9f, 6.0f), false);
        Torch(c, torches, new Vector3(-14.42f, 1.2f, 11.0f), false);
        Torch(c, torches, new Vector3(-9.58f, 1.2f, 11.0f), false);

        // Фонари на столбах во дворе.
        LanternPost(c, torches, new Vector3(-2.2f, 0f, -9.8f));
        LanternPost(c, torches, new Vector3(3.4f, 0f, -9.4f));
        LanternPost(c, torches, new Vector3(-7.4f, 0f, -1.2f));
        LanternPost(c, torches, new Vector3(7.4f, 0f, -1.2f));
    }

    // ─────────────────────────────────────────────────────────
    // Декор ворот
    // ─────────────────────────────────────────────────────────

    private static void BuildGateDecor(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject gate = c.Child(parent, "Gate_Decor_Details");

        // Цепи подъёмного моста.
        Chain(c, gate, new Vector3(-3.2f, 4.4f, c.FrontZ - 1.15f), new Vector3(-3.2f, 1.1f, c.FrontZ - 3.2f), 9);
        Chain(c, gate, new Vector3(3.2f, 4.4f, c.FrontZ - 1.15f), new Vector3(3.2f, 1.1f, c.FrontZ - 3.2f), 9);

        // Шипы на решётке и нижняя линия.
        for (int i = -4; i <= 4; i++)
        {
            c.Cube(gate, "Portcullis_Spike_" + i,
                new Vector3(i * 0.38f, 4.05f, c.FrontZ - 1.18f),
                new Vector3(0.10f, 0.45f, 0.10f),
                c.MatIron,
                false).transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        }

        // Деревянные брусья по краям входа.
        c.Cube(gate, "Gate_Wooden_Beam_Left", new Vector3(-3.9f, 3.2f, c.FrontZ - 1.0f), new Vector3(0.18f, 4.4f, 0.18f), c.MatWood, false);
        c.Cube(gate, "Gate_Wooden_Beam_Right", new Vector3(3.9f, 3.2f, c.FrontZ - 1.0f), new Vector3(0.18f, 4.4f, 0.18f), c.MatWood, false);
    }

    // ─────────────────────────────────────────────────────────
    // Двор
    // ─────────────────────────────────────────────────────────

    private static void BuildCourtyardProps(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject props = c.Child(parent, "Courtyard_Organized_Props_V6");

        // Главный центр двора не трогаем. Декор живёт по краям.
        // Левая зона — хозяйственная.
        HayStack(c, props, new Vector3(-13.8f, 0.30f, -7.5f));
        HayStack(c, props, new Vector3(-12.1f, 0.28f, -8.6f));
        BarrelGroup(c, props, new Vector3(-13.7f, 0.58f, -2.2f), 0);
        Bucket(c, props, new Vector3(-10.4f, 0.27f, -4.7f));

        // Телегу отводим ближе к стене, чтобы не создавалась каша на маршруте.
        Cart(c, props, new Vector3(-12.4f, 0.45f, -10.0f), 4f);

        // Правая зона — костёр/стража.
        Campfire(c, props, new Vector3(9.6f, 0.22f, -8.0f));
        Bench(c, props, new Vector3(7.7f, 0.35f, -9.25f), 0f);
        Bench(c, props, new Vector3(11.4f, 0.35f, -9.25f), 0f);
        SmallTable(c, props, new Vector3(9.55f, 0.48f, -10.1f));

        TrainingDummy(c, props, new Vector3(12.0f, 0f, -4.9f));
        WeaponRack(c, props, new Vector3(14.0f, 0f, -4.1f), 0f);

        // Складовые предметы прижаты к стенам.
        CrateStack(c, props, new Vector3(12.2f, 0.52f, -1.4f), 0);
        BarrelGroup(c, props, new Vector3(14.2f, 0.58f, -0.6f), 0);

        SignPost(c, props, new Vector3(-4.0f, 0f, -10.8f), "Route_Sign_To_Keep");
        SignPost(c, props, new Vector3(4.2f, 0f, -10.8f), "Route_Sign_To_Dungeon");

        // Камни только по краям маршрута.
        for (int i = 0; i < 12; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (5.8f + (i % 3) * 1.4f);
            float z = -9.6f + (i / 2) * 1.75f;

            c.Sphere(props, "Small_Courtyard_Edge_Stone_Clean_" + i,
                new Vector3(x, 0.18f, z),
                new Vector3(0.30f, 0.09f, 0.24f),
                c.MatRock,
                false);
        }

        // Парадные огни у входа в донжон.
        c.Cube(props, "Keep_Entrance_Brazier_Base_Left", new Vector3(-2.9f, 0.35f, 2.7f), new Vector3(0.55f, 0.70f, 0.55f), c.MatStoneLight);
        c.Cube(props, "Keep_Entrance_Brazier_Fire_Left", new Vector3(-2.9f, 0.92f, 2.7f), new Vector3(0.36f, 0.38f, 0.36f), c.MatFire, false);
        c.Cube(props, "Keep_Entrance_Brazier_Base_Right", new Vector3(2.9f, 0.35f, 2.7f), new Vector3(0.55f, 0.70f, 0.55f), c.MatStoneLight);
        c.Cube(props, "Keep_Entrance_Brazier_Fire_Right", new Vector3(2.9f, 0.92f, 2.7f), new Vector3(0.36f, 0.38f, 0.36f), c.MatFire, false);

        c.PointLight(props, "Keep_Entrance_Brazier_Light_Left", new Vector3(-2.9f, 1.15f, 2.7f), new Color(1f, 0.45f, 0.10f), 0.8f, 4.0f, true);
        c.PointLight(props, "Keep_Entrance_Brazier_Light_Right", new Vector3(2.9f, 1.15f, 2.7f), new Color(1f, 0.45f, 0.10f), 0.8f, 4.0f, true);
    }

    private static void BuildStableAndForgeDecor(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject decor = c.Child(parent, "Stable_And_Forge_Decor_Reworked_V4");

        // Конюшня у левой стены.
        c.Cube(decor, "Stable_Feeding_Trough", new Vector3(-11.9f, 0.35f, -5.7f), new Vector3(2.4f, 0.35f, 0.50f), c.MatWood);
        HayStack(c, decor, new Vector3(-13.2f, 0.25f, -3.2f));
        HayStack(c, decor, new Vector3(-11.1f, 0.25f, -2.2f));
        Bucket(c, decor, new Vector3(-10.4f, 0.27f, -5.5f));

        // Кузница у правой стены.
        Anvil(c, decor, new Vector3(10.9f, 0.55f, -4.7f));
        CoalPile(c, decor, new Vector3(8.8f, 0.18f, -4.9f));
        ToolRack(c, decor, new Vector3(12.6f, 0f, -2.0f));

        // Небольшая линия досок/дров возле кузницы.
        for (int i = 0; i < 5; i++)
        {
            GameObject plank = c.Cube(decor, "Forge_Wood_Plank_" + i,
                new Vector3(9.0f + i * 0.45f, 0.23f, -2.6f),
                new Vector3(0.65f, 0.12f, 0.16f),
                c.MatWood,
                false);
            plank.transform.rotation = Quaternion.Euler(0f, i * 11f, 0f);
        }
    }

    private static void BuildWallDecor(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject wall = c.Child(parent, "Wall_Decor_Ivy_Banners_Drains");

        // Плющ на стенах и донжоне.
        IvyPatch(c, wall, new Vector3(-6.5f, 3.0f, c.FrontZ - 0.56f), true, 0);
        IvyPatch(c, wall, new Vector3(12.6f, 3.4f, c.FrontZ - 0.56f), true, 1);
        IvyPatch(c, wall, new Vector3(-7.9f, 4.0f, 4.1f), true, 2);
        IvyPatch(c, wall, new Vector3(7.9f, 4.0f, 4.1f), true, 3);

        // Деревянные водостоки/балки на стенах.
        c.Cube(wall, "Wall_Drain_Left", new Vector3(-13.2f, 4.7f, c.FrontZ - 0.75f), new Vector3(0.18f, 0.18f, 1.25f), c.MatWood, false);
        c.Cube(wall, "Wall_Drain_Right", new Vector3(13.2f, 4.7f, c.FrontZ - 0.75f), new Vector3(0.18f, 0.18f, 1.25f), c.MatWood, false);
    }

    // ─────────────────────────────────────────────────────────
    // Тронный зал
    // ─────────────────────────────────────────────────────────

    private static void BuildThroneHallProps(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject hall = c.Child(parent, "Throne_Hall_Clean_Royal_Interior_BASE_V7");

        // Основной декор минимальный: дополнительную полировку добавляет CastleRoyalInteriorPolishBuilder.
        Banner(c, hall, new Vector3(-7.35f, 5.35f, 8.9f), false);
        Banner(c, hall, new Vector3(7.35f, 5.35f, 8.9f), false);
        Banner(c, hall, new Vector3(-3.3f, 5.45f, 13.9f), true);
        Banner(c, hall, new Vector3(3.3f, 5.45f, 13.9f), true);

        // Столы и лавки строго у стен.
        LongTable(c, hall, new Vector3(-6.15f, 0.65f, 8.05f), 0f);
        LongTable(c, hall, new Vector3(6.15f, 0.65f, 8.05f), 0f);
        Bench(c, hall, new Vector3(-6.15f, 0.36f, 6.95f), 0f);
        Bench(c, hall, new Vector3(-6.15f, 0.36f, 9.15f), 0f);
        Bench(c, hall, new Vector3(6.15f, 0.36f, 6.95f), 0f);
        Bench(c, hall, new Vector3(6.15f, 0.36f, 9.15f), 0f);

        Candelabra(c, hall, new Vector3(-2.45f, 0.5f, 11.25f));
        Candelabra(c, hall, new Vector3(2.45f, 0.5f, 11.25f));

        Chest(c, hall, new Vector3(-3.6f, 0.45f, 12.85f));
        Chest(c, hall, new Vector3(3.6f, 0.45f, 12.85f));
        CrownOnThrone(c, hall, new Vector3(0f, 3.95f, 13.0f));

        c.PointLight(hall, "Warm_Throne_Back_Light", new Vector3(0f, 4.8f, 12.9f), new Color(1f, 0.45f, 0.12f), 0.9f, 5.2f, true);
    }

    // ─────────────────────────────────────────────────────────
    // Подземелье
    // ─────────────────────────────────────────────────────────

    private static void BuildDungeonProps(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject dungeon = c.Child(parent, "Dungeon_Props_Enhanced");

        DungeonBars(c, dungeon, new Vector3(-12f, 0.2f, 9.72f));

        // Узник.
        c.Cube(dungeon, "Prisoner_Body",
            new Vector3(-12f, -0.35f, 12.3f),
            new Vector3(0.48f, 1.75f, 0.48f),
            c.NewMaterial(new Color(0.46f, 0.38f, 0.24f)));

        c.Cube(dungeon, "Prisoner_Head",
            new Vector3(-12f, 0.75f, 12.3f),
            new Vector3(0.42f, 0.42f, 0.42f),
            c.NewMaterial(new Color(0.72f, 0.58f, 0.42f)),
            false);

        // Цепи и скелетные детали.
        Chain(c, dungeon, new Vector3(-14.58f, 1.45f, 11.0f), new Vector3(-14.58f, 0.4f, 11.0f), 5);
        Chain(c, dungeon, new Vector3(-14.58f, 1.45f, 12.7f), new Vector3(-14.58f, 0.4f, 12.7f), 5);
        BonePile(c, dungeon, new Vector3(-10.2f, -1.15f, 12.9f));
        Barrel(c, dungeon, new Vector3(-13.9f, -0.78f, 13.0f));
        Crate(c, dungeon, new Vector3(-10.1f, -0.84f, 10.7f));

        c.PointLight(dungeon, "Weak_Dungeon_Light",
            new Vector3(-12f, 1.7f, 10.9f),
            new Color(1f, 0.48f, 0.12f),
            1.05f,
            6f,
            true);
    }

    // ─────────────────────────────────────────────────────────
    // GLB-маркеры
    // ─────────────────────────────────────────────────────────

    private static void BuildImportedAssetMarkers(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject imported = c.Child(parent, "Imported_GLB_Attempts");

        // castle_gate.glb временно не вставляем: импортированный prefab может иметь собственные
        // коллайдеры и снова заблокировать проход через ворота. Ручные ворота уже построены
        // в CastleBlockoutBuilder.cs.
        c.TryImportedAsset(imported, "barrel.glb", "Imported_Barrel_GLB",
            new Vector3(13.8f, 0.15f, -4.4f),
            Vector3.one,
            Quaternion.identity);

        c.TryImportedAsset(imported, "wooden_crate.glb", "Imported_Wooden_Crate_GLB",
            new Vector3(14.8f, 0.15f, -4.4f),
            Vector3.one,
            Quaternion.identity);

        c.TryImportedAsset(imported, "wall_torch.glb", "Imported_Wall_Torch_GLB",
            new Vector3(-2.8f, 2.2f, c.FrontZ - 1.05f),
            Vector3.one,
            Quaternion.identity);

        c.TryImportedAsset(imported, "dungeon_bars.glb", "Imported_Dungeon_Bars_GLB",
            new Vector3(-12f, -0.65f, 9.45f),
            Vector3.one,
            Quaternion.identity);

        c.TryImportedAsset(imported, "stone_pillar.glb", "Imported_Stone_Pillar_GLB",
            new Vector3(-5.2f, 0.15f, 11.0f),
            Vector3.one,
            Quaternion.identity);
    }

    // ─────────────────────────────────────────────────────────
    // Помощники декора
    // ─────────────────────────────────────────────────────────

    private static void Torch(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front)
    {
        GameObject torch = c.Child(parent, "Wall_Torch");

        c.Cube(torch, "Backplate", pos + new Vector3(0f, 0.02f, front ? 0.08f : 0f), new Vector3(0.35f, 0.72f, 0.08f), c.MatIron, false);
        c.Cube(torch, "Handle", pos, new Vector3(0.11f, 0.6f, 0.11f), c.MatWood, false);
        c.Cube(torch, "Flame_Core", pos + Vector3.up * 0.43f, new Vector3(0.25f, 0.35f, 0.25f), c.MatFire, false);
        c.Cube(torch, "Flame_Glow", pos + Vector3.up * 0.48f, new Vector3(0.38f, 0.25f, 0.38f), c.MatFire, false);

        c.PointLight(torch, "Torch_Point_Light",
            pos + Vector3.up * 0.55f,
            new Color(1f, 0.56f, 0.13f),
            1.45f,
            5.2f,
            true);
    }

    private static void LanternPost(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject lantern = c.Child(parent, "Lantern_Post");

        c.Cube(lantern, "Post", pos + new Vector3(0f, 1.05f, 0f), new Vector3(0.14f, 2.1f, 0.14f), c.MatWood);
        c.Cube(lantern, "Arm", pos + new Vector3(0.45f, 2.0f, 0f), new Vector3(0.9f, 0.12f, 0.12f), c.MatWood, false);
        c.Cube(lantern, "Lantern_Box", pos + new Vector3(0.9f, 1.72f, 0f), new Vector3(0.35f, 0.42f, 0.35f), c.MatIron, false);
        c.Cube(lantern, "Lantern_Flame", pos + new Vector3(0.9f, 1.74f, 0f), new Vector3(0.18f, 0.24f, 0.18f), c.MatFire, false);

        c.PointLight(lantern, "Lantern_Light", pos + new Vector3(0.9f, 1.8f, 0f), new Color(1f, 0.52f, 0.13f), 0.75f, 4.2f, true);
    }

    private static void Flag(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        c.Cube(parent, "Flag_Pole", pos + new Vector3(0f, 0.85f * scale, 0f), new Vector3(0.08f, 1.9f * scale, 0.08f), c.MatWood, false);
        c.Cube(parent, "Red_Flag_Main", pos + new Vector3(0.55f * scale, 1.45f * scale, 0f), new Vector3(1.1f * scale, 0.58f * scale, 0.06f), c.MatRed, false);
        c.Cube(parent, "Red_Flag_Tail", pos + new Vector3(1.18f * scale, 1.28f * scale, 0f), new Vector3(0.45f * scale, 0.25f * scale, 0.06f), c.MatRed, false);
    }

    private static void Banner(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front)
    {
        GameObject banner = c.Child(parent, "Wall_Banner");

        Vector3 holderScale = front ? new Vector3(1.35f, 0.10f, 0.10f) : new Vector3(0.10f, 0.10f, 1.35f);
        Vector3 clothScale = front ? new Vector3(1.1f, 2.2f, 0.06f) : new Vector3(0.06f, 2.2f, 1.1f);

        c.Cube(banner, "Banner_Holder", pos + new Vector3(0f, 0.7f, 0f), holderScale, c.MatWood, false);
        c.Cube(banner, "Red_Banner_Cloth", pos + new Vector3(0f, -0.25f, 0f), clothScale, c.MatRed, false);
        c.Cube(banner, "Banner_Gold_Mark", pos + new Vector3(0f, -0.1f, front ? -0.04f : 0f), front ? new Vector3(0.35f, 0.35f, 0.04f) : new Vector3(0.04f, 0.35f, 0.35f), c.NewMaterial(new Color(0.9f, 0.65f, 0.18f)), false);
    }

    private static void Shield(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front)
    {
        Material gold = c.NewMaterial(new Color(0.88f, 0.62f, 0.16f));

        Vector3 scale = front ? new Vector3(0.8f, 1.0f, 0.08f) : new Vector3(0.08f, 1.0f, 0.8f);
        c.Cube(parent, "Wall_Shield_Base", pos, scale, c.MatRed, false);
        c.Cube(parent, "Wall_Shield_Cross", pos + new Vector3(0f, 0f, front ? -0.05f : 0f), front ? new Vector3(0.16f, 0.9f, 0.05f) : new Vector3(0.05f, 0.9f, 0.16f), gold, false);
        c.Cube(parent, "Wall_Shield_Cross_Horizontal", pos + new Vector3(0f, 0f, front ? -0.06f : 0f), front ? new Vector3(0.65f, 0.16f, 0.05f) : new Vector3(0.05f, 0.16f, 0.65f), gold, false);
    }

    private static void Crate(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject crate = c.Child(parent, "Wooden_Crate");

        c.Cube(crate, "Body", pos, new Vector3(0.9f, 0.9f, 0.9f), c.MatWood);
        c.Cube(crate, "Iron_Band_X", pos + new Vector3(0f, 0.02f, 0f), new Vector3(0.95f, 0.08f, 0.95f), c.MatIron, false);
        c.Cube(crate, "Iron_Band_Y", pos + new Vector3(0f, 0.02f, 0f), new Vector3(0.08f, 0.95f, 0.95f), c.MatIron, false);
        c.Cube(crate, "Diagonal_Brace", pos + new Vector3(0f, 0.02f, -0.48f), new Vector3(1.15f, 0.08f, 0.07f), c.MatIron, false).transform.rotation = Quaternion.Euler(0f, 0f, 35f);
    }

    private static void CrateStack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, int seed)
    {
        Crate(c, parent, pos);
        Crate(c, parent, pos + new Vector3(1.05f, 0f, 0.05f));
        Crate(c, parent, pos + new Vector3(0.55f, 0.94f, 0.02f));
        Crate(c, parent, pos + new Vector3(-0.8f, 0f, 0.75f));
    }

    private static void Barrel(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject barrel = c.Child(parent, "Barrel");

        c.Cylinder(barrel, "Body", pos, new Vector3(0.55f, 0.55f, 0.55f), c.MatWood);
        c.Cylinder(barrel, "Top_Hoop", pos + Vector3.up * 0.42f, new Vector3(0.58f, 0.04f, 0.58f), c.MatIron, false);
        c.Cylinder(barrel, "Bottom_Hoop", pos + Vector3.down * 0.42f, new Vector3(0.58f, 0.04f, 0.58f), c.MatIron, false);
        c.Cylinder(barrel, "Middle_Hoop", pos, new Vector3(0.57f, 0.035f, 0.57f), c.MatIron, false);
    }

    private static void BarrelGroup(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, int seed)
    {
        Barrel(c, parent, pos);
        Barrel(c, parent, pos + new Vector3(0.85f, 0f, 0.95f));
        Barrel(c, parent, pos + new Vector3(-0.75f, 0f, 0.80f));
    }

    private static void Campfire(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject fire = c.Child(parent, "Campfire_Detailed");

        c.Cube(fire, "Wood_1", pos + new Vector3(0f, 0.12f, 0f), new Vector3(1.25f, 0.18f, 0.18f), c.MatWood);
        c.Cube(fire, "Wood_2", pos + new Vector3(0f, 0.12f, 0f), new Vector3(0.18f, 0.18f, 1.25f), c.MatWood);
        c.Cube(fire, "Wood_3", pos + new Vector3(0f, 0.20f, 0f), new Vector3(1.05f, 0.16f, 0.16f), c.MatWood).transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        c.Cube(fire, "Flame_Core", pos + new Vector3(0f, 0.48f, 0f), new Vector3(0.55f, 0.68f, 0.55f), c.MatFire, false);
        c.Cube(fire, "Flame_Tip", pos + new Vector3(0f, 0.95f, 0f), new Vector3(0.32f, 0.44f, 0.32f), c.MatFire, false);

        c.PointLight(fire, "Campfire_Light", pos + new Vector3(0f, 1.15f, 0f), new Color(1f, 0.45f, 0.08f), 2.2f, 7f, true);
    }

    private static void HayStack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        Material hay = c.NewMaterial(new Color(0.72f, 0.62f, 0.22f));

        c.Cube(parent, "Hay_Stack_Base", pos, new Vector3(2.2f, 0.7f, 1.4f), hay);
        c.Cube(parent, "Hay_Stack_Top", pos + new Vector3(0f, 0.55f, 0f), new Vector3(1.6f, 0.5f, 1.0f), hay, false);

        for (int i = 0; i < 5; i++)
        {
            c.Cube(parent, "Loose_Hay_" + i,
                pos + new Vector3(-0.8f + i * 0.4f, 0.85f, -0.25f + (i % 2) * 0.45f),
                new Vector3(0.08f, 0.08f, 0.9f),
                hay,
                false).transform.rotation = Quaternion.Euler(0f, i * 18f, 8f);
        }
    }

    private static void SignPost(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, string name)
    {
        GameObject sign = c.Child(parent, name);

        c.Cube(sign, "Post", pos + new Vector3(0f, 0.9f, 0f), new Vector3(0.16f, 1.8f, 0.16f), c.MatWood);
        c.Cube(sign, "Arrow_To_Keep", pos + new Vector3(0.7f, 1.55f, 0f), new Vector3(1.3f, 0.35f, 0.12f), c.MatWood, false);
        c.Cube(sign, "Arrow_Point", pos + new Vector3(1.42f, 1.55f, 0f), new Vector3(0.35f, 0.35f, 0.12f), c.MatWood, false);
    }

    private static void DungeonBars(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject bars = c.Child(parent, "Dungeon_Bars");

        c.Cube(bars, "Top", pos + new Vector3(0f, 1.7f, 0f), new Vector3(3.8f, 0.1f, 0.1f), c.MatIron, false);
        c.Cube(bars, "Bottom", pos + new Vector3(0f, -0.2f, 0f), new Vector3(3.8f, 0.1f, 0.1f), c.MatIron, false);
        c.Cube(bars, "Left_Frame", pos + new Vector3(-1.9f, 0.75f, 0f), new Vector3(0.1f, 2.0f, 0.1f), c.MatIron, false);
        c.Cube(bars, "Right_Frame", pos + new Vector3(1.9f, 0.75f, 0f), new Vector3(0.1f, 2.0f, 0.1f), c.MatIron, false);

        for (int i = -3; i <= 3; i++)
        {
            c.Cube(bars, "Vertical_Bar_" + i,
                pos + new Vector3(i * 0.48f, 0.75f, 0f),
                new Vector3(0.08f, 2.0f, 0.08f),
                c.MatIron,
                false);
        }
    }

    private static void Bench(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject bench = c.Child(parent, "Bench");
        bench.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(bench, "Seat", pos + new Vector3(0f, 0.35f, 0f), new Vector3(1.8f, 0.18f, 0.5f), c.MatWood);
        c.Cube(bench, "Leg_Left", pos + new Vector3(-0.65f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 0.18f), c.MatWood);
        c.Cube(bench, "Leg_Right", pos + new Vector3(0.65f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 0.18f), c.MatWood);
    }

    private static void SmallTable(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cube(parent, "Small_Table_Top", pos + new Vector3(0f, 0.55f, 0f), new Vector3(1.3f, 0.16f, 0.8f), c.MatWood);
        c.Cube(parent, "Small_Table_Leg_1", pos + new Vector3(-0.45f, 0.28f, -0.25f), new Vector3(0.14f, 0.55f, 0.14f), c.MatWood);
        c.Cube(parent, "Small_Table_Leg_2", pos + new Vector3(0.45f, 0.28f, -0.25f), new Vector3(0.14f, 0.55f, 0.14f), c.MatWood);
        c.Cube(parent, "Small_Table_Leg_3", pos + new Vector3(-0.45f, 0.28f, 0.25f), new Vector3(0.14f, 0.55f, 0.14f), c.MatWood);
        c.Cube(parent, "Small_Table_Leg_4", pos + new Vector3(0.45f, 0.28f, 0.25f), new Vector3(0.14f, 0.55f, 0.14f), c.MatWood);
    }

    private static void LongTable(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject table = c.Child(parent, "Long_Table");
        table.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(table, "Top", pos + new Vector3(0f, 0.58f, 0f), new Vector3(3.2f, 0.18f, 0.9f), c.MatWood);
        c.Cube(table, "Left_Leg", pos + new Vector3(-1.25f, 0.30f, 0f), new Vector3(0.18f, 0.6f, 0.18f), c.MatWood);
        c.Cube(table, "Right_Leg", pos + new Vector3(1.25f, 0.30f, 0f), new Vector3(0.18f, 0.6f, 0.18f), c.MatWood);
        c.Cube(table, "Cup_1", pos + new Vector3(-0.75f, 0.78f, 0.2f), new Vector3(0.18f, 0.24f, 0.18f), c.MatIron, false);
        c.Cube(table, "Cup_2", pos + new Vector3(0.75f, 0.78f, -0.2f), new Vector3(0.18f, 0.24f, 0.18f), c.MatIron, false);
    }

    private static void Cart(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject cart = c.Child(parent, "Small_Wooden_Cart");
        cart.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(cart, "Cart_Body", pos + new Vector3(0f, 0.55f, 0f), new Vector3(2.0f, 0.55f, 1.0f), c.MatWood);
        c.Cylinder(cart, "Wheel_Left", pos + new Vector3(-0.95f, 0.35f, -0.65f), new Vector3(0.32f, 0.08f, 0.32f), c.MatWood, false).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        c.Cylinder(cart, "Wheel_Right", pos + new Vector3(0.95f, 0.35f, -0.65f), new Vector3(0.32f, 0.08f, 0.32f), c.MatWood, false).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        c.Cube(cart, "Cart_Handle", pos + new Vector3(0f, 0.65f, 0.95f), new Vector3(0.22f, 0.18f, 1.1f), c.MatWood, false);
    }

    private static void TrainingDummy(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject dummy = c.Child(parent, "Training_Dummy");

        c.Cube(dummy, "Post", pos + new Vector3(0f, 0.9f, 0f), new Vector3(0.18f, 1.8f, 0.18f), c.MatWood);
        c.Cube(dummy, "Body", pos + new Vector3(0f, 1.45f, 0f), new Vector3(0.75f, 0.8f, 0.35f), c.MatWood);
        c.Cube(dummy, "Arm_Left", pos + new Vector3(-0.7f, 1.55f, 0f), new Vector3(0.9f, 0.14f, 0.14f), c.MatWood, false);
        c.Cube(dummy, "Arm_Right", pos + new Vector3(0.7f, 1.55f, 0f), new Vector3(0.9f, 0.14f, 0.14f), c.MatWood, false);
        c.Cube(dummy, "Head", pos + new Vector3(0f, 2.05f, 0f), new Vector3(0.45f, 0.45f, 0.45f), c.MatWood, false);
    }

    private static void WeaponRack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject rack = c.Child(parent, "Weapon_Rack");
        rack.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(rack, "Rack_Back", pos + new Vector3(0f, 1.0f, 0f), new Vector3(1.8f, 1.8f, 0.12f), c.MatWood, false);

        for (int i = -2; i <= 2; i++)
        {
            c.Cube(rack, "Spear_" + i, pos + new Vector3(i * 0.35f, 1.25f, -0.1f), new Vector3(0.05f, 1.9f, 0.05f), c.MatWood, false);
            c.Cube(rack, "Spear_Tip_" + i, pos + new Vector3(i * 0.35f, 2.25f, -0.1f), new Vector3(0.16f, 0.22f, 0.06f), c.MatIron, false);
        }
    }

    private static void ShieldRack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject rack = c.Child(parent, "Shield_Rack");
        rack.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(rack, "Rack_Post", pos + new Vector3(0f, 0.85f, 0f), new Vector3(0.12f, 1.7f, 0.12f), c.MatWood, false);
        Shield(c, rack, pos + new Vector3(-0.45f, 1.2f, -0.05f), true);
        Shield(c, rack, pos + new Vector3(0.45f, 1.2f, -0.05f), true);
    }

    private static void Bucket(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cylinder(parent, "Bucket", pos, new Vector3(0.28f, 0.27f, 0.28f), c.MatWood);
        c.Cube(parent, "Bucket_Handle", pos + new Vector3(0f, 0.28f, 0f), new Vector3(0.56f, 0.06f, 0.06f), c.MatIron, false);
    }

    private static void Anvil(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cube(parent, "Anvil_Base", pos + new Vector3(0f, -0.25f, 0f), new Vector3(0.55f, 0.35f, 0.45f), c.MatIron);
        c.Cube(parent, "Anvil_Top", pos + new Vector3(0f, 0.05f, 0f), new Vector3(1.2f, 0.28f, 0.42f), c.MatIron);
        c.Cube(parent, "Anvil_Horn", pos + new Vector3(0.75f, 0.05f, 0f), new Vector3(0.55f, 0.18f, 0.22f), c.MatIron, false);
    }

    private static void CoalPile(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        for (int i = 0; i < 8; i++)
        {
            c.Sphere(parent, "Coal_" + i,
                pos + new Vector3(-0.45f + i * 0.13f, 0f, (i % 3) * 0.17f),
                new Vector3(0.18f, 0.08f, 0.16f),
                c.MatDark,
                false);
        }
    }

    private static void ToolRack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cube(parent, "Tool_Rack_Back", pos + new Vector3(0f, 1.0f, 0f), new Vector3(1.7f, 1.8f, 0.12f), c.MatWood, false);
        for (int i = -2; i <= 2; i++)
        {
            c.Cube(parent, "Tool_Handle_" + i, pos + new Vector3(i * 0.32f, 1.0f, -0.08f), new Vector3(0.05f, 1.2f, 0.05f), c.MatWood, false);
            c.Cube(parent, "Tool_Head_" + i, pos + new Vector3(i * 0.32f, 1.65f, -0.08f), new Vector3(0.20f, 0.18f, 0.06f), c.MatIron, false);
        }
    }

    private static void IvyPatch(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front, int seed)
    {
        Material ivy = c.NewMaterial(new Color(0.10f, 0.32f, 0.10f));

        for (int i = 0; i < 12; i++)
        {
            float dx = Mathf.Sin((seed + i) * 1.7f) * 0.9f;
            float dy = -i * 0.18f;
            float z = front ? -0.04f : 0f;

            c.Cube(parent, "Ivy_Leaf_" + seed + "_" + i,
                pos + new Vector3(dx, dy, z),
                front ? new Vector3(0.22f, 0.16f, 0.04f) : new Vector3(0.04f, 0.16f, 0.22f),
                ivy,
                false);
        }

        c.Cube(parent, "Ivy_Stem_" + seed,
            pos + new Vector3(0f, -0.9f, front ? -0.03f : 0f),
            front ? new Vector3(0.07f, 1.9f, 0.035f) : new Vector3(0.035f, 1.9f, 0.07f),
            ivy,
            false);
    }

    private static void Chain(CastleGenerator.CastleContext c, GameObject parent, Vector3 from, Vector3 to, int links)
    {
        Vector3 delta = to - from;

        for (int i = 0; i < links; i++)
        {
            float t = links <= 1 ? 0f : i / (float)(links - 1);
            Vector3 pos = from + delta * t;

            GameObject link = c.Cube(parent, "Chain_Link_" + i,
                pos,
                new Vector3(0.12f, 0.22f, 0.08f),
                c.MatIron,
                false);
            link.transform.rotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 45f : -45f);
        }
    }

    private static void Candelabra(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject cand = c.Child(parent, "Candelabra");

        c.Cube(cand, "Stand", pos + new Vector3(0f, 0.75f, 0f), new Vector3(0.10f, 1.5f, 0.10f), c.MatIron, false);
        c.Cube(cand, "Arm", pos + new Vector3(0f, 1.35f, 0f), new Vector3(1.2f, 0.08f, 0.08f), c.MatIron, false);

        for (int i = -1; i <= 1; i++)
        {
            c.Cube(cand, "Candle_" + i, pos + new Vector3(i * 0.5f, 1.55f, 0f), new Vector3(0.12f, 0.35f, 0.12f), c.MatStoneLight, false);
            c.Cube(cand, "Candle_Flame_" + i, pos + new Vector3(i * 0.5f, 1.78f, 0f), new Vector3(0.12f, 0.16f, 0.12f), c.MatFire, false);
            c.PointLight(cand, "Candle_Light_" + i, pos + new Vector3(i * 0.5f, 1.82f, 0f), new Color(1f, 0.55f, 0.16f), 0.38f, 2.1f, true);
        }
    }

    private static void Chest(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        Material gold = c.NewMaterial(new Color(0.9f, 0.65f, 0.15f));

        c.Cube(parent, "Chest_Body", pos, new Vector3(1.1f, 0.55f, 0.7f), c.MatWood);
        c.Cube(parent, "Chest_Lid", pos + new Vector3(0f, 0.36f, 0f), new Vector3(1.16f, 0.25f, 0.74f), c.MatWood, false);
        c.Cube(parent, "Chest_Gold_Lock", pos + new Vector3(0f, 0.16f, -0.38f), new Vector3(0.22f, 0.20f, 0.06f), gold, false);
    }

    private static void CrownOnThrone(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        Material gold = c.NewMaterial(new Color(0.95f, 0.72f, 0.16f));

        c.Cylinder(parent, "Crown_Base", pos, new Vector3(0.45f, 0.08f, 0.45f), gold, false);

        for (int i = 0; i < 6; i++)
        {
            float a = i * 60f * Mathf.Deg2Rad;
            c.Cube(parent, "Crown_Spike_" + i,
                pos + new Vector3(Mathf.Cos(a) * 0.34f, 0.22f, Mathf.Sin(a) * 0.34f),
                new Vector3(0.08f, 0.34f, 0.08f),
                gold,
                false);
        }
    }

    private static void BonePile(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        Material bone = c.NewMaterial(new Color(0.75f, 0.68f, 0.55f));

        for (int i = 0; i < 5; i++)
        {
            GameObject b = c.Cube(parent, "Bone_" + i,
                pos + new Vector3(-0.5f + i * 0.25f, 0f, (i % 2) * 0.18f),
                new Vector3(0.55f, 0.07f, 0.07f),
                bone,
                false);
            b.transform.rotation = Quaternion.Euler(0f, i * 28f, i * 8f);
        }

        c.Sphere(parent, "Skull_Blockout", pos + new Vector3(0.55f, 0.10f, 0.18f), new Vector3(0.28f, 0.22f, 0.24f), bone, false);
    }
}
