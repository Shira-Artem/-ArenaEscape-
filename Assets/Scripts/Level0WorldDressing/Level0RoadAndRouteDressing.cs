using UnityEngine;

/// <summary>
/// Дорога от замка к орочьей арене. Три атмосферных участка:
/// 1. Безопасная зона (от замка) — столбики, факелы, кусты.
/// 2. Зона конфликта (деревня) — баррикады, стрелы, следы огня.
/// 3. Орочья территория (к арене) — черепа, знамёна, обугленные деревья.
/// Использует ассеты Pure Poly Nature Pack.
/// </summary>
public static class Level0RoadAndRouteDressing
{
    private static readonly string TreePrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_02.prefab";
    private static readonly string BirchPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_06.prefab";
    private static readonly string RockPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_10.prefab";
    private static readonly string GrassPrefabPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_15.prefab";

    public static void Build(GameObject container, Vector3 gate, Vector3 playerStart, Vector3 roadEnd, Vector3 campCenter, Vector3 villageCenter)
    {
        Transform root = container.transform;

        BuildRoadSurface(root, playerStart, roadEnd);
        BuildSafeZone(root, gate, playerStart);
        BuildConflictZone(root, campCenter, villageCenter);
        BuildOrcTerritory(root, campCenter, roadEnd);
        BuildRoadsideVegetation(root, playerStart, roadEnd);
        BuildSigns(root, gate, campCenter, villageCenter, roadEnd);
    }

    // ═══════════════════ ДОРОГА ═══════════════════

    private static void BuildRoadSurface(Transform root, Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float totalLength = Vector3.Distance(start, end);
        Vector3 mid = (start + end) * 0.5f;

        Color roadColor = new Color(0.32f, 0.25f, 0.16f);
        Color edgeColor = new Color(0.26f, 0.22f, 0.15f);
        Color rut = new Color(0.22f, 0.16f, 0.10f);

        // Основное полотно
        GameObject road = WorldDressingPropFactory.CreateSimpleProp(root, "RoadMain", PrimitiveType.Cube,
            mid, new Vector3(3.5f, 0.05f, totalLength), roadColor, false);
        road.transform.rotation = Quaternion.LookRotation(dir);

        // Обочины
        GameObject edgeL = WorldDressingPropFactory.CreateSimpleProp(root, "RoadEdgeL", PrimitiveType.Cube,
            mid + Quaternion.LookRotation(dir) * new Vector3(-2.25f, 0.03f, 0f),
            new Vector3(1f, 0.04f, totalLength), edgeColor, false);
        edgeL.transform.rotation = Quaternion.LookRotation(dir);

        GameObject edgeR = WorldDressingPropFactory.CreateSimpleProp(root, "RoadEdgeR", PrimitiveType.Cube,
            mid + Quaternion.LookRotation(dir) * new Vector3(2.25f, 0.03f, 0f),
            new Vector3(1f, 0.04f, totalLength), edgeColor, false);
        edgeR.transform.rotation = Quaternion.LookRotation(dir);

        // Колея от колёс
        GameObject rutL = WorldDressingPropFactory.CreateSimpleProp(root, "RutLeft", PrimitiveType.Cube,
            mid + Quaternion.LookRotation(dir) * new Vector3(-0.5f, 0.055f, 0f),
            new Vector3(0.15f, 0.01f, totalLength * 0.85f), rut, false);
        rutL.transform.rotation = Quaternion.LookRotation(dir);

        GameObject rutR = WorldDressingPropFactory.CreateSimpleProp(root, "RutRight", PrimitiveType.Cube,
            mid + Quaternion.LookRotation(dir) * new Vector3(0.5f, 0.055f, 0f),
            new Vector3(0.15f, 0.01f, totalLength * 0.85f), rut, false);
        rutR.transform.rotation = Quaternion.LookRotation(dir);
    }

    // ═══════════════════ ЗОНА 1: БЕЗОПАСНАЯ (у замка) ═══════════════════

    private static void BuildSafeZone(Transform root, Vector3 gate, Vector3 playerStart)
    {
        // Каменные столбики
        Vector3[] pillars = {
            gate + new Vector3(-3f, 0f, -5f),
            gate + new Vector3(3f, 0f, -5f),
            gate + new Vector3(-3.5f, 0f, -12f),
            gate + new Vector3(3.5f, 0f, -12f),
        };
        for (int i = 0; i < pillars.Length; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(root, "StonePillar_" + i, PrimitiveType.Cylinder,
                pillars[i] + Vector3.up * 0.4f, new Vector3(0.2f, 0.8f, 0.2f),
                new Color(0.5f, 0.48f, 0.45f), false);
        }

        // Факелы безопасной зоны (чередуя стороны)
        for (int i = 0; i < 4; i++)
        {
            float t = (i + 1) / 5f;
            Vector3 pos = Vector3.Lerp(gate, gate + new Vector3(0f, 0f, -20f), t);
            float side = (i % 2 == 0) ? 2.5f : -2.5f;
            pos.x += side;
            WorldDressingPropFactory.CreateTorch(pos, root);
            L0Props.CreatePointLight("SafeTorch_" + i, pos + Vector3.up * 2f,
                new Color(1f, 0.7f, 0.3f), 0.7f, 6f, root);
        }

        // Кусты по бокам (примитивы или ассеты)
        Color bush = new Color(0.15f, 0.35f, 0.12f);
        Vector3[] bushPositions = {
            gate + new Vector3(-5f, 0.15f, -3f),
            gate + new Vector3(5.5f, 0.18f, -8f),
            gate + new Vector3(-4.5f, 0.2f, -14f),
        };
        for (int i = 0; i < bushPositions.Length; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(root, "SafeBush_" + i, PrimitiveType.Sphere,
                bushPositions[i], new Vector3(0.6f + i * 0.1f, 0.5f, 0.7f + i * 0.05f), bush, false);
        }
    }

    // ═══════════════════ ЗОНА 2: КОНФЛИКТ (деревня) ═══════════════════

    private static void BuildConflictZone(Transform root, Vector3 campCenter, Vector3 villageCenter)
    {
        Vector3 mid = (campCenter + villageCenter) * 0.5f;

        // Перевёрнутая телега — полублокирует дорогу
        WorldDressingPropFactory.CreateCart(mid + new Vector3(0.5f, 0f, 2f), Quaternion.Euler(0f, 65f, 22f), root);

        // Сломанная баррикада
        Color wood = new Color(0.25f, 0.15f, 0.08f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Barricade1", PrimitiveType.Cube,
            mid + new Vector3(-0.8f, 0.3f, -2f), new Vector3(2f, 0.6f, 0.3f), wood, false)
            .transform.rotation = Quaternion.Euler(0f, 15f, 8f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Barricade2", PrimitiveType.Cube,
            mid + new Vector3(0.5f, 0.15f, -1.5f), new Vector3(1.2f, 0.3f, 0.2f), wood, false)
            .transform.rotation = Quaternion.Euler(0f, -25f, 15f);

        // Щиты в земле
        Color shield = new Color(0.3f, 0.2f, 0.15f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Shield1", PrimitiveType.Cube,
            mid + new Vector3(2.5f, 0.25f, 1f), new Vector3(0.5f, 0.6f, 0.06f), shield, false)
            .transform.rotation = Quaternion.Euler(15f, 30f, 0f);
        WorldDressingPropFactory.CreateSimpleProp(root, "Shield2", PrimitiveType.Cube,
            mid + new Vector3(-2f, 0.2f, -0.5f), new Vector3(0.45f, 0.55f, 0.06f), shield, false)
            .transform.rotation = Quaternion.Euler(20f, -45f, 0f);

        // Стрелы в земле
        Vector3[] arrowPos = {
            mid + new Vector3(1f, 0f, 0.5f),
            mid + new Vector3(-1.5f, 0f, 1.5f),
            mid + new Vector3(0.3f, 0f, -3f),
            mid + new Vector3(2f, 0f, -1f),
            mid + new Vector3(-0.5f, 0f, 3f),
        };
        for (int i = 0; i < arrowPos.Length; i++)
        {
            float tilt = 55f + i * 5f;
            float yaw = i * 73f;
            WorldDressingPropFactory.CreateSimpleProp(root, "GroundArrow_" + i, PrimitiveType.Cylinder,
                arrowPos[i] + Vector3.up * 0.2f, new Vector3(0.02f, 0.35f, 0.02f),
                new Color(0.3f, 0.2f, 0.1f), false)
                .transform.rotation = Quaternion.Euler(tilt, yaw, 0f);
        }

        // Следы огня на земле
        Color scorched = new Color(0.05f, 0.04f, 0.03f);
        WorldDressingPropFactory.CreateSimpleProp(root, "ScorchMark1", PrimitiveType.Cube,
            mid + new Vector3(-1.5f, 0.01f, 4f), new Vector3(1.5f, 0.01f, 1.2f), scorched, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "ScorchMark2", PrimitiveType.Cube,
            mid + new Vector3(2f, 0.01f, -2.5f), new Vector3(1.2f, 0.01f, 0.9f), scorched, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "ScorchMark3", PrimitiveType.Cube,
            mid + new Vector3(0.5f, 0.01f, 1f), new Vector3(0.8f, 0.01f, 0.7f), scorched, false);

        // Тлеющие угли
        Color ember = new Color(0.6f, 0.15f, 0.02f);
        WorldDressingPropFactory.CreateSimpleProp(root, "EmberPatch", PrimitiveType.Cube,
            mid + new Vector3(-1.5f, 0.02f, 4f), new Vector3(0.6f, 0.02f, 0.5f), ember, false, ember, 1.5f);

        // Партикл тлеющих углей
        CreateEmberParticles(root, mid + new Vector3(-1.5f, 0.1f, 4f));

        // Следы боя (замена рандомного мусора)
        WorldDressingPropFactory.CreateBattleDebris(mid + new Vector3(3f, 0f, 3f), root);
        WorldDressingPropFactory.CreateBattleDebris(mid + new Vector3(-3f, 0f, -1f), root);
        WorldDressingPropFactory.CreateBattleDebris(mid + new Vector3(1f, 0f, -4f), root);
    }

    // ═══════════════════ ЗОНА 3: ОРОЧЬЯ ТЕРРИТОРИЯ ═══════════════════

    private static void BuildOrcTerritory(Transform root, Vector3 campCenter, Vector3 roadEnd)
    {
        // Черепа на кольях
        Vector3[] skullPositions = {
            campCenter + new Vector3(-3f, 0f, -10f),
            campCenter + new Vector3(3.5f, 0f, -18f),
            campCenter + new Vector3(-2.5f, 0f, -26f),
        };
        for (int i = 0; i < skullPositions.Length; i++)
        {
            float side = (i % 2 == 0) ? 1 : -1;
            Vector3 pos = skullPositions[i];
            pos.x = Mathf.Abs(pos.x) * side;

            // Кол
            WorldDressingPropFactory.CreateSimpleProp(root, "SkullStake_" + i, PrimitiveType.Cylinder,
                pos + Vector3.up * 1.0f, new Vector3(0.06f, 2.0f, 0.06f), new Color(0.22f, 0.14f, 0.07f), false);
            // Череп
            WorldDressingPropFactory.CreateSimpleProp(root, "SkullOnStake_" + i, PrimitiveType.Sphere,
                pos + Vector3.up * 2.1f, new Vector3(0.15f, 0.12f, 0.15f), new Color(0.7f, 0.6f, 0.5f), false);
        }

        // Красные знамёна орков
        WorldDressingPropFactory.CreateOrcBanner(campCenter + new Vector3(-4f, 0f, -15f), Quaternion.Euler(0f, -20f, 0f), root);
        WorldDressingPropFactory.CreateOrcBanner(campCenter + new Vector3(4.5f, 0f, -24f), Quaternion.Euler(0f, 25f, 0f), root);

        // Красноватый свет у знамён
        L0Props.CreatePointLight("BannerGlow1", campCenter + new Vector3(-4f, 1.5f, -15f),
            new Color(0.8f, 0.15f, 0.05f), 0.5f, 6f, root);
        L0Props.CreatePointLight("BannerGlow2", campCenter + new Vector3(4.5f, 1.5f, -24f),
            new Color(0.8f, 0.15f, 0.05f), 0.5f, 6f, root);

        // Обугленное дерево
        Vector3 burntTreePos = campCenter + new Vector3(6f, 0f, -20f);
        Color charred = new Color(0.06f, 0.04f, 0.03f);
        WorldDressingPropFactory.CreateSimpleProp(root, "BurntTrunk", PrimitiveType.Cylinder,
            burntTreePos + Vector3.up * 1.5f, new Vector3(0.2f, 3f, 0.2f), charred, false);
        WorldDressingPropFactory.CreateSimpleProp(root, "BurntCrown", PrimitiveType.Sphere,
            burntTreePos + Vector3.up * 3.2f, new Vector3(1.5f, 1.0f, 1.5f), charred, false);
        // Ветка сломанная
        GameObject branch = WorldDressingPropFactory.CreateSimpleProp(root, "BurntBranch", PrimitiveType.Cylinder,
            burntTreePos + new Vector3(0.5f, 2f, 0f), new Vector3(0.06f, 1f, 0.06f), charred, false);
        branch.transform.rotation = Quaternion.Euler(0f, 0f, 45f);

        // Кости на земле
        Color bone = new Color(0.75f, 0.7f, 0.6f);
        Vector3 boneCluster = campCenter + new Vector3(-1f, 0f, -22f);
        for (int i = 0; i < 5; i++)
        {
            float ox = (i - 2) * 0.25f;
            float oz = (i % 2) * 0.15f;
            WorldDressingPropFactory.CreateSimpleProp(root, "Bone_" + i, PrimitiveType.Cylinder,
                boneCluster + new Vector3(ox, 0.02f, oz), new Vector3(0.03f, 0.12f + i * 0.02f, 0.03f), bone, false)
                .transform.rotation = Quaternion.Euler(85f, i * 35f, 0f);
        }

        // Факелы орочьей зоны
        WorldDressingPropFactory.CreateTorch(campCenter + new Vector3(-3f, 0f, -14f), root);
        WorldDressingPropFactory.CreateTorch(campCenter + new Vector3(3f, 0f, -22f), root);
    }

    // ═══════════════════ РАСТИТЕЛЬНОСТЬ (АССЕТЫ) ═══════════════════

    private static void BuildRoadsideVegetation(Transform root, Vector3 start, Vector3 end)
    {
        GameObject treePrefab = LoadPrefab(TreePrefabPath);
        GameObject birchPrefab = LoadPrefab(BirchPrefabPath);
        GameObject rockPrefab = LoadPrefab(RockPrefabPath);
        GameObject grassPrefab = LoadPrefab(GrassPrefabPath);

        // Деревья вдоль дороги — группами по 2-3, расстояние 8-15 от дороги
        Vector3[] treePositions = {
            start + new Vector3(-10f, 0f, -3f),
            start + new Vector3(-11f, 0f, -6f),
            start + new Vector3(9f, 0f, -8f),
            start + new Vector3(12f, 0f, -5f),
            start + new Vector3(11f, 0f, -7f),
            start + new Vector3(-9f, 0f, -15f),
            start + new Vector3(10f, 0f, -20f),
            start + new Vector3(13f, 0f, -22f),
            start + new Vector3(-12f, 0f, -25f),
            start + new Vector3(-10f, 0f, -28f),
            start + new Vector3(9f, 0f, -35f),
            start + new Vector3(-11f, 0f, -40f),
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            GameObject prefab = (i % 3 == 0 && birchPrefab != null) ? birchPrefab : treePrefab;
            float scale = 0.6f + (i % 4) * 0.15f;

            if (prefab != null)
            {
                GameObject tree = Object.Instantiate(prefab, treePositions[i], Quaternion.Euler(0f, i * 53f, 0f), root);
                tree.name = "RoadTree_" + i;
                tree.transform.localScale = Vector3.one * scale;
                FixPrefabMaterials(tree);
            }
            else
            {
                CreateFallbackTree(root, treePositions[i], 2f + (i % 3) * 1.5f);
            }
        }

        // Валуны
        Vector3[] rockPositions = {
            start + new Vector3(7f, 0f, -4f),
            start + new Vector3(-8f, 0f, -18f),
            start + new Vector3(6f, 0f, -30f),
            start + new Vector3(-7f, 0f, -38f),
            start + new Vector3(9f, 0f, -12f),
            start + new Vector3(-10f, 0f, -33f),
        };

        for (int i = 0; i < rockPositions.Length; i++)
        {
            float scale = 0.4f + (i % 3) * 0.3f;
            if (rockPrefab != null)
            {
                GameObject rock = Object.Instantiate(rockPrefab, rockPositions[i], Quaternion.Euler(0f, i * 67f, 0f), root);
                rock.name = "RoadRock_" + i;
                rock.transform.localScale = Vector3.one * scale;
                FixPrefabMaterials(rock);
            }
            else
            {
                WorldDressingPropFactory.CreateSimpleProp(root, "RoadRock_" + i, PrimitiveType.Sphere,
                    rockPositions[i] + Vector3.up * 0.15f,
                    new Vector3(0.8f * scale, 0.5f * scale, 0.7f * scale),
                    new Color(0.45f, 0.42f, 0.38f), false);
            }
        }

        // Кусты между деревьями
        Color[] bushColors = {
            new Color(0.15f, 0.35f, 0.12f),
            new Color(0.12f, 0.30f, 0.10f),
            new Color(0.18f, 0.38f, 0.14f),
        };

        Vector3[] bushPositions = {
            start + new Vector3(-8f, 0.15f, -2f),
            start + new Vector3(7f, 0.12f, -10f),
            start + new Vector3(-9f, 0.18f, -17f),
            start + new Vector3(8f, 0.14f, -24f),
            start + new Vector3(-7f, 0.16f, -30f),
            start + new Vector3(9f, 0.13f, -37f),
            start + new Vector3(-8f, 0.15f, -42f),
            start + new Vector3(7f, 0.17f, -45f),
        };

        for (int i = 0; i < bushPositions.Length; i++)
        {
            float s = 0.3f + (i % 3) * 0.15f;
            WorldDressingPropFactory.CreateSimpleProp(root, "RoadBush_" + i, PrimitiveType.Sphere,
                bushPositions[i], new Vector3(s + 0.2f, s, s + 0.1f), bushColors[i % bushColors.Length], false);
        }

        // Трава (ассет)
        if (grassPrefab != null)
        {
            for (int i = 0; i < 10; i++)
            {
                float t = (i + 0.5f) / 10f;
                Vector3 pos = Vector3.Lerp(start, end, t);
                float side = (i % 2 == 0) ? 5f : -5f;
                pos.x += side + (i % 3) * 1.5f;
                GameObject grass = Object.Instantiate(grassPrefab, pos, Quaternion.Euler(0f, i * 36f, 0f), root);
                grass.name = "RoadGrass_" + i;
                grass.transform.localScale = Vector3.one * 0.5f;
                FixPrefabMaterials(grass);
            }
        }
    }

    // ═══════════════════ ТАБЛИЧКИ ═══════════════════

    private static void BuildSigns(Transform root, Vector3 gate, Vector3 campCenter, Vector3 villageCenter, Vector3 roadEnd)
    {
        CreateSign(root, "Gate", gate + new Vector3(-3f, 0f, -2f), Quaternion.Euler(0f, -30f, 0f));
        CreateSign(root, "Orc camp", campCenter + new Vector3(-4f, 0f, 3f), Quaternion.Euler(0f, 30f, 0f));
        CreateSign(root, "Village", villageCenter + new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        CreateSign(root, ">>>", roadEnd + new Vector3(0f, 0f, -3f), Quaternion.Euler(0f, 0f, 0f));
    }

    private static void CreateSign(Transform parent, string text, Vector3 position, Quaternion rotation)
    {
        GameObject sign = new GameObject("Sign_" + text);
        sign.transform.SetParent(parent, true);
        sign.transform.position = position;
        sign.transform.rotation = rotation;

        WorldDressingPropFactory.CreateSimpleProp(sign.transform, "Post", PrimitiveType.Cylinder,
            Vector3.up * 0.9f, new Vector3(0.08f, 1.8f, 0.08f), new Color(0.3f, 0.2f, 0.1f), false);
        WorldDressingPropFactory.CreateSimpleProp(sign.transform, "Board", PrimitiveType.Cube,
            Vector3.up * 1.5f, new Vector3(0.8f, 0.3f, 0.05f), new Color(0.5f, 0.3f, 0.1f), false);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(sign.transform, false);
        labelObj.transform.localPosition = Vector3.up * 1.5f;
        TextMesh tm = labelObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;

        Level0WorldBillboardLabel bLabel = labelObj.AddComponent<Level0WorldBillboardLabel>();
        bLabel.visibleDistance = 14f;
        bLabel.fadeByDistance = true;
    }

    // ═══════════════════ УТИЛИТЫ ═══════════════════

    private static void CreateEmberParticles(Transform root, Vector3 pos)
    {
        GameObject psObj = new GameObject("EmberParticles");
        psObj.transform.SetParent(root, true);
        psObj.transform.position = pos;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.8f;
        main.startSize = 0.04f;
        main.startColor = new Color(1f, 0.4f, 0.05f, 0.9f);
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 4f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.4f, 0.05f), 0f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        renderer.material = new Material(particleShader);
        renderer.material.color = new Color(1f, 0.4f, 0.05f, 0.9f);
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
