using UnityEngine;

/// <summary>
/// Фаза 6: Визуальная доводка орочьей арены.
/// Слой ассетов поверх существующей модульной арены:
/// — Оружие из Medieval Weapons pack (мечи, копья, топоры, щиты на земле и стойках)
/// — Дым из msVFX Smoke pack (костры, портал)
/// — Камни из Pure Poly Nature (граница арены)
/// — Ритуальный круг в центре
/// — Орочья кузня, клетки, тотемы у портала
/// — Портальная атмосфера (фиолетовые партиклы)
/// Не трогает PlacementGuard — ставим только на визуальный слой (NoCollider).
/// </summary>
public static class L0OrcArenaEnhancement
{
    // ───── ОРУЖИЕ (Medieval Weapons) ─────
    private static readonly string Sword = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/sword-1.prefab";
    private static readonly string Spear = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/spear-1.prefab";
    private static readonly string Axe = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/axe-1.prefab";
    private static readonly string Dagger = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Dagger.prefab";
    private static readonly string Maul = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Maul.prefab";
    private static readonly string TwoHandedAxe = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/2HandedAxe.prefab";
    private static readonly string Shield1 = "Assets/New Folder/Low Poly Modular Medieval Weapons/Prefabs/shield/shield 1.prefab";
    private static readonly string Shield2 = "Assets/New Folder/Low Poly Modular Medieval Weapons/Prefabs/shield/shield 2.prefab";
    private static readonly string Shield3 = "Assets/New Folder/Low Poly Modular Medieval Weapons/Prefabs/shield/shield 3.prefab";
    private static readonly string Spear3 = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/spear-3.prefab";

    // ───── ПРИРОДА (Pure Poly) ─────
    private static readonly string RockMoss09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    private static readonly string RockPile10 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_10.prefab";
    private static readonly string Pebbles09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Cemetery_Pebbles_09.prefab";
    private static readonly string FloorTile = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Floor_Tile_15.prefab";

    // ───── DUNGEON (тёмная крепость) ─────
    private const string Dungeon = "Assets/DungeonAssetPack/Prefabs/";
    private static readonly string DgTorch = Dungeon + "Props/Torch.prefab";
    private static readonly string DgCauldron = Dungeon + "Props/Big_Cauldron.prefab";
    private static readonly string DgStone = Dungeon + "Props/Stone.prefab";
    private static readonly string DgBrickPile = Dungeon + "Props/Brick_Pile.prefab";
    private static readonly string DgBrokenColumn = Dungeon + "Props/Broken_Column.prefab";
    private static readonly string DgColumn = Dungeon + "Props/Column.prefab";
    private static readonly string DgStonePedestal = Dungeon + "Props/Stone_Pedestal.prefab";
    private static readonly string DgFloorFireTrap = Dungeon + "Traps/Floor_Fire_Trap.prefab";
    private static readonly string DgFloorTrap = Dungeon + "StructureParts/FloorTrap.prefab";
    private static readonly string DgJailWall = Dungeon + "StructureParts/Jail_Wall.prefab";
    private static readonly string DgJailColumn = Dungeon + "StructureParts/Jail_Column.prefab";
    private static readonly string DgBanner = Dungeon + "Props/Banner.prefab";
    private static readonly string DgCandelabrum = Dungeon + "Props/Candelabrum.prefab";
    private static readonly string DgBarrel = Dungeon + "Props/Barrel.prefab";
    private static readonly string DgDecorativeShield = Dungeon + "Props/Decorative_Shield.prefab";

    // ───── SYNTY ─────
    private static readonly string SyntyFire = "Assets/Synty/PolygonGeneric/Prefabs/FX/FX_Fire_01.prefab";

    // ───── ДЫМКА ─────
    private static readonly string Smoke1 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab";
    private static readonly string Smoke2 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2.prefab";
    private static readonly string Smoke4 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 4.prefab";

    // Light budget — cap active PointLights on the arena to avoid GPU overload.
    private static int _lightBudget;
    private const int MaxArenaLights = 22;

    private static void PlaceLight(string n, Vector3 p, Color c, float intensity, float range, Transform t)
    {
        if (_lightBudget >= MaxArenaLights) return;
        _lightBudget++;
        L0Props.CreatePointLight(n, p, c, intensity, range, t);
    }

    public static void Build(Transform arenaRoot)
    {
        if (arenaRoot == null) return;
        _lightBudget = 0;

        GameObject enhRoot = new GameObject("ArenaEnhancement");
        enhRoot.transform.SetParent(arenaRoot, false);
        Transform root = enhRoot.transform;

        BuildScatteredWeapons(root);
        BuildWeaponRacks(root);
        BuildRitualCircle(root);
        BuildOrcForge(root);
        BuildPrisonerCages(root);
        BuildPortalTotems(root);
        BuildPortalAtmosphere(root);
        BuildArenaRocks(root);
        BuildArenaSmokeEffects(root);

        // Step 8: Central battle dread — Dungeon-ассеты в ритуальном центре + strongpoint акценты
        BuildCentralDread(root);
        // Step 9: Portal sacred path — каменный путь, колонны, усиленная атмосфера
        BuildPortalSacredPath(root);

        // Step 10A: Укрытия в BattleCenter — засеки, 4 костра, кровавые пятна
        BuildBattleCenterObstacles(root);
        // Step 10B: Начало коридора к порталу — ворота черепов, факелы, тотемы (45м)
        BuildCorridorApproach(root);
    }

    // ═══════════════════ РАЗБРОСАННОЕ ОРУЖИЕ ═══════════════════

    private static void BuildScatteredWeapons(Transform root)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;

        // Мечи на земле (после битвы)
        SpawnWeaponOnGround(root, Sword, center + side * -8f + gate * 5f, 85f, 30f, 1.5f);
        SpawnWeaponOnGround(root, Sword, center + side * 12f + gate * -3f, 80f, 120f, 1.4f);

        // Копья воткнуты в землю
        SpawnWeaponStuck(root, Spear, center + side * -15f + gate * 10f, 15f, 0f, 1.8f);
        SpawnWeaponStuck(root, Spear3, center + side * 18f + gate * -8f, 12f, 45f, 1.6f);
        SpawnWeaponStuck(root, Spear, center + side * -5f + gate * -15f, 8f, 90f, 1.7f);

        // Топоры
        SpawnWeaponOnGround(root, Axe, center + side * 6f + gate * 12f, 88f, 200f, 1.5f);
        SpawnWeaponOnGround(root, TwoHandedAxe, center + side * -20f + gate * -5f, 82f, 310f, 1.3f);

        // Кинжалы
        SpawnWeaponOnGround(root, Dagger, center + side * 3f + gate * -10f, 90f, 70f, 1.2f);
        SpawnWeaponOnGround(root, Dagger, center + side * -10f + gate * 8f, 85f, 180f, 1.3f);

        // Молот
        SpawnWeaponOnGround(root, Maul, center + side * -22f + gate * 2f, 87f, 250f, 1.4f);

        // Щиты на земле
        SpawnShieldOnGround(root, Shield1, center + side * -7f + gate * 7f, 0f, 1.5f);
        SpawnShieldOnGround(root, Shield2, center + side * 14f + gate * -6f, 45f, 1.4f);
        SpawnShieldOnGround(root, Shield3, center + side * -18f + gate * -10f, 90f, 1.3f);
        SpawnShieldOnGround(root, Shield1, center + side * 8f + gate * 15f, 120f, 1.5f);
    }

    // ═══════════════════ СТОЙКИ ОРУЖИЯ ═══════════════════

    private static void BuildWeaponRacks(Transform root)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;

        // Стойка у левого лагеря
        CreateWeaponRack(root, center + side * -25f + gate * 15f,
            Quaternion.LookRotation(side), new string[] { Sword, Spear, Axe });

        // Стойка у правого лагеря
        CreateWeaponRack(root, center + side * 25f + gate * -10f,
            Quaternion.LookRotation(-side), new string[] { Spear3, TwoHandedAxe, Dagger });
    }

    private static void CreateWeaponRack(Transform root, Vector3 pos, Quaternion rot, string[] weaponPaths)
    {
        GameObject rack = new GameObject("WeaponRack_Asset");
        rack.transform.SetParent(root, true);
        rack.transform.position = pos;
        rack.transform.rotation = rot;

        // Деревянная рама
        Color wood = new Color(0.28f, 0.18f, 0.09f);
        L0OrcArenaPrimitiveKit.CreatePrimitive(rack.transform, "RackPostL_NoCollider", PrimitiveType.Cylinder,
            new Vector3(-0.6f, 0.8f, 0f), Quaternion.identity, new Vector3(0.08f, 1.6f, 0.08f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(rack.transform, "RackPostR_NoCollider", PrimitiveType.Cylinder,
            new Vector3(0.6f, 0.8f, 0f), Quaternion.identity, new Vector3(0.08f, 1.6f, 0.08f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(rack.transform, "RackBar_NoCollider", PrimitiveType.Cube,
            new Vector3(0f, 1.4f, 0f), Quaternion.identity, new Vector3(1.4f, 0.06f, 0.06f), wood, false);

        // Оружие прислонено к стойке
        for (int i = 0; i < weaponPaths.Length; i++)
        {
            GameObject wpn = LoadPrefab(weaponPaths[i]);
            if (wpn != null)
            {
                float x = -0.35f + i * 0.35f;
                GameObject w = Object.Instantiate(wpn, Vector3.zero, Quaternion.identity, rack.transform);
                w.name = "RackWeapon_" + i;
                w.transform.localPosition = new Vector3(x, 0.6f, 0.1f);
                w.transform.localRotation = Quaternion.Euler(5f, 0f, 3f - i * 3f);
                w.transform.localScale = Vector3.one * 1.5f;
                RemoveAllColliders(w);
                FixMaterials(w);
            }
        }
    }

    // ═══════════════════ РИТУАЛЬНЫЙ КРУГ ═══════════════════

    private static void BuildRitualCircle(Transform root)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        float radius = 8f;

        // 8 каменных плит по кругу
        GameObject floorTile = LoadPrefab(FloorTile);
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0.02f, Mathf.Sin(angle) * radius);
            Quaternion rot = Quaternion.Euler(0f, i * 45f, 0f);

            if (floorTile != null)
            {
                GameObject tile = Object.Instantiate(floorTile, pos, rot, root);
                tile.name = "RitualSlab_" + i;
                tile.transform.localScale = Vector3.one * 1.2f;
                RemoveAllColliders(tile);
                FixMaterials(tile);
            }
            else
            {
                L0OrcArenaPrimitiveKit.CreatePrimitive(root, "RitualSlab_" + i + "_NoCollider", PrimitiveType.Cube,
                    pos, rot, new Vector3(1.5f, 0.3f, 0.8f), L0OrcArenaMaterials.DarkStone, false);
            }
        }

        // 4 руны между плитами (emission)
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 22.5f) * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius);
            Color runeColor = (i % 2 == 0) ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.6f, 0.05f, 0.02f);
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, "Rune_" + i + "_NoCollider", PrimitiveType.Cube,
                pos, Quaternion.Euler(0f, i * 90f + 22.5f, 0f),
                new Vector3(0.6f, 0.04f, 0.6f), runeColor, false, true);
        }

        // Центральный жертвенник
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "Altar_Base_NoCollider", PrimitiveType.Cube,
            center + Vector3.up * 0.4f, Quaternion.Euler(0f, 22.5f, 0f),
            new Vector3(1.5f, 0.8f, 1.5f), L0OrcArenaMaterials.DarkStone, false);

        // Чаша с кровью (emission)
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "Altar_Bowl_NoCollider", PrimitiveType.Cylinder,
            center + Vector3.up * 0.85f, Quaternion.identity,
            new Vector3(0.6f, 0.08f, 0.6f), new Color(0.5f, 0.02f, 0.01f), false, true);

        // Свечение жертвенника
        PlaceLight("AltarGlow", center + Vector3.up * 1.2f,
            new Color(0.8f, 0.2f, 0.07f), 0.85f, 5f, root);
    }

    // ═══════════════════ ОРОЧЬЯ КУЗНЯ ═══════════════════

    private static void BuildOrcForge(Transform root)
    {
        Vector3 forgePos = L0OrcArenaConfig.RightFrontCampIdentity + L0OrcArenaConfig.SideDir * 6f;
        GameObject forge = new GameObject("OrcForge");
        forge.transform.SetParent(root, true);
        forge.transform.position = forgePos;

        Color stone = new Color(0.3f, 0.28f, 0.25f);
        Color iron = new Color(0.35f, 0.33f, 0.3f);
        Color ember = new Color(0.8f, 0.25f, 0.05f);

        // Горн
        L0OrcArenaPrimitiveKit.CreatePrimitive(forge.transform, "FurnaceBase_NoCollider", PrimitiveType.Cube,
            new Vector3(0f, 0.5f, 0f), Quaternion.identity, new Vector3(1.5f, 1f, 1.2f), stone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(forge.transform, "FurnaceCoals_NoCollider", PrimitiveType.Cube,
            new Vector3(0f, 0.95f, 0f), Quaternion.identity, new Vector3(1.0f, 0.1f, 0.8f), ember, false, true);
        PlaceLight("ForgeGlow", forgePos + Vector3.up * 1.2f,
            new Color(1f, 0.4f, 0.08f), 1.5f, 8f, root);

        // Наковальня
        L0OrcArenaPrimitiveKit.CreatePrimitive(forge.transform, "Anvil_NoCollider", PrimitiveType.Cube,
            new Vector3(2f, 0.45f, 0f), Quaternion.identity, new Vector3(0.8f, 0.9f, 0.5f), iron, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(forge.transform, "AnvilTop_NoCollider", PrimitiveType.Cube,
            new Vector3(2f, 0.95f, 0f), Quaternion.identity, new Vector3(1.0f, 0.12f, 0.6f), iron, false);

        // Оружие у кузни (ассеты)
        SpawnWeaponOnGround(root, Sword, forgePos + new Vector3(1.5f, 0f, 1.5f), 88f, 45f, 1.4f);
        SpawnWeaponOnGround(root, Axe, forgePos + new Vector3(-1f, 0f, 1.8f), 85f, 160f, 1.3f);

        // Дым от горна
        GameObject smoke = LoadPrefab(Smoke1);
        if (smoke != null)
        {
            GameObject s = Object.Instantiate(smoke, forgePos + Vector3.up * 1.5f, Quaternion.identity, root);
            s.name = "ForgeSmokeVFX";
            s.transform.localScale = Vector3.one * 0.4f;
            FixMaterials(s);
        }

        // Искры от горна
        CreateForgeSparkParticles(forge.transform, new Vector3(0f, 1.1f, 0f));
    }

    // ═══════════════════ КЛЕТКИ ═══════════════════

    private static void BuildPrisonerCages(Transform root)
    {
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;

        // 3 клетки у разных лагерных точек
        CreateArenaCage(root, L0OrcArenaConfig.LeftFrontCampIdentity + side * -5f + gate * 3f);
        CreateArenaCage(root, L0OrcArenaConfig.LeftRearCampIdentity + side * -4f + gate * -2f);
        CreateArenaCage(root, L0OrcArenaConfig.RightRearCampIdentity + side * 5f + gate * 1f);
    }

    private static void CreateArenaCage(Transform root, Vector3 pos)
    {
        Level0OrcSiegeCampDressing.CreateCage(root, pos);
    }

    // ═══════════════════ ТОТЕМЫ У ПОРТАЛА ═══════════════════

    private static void BuildPortalTotems(Transform root)
    {
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        float radius = L0OrcArenaConfig.PortalShrineRadius + 5f;

        Color wood = new Color(0.18f, 0.1f, 0.05f);
        Color bone = new Color(0.7f, 0.6f, 0.5f);
        Color purple = new Color(0.4f, 0.1f, 0.6f);

        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            Vector3 pos = portal + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject totem = new GameObject("PortalTotem_" + i);
            totem.transform.SetParent(root, true);
            totem.transform.position = pos;
            totem.transform.LookAt(new Vector3(portal.x, pos.y, portal.z));

            // Столб
            L0OrcArenaPrimitiveKit.CreatePrimitive(totem.transform, "Pole_NoCollider", PrimitiveType.Cylinder,
                Vector3.up * 2.5f, Quaternion.identity, new Vector3(0.2f, 5f, 0.2f), wood, false);

            // Черепа
            L0OrcArenaPrimitiveKit.CreatePrimitive(totem.transform, "Skull1_NoCollider", PrimitiveType.Sphere,
                new Vector3(0.18f, 4.5f, 0f), Quaternion.identity, new Vector3(0.2f, 0.17f, 0.2f), bone, false);
            L0OrcArenaPrimitiveKit.CreatePrimitive(totem.transform, "Skull2_NoCollider", PrimitiveType.Sphere,
                new Vector3(-0.15f, 3.8f, 0.1f), Quaternion.identity, new Vector3(0.18f, 0.15f, 0.18f), bone, false);

            // Тряпки красные
            L0OrcArenaPrimitiveKit.CreatePrimitive(totem.transform, "RedRag_NoCollider", PrimitiveType.Cube,
                new Vector3(0.2f, 4.0f, 0f), Quaternion.Euler(0f, 0f, -25f),
                new Vector3(0.5f, 0.3f, 0.04f), L0OrcArenaMaterials.OrcRed, false);

            // Фиолетовое свечение (под цвет портала)
            PlaceLight("TotemPortalGlow_" + i, pos + Vector3.up * 3f,
                purple, 0.6f, 6f, root);
        }
    }

    // ═══════════════════ ПОРТАЛЬНАЯ АТМОСФЕРА ═══════════════════

    private static void BuildPortalAtmosphere(Transform root)
    {
        Vector3 portal = L0OrcArenaConfig.PortalShrine;

        // Фиолетовый свет над порталом
        PlaceLight("PortalPurpleGlow", portal + Vector3.up * 4f,
            new Color(0.5f, 0.1f, 0.8f), 2f, 18f, root);

        // Каменные руны на земле (фиолетовое emission)
        Color runeColor = new Color(0.4f, 0.1f, 0.7f);
        float runeRadius = L0OrcArenaConfig.PortalShrineRadius * 0.7f;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 pos = portal + new Vector3(Mathf.Cos(angle) * runeRadius, 0.06f, Mathf.Sin(angle) * runeRadius);
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, "PortalGroundRune_" + i + "_NoCollider", PrimitiveType.Cube,
                pos, Quaternion.Euler(0f, i * 45f, 0f),
                new Vector3(0.5f, 0.03f, 0.5f), runeColor, false, true);
        }

        // Партикль-кольцо вокруг портала (фиолетовые частицы)
        CreatePortalRingParticles(root, portal + Vector3.up * 1f);

        // Гравий/камешки у портала (ассет)
        GameObject pebbles = LoadPrefab(Pebbles09);
        if (pebbles != null)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                float r = L0OrcArenaConfig.PortalShrineRadius + 2f;
                Vector3 pos = portal + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
                GameObject p = Object.Instantiate(pebbles, pos, Quaternion.Euler(0, i * 60f, 0), root);
                p.name = "PortalPebbles_" + i;
                p.transform.localScale = Vector3.one * 0.8f;
                RemoveAllColliders(p);
                FixMaterials(p);
            }
        }
    }

    // ═══════════════════ КАМНИ ПО АРЕНЕ ═══════════════════

    private static void BuildArenaRocks(Transform root)
    {
        GameObject rock09 = LoadPrefab(RockMoss09);
        GameObject rockPile = LoadPrefab(RockPile10);

        if (rock09 == null && rockPile == null) return;

        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;

        Vector3[] rockPositions = {
            center + side * -30f + gate * 20f,
            center + side * 32f + gate * 18f,
            center + side * -35f + gate * -5f,
            center + side * 33f + gate * -8f,
            center + side * -28f + gate * -25f,
            center + side * 30f + gate * -22f,
            center + side * -25f + gate * 30f,
            center + side * 27f + gate * 28f,
            center + side * -38f + gate * 10f,
            center + side * 36f + gate * 5f,
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? rock09 : rockPile;
            if (prefab == null) prefab = rock09 ?? rockPile;
            float scale = 0.5f + (i % 4) * 0.3f;
            GameObject r = Object.Instantiate(prefab, rockPositions[i], Quaternion.Euler(0, i * 47f, 0), root);
            r.name = "ArenaRock_" + i;
            r.transform.localScale = Vector3.one * scale;
            RemoveAllColliders(r);
            FixMaterials(r);
        }
    }

    // ═══════════════════ ДЫМ ПО АРЕНЕ ═══════════════════

    private static void BuildArenaSmokeEffects(Transform root)
    {
        GameObject smoke2 = LoadPrefab(Smoke2);
        GameObject smoke4 = LoadPrefab(Smoke4);

        if (smoke2 == null && smoke4 == null) return;

        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 entrance = L0OrcArenaConfig.BaseEntrance;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 side = L0OrcArenaConfig.SideDir;

        // Дым у лагерных костров
        Vector3[] smokePositions = {
            L0OrcArenaConfig.LeftFrontCampIdentity + Vector3.up * 1.5f,
            L0OrcArenaConfig.RightFrontCampIdentity + Vector3.up * 1.5f,
            L0OrcArenaConfig.LeftRearCampIdentity + Vector3.up * 1.5f,
            L0OrcArenaConfig.RightRearCampIdentity + Vector3.up * 1.5f,
            entrance + side * -10f + Vector3.up * 2f,
            entrance + side * 10f + Vector3.up * 2f,
        };

        for (int i = 0; i < smokePositions.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? smoke2 : smoke4;
            if (prefab == null) prefab = smoke2 ?? smoke4;
            float scale = 0.5f + (i % 3) * 0.2f;
            GameObject s = Object.Instantiate(prefab, smokePositions[i], Quaternion.identity, root);
            s.name = "ArenaCampSmoke_" + i;
            s.transform.localScale = Vector3.one * scale;
            FixMaterials(s);
        }
    }

    // ═══════════════════ ШАГ 10A: УКРЫТИЯ В BATTLECENTER ═══════════════════

    private static void BuildBattleCenterObstacles(Transform root)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 b = L0OrcArenaConfig.BackDir;

        Color darkWood = new Color(0.20f, 0.12f, 0.05f);
        Color bone     = new Color(0.68f, 0.62f, 0.52f);
        Color stone    = new Color(0.28f, 0.26f, 0.23f);
        Color blood    = new Color(0.28f, 0.04f, 0.02f);

        // 4 засеки из кольев по диагоналям (r≈24), не перекрывают ось проходов
        float[] barrX  = { -22f,  22f, -22f,  22f };
        float[] barrZ  = {  18f,  18f, -10f, -10f };
        float[] barrRY = {  35f, -35f, 145f,-145f };
        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = c + s * barrX[i] + g * barrZ[i];
            CreateCrudeBarricade(root, pos, barrRY[i], darkWood, bone);
        }

        // 4 костра по периметру BattleCenter (r≈30, угол +22° — между засеками)
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 22f) * Mathf.Deg2Rad;
            Vector3 pos = c + new Vector3(Mathf.Cos(angle) * 30f, 0f, Mathf.Sin(angle) * 30f);

            // Кольцо булыжников
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, "BFire_Ring" + i + "_NC", PrimitiveType.Cylinder,
                pos + Vector3.up * 0.12f, Quaternion.identity, new Vector3(1.8f, 0.24f, 1.8f), stone, false);
            // Угли (emission)
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, "BFire_Ember" + i + "_NC", PrimitiveType.Cube,
                pos + Vector3.up * 0.26f, Quaternion.Euler(0, i * 44f, 0),
                new Vector3(0.95f, 0.1f, 0.95f), new Color(0.85f, 0.28f, 0.04f), false, true);

            GameObject fire = LoadPrefab(SyntyFire);
            if (fire != null)
            {
                var f = Object.Instantiate(fire, pos + Vector3.up * 0.45f, Quaternion.identity, root);
                f.name = "BFire_VFX_" + i;
                f.transform.localScale = Vector3.one * 1.8f;
                FixMaterials(f);
            }
            PlaceLight("BFire_Light_" + i, pos + Vector3.up * 1.1f,
                new Color(1f, 0.42f, 0.08f), 1.6f, 11f, root);
        }

        // 8 кровавых пятен — следы тяжёлых боёв
        for (int i = 0; i < 8; i++)
        {
            float angle = (i * 45f + 11f) * Mathf.Deg2Rad;
            float r     = 6f + (i % 3) * 7f;
            Vector3 pos = c + new Vector3(Mathf.Cos(angle) * r, 0.03f, Mathf.Sin(angle) * r);
            L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(root, "Blood_" + i + "_NC",
                pos, Quaternion.Euler(0, i * 53f, 0),
                new Vector3(1.8f + i % 3, 0.02f, 1.4f + i % 2), blood);
        }
    }

    private static void CreateCrudeBarricade(Transform root, Vector3 worldPos, float yRot, Color wood, Color bone)
    {
        var grp = new GameObject("CrudeBarricade");
        grp.transform.SetParent(root, true);
        grp.transform.position = worldPos;
        grp.transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        float[] leans = { 8f, -4f, 6f };
        float[] xOff  = { -1.15f, 0f, 1.15f };
        for (int i = 0; i < 3; i++)
        {
            L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "Stake" + i + "_NC", PrimitiveType.Cylinder,
                new Vector3(xOff[i], 1.15f, 0f), Quaternion.Euler(leans[i], 0f, 0f),
                new Vector3(0.22f, 2.3f, 0.22f), wood, false);
        }
        L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "Plank_Hi_NC", PrimitiveType.Cube,
            new Vector3(0f, 1.85f, 0f), Quaternion.identity, new Vector3(3.0f, 0.16f, 0.18f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "Plank_Lo_NC", PrimitiveType.Cube,
            new Vector3(0f, 0.85f, 0f), Quaternion.identity, new Vector3(2.7f, 0.14f, 0.16f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "SkullL_NC", PrimitiveType.Sphere,
            new Vector3(-1.15f, 2.55f, 0f), Quaternion.identity, new Vector3(0.30f, 0.26f, 0.30f), bone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "SkullR_NC", PrimitiveType.Sphere,
            new Vector3( 1.15f, 2.55f, 0f), Quaternion.identity, new Vector3(0.30f, 0.26f, 0.30f), bone, false);
        // Каменное основание — CreatePrimitive, не Decorative, чтобы localPosition работало от grp
        L0OrcArenaPrimitiveKit.CreatePrimitive(grp.transform, "BarrBase_NC", PrimitiveType.Cube,
            new Vector3(0f, 0.1f, 0f), Quaternion.identity,
            new Vector3(3.6f, 0.28f, 1.0f), new Color(0.24f, 0.21f, 0.17f), false);
    }

    // ═══════════════════ ШАГ 10B: НАЧАЛО КОРИДОРА К ПОРТАЛУ ═══════════════════

    private static void BuildCorridorApproach(Transform root)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 b = L0OrcArenaConfig.BackDir;

        Color darkWood = new Color(0.18f, 0.10f, 0.04f);
        Color bone     = new Color(0.70f, 0.64f, 0.54f);

        // Ворота черепов на входе в коридор
        Vector3 gatePos = c + b * 14f;
        CreateSkullGate(root, gatePos, s, darkWood, bone);

        // 4 пары факельных столбов вдоль первых 45м коридора
        for (int i = 0; i < 4; i++)
        {
            float dist  = Mathf.Lerp(10f, 42f, i / 3f);
            Vector3 mid   = c + b * dist;
            Vector3 left  = mid + s * -7f;
            Vector3 right = mid + s *  7f;

            CreateTorchPole(root, left,  "CTorch_L" + i, darkWood);
            CreateTorchPole(root, right, "CTorch_R" + i, darkWood);

            // Свет только на чётных — контраст тени
            if (i % 2 == 0)
            {
                PlaceLight("CLight_L" + i, left  + Vector3.up * 3.4f,
                    new Color(0.85f, 0.28f, 0.06f), 0.9f, 6f, root);
                PlaceLight("CLight_R" + i, right + Vector3.up * 3.4f,
                    new Color(0.85f, 0.28f, 0.06f), 0.9f, 6f, root);
            }
        }

        // 3 тотема с черепами, чередуя стороны
        float[] totemDists = { 18f, 28f, 38f };
        float[] totemSides = { -5f,  5f, -5f };
        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = c + b * totemDists[i] + s * totemSides[i];
            CreateSkullTotem(root, pos, darkWood, bone, "CTotem_" + i);
        }

        // Тёмная дорожка на земле — ориентир «куда идти»
        for (int i = 0; i < 4; i++)
        {
            float dist  = Mathf.Lerp(10f, 42f, i / 3f);
            Vector3 pos = c + b * dist + Vector3.up * 0.03f;
            L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(root, "CorrGround_" + i + "_NC",
                pos, L0OrcArenaConfig.FaceBack,
                new Vector3(8f, 0.025f, 8f), new Color(0.11f, 0.08f, 0.06f));
        }
    }

    private static void CreateSkullGate(Transform root, Vector3 pos, Vector3 sideDir, Color wood, Color bone)
    {
        float halfW = 5.5f;
        for (int side = -1; side <= 1; side += 2)
        {
            Vector3 p = pos + sideDir * (halfW * side);
            L0OrcArenaPrimitiveKit.CreatePrimitive(root,
                "GPost" + (side > 0 ? "R" : "L") + "_NC", PrimitiveType.Cylinder,
                p + Vector3.up * 1.9f, Quaternion.identity,
                new Vector3(0.28f, 3.8f, 0.28f), wood, false);
            for (int j = 0; j < 3; j++)
            {
                L0OrcArenaPrimitiveKit.CreatePrimitive(root,
                    "GSkull" + (side > 0 ? "R" : "L") + j + "_NC", PrimitiveType.Sphere,
                    p + Vector3.up * (2.9f + j * 0.42f), Quaternion.Euler(0, j * 60f, 0),
                    new Vector3(0.27f, 0.23f, 0.27f), bone, false);
            }
            PlaceLight("GGlow" + (side > 0 ? "R" : "L"),
                p + Vector3.up * 4.1f, new Color(0.8f, 0.06f, 0.02f), 0.65f, 5f, root);
        }
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "GBar_NC", PrimitiveType.Cube,
            pos + Vector3.up * 3.55f, Quaternion.identity,
            new Vector3(halfW * 2f + 0.5f, 0.16f, 0.18f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, "GCenterSkull_NC", PrimitiveType.Sphere,
            pos + Vector3.up * 4.05f, Quaternion.identity,
            new Vector3(0.42f, 0.37f, 0.42f), bone, false);
    }

    private static void CreateTorchPole(Transform root, Vector3 worldPos, string name, Color wood)
    {
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Pole_NC", PrimitiveType.Cylinder,
            worldPos + Vector3.up * 1.5f, Quaternion.identity,
            new Vector3(0.11f, 3.0f, 0.11f), wood, false);

        GameObject torchPrefab = LoadPrefab(DgTorch);
        if (torchPrefab != null)
        {
            var t = Object.Instantiate(torchPrefab, worldPos, Quaternion.identity, root);
            t.name = name + "_DgTorch";
            t.transform.localScale = Vector3.one * 2.2f;
            RemoveAllColliders(t);
            FixMaterials(t);
        }
        else
        {
            L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Head_NC", PrimitiveType.Sphere,
                worldPos + Vector3.up * 3.1f, Quaternion.identity,
                new Vector3(0.22f, 0.22f, 0.22f), new Color(0.85f, 0.32f, 0.05f), false, true);
        }
    }

    private static void CreateSkullTotem(Transform root, Vector3 worldPos, Color wood, Color bone, string name)
    {
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Pole_NC", PrimitiveType.Cylinder,
            worldPos + Vector3.up * 1.5f, Quaternion.identity, new Vector3(0.10f, 3.0f, 0.10f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Bar_NC", PrimitiveType.Cube,
            worldPos + Vector3.up * 2.65f, Quaternion.identity, new Vector3(1.2f, 0.09f, 0.09f), wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Skull0_NC", PrimitiveType.Sphere,
            worldPos + Vector3.up * 3.1f, Quaternion.identity, new Vector3(0.28f, 0.24f, 0.28f), bone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Skull1_NC", PrimitiveType.Sphere,
            worldPos + new Vector3(-0.48f, 2.68f, 0.05f), Quaternion.identity,
            new Vector3(0.20f, 0.17f, 0.20f), bone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Skull2_NC", PrimitiveType.Sphere,
            worldPos + new Vector3( 0.48f, 2.68f, 0.05f), Quaternion.identity,
            new Vector3(0.20f, 0.17f, 0.20f), bone, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root, name + "_Rag_NC", PrimitiveType.Cube,
            worldPos + Vector3.up * 2.45f, Quaternion.Euler(0f, 0f, -18f),
            new Vector3(0.58f, 0.22f, 0.04f), new Color(0.52f, 0.05f, 0.02f), false);
    }

    // ═══════════════════ ШАГ 8: ЦЕНТРАЛЬНАЯ ЖУТЬ ═══════════════════

    private static void BuildCentralDread(Transform root)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 b = L0OrcArenaConfig.BackDir;

        // --- Ритуальный котёл в центре (заменяет примитивный алтарь Dungeon-ассетом) ---
        L0Util.Place(DgCauldron, c + Vector3.up * 0.05f, Quaternion.identity, 3.5f, root, "RitualCauldron_Center");
        L0Util.Place(SyntyFire, c + Vector3.up * 1.8f, Quaternion.identity, 2.5f, root, "RitualCauldronFire");
        L0Util.PlaceSmoke(Smoke1, c + Vector3.up * 2.5f, 0.8f, root, "RitualCauldronSmoke");
        PlaceLight("RitualCauldronGlow", c + Vector3.up * 2.2f,
            new Color(1f, 0.45f, 0.12f), 1.1f, 6.5f, root);

        // --- 4 Dungeon-факела вокруг центрального котла (radius ~12) ---
        float torchR = 12f;
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            Vector3 tPos = c + new Vector3(Mathf.Cos(angle) * torchR, 0f, Mathf.Sin(angle) * torchR);
            Quaternion tRot = Quaternion.LookRotation(c - tPos, Vector3.up);
            L0Util.Place(DgTorch, tPos, tRot, 3.0f, root, "RitualTorch_" + i);
            PlaceLight("RitualTorchLight_" + i, tPos + Vector3.up * 3.5f,
                new Color(1f, 0.5f, 0.1f), 0.9f, 8f, root);
        }

        // --- 4 камня Dungeon по диагоналям центра (radius ~18, не мешают проходу) ---
        float stoneR = 18f;
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f) * Mathf.Deg2Rad;
            Vector3 sPos = c + new Vector3(Mathf.Cos(angle) * stoneR, 0f, Mathf.Sin(angle) * stoneR);
            L0Util.Place(DgStone, sPos, Quaternion.Euler(0, i * 73f, 0), 2.5f, root, "RitualStone_" + i);
        }

        // --- Тёмные пятна боя на земле (6 шт по TransitionAisle, radius 30-38) ---
        float patchR = 34f;
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 60f + 15f) * Mathf.Deg2Rad;
            Vector3 pPos = c + new Vector3(Mathf.Cos(angle) * patchR, 0.04f, Mathf.Sin(angle) * patchR);
            L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(root, "BattleScar_" + i + "_NoCollider",
                pPos, Quaternion.Euler(0, i * 60f, 0),
                new Vector3(5f + i % 3, 0.03f, 4f + i % 2),
                new Color(0.12f, 0.09f, 0.07f));
        }

        // --- Ловушки Dungeon (3 шт, radius 22-28, между центром и strongpoints) ---
        Vector3[] trapPositions = {
            c + s * -16f + g * 20f,
            c + s * 18f + b * 12f,
            c + s * -8f + b * 24f,
        };
        string[] trapPrefabs = { DgFloorFireTrap, DgFloorTrap, DgFloorFireTrap };
        for (int i = 0; i < trapPositions.Length; i++)
        {
            L0Util.Place(trapPrefabs[i], trapPositions[i], Quaternion.Euler(0, i * 120f, 0),
                3.0f, root, "ArenaTrap_" + i);
        }

        // --- Dungeon-пропсы у каждой пары strongpoints (кирпичи, обломки колонн, факелы) ---
        // Левая сторона: у Watch + Cage
        L0Util.Place(DgBrickPile, L0OrcArenaConfig.CentralLeftWatchPost + s * 6f + g * -3f,
            Quaternion.Euler(0, 35, 0), 2.8f, root, "StrongpointBricks_WatchL");
        L0Util.Place(DgBrokenColumn, L0OrcArenaConfig.CentralLeftCagePost + s * 5f + b * -4f,
            Quaternion.Euler(0, 120, 0), 2.5f, root, "StrongpointRuins_CageL");
        L0Util.Place(DgTorch, L0OrcArenaConfig.CentralLeftWatchPost + s * -4f,
            Quaternion.LookRotation(c - L0OrcArenaConfig.CentralLeftWatchPost), 2.5f, root, "StrongpointTorch_WatchL");

        // Правая сторона: у Watch + Spike
        L0Util.Place(DgBrickPile, L0OrcArenaConfig.CentralRightWatchPost + s * -6f + g * -3f,
            Quaternion.Euler(0, -35, 0), 2.8f, root, "StrongpointBricks_WatchR");
        L0Util.Place(DgBrokenColumn, L0OrcArenaConfig.CentralRightSpikePost + s * -5f + b * -4f,
            Quaternion.Euler(0, 210, 0), 2.5f, root, "StrongpointRuins_SpikeR");
        L0Util.Place(DgTorch, L0OrcArenaConfig.CentralRightWatchPost + s * 4f,
            Quaternion.LookRotation(c - L0OrcArenaConfig.CentralRightWatchPost), 2.5f, root, "StrongpointTorch_WatchR");

        // Задняя пара: Rear strongpoints
        L0Util.Place(DgStone, L0OrcArenaConfig.RearLeftAssaultPost + s * 4f + g * 5f,
            Quaternion.Euler(0, 55, 0), 2.0f, root, "StrongpointStone_RearL");
        L0Util.Place(DgStone, L0OrcArenaConfig.RearRightAssaultPost + s * -4f + g * 5f,
            Quaternion.Euler(0, -55, 0), 2.0f, root, "StrongpointStone_RearR");

        // Входная пара: Entrance strongpoints
        L0Util.Place(DgBarrel, L0OrcArenaConfig.EntranceLeftRampPost + s * 5f + g * -3f,
            Quaternion.Euler(0, 22, 0), 2.5f, root, "StrongpointBarrel_EntL");
        L0Util.Place(DgBarrel, L0OrcArenaConfig.EntranceRightRampPost + s * -5f + g * -3f,
            Quaternion.Euler(0, -22, 0), 2.5f, root, "StrongpointBarrel_EntR");

        // --- Дополнительное оружие у strongpoints (разбросанное после боёв) ---
        SpawnWeaponOnGround(root, Sword, L0OrcArenaConfig.CentralLeftWatchPost + s * 2f + g * 8f, 86f, 155f, 1.6f);
        SpawnWeaponOnGround(root, Axe, L0OrcArenaConfig.CentralRightSpikePost + s * -3f + g * 6f, 84f, 280f, 1.4f);
        SpawnWeaponStuck(root, Spear, L0OrcArenaConfig.RearLeftAssaultPost + s * -3f + g * 3f, 12f, 70f, 1.8f);
        SpawnWeaponStuck(root, Spear3, L0OrcArenaConfig.RearRightAssaultPost + s * 3f + g * 3f, 10f, -45f, 1.7f);
        SpawnShieldOnGround(root, Shield2, L0OrcArenaConfig.EntranceLeftRampPost + s * -3f + g * 5f, 200f, 1.5f);
        SpawnShieldOnGround(root, Shield3, L0OrcArenaConfig.EntranceRightRampPost + s * 3f + g * 5f, 60f, 1.4f);

        // --- Канделябры по краям ритуальной зоны ---
        L0Util.Place(DgCandelabrum, c + s * -22f + g * 8f, Quaternion.identity, 3.0f, root, "RitualCandelabrum_L");
        L0Util.Place(DgCandelabrum, c + s * 22f + g * -8f, Quaternion.identity, 3.0f, root, "RitualCandelabrum_R");
    }

    // ═══════════════════ ШАГ 9: ПУТЬ К ПОРТАЛУ + АТМОСФЕРА ═══════════════════

    private static void BuildPortalSacredPath(Transform root)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 b = L0OrcArenaConfig.BackDir;
        Quaternion faceBack = L0OrcArenaConfig.FaceBack;

        // --- Каменный путь от арены к порталу (серия Dungeon Stone_Pedestal) ---
        // Путь идёт от BaseCenter+BackDir*45 до PortalShrine+GateDir*15
        Vector3 pathStart = c + b * 45f;
        Vector3 pathEnd = portal + g * 15f;
        int pedestalCount = 8;
        for (int i = 0; i < pedestalCount; i++)
        {
            float t = (float)i / (pedestalCount - 1);
            Vector3 pos = Vector3.Lerp(pathStart, pathEnd, t);
            // Чередуем лево-право от оси
            float offset = (i % 2 == 0) ? -8f : 8f;
            Vector3 sidePos = pos + s * offset;
            L0Util.Place(DgStonePedestal, sidePos, Quaternion.Euler(0, i * 45f, 0),
                2.5f, root, "PortalPathPedestal_" + i);
        }

        // --- Dungeon-колонны (целые) по бокам пути (4 пары) ---
        for (int i = 0; i < 4; i++)
        {
            float t = 0.1f + i * 0.25f;
            Vector3 pos = Vector3.Lerp(pathStart, pathEnd, t);
            L0Util.Place(DgColumn, pos + s * -12f, faceBack, 3.5f, root, "PortalPathColumn_L" + i);
            L0Util.Place(DgColumn, pos + s * 12f, faceBack, 3.5f, root, "PortalPathColumn_R" + i);
        }

        // --- Факелы вдоль пути (8 пар — чередуя красный/фиолетовый свет) ---
        for (int i = 0; i < 8; i++)
        {
            float t = (float)i / 7f;
            Vector3 pos = Vector3.Lerp(pathStart, pathEnd, t);
            Vector3 leftPos = pos + s * -6f;
            Vector3 rightPos = pos + s * 6f;

            L0Util.Place(DgTorch, leftPos, Quaternion.LookRotation(s), 2.5f, root, "PortalPathTorch_L" + i);
            L0Util.Place(DgTorch, rightPos, Quaternion.LookRotation(-s), 2.5f, root, "PortalPathTorch_R" + i);

            // Свет — только на каждом втором факеле, чтобы не сливался в сплошную полосу.
            // Тёплый ember у входа, фиолет ближе к порталу (нарастание к финалу).
            if (i % 2 != 0) continue;
            Color lightColor = (t < 0.6f)
                ? new Color(0.75f, 0.22f, 0.06f)  // тёплый огонь
                : new Color(0.45f, 0.1f, 0.7f);    // фиолетовый (ближе к порталу)
            float intensity = 0.45f + (t * 0.4f);
            PlaceLight("PortalPathLight_L" + i, leftPos + Vector3.up * 3f,
                lightColor, intensity, 4.5f, root);
            PlaceLight("PortalPathLight_R" + i, rightPos + Vector3.up * 3f,
                lightColor, intensity, 4.5f, root);
        }

        // --- Тёмная дорожка на земле (от арены к порталу) ---
        int patchCount = 6;
        for (int i = 0; i < patchCount; i++)
        {
            float t = (float)i / (patchCount - 1);
            Vector3 pos = Vector3.Lerp(pathStart, pathEnd, t) + Vector3.up * 0.03f;
            L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(root, "PortalPathGround_" + i + "_NoCollider",
                pos, faceBack,
                new Vector3(10f, 0.025f, 8f),
                new Color(0.14f, 0.1f, 0.08f));
        }

        // --- Дополнительные обелиски по бокам портала (усиление стен 9) ---
        L0Util.Place(DgColumn, portal + s * -22f + b * 3f, faceBack, 4.0f, root, "PortalExtraColumn_L");
        L0Util.Place(DgColumn, portal + s * 22f + b * 3f, faceBack, 4.0f, root, "PortalExtraColumn_R");

        // --- Усиленное фиолетовое свечение портала ---
        PlaceLight("PortalDeepGlow_1", portal + Vector3.up * 6f,
            new Color(0.55f, 0.08f, 0.85f), 2.5f, 22f, root);
        PlaceLight("PortalDeepGlow_2", portal + s * -8f + Vector3.up * 3f,
            new Color(0.4f, 0.05f, 0.7f), 1.2f, 10f, root);
        PlaceLight("PortalDeepGlow_3", portal + s * 8f + Vector3.up * 3f,
            new Color(0.4f, 0.05f, 0.7f), 1.2f, 10f, root);

        // --- Дым/дымка у портала (усиленный) ---
        L0Util.PlaceSmoke(Smoke2, portal + Vector3.up * 1.5f + b * 3f, 1.0f, root, "PortalHaze_1");
        L0Util.PlaceSmoke(Smoke4, portal + s * -10f + Vector3.up * 1f, 0.7f, root, "PortalHaze_2");
        L0Util.PlaceSmoke(Smoke4, portal + s * 10f + Vector3.up * 1f, 0.7f, root, "PortalHaze_3");
        L0Util.PlaceSmoke(Smoke1, portal + Vector3.up * 4f, 0.5f, root, "PortalHaze_Top");

        // --- Визуальный барьер "портал закрыт": решётка из Jail-стен ---
        // 3 секции Jail_Wall перед порталом, scale ~3.5 (≈7м высота)
        float barrierZ = 10f;
        Vector3 barrierCenter = portal + g * barrierZ;
        L0Util.Place(DgJailWall, barrierCenter, faceBack, 3.5f, root, "PortalBarrier_Center");
        L0Util.Place(DgJailWall, barrierCenter + s * -7f, faceBack, 3.5f, root, "PortalBarrier_L");
        L0Util.Place(DgJailWall, barrierCenter + s * 7f, faceBack, 3.5f, root, "PortalBarrier_R");
        L0Util.Place(DgJailColumn, barrierCenter + s * -10.5f, faceBack, 3.5f, root, "PortalBarrierPost_L");
        L0Util.Place(DgJailColumn, barrierCenter + s * 10.5f, faceBack, 3.5f, root, "PortalBarrierPost_R");
        // Красное свечение на решётке (зловещий индикатор "закрыто")
        PlaceLight("PortalBarrierRedGlow", barrierCenter + Vector3.up * 3f,
            new Color(0.9f, 0.08f, 0.03f), 1.1f, 7f, root);

        // --- Знамёна Dungeon у входа на портальный путь ---
        L0Util.Place(DgBanner, pathStart + s * -10f + Vector3.up * 0.5f,
            Quaternion.LookRotation(b), 3.0f, root, "PortalPathBanner_L");
        L0Util.Place(DgBanner, pathStart + s * 10f + Vector3.up * 0.5f,
            Quaternion.LookRotation(b), 3.0f, root, "PortalPathBanner_R");

        // --- Декоративные щиты на колоннах ---
        L0Util.Place(DgDecorativeShield, portal + s * -16f + g * 7f + Vector3.up * 3f,
            Quaternion.LookRotation(s), 3.0f, root, "PortalShield_L");
        L0Util.Place(DgDecorativeShield, portal + s * 16f + g * 7f + Vector3.up * 3f,
            Quaternion.LookRotation(-s), 3.0f, root, "PortalShield_R");
    }

    // ═══════════════════ ПАРТИКЛЫ ═══════════════════

    private static void CreatePortalRingParticles(Transform root, Vector3 pos)
    {
        GameObject psObj = new GameObject("PortalRingParticles");
        psObj.transform.SetParent(root, true);
        psObj.transform.position = pos;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = 0.5f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.5f, 0.15f, 0.8f, 0.6f);
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = L0OrcArenaConfig.PortalShrineRadius * 0.6f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.5f, 0.15f, 0.8f), 0f),
                new GradientColorKey(new Color(0.3f, 0.05f, 0.5f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.02f, 0.2f), 1f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.6f, 0.2f),
                new GradientAlphaKey(0.6f, 0.7f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        colorOverLifetime.color = grad;

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = 0.5f;
        velocityOverLifetime.orbitalY = 0.3f;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
        renderer.material.color = new Color(0.5f, 0.15f, 0.8f, 0.6f);
    }

    private static void CreateForgeSparkParticles(Transform parent, Vector3 localPos)
    {
        GameObject psObj = new GameObject("ForgeSparks");
        psObj.transform.SetParent(parent, false);
        psObj.transform.localPosition = localPos;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 2.5f;
        main.startSize = 0.04f;
        main.startColor = new Color(1f, 0.6f, 0.1f, 0.9f);
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.2f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 1f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        colorOverLifetime.color = grad;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
        renderer.material.color = new Color(1f, 0.6f, 0.1f, 0.9f);
    }

    // ═══════════════════ УТИЛИТЫ ═══════════════════

    private static void SpawnWeaponOnGround(Transform root, string path, Vector3 pos, float tiltX, float yRot, float scale)
    {
        GameObject prefab = LoadPrefab(path);
        if (prefab == null) return;
        GameObject w = Object.Instantiate(prefab, pos + Vector3.up * 0.05f, Quaternion.Euler(tiltX, yRot, 0f), root);
        w.name = "GroundWeapon_" + w.GetInstanceID();
        w.transform.localScale = Vector3.one * scale;
        RemoveAllColliders(w);
        FixMaterials(w);
    }

    private static void SpawnWeaponStuck(Transform root, string path, Vector3 pos, float tiltX, float yRot, float scale)
    {
        GameObject prefab = LoadPrefab(path);
        if (prefab == null) return;
        GameObject w = Object.Instantiate(prefab, pos, Quaternion.Euler(tiltX, yRot, 0f), root);
        w.name = "StuckWeapon_" + w.GetInstanceID();
        w.transform.localScale = Vector3.one * scale;
        RemoveAllColliders(w);
        FixMaterials(w);
    }

    private static void SpawnShieldOnGround(Transform root, string path, Vector3 pos, float yRot, float scale)
    {
        GameObject prefab = LoadPrefab(path);
        if (prefab == null) return;
        GameObject w = Object.Instantiate(prefab, pos + Vector3.up * 0.1f, Quaternion.Euler(70f, yRot, 0f), root);
        w.name = "GroundShield_" + w.GetInstanceID();
        w.transform.localScale = Vector3.one * scale;
        RemoveAllColliders(w);
        FixMaterials(w);
    }

    private static void RemoveAllColliders(GameObject obj) => L0Util.RemoveAllColliders(obj);

    private static void FixMaterials(GameObject obj) => L0Util.FixMaterials(obj);

    private static GameObject LoadPrefab(string path) => L0Util.LoadPrefab(path);
}
