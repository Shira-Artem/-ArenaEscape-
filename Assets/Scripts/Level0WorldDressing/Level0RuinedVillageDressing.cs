using UnityEngine;

/// <summary>
/// Разграбленная деревня для Level0.
/// Авторская планировка: крестообразные улицы, центральная площадь, 8 домов трёх типов.
/// Использует ассеты из Pure Poly Nature Pack где возможно.
/// </summary>
public static class Level0RuinedVillageDressing
{
    private static readonly string TreePrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_10.prefab";
    private static readonly string BirchPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_05.prefab";
    private static readonly string RockPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    private static readonly string RockPilePrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_05.prefab";
    private static readonly string FencePrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Small_Fence_01.prefab";
    private static readonly string GrassPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_11.prefab";
    private static readonly string FlowerPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Daffodil_03.prefab";
    private static readonly string SmokePrefabPath = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab";

    public static void Build(GameObject container, Vector3 center, int houseCount)
    {
        Transform root = container.transform;

        BuildStreets(root, center);
        BuildSquare(root, center);
        BuildHouses(root, center);
        BuildFencesAndGardens(root, center);
        BuildRaidDebris(root, center);
        BuildVegetation(root, center);
        BuildAtmosphere(root, center);
    }

    // ═══════════════════ УЛИЦЫ ═══════════════════

    private static void BuildStreets(Transform root, Vector3 c)
    {
        Color roadColor = new Color(0.32f, 0.25f, 0.16f);
        Color roadEdge = new Color(0.28f, 0.22f, 0.14f);

        // Главная улица (север-юг)
        WorldDressingPropFactory.CreateSimpleProp(root, "MainStreet", PrimitiveType.Cube,
            c, new Vector3(2.8f, 0.04f, 28f), roadColor, false);

        // Поперечная улица (запад-восток)
        WorldDressingPropFactory.CreateSimpleProp(root, "CrossStreet", PrimitiveType.Cube,
            c, new Vector3(24f, 0.04f, 2.5f), roadColor, false);

        // Обочины главной улицы
        WorldDressingPropFactory.CreateSimpleProp(root, "MainStreetEdgeL", PrimitiveType.Cube,
            c + new Vector3(-1.8f, 0.02f, 0f), new Vector3(0.6f, 0.03f, 28f), roadEdge, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "MainStreetEdgeR", PrimitiveType.Cube,
            c + new Vector3(1.8f, 0.02f, 0f), new Vector3(0.6f, 0.03f, 28f), roadEdge, false);
    }

    // ═══════════════════ ЦЕНТРАЛЬНАЯ ПЛОЩАДЬ ═══════════════════

    private static void BuildSquare(Transform root, Vector3 c)
    {
        Color squareColor = new Color(0.4f, 0.35f, 0.28f);

        // Площадь
        WorldDressingPropFactory.CreateSimpleProp(root, "Square", PrimitiveType.Cube,
            c + new Vector3(0f, 0.025f, 0f), new Vector3(7f, 0.035f, 7f), squareColor, false);

        // Колодец в центре
        WorldDressingPropFactory.CreateWell(c + new Vector3(0.5f, 0f, 0.8f), root);

        // Разбитая телега (перевёрнута)
        GameObject cart = new GameObject("BrokenCart");
        cart.transform.SetParent(root, true);
        cart.transform.position = c + new Vector3(-2.2f, 0f, -1.5f);
        cart.transform.rotation = Quaternion.Euler(0f, 55f, 25f);
        WorldDressingPropFactory.CreateSimpleProp(cart.transform, "CartBody", PrimitiveType.Cube,
            new Vector3(0f, 0.35f, 0f), new Vector3(1.8f, 0.35f, 0.9f), new Color(0.3f, 0.2f, 0.1f), false);
        WorldDressingPropFactory.CreateSimpleProp(cart.transform, "CartWheel", PrimitiveType.Cylinder,
            new Vector3(0.9f, 0.15f, -0.5f), new Vector3(0.3f, 0.04f, 0.3f), new Color(0.25f, 0.15f, 0.08f), false);

        // Перевёрнутые бочки и мешки
        WorldDressingPropFactory.CreateBarrel(c + new Vector3(2.5f, 0f, -1.8f), root);
        GameObject tippedBarrel = WorldDressingPropFactory.CreateSimpleProp(root, "TippedBarrel", PrimitiveType.Cylinder,
            c + new Vector3(2.8f, 0.15f, -2.5f), new Vector3(0.35f, 0.5f, 0.35f), new Color(0.25f, 0.15f, 0.08f), false);
        tippedBarrel.transform.rotation = Quaternion.Euler(75f, 30f, 0f);

        WorldDressingPropFactory.CreateSimpleProp(root, "ScatteredSack1", PrimitiveType.Sphere,
            c + new Vector3(1.8f, 0.12f, -2.2f), new Vector3(0.4f, 0.3f, 0.35f), new Color(0.6f, 0.5f, 0.35f), false);
        WorldDressingPropFactory.CreateSimpleProp(root, "ScatteredSack2", PrimitiveType.Sphere,
            c + new Vector3(2.3f, 0.1f, -1.2f), new Vector3(0.35f, 0.25f, 0.3f), new Color(0.55f, 0.45f, 0.3f), false);

        // Повалённое пугало
        GameObject scarecrow = new GameObject("FallenScarecrow");
        scarecrow.transform.SetParent(root, true);
        scarecrow.transform.position = c + new Vector3(-1.5f, 0f, 2.2f);
        scarecrow.transform.rotation = Quaternion.Euler(0f, 20f, 70f);
        WorldDressingPropFactory.CreateSimpleProp(scarecrow.transform, "Pole", PrimitiveType.Cylinder,
            new Vector3(0f, 0.6f, 0f), new Vector3(0.06f, 0.8f, 0.06f), new Color(0.3f, 0.2f, 0.1f), false);
        WorldDressingPropFactory.CreateSimpleProp(scarecrow.transform, "Arms", PrimitiveType.Cube,
            new Vector3(0f, 1.0f, 0f), new Vector3(1.0f, 0.05f, 0.05f), new Color(0.3f, 0.2f, 0.1f), false);
        WorldDressingPropFactory.CreateSimpleProp(scarecrow.transform, "Head", PrimitiveType.Sphere,
            new Vector3(0f, 1.25f, 0f), new Vector3(0.2f, 0.22f, 0.2f), new Color(0.65f, 0.55f, 0.4f), false);
    }

    // ═══════════════════ 8 ДОМОВ (3 ТИПА) ═══════════════════

    private static void BuildHouses(Transform root, Vector3 c)
    {
        // Восточная сторона главной улицы (фасады на запад)
        CreateHouseIntact(root, c + new Vector3(5.5f, 0f, 8f), Quaternion.Euler(0, 270, 0), 1.0f);
        CreateHouseDamaged(root, c + new Vector3(6f, 0f, 2f), Quaternion.Euler(0, 260, 0), 0.9f);
        CreateHouseIntact(root, c + new Vector3(5.5f, 0f, -4f), Quaternion.Euler(0, 275, 0), 1.1f);

        // Западная сторона главной улицы (фасады на восток)
        CreateHouseDamaged(root, c + new Vector3(-6f, 0f, 7f), Quaternion.Euler(0, 90, 0), 0.95f);
        CreateHouseIntact(root, c + new Vector3(-5.5f, 0f, 1f), Quaternion.Euler(0, 85, 0), 1.0f);
        CreateHouseDamaged(root, c + new Vector3(-6.5f, 0f, -5f), Quaternion.Euler(0, 95, 0), 1.05f);

        // Сгоревшие руины на окраине
        CreateHouseBurned(root, c + new Vector3(9f, 0f, -9f), Quaternion.Euler(0, 200, 0), 0.9f);
        CreateHouseBurned(root, c + new Vector3(-9.5f, 0f, -8f), Quaternion.Euler(0, 150, 0), 1.0f);
    }

    private static void CreateHouseIntact(Transform root, Vector3 pos, Quaternion rot, float scale)
    {
        WorldDressingPropFactory.CreateSimpleHouse(pos, rot, scale, false, root);

        // Горшки у двери
        WorldDressingPropFactory.CreateSimpleProp(root, "Pot", PrimitiveType.Cylinder,
            pos + rot * new Vector3(0.5f, 0.1f, 1.1f * scale), new Vector3(0.15f, 0.12f, 0.15f), new Color(0.5f, 0.3f, 0.15f), false);
    }

    private static void CreateHouseDamaged(Transform root, Vector3 pos, Quaternion rot, float scale)
    {
        WorldDressingPropFactory.CreateSimpleHouse(pos, rot, scale, true, root);

        // Стрелы в стене
        float wallOffset = 1.0f * scale;
        WorldDressingPropFactory.CreateSimpleProp(root, "ArrowInWall", PrimitiveType.Cylinder,
            pos + rot * new Vector3(0.3f, 1.2f * scale, wallOffset + 0.1f),
            new Vector3(0.02f, 0.3f, 0.02f), new Color(0.3f, 0.2f, 0.1f), false)
            .transform.rotation = rot * Quaternion.Euler(60f, 10f, 0f);

        WorldDressingPropFactory.CreateSimpleProp(root, "ArrowInWall2", PrimitiveType.Cylinder,
            pos + rot * new Vector3(-0.4f, 0.9f * scale, wallOffset + 0.05f),
            new Vector3(0.02f, 0.25f, 0.02f), new Color(0.3f, 0.2f, 0.1f), false)
            .transform.rotation = rot * Quaternion.Euler(55f, -20f, 0f);

        // Сломанная дверь (лежит на земле)
        WorldDressingPropFactory.CreateSimpleProp(root, "BrokenDoor", PrimitiveType.Cube,
            pos + rot * new Vector3(0.3f, 0.03f, wallOffset + 0.4f),
            new Vector3(0.5f * scale, 0.04f, 0.9f * scale), new Color(0.22f, 0.13f, 0.06f), false)
            .transform.rotation = rot * Quaternion.Euler(0f, 25f, 0f);
    }

    private static void CreateHouseBurned(Transform root, Vector3 pos, Quaternion rot, float scale)
    {
        float w = 1.8f * scale;
        float d = 2.0f * scale;
        float halfH = 1.0f * scale;

        Color charred = new Color(0.08f, 0.06f, 0.04f);
        Color ember = new Color(0.4f, 0.12f, 0.02f);

        // Нижняя половина стен
        WorldDressingPropFactory.CreateSimpleProp(root, "BurnedWallsBase", PrimitiveType.Cube,
            pos + new Vector3(0f, halfH * 0.5f, 0f), new Vector3(w, halfH, d), charred, false)
            .transform.rotation = rot;

        // Обломки стен
        WorldDressingPropFactory.CreateSimpleProp(root, "BurnedWallFragment1", PrimitiveType.Cube,
            pos + rot * new Vector3(w * 0.3f, halfH + 0.2f, 0f),
            new Vector3(0.4f * scale, 0.5f * scale, d * 0.4f), charred, false)
            .transform.rotation = rot * Quaternion.Euler(0f, 0f, 8f);

        // Обугленная крыша (рухнувшая)
        WorldDressingPropFactory.CreateSimpleProp(root, "CollapsedRoof", PrimitiveType.Cube,
            pos + rot * new Vector3(-0.3f, 0.15f, 0.2f),
            new Vector3(w * 0.8f, 0.12f, d * 0.6f), new Color(0.06f, 0.04f, 0.03f), false)
            .transform.rotation = rot * Quaternion.Euler(12f, 5f, -8f);

        // Тлеющие угли (emission)
        WorldDressingPropFactory.CreateSimpleProp(root, "Embers", PrimitiveType.Cube,
            pos + new Vector3(0f, 0.08f, 0f), new Vector3(w * 0.6f, 0.06f, d * 0.5f), ember, false, ember, 1.5f)
            .transform.rotation = rot;

        // Тёплый свет углей
        L0Props.CreatePointLight("BurnedHouseGlow", pos + Vector3.up * 0.5f,
            new Color(1f, 0.3f, 0.05f), 0.6f, 5f, root);

        // Дым (партикл из ассета или fallback)
        TryInstantiateSmoke(root, pos + Vector3.up * 1.2f, 0.5f);
    }

    // ═══════════════════ ЗАБОРЫ И ОГОРОДЫ ═══════════════════

    private static void BuildFencesAndGardens(Transform root, Vector3 c)
    {
        // Заборы по участкам восточных домов
        CreateFenceL(root, c + new Vector3(3.5f, 0f, 10.5f), c + new Vector3(3.5f, 0f, 5f));
        CreateFenceL(root, c + new Vector3(3.5f, 0f, 5f), c + new Vector3(3.5f, 0f, -1f));
        CreateFenceL(root, c + new Vector3(3.5f, 0f, -1f), c + new Vector3(3.5f, 0f, -6.5f));

        // Заборы по участкам западных домов
        CreateFenceL(root, c + new Vector3(-3.5f, 0f, 9.5f), c + new Vector3(-3.5f, 0f, 4f));
        CreateFenceL(root, c + new Vector3(-3.5f, 0f, 4f), c + new Vector3(-3.5f, 0f, -2f));

        // Огороды за восточными домами
        CreateGardenPatch(root, c + new Vector3(9f, 0f, 6.5f));
        CreateGardenPatch(root, c + new Vector3(9.5f, 0f, 0.5f));

        // Огород за западным домом
        CreateGardenPatch(root, c + new Vector3(-9.5f, 0f, 5f));
    }

    private static void CreateFenceL(Transform root, Vector3 start, Vector3 end)
    {
        GameObject prefab = LoadPrefab(FencePrefabPath);
        if (prefab != null)
        {
            float dist = Vector3.Distance(start, end);
            Vector3 dir = (end - start).normalized;
            int count = Mathf.Max(1, Mathf.RoundToInt(dist / 2f));
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                Vector3 pos = Vector3.Lerp(start, end, t);
                GameObject fence = Object.Instantiate(prefab, pos, Quaternion.LookRotation(dir), root);
                fence.name = "Fence_Asset_" + i;
                fence.transform.localScale = Vector3.one * 0.8f;
                FixPrefabMaterials(fence);
            }
        }
        else
        {
            WorldDressingPropFactory.CreateFenceSegment(start, end, root);
        }
    }

    private static void CreateGardenPatch(Transform root, Vector3 pos)
    {
        Color soil = new Color(0.22f, 0.15f, 0.08f);
        Color green = new Color(0.2f, 0.45f, 0.15f);

        WorldDressingPropFactory.CreateSimpleProp(root, "GardenSoil", PrimitiveType.Cube,
            pos, new Vector3(2.5f, 0.04f, 1.8f), soil, false);

        for (int i = 0; i < 4; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(root, "CropRow_" + i, PrimitiveType.Cube,
                pos + new Vector3(0f, 0.08f, -0.6f + i * 0.4f), new Vector3(2.0f, 0.08f, 0.15f), green, false);
        }
    }

    // ═══════════════════ СЛЕДЫ РАЗГРАБЛЕНИЯ ═══════════════════

    private static void BuildRaidDebris(Transform root, Vector3 c)
    {
        Color blood = new Color(0.35f, 0.04f, 0.02f);

        // Кровавые следы
        WorldDressingPropFactory.CreateSimpleProp(root, "BloodStain1", PrimitiveType.Cube,
            c + new Vector3(1.2f, 0.015f, 3.5f), new Vector3(0.8f, 0.01f, 0.5f), blood, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "BloodStain2", PrimitiveType.Cube,
            c + new Vector3(-3f, 0.015f, -3f), new Vector3(0.6f, 0.01f, 0.7f), blood, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "BloodStain3", PrimitiveType.Cube,
            c + new Vector3(4.5f, 0.015f, -1.5f), new Vector3(0.5f, 0.01f, 0.4f), blood, false);

        // Сломанная повозка у въезда
        GameObject wreck = new GameObject("WreckedWagon");
        wreck.transform.SetParent(root, true);
        wreck.transform.position = c + new Vector3(0f, 0f, 13f);
        wreck.transform.rotation = Quaternion.Euler(0f, 70f, 18f);
        WorldDressingPropFactory.CreateSimpleProp(wreck.transform, "WagonBody", PrimitiveType.Cube,
            new Vector3(0f, 0.25f, 0f), new Vector3(2.2f, 0.3f, 1.0f), new Color(0.28f, 0.18f, 0.08f), false);
        WorldDressingPropFactory.CreateSimpleProp(wreck.transform, "WagonWheel1", PrimitiveType.Cylinder,
            new Vector3(-1.1f, 0.2f, -0.5f), new Vector3(0.35f, 0.04f, 0.35f), new Color(0.2f, 0.12f, 0.06f), false);

        // Раскиданные инструменты
        WorldDressingPropFactory.CreateSimpleProp(root, "DroppedTool1", PrimitiveType.Cylinder,
            c + new Vector3(-1.5f, 0.04f, 4.5f), new Vector3(0.03f, 0.4f, 0.03f), new Color(0.3f, 0.2f, 0.1f), false)
            .transform.rotation = Quaternion.Euler(85f, 40f, 0f);

        WorldDressingPropFactory.CreateSimpleProp(root, "DroppedPot", PrimitiveType.Sphere,
            c + new Vector3(3.2f, 0.1f, 2.8f), new Vector3(0.2f, 0.15f, 0.2f), new Color(0.45f, 0.3f, 0.15f), false);

        // Разорванная ткань на земле
        WorldDressingPropFactory.CreateSimpleProp(root, "TornCloth1", PrimitiveType.Cube,
            c + new Vector3(-2.5f, 0.02f, 5f), new Vector3(0.8f, 0.01f, 0.5f), new Color(0.6f, 0.55f, 0.4f), false)
            .transform.rotation = Quaternion.Euler(0f, 35f, 0f);

        // Стога сена (фиксированные позиции)
        WorldDressingPropFactory.CreateHaystack(c + new Vector3(7.5f, 0f, -2f), root);
        WorldDressingPropFactory.CreateHaystack(c + new Vector3(-8f, 0f, 3f), root);
    }

    // ═══════════════════ РАСТИТЕЛЬНОСТЬ (АССЕТЫ) ═══════════════════

    private static void BuildVegetation(Transform root, Vector3 c)
    {
        // Деревья вокруг деревни (ассеты)
        Vector3[] treePositions = new Vector3[]
        {
            c + new Vector3(-13f, 0f, 5f),
            c + new Vector3(-12f, 0f, -4f),
            c + new Vector3(12f, 0f, 7f),
            c + new Vector3(13f, 0f, -3f),
            c + new Vector3(11f, 0f, 12f),
            c + new Vector3(-11f, 0f, 11f),
            c + new Vector3(0f, 0f, -13f),
            c + new Vector3(-14f, 0f, -9f),
        };

        GameObject treePrefab = LoadPrefab(TreePrefabPath);
        GameObject birchPrefab = LoadPrefab(BirchPrefabPath);

        for (int i = 0; i < treePositions.Length; i++)
        {
            GameObject prefab = (i % 3 == 0 && birchPrefab != null) ? birchPrefab : treePrefab;
            if (prefab != null)
            {
                float s = 0.7f + (i % 3) * 0.15f;
                GameObject tree = Object.Instantiate(prefab, treePositions[i], Quaternion.Euler(0f, i * 47f, 0f), root);
                tree.name = "VillageTree_" + i;
                tree.transform.localScale = Vector3.one * s;
                FixPrefabMaterials(tree);
            }
            else
            {
                CreateFallbackTree(root, treePositions[i], 2.5f + (i % 3) * 1f);
            }
        }

        // Камни
        GameObject rockPrefab = LoadPrefab(RockPrefabPath);
        Vector3[] rockPositions = new Vector3[]
        {
            c + new Vector3(10f, 0f, 10f),
            c + new Vector3(-10f, 0f, -6f),
            c + new Vector3(3f, 0f, -11f),
            c + new Vector3(-13f, 0f, 8f),
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            if (rockPrefab != null)
            {
                GameObject rock = Object.Instantiate(rockPrefab, rockPositions[i], Quaternion.Euler(0f, i * 73f, 0f), root);
                rock.name = "VillageRock_" + i;
                rock.transform.localScale = Vector3.one * (0.5f + i * 0.2f);
                FixPrefabMaterials(rock);
            }
            else
            {
                WorldDressingPropFactory.CreateSimpleProp(root, "Rock_" + i, PrimitiveType.Sphere,
                    rockPositions[i] + Vector3.up * 0.2f,
                    new Vector3(0.8f + i * 0.3f, 0.5f + i * 0.15f, 0.7f + i * 0.2f),
                    new Color(0.45f, 0.42f, 0.38f), false);
            }
        }

        // Трава (ассет)
        GameObject grassPrefab = LoadPrefab(GrassPrefabPath);
        if (grassPrefab != null)
        {
            Vector3[] grassSpots = new Vector3[]
            {
                c + new Vector3(4f, 0f, 9f), c + new Vector3(-4f, 0f, 8f),
                c + new Vector3(8f, 0f, 4f), c + new Vector3(-7f, 0f, -1f),
                c + new Vector3(3f, 0f, -8f), c + new Vector3(-5f, 0f, -10f),
            };
            for (int i = 0; i < grassSpots.Length; i++)
            {
                GameObject grass = Object.Instantiate(grassPrefab, grassSpots[i], Quaternion.Euler(0f, i * 60f, 0f), root);
                grass.name = "VillageGrass_" + i;
                grass.transform.localScale = Vector3.one * 0.6f;
                FixPrefabMaterials(grass);
            }
        }
    }

    // ═══════════════════ АТМОСФЕРА ═══════════════════

    private static void BuildAtmosphere(Transform root, Vector3 c)
    {
        // Фонарные столбы вдоль главной улицы
        WorldDressingPropFactory.CreateTorch(c + new Vector3(2.2f, 0f, 6f), root);
        WorldDressingPropFactory.CreateTorch(c + new Vector3(-2.2f, 0f, -3f), root);
        WorldDressingPropFactory.CreateTorch(c + new Vector3(2.2f, 0f, -8f), root);

        // Костёр на площади (с Point Light)
        WorldDressingPropFactory.CreateCampfire(c + new Vector3(-0.5f, 0f, -1.5f), root);
        L0Props.CreatePointLight("VillageFire", c + new Vector3(-0.5f, 0.5f, -1.5f),
            new Color(1f, 0.45f, 0.1f), 1.5f, 8f, root);

        // Партиклы дыма от сгоревших домов
        TryInstantiateSmoke(root, c + new Vector3(9f, 2f, -9f), 0.8f);
        TryInstantiateSmoke(root, c + new Vector3(-9.5f, 2f, -8f), 0.7f);
    }

    // ═══════════════════ УТИЛИТЫ ═══════════════════

    private static void TryInstantiateSmoke(Transform parent, Vector3 position, float scale)
    {
        GameObject prefab = LoadPrefab(SmokePrefabPath);
        if (prefab != null)
        {
            GameObject smoke = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            smoke.name = "Smoke_VFX";
            smoke.transform.localScale = Vector3.one * scale;
            FixPrefabMaterials(smoke);
        }
        else
        {
            CreateFallbackSmoke(parent, position);
        }
    }

    private static void CreateFallbackSmoke(Transform parent, Vector3 position)
    {
        GameObject smokeObj = new GameObject("FallbackSmoke");
        smokeObj.transform.SetParent(parent, true);
        smokeObj.transform.position = position;

        ParticleSystem ps = smokeObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.5f;
        main.startSize = 0.6f;
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 3f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0f), new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = 0.8f;

        var renderer = smokeObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
        renderer.material.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
    }

    private static void CreateFallbackTree(Transform parent, Vector3 pos, float height)
    {
        Color trunk = new Color(0.3f, 0.2f, 0.1f);
        Color crown = new Color(0.18f, 0.4f, 0.15f);

        WorldDressingPropFactory.CreateSimpleProp(parent, "TreeTrunk", PrimitiveType.Cylinder,
            pos + Vector3.up * (height * 0.4f), new Vector3(0.15f, height * 0.4f, 0.15f), trunk, false);
        WorldDressingPropFactory.CreateSimpleProp(parent, "TreeCrown", PrimitiveType.Sphere,
            pos + Vector3.up * (height * 0.75f), new Vector3(height * 0.6f, height * 0.5f, height * 0.6f), crown, false);
    }

    private static void FixPrefabMaterials(GameObject obj)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Shader standard = Shader.Find("Standard");
        Shader fallback = urpLit != null ? urpLit : standard;
        if (fallback == null) fallback = Shader.Find("Diffuse");
        if (fallback == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                bool isBroken = mats[m] == null || mats[m].shader == null
                    || mats[m].shader.name.Contains("Error")
                    || mats[m].shader.name.Contains("Hidden/InternalErrorShader")
                    || mats[m].shader.name == "Hidden/InternalErrorShader";

                if (!isBroken) continue;

                Color origColor = new Color(0.5f, 0.5f, 0.5f);
                Texture origTex = null;

                if (mats[m] != null)
                {
                    if (mats[m].HasProperty("_BaseColor"))
                        origColor = mats[m].GetColor("_BaseColor");
                    else if (mats[m].HasProperty("_Color"))
                        origColor = mats[m].color;

                    if (mats[m].HasProperty("_BaseMap"))
                        origTex = mats[m].GetTexture("_BaseMap");
                    else if (mats[m].HasProperty("_MainTex"))
                        origTex = mats[m].GetTexture("_MainTex");
                }

                mats[m] = new Material(fallback);
                if (mats[m].HasProperty("_BaseColor"))
                    mats[m].SetColor("_BaseColor", origColor);
                if (mats[m].HasProperty("_Color"))
                    mats[m].color = origColor;
                if (origTex != null)
                {
                    if (mats[m].HasProperty("_BaseMap"))
                        mats[m].SetTexture("_BaseMap", origTex);
                    if (mats[m].HasProperty("_MainTex"))
                        mats[m].mainTexture = origTex;
                }
                changed = true;
            }
            if (changed)
                renderers[i].materials = mats;
        }
    }

    private static GameObject LoadPrefab(string path) => L0Util.LoadPrefab(path);
}
