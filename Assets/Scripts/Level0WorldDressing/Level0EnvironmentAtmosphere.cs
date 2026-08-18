using UnityEngine;

/// <summary>
/// Фаза 5: Окружение и атмосфера всего Level0.
/// Лес по краям, горы-задник, луга, цветы, грибы, валуны, холмы, освещение, туман, скайбокс.
/// Максимально использует ассеты Pure Poly Nature Pack.
/// </summary>
public static class Level0EnvironmentAtmosphere
{
    // ───── ПРИРОДА (Pure Poly) ─────
    private static readonly string Tree02 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_02.prefab";
    private static readonly string Tree10 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_10.prefab";
    private static readonly string Birch05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_05.prefab";
    private static readonly string Birch06 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_06.prefab";
    private static readonly string RockMoss09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    private static readonly string RockMoss11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_11.prefab";
    private static readonly string RockPile05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_05.prefab";
    private static readonly string RockPile10 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_10.prefab";
    private static readonly string Mountain01 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Forest_Mountain_Moss_01.prefab";
    private static readonly string Mountain02 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Forest_Mountain_Moss_02.prefab";
    private static readonly string Grass11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_11.prefab";
    private static readonly string Grass15 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_15.prefab";
    private static readonly string Meadow07 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_07.prefab";
    private static readonly string Meadow08 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_08.prefab";
    private static readonly string MeadowPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_Path_05.prefab";
    private static readonly string Daffodil = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Daffodil_03.prefab";
    private static readonly string Hyacinth = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Hyacinth_04.prefab";
    private static readonly string Sunflower = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Sunflower_04.prefab";
    private static readonly string MushroomOrange = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Mushroom_Fantasy_Orange_09.prefab";
    private static readonly string MushroomPurple = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Mushroom_Fantasy_Purple_05.prefab";
    private static readonly string FloorTile = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Floor_Tile_06.prefab";
    private static readonly string Pebbles03 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Cemetery_Pebbles_03.prefab";

    // ───── ДЫМКА ─────
    private static readonly string Smoke3 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 3.prefab";
    private static readonly string Smoke4 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 4.prefab";

    public static void Build(GameObject container, Vector3 gate, Vector3 roadEnd, Vector3 village, Vector3 camp)
    {
        Transform root = container.transform;

        BuildDistantMountains(root, gate, roadEnd);
        BuildWestForest(root, gate, roadEnd);
        BuildEastForest(root, gate, roadEnd);
        BuildMeadowsAndGrass(root, gate, roadEnd, village);
        BuildBoulders(root, gate, roadEnd);
        BuildFlowersAndMushrooms(root, village);
        BuildHills(root, gate, roadEnd);
        BuildFogSmoke(root, gate, roadEnd);
        SetupLighting();
    }

    // ═══════════════════ ГОРЫ-ЗАДНИК ═══════════════════

    private static void BuildDistantMountains(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        GameObject m1 = LoadPrefab(Mountain01);
        GameObject m2 = LoadPrefab(Mountain02);

        if (m1 == null && m2 == null) return;

        // Горы далеко по западу (X = -150..-200, маленький scale)
        Vector3[] westMountains = {
            new Vector3(-160f, -5f, gate.z + 20f),
            new Vector3(-180f, -8f, gate.z - 30f),
            new Vector3(-150f, -3f, roadEnd.z + 10f),
        };
        for (int i = 0; i < westMountains.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? m1 : m2;
            if (prefab == null) prefab = m1 ?? m2;
            float scale = 0.4f + (i % 3) * 0.15f;
            GameObject mt = Object.Instantiate(prefab, westMountains[i], Quaternion.Euler(0, i * 40f, 0), root);
            mt.name = "Mountain_W_" + i;
            mt.transform.localScale = Vector3.one * scale;
            FixMaterials(mt);
        }

        // Горы далеко по востоку
        Vector3[] eastMountains = {
            new Vector3(160f, -5f, gate.z + 10f),
            new Vector3(175f, -6f, gate.z - 35f),
            new Vector3(155f, -4f, roadEnd.z + 5f),
        };
        for (int i = 0; i < eastMountains.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? m2 : m1;
            if (prefab == null) prefab = m1 ?? m2;
            float scale = 0.35f + (i % 3) * 0.12f;
            GameObject mt = Object.Instantiate(prefab, eastMountains[i], Quaternion.Euler(0, 180 + i * 35f, 0), root);
            mt.name = "Mountain_E_" + i;
            mt.transform.localScale = Vector3.one * scale;
            FixMaterials(mt);
        }

        // Горы сзади (за замком, очень далеко)
        Vector3[] backMountains = {
            new Vector3(-60f, -8f, gate.z + 150f),
            new Vector3(0f, -5f, gate.z + 180f),
            new Vector3(70f, -8f, gate.z + 140f),
        };
        for (int i = 0; i < backMountains.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? m1 : m2;
            if (prefab == null) prefab = m1 ?? m2;
            float scale = 0.5f + i * 0.15f;
            GameObject mt = Object.Instantiate(prefab, backMountains[i], Quaternion.Euler(0, i * 60f, 0), root);
            mt.name = "Mountain_B_" + i;
            mt.transform.localScale = Vector3.one * scale;
            FixMaterials(mt);
        }
    }

    // ═══════════════════ ЗАПАДНЫЙ ЛЕС ═══════════════════

    private static void BuildWestForest(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        GameObject[] treePrefabs = {
            LoadPrefab(Tree02), LoadPrefab(Tree10),
            LoadPrefab(Birch05), LoadPrefab(Birch06)
        };

        float startZ = gate.z + 5f;
        float endZ = roadEnd.z - 10f;
        float totalZ = startZ - endZ;

        // 25 деревьев вдоль западной стены (X = -25..-45)
        for (int i = 0; i < 25; i++)
        {
            float t = i / 24f;
            float z = Mathf.Lerp(startZ, endZ, t);
            float x = -25f - (i % 5) * 4f - (i % 3) * 2f;
            float scale = 0.6f + (i % 4) * 0.15f;
            float yRot = i * 37f;

            GameObject prefab = PickTree(treePrefabs, i);
            if (prefab != null)
                InstantiateAndFix(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, yRot, 0), root, "WestForest_" + i, scale);
        }
    }

    // ═══════════════════ ВОСТОЧНЫЙ ЛЕС ═══════════════════

    private static void BuildEastForest(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        GameObject[] treePrefabs = {
            LoadPrefab(Tree02), LoadPrefab(Tree10),
            LoadPrefab(Birch05), LoadPrefab(Birch06)
        };

        float startZ = gate.z + 5f;
        float endZ = roadEnd.z - 10f;

        // 25 деревьев вдоль восточной стены (X = 25..45)
        for (int i = 0; i < 25; i++)
        {
            float t = i / 24f;
            float z = Mathf.Lerp(startZ, endZ, t);
            float x = 25f + (i % 5) * 4f + (i % 3) * 2f;
            float scale = 0.55f + (i % 4) * 0.15f;
            float yRot = i * 43f;

            GameObject prefab = PickTree(treePrefabs, i);
            if (prefab != null)
                InstantiateAndFix(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, yRot, 0), root, "EastForest_" + i, scale);
        }
    }

    // ═══════════════════ ЛУГА И ТРАВА ═══════════════════

    private static void BuildMeadowsAndGrass(Transform root, Vector3 gate, Vector3 roadEnd, Vector3 village)
    {
        // Луговые патчи (большие плоскости с травой)
        GameObject meadow07 = LoadPrefab(Meadow07);
        GameObject meadow08 = LoadPrefab(Meadow08);
        GameObject meadowPath = LoadPrefab(MeadowPath);
        GameObject grass11 = LoadPrefab(Grass11);
        GameObject grass15 = LoadPrefab(Grass15);

        // Луга вокруг деревни
        Vector3[] meadowPositions = {
            village + new Vector3(-5f, 0f, 15f),
            village + new Vector3(8f, 0f, 12f),
            village + new Vector3(-10f, 0f, -5f),
            village + new Vector3(12f, 0f, -8f),
            gate + new Vector3(-15f, 0f, -10f),
            gate + new Vector3(15f, 0f, -15f),
            gate + new Vector3(-20f, 0f, -40f),
            gate + new Vector3(20f, 0f, -50f),
        };

        for (int i = 0; i < meadowPositions.Length; i++)
        {
            GameObject prefab = (i % 3 == 0) ? meadow08 : (i % 3 == 1) ? meadow07 : meadowPath;
            if (prefab != null)
                InstantiateAndFix(prefab, meadowPositions[i], Quaternion.Euler(0, i * 45f, 0), root, "Meadow_" + i, 1.5f + (i % 3) * 0.5f);
        }

        // Трава — густо по всей карте (вдали от дороги)
        float startZ = gate.z + 5f;
        float endZ = roadEnd.z - 5f;

        for (int i = 0; i < 40; i++)
        {
            float t = i / 39f;
            float z = Mathf.Lerp(startZ, endZ, t);
            float x;
            // Чередуем стороны, не ближе 6 от центра дороги
            if (i % 2 == 0)
                x = -6f - (i % 7) * 3f;
            else
                x = 6f + (i % 7) * 3f;

            GameObject prefab = (i % 2 == 0) ? grass11 : grass15;
            if (prefab != null)
                InstantiateAndFix(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, i * 29f, 0), root, "EnvGrass_" + i, 0.4f + (i % 5) * 0.1f);
        }
    }

    // ═══════════════════ ВАЛУНЫ ═══════════════════

    private static void BuildBoulders(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        GameObject[] rockPrefabs = {
            LoadPrefab(RockMoss09), LoadPrefab(RockMoss11),
            LoadPrefab(RockPile05), LoadPrefab(RockPile10)
        };

        // Камни и скалы по карте — 20 штук
        float startZ = gate.z + 5f;
        float endZ = roadEnd.z - 5f;

        for (int i = 0; i < 20; i++)
        {
            float t = i / 19f;
            float z = Mathf.Lerp(startZ, endZ, t);
            float x;
            if (i % 2 == 0)
                x = -8f - (i % 5) * 4f;
            else
                x = 8f + (i % 5) * 4f;

            GameObject prefab = PickRock(rockPrefabs, i);
            if (prefab != null)
            {
                float scale = 0.3f + (i % 4) * 0.25f;
                GameObject r = Object.Instantiate(prefab, new Vector3(x, 0f, z), Quaternion.Euler(0, i * 67f, 0), root);
                r.name = "EnvRock_" + i;
                r.transform.localScale = Vector3.one * scale;
                FixMaterials(r);
            }
        }

        // Крупные ориентиры (5-6 больших камней)
        GameObject pebbles = LoadPrefab(Pebbles03);
        Vector3[] bigRockPos = {
            gate + new Vector3(-18f, 0f, -8f),
            gate + new Vector3(22f, 0f, -25f),
            gate + new Vector3(-20f, 0f, -45f),
            gate + new Vector3(18f, 0f, -60f),
            gate + new Vector3(-15f, 0f, -75f),
        };
        for (int i = 0; i < bigRockPos.Length; i++)
        {
            GameObject prefab = PickRock(rockPrefabs, i + 10);
            if (prefab != null)
            {
                float scale = 1.2f + (i % 3) * 0.5f;
                GameObject r = Object.Instantiate(prefab, bigRockPos[i], Quaternion.Euler(0, i * 47f, 0), root);
                r.name = "BigRock_" + i;
                r.transform.localScale = Vector3.one * scale;
                FixMaterials(r);
            }

            // Гравий у подножия крупных камней
            if (pebbles != null)
            {
                GameObject p = Object.Instantiate(pebbles, bigRockPos[i] + new Vector3(1f, 0f, 0.5f), Quaternion.Euler(0, i * 90f, 0), root);
                p.name = "Pebbles_" + i;
                p.transform.localScale = Vector3.one * 0.6f;
                FixMaterials(p);
            }
        }
    }

    // ═══════════════════ ЦВЕТЫ И ГРИБЫ ═══════════════════

    private static void BuildFlowersAndMushrooms(Transform root, Vector3 village)
    {
        GameObject daffodil = LoadPrefab(Daffodil);
        GameObject hyacinth = LoadPrefab(Hyacinth);
        GameObject sunflower = LoadPrefab(Sunflower);
        GameObject mushOrange = LoadPrefab(MushroomOrange);
        GameObject mushPurple = LoadPrefab(MushroomPurple);

        // Цветы вокруг деревни (безопасная зона — жизнь)
        Vector3[] flowerSpots = {
            village + new Vector3(-14f, 0f, 8f),
            village + new Vector3(13f, 0f, 10f),
            village + new Vector3(-12f, 0f, -3f),
            village + new Vector3(15f, 0f, 2f),
            village + new Vector3(-8f, 0f, 14f),
            village + new Vector3(10f, 0f, -10f),
            village + new Vector3(0f, 0f, 16f),
            village + new Vector3(-16f, 0f, 0f),
            village + new Vector3(16f, 0f, 7f),
            village + new Vector3(-6f, 0f, -12f),
        };

        GameObject[] flowers = { daffodil, hyacinth, sunflower };
        for (int i = 0; i < flowerSpots.Length; i++)
        {
            GameObject prefab = flowers[i % flowers.Length];
            if (prefab != null)
            {
                float scale = 0.5f + (i % 3) * 0.15f;
                GameObject f = Object.Instantiate(prefab, flowerSpots[i], Quaternion.Euler(0, i * 36f, 0), root);
                f.name = "Flower_" + i;
                f.transform.localScale = Vector3.one * scale;
                FixMaterials(f);
            }
        }

        // Грибы (ближе к орочьей территории и лесу — тёмные зоны)
        Vector3[] mushroomSpots = {
            village + new Vector3(-22f, 0f, -5f),
            village + new Vector3(20f, 0f, -8f),
            village + new Vector3(-18f, 0f, -15f),
            village + new Vector3(22f, 0f, -18f),
            village + new Vector3(-25f, 0f, 3f),
            village + new Vector3(25f, 0f, -3f),
        };

        for (int i = 0; i < mushroomSpots.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? mushOrange : mushPurple;
            if (prefab != null)
            {
                float scale = 0.6f + (i % 3) * 0.2f;
                GameObject m = Object.Instantiate(prefab, mushroomSpots[i], Quaternion.Euler(0, i * 60f, 0), root);
                m.name = "Mushroom_" + i;
                m.transform.localScale = Vector3.one * scale;
                FixMaterials(m);
            }
        }
    }

    // ═══════════════════ ХОЛМЫ (РЕЛЬЕФ) ═══════════════════

    private static void BuildHills(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        Color[] hillColors = {
            new Color(0.22f, 0.42f, 0.18f),
            new Color(0.18f, 0.38f, 0.15f),
            new Color(0.25f, 0.45f, 0.20f),
        };

        Vector3[] hillPositions = {
            gate + new Vector3(-30f, -0.1f, -5f),
            gate + new Vector3(35f, -0.1f, -15f),
            gate + new Vector3(-28f, -0.1f, -35f),
            gate + new Vector3(32f, -0.1f, -42f),
            gate + new Vector3(-35f, -0.1f, -55f),
            gate + new Vector3(30f, -0.1f, -65f),
            gate + new Vector3(-25f, -0.1f, -78f),
            gate + new Vector3(28f, -0.1f, -82f),
        };

        float[] hillScalesXZ = { 12f, 10f, 14f, 9f, 11f, 13f, 10f, 15f };
        float[] hillScalesY = { 0.35f, 0.25f, 0.4f, 0.2f, 0.3f, 0.45f, 0.28f, 0.5f };

        for (int i = 0; i < hillPositions.Length; i++)
        {
            WorldDressingPropFactory.CreateSimpleProp(root, "Hill_" + i, PrimitiveType.Sphere,
                hillPositions[i],
                new Vector3(hillScalesXZ[i], hillScalesY[i], hillScalesXZ[i] * 0.8f),
                hillColors[i % hillColors.Length], false);
        }
    }

    // ═══════════════════ ТУМАН И ДЫМ ═══════════════════

    private static void BuildFogSmoke(Transform root, Vector3 gate, Vector3 roadEnd)
    {
        GameObject smoke3 = LoadPrefab(Smoke3);
        GameObject smoke4 = LoadPrefab(Smoke4);

        // Атмосферный дым в низинах
        Vector3[] smokePositions = {
            gate + new Vector3(-15f, 0.5f, -20f),
            gate + new Vector3(18f, 0.3f, -35f),
            gate + new Vector3(-20f, 0.4f, -55f),
            gate + new Vector3(15f, 0.6f, -70f),
            gate + new Vector3(0f, 0.3f, -85f),
        };

        for (int i = 0; i < smokePositions.Length; i++)
        {
            GameObject prefab = (i % 2 == 0) ? smoke3 : smoke4;
            if (prefab != null)
            {
                float scale = 2f + (i % 3) * 1f;
                GameObject s = Object.Instantiate(prefab, smokePositions[i], Quaternion.identity, root);
                s.name = "AtmoSmoke_" + i;
                s.transform.localScale = Vector3.one * scale;
                FixMaterials(s);
            }
        }
    }

    // ═══════════════════ ОСВЕЩЕНИЕ И АТМОСФЕРА ═══════════════════

    private static void SetupLighting()
    {
        // Directional Light — закатный тёплый
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.color = new Color(1f, 0.85f, 0.6f);
                l.intensity = 1.1f;
                l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                l.shadows = LightShadows.Soft;
                break;
            }
        }

        // Ambient — тёмно-фиолетовый (контраст с тёплым светом)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.12f, 0.22f);

        // Лёгкий туман
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.25f, 0.22f, 0.3f);
        RenderSettings.fogDensity = 0.006f;
    }

    // ═══════════════════ УТИЛИТЫ ═══════════════════

    private static GameObject PickTree(GameObject[] prefabs, int index)
    {
        for (int attempt = 0; attempt < prefabs.Length; attempt++)
        {
            int idx = (index + attempt) % prefabs.Length;
            if (prefabs[idx] != null) return prefabs[idx];
        }
        return null;
    }

    private static GameObject PickRock(GameObject[] prefabs, int index)
    {
        for (int attempt = 0; attempt < prefabs.Length; attempt++)
        {
            int idx = (index + attempt) % prefabs.Length;
            if (prefabs[idx] != null) return prefabs[idx];
        }
        return null;
    }

    private static void FixMaterials(GameObject obj) => L0Util.FixMaterials(obj);

    private static GameObject InstantiateAndFix(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, string name, float scale)
    {
        GameObject obj = Object.Instantiate(prefab, pos, rot, parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        FixMaterials(obj);
        return obj;
    }

    private static GameObject LoadPrefab(string path) => L0Util.LoadPrefab(path);
}
