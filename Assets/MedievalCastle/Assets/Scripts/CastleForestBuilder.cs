using UnityEngine;

/// <summary>
/// Forest Composition v1.
/// Файл отвечает только за расстановку леса.
/// Цель патча:
/// - убрать ощущение случайных деревьев на пустом поле;
/// - сделать лес группами/массивами;
/// - оставить красивый чистый коридор дороги к воротам;
/// - добавить плотность по бокам дороги за счёт кустов, травы и маленьких деревьев;
/// - убрать гигантские деревья прямо перед камерой и перед воротами.
/// </summary>
public static class CastleForestBuilder
{
    public static void BuildDenseForest(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject forest = c.Child(parent, "Forest_Composition_V1_Grouped_Woods");

        // 1. Дальний лес у гор. Он закрывает пустоту между зелёной плоскостью и горами.
        BuildBackForestMass(c, forest);

        // 2. Левый и правый лесные массивы. Это не ряды, а группы с разной глубиной.
        BuildSideForestMass(c, forest, true);
        BuildSideForestMass(c, forest, false);

        // 3. Редкие акцентные деревья около замка, но не перед воротами.
        BuildCastleEdgeAccentTrees(c, forest);

        // 4. Несколько готических сухих деревьев как атмосфера, но редко.
        BuildDeadTreeAccents(c, forest);

        // 5. Тёмные пятна под массивами леса — визуально дают глубину.
        BuildForestShadowPatches(c, forest);
    }

    /// <summary>
    /// Лесной коридор вдоль дороги.
    /// Здесь главное — не перекрыть игроку путь и не ставить гигантов в лицо камере.
    /// </summary>
    public static void BuildRoadsideForestCorridor(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject corridor = c.Child(parent, "Forest_Composition_V1_Roadside_Corridor");

        // Ближняя зона старта игрока. Тут деревья меньше и стоят дальше от центра дороги.
        BuildRoadsideSegment(
            c,
            corridor,
            zStart: -74f,
            zEnd: -52f,
            step: 4.2f,
            minDistanceFromRoad: 13.5f,
            maxExtraDistance: 8.5f,
            treeScaleMin: 0.55f,
            treeScaleMax: 0.95f,
            lod: 1,
            seedOffset: 1000);

        // Средняя зона подхода к воротам. Делаем лес плотнее, но всё равно не лезем в дорогу.
        BuildRoadsideSegment(
            c,
            corridor,
            zStart: -52f,
            zEnd: -25f,
            step: 3.6f,
            minDistanceFromRoad: 10.8f,
            maxExtraDistance: 7.0f,
            treeScaleMin: 0.65f,
            treeScaleMax: 1.15f,
            lod: 0,
            seedOffset: 1200);

        // Зона возле ворот. Тут деревьев мало: замок должен читаться как главный объект.
        BuildRoadsideSegment(
            c,
            corridor,
            zStart: -25f,
            zEnd: -15f,
            step: 5.5f,
            minDistanceFromRoad: 15.0f,
            maxExtraDistance: 5.0f,
            treeScaleMin: 0.55f,
            treeScaleMax: 0.85f,
            lod: 1,
            seedOffset: 1400);

        // Кусты, трава, грибы и пни вдоль дороги. Они дают атмосферу без перегруза большими стволами.
        BuildRoadsideGroundLayer(c, corridor);

        // Несколько выразительных объектов на переднем/среднем плане.
        BuildRoadsideHeroDetails(c, corridor);
    }

    // ─────────────────────────────────────────────────────────
    // Дальний и боковой лес
    // ─────────────────────────────────────────────────────────

    private static void BuildBackForestMass(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject back = c.Child(parent, "Back_Forest_Mass_In_Front_Of_Mountains");

        // Дальний задний пояс. LOD1 — далеко, детализация не нужна.
        for (int i = 0; i < 58; i++)
        {
            float x = -104f + i * 3.8f;
            float z = 42f + Mathf.Sin(i * 0.83f) * 4.5f + CastleNatureAssets.Hash01(i + 500) * 4.0f;
            float s = 0.55f + CastleNatureAssets.Hash01(i + 510) * 0.65f;

            CastleNatureAssets.MixedTree(c, back, new Vector3(x, 0f, z), s, 2100 + i, 1);
        }

        // Второй слой — чуть ближе, но с просветами, чтобы не получилась зелёная стена.
        for (int i = 0; i < 34; i++)
        {
            float x = -76f + i * 4.7f;
            float z = 29f + Mathf.Sin(i * 1.17f) * 5.0f;
            float s = 0.50f + CastleNatureAssets.Hash01(i + 530) * 0.55f;

            if (i % 5 == 0) continue;

            CastleNatureAssets.MixedTree(c, back, new Vector3(x, 0f, z), s, 2200 + i, 1);
        }
    }

    private static void BuildSideForestMass(CastleGenerator.CastleContext c, GameObject parent, bool leftSide)
    {
        GameObject side = c.Child(parent, leftSide ? "Left_Forest_Mass_Grouped" : "Right_Forest_Mass_Grouped");

        float sign = leftSide ? -1f : 1f;

        // Несколько кластеров вместо ровной линии.
        for (int cluster = 0; cluster < 8; cluster++)
        {
            float clusterZ = -55f + cluster * 14f + CastleNatureAssets.Hash01(cluster + (leftSide ? 3000 : 3100)) * 6f;
            float clusterX = sign * (35f + CastleNatureAssets.Hash01(cluster + (leftSide ? 3010 : 3110)) * 18f);

            int treeCount = 5 + Mathf.FloorToInt(CastleNatureAssets.Hash01(cluster + (leftSide ? 3020 : 3120)) * 5f);

            for (int i = 0; i < treeCount; i++)
            {
                float ox = (CastleNatureAssets.Hash01(cluster * 31 + i * 11 + 1) - 0.5f) * 16f;
                float oz = (CastleNatureAssets.Hash01(cluster * 31 + i * 13 + 2) - 0.5f) * 13f;

                float scale = 0.55f + CastleNatureAssets.Hash01(cluster * 31 + i * 17 + 3) * 0.95f;
                int lod = Mathf.Abs(clusterX + ox) > 48f || clusterZ > 25f ? 1 : 0;

                CastleNatureAssets.MixedTree(c, side,
                    new Vector3(clusterX + ox, 0f, clusterZ + oz),
                    scale,
                    2400 + cluster * 40 + i + (leftSide ? 0 : 500),
                    lod);

                if (i % 2 == 0)
                {
                    CastleNatureAssets.MixedBush(c, side,
                        new Vector3(clusterX + ox * 0.92f, 0.22f, clusterZ + oz * 0.92f + 1.2f),
                        0.50f + CastleNatureAssets.Hash01(cluster * 31 + i * 19 + 4) * 0.45f,
                        2600 + cluster * 40 + i);
                }
            }
        }
    }

    private static void BuildCastleEdgeAccentTrees(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject accents = c.Child(parent, "Castle_Edge_Accent_Trees");

        // Специальные позиции: красиво сбоку от замка, но не в воротах и не в дороге.
        Vector3[] positions =
        {
            new Vector3(-29f, 0f, -10f),
            new Vector3(29f, 0f, -8f),
            new Vector3(-31f, 0f, 14f),
            new Vector3(31f, 0f, 15f),
            new Vector3(-35f, 0f, 2f),
            new Vector3(36f, 0f, 5f)
        };

        float[] scales = { 1.05f, 0.95f, 1.10f, 1.05f, 0.85f, 0.85f };

        for (int i = 0; i < positions.Length; i++)
        {
            CastleNatureAssets.MixedTree(c, accents, positions[i], scales[i], 3300 + i, 0);

            CastleNatureAssets.MixedBush(c, accents,
                positions[i] + new Vector3(1.6f * Mathf.Sign(positions[i].x), 0.2f, -1.0f),
                0.75f,
                3350 + i);
        }
    }

    private static void BuildDeadTreeAccents(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject dead = c.Child(parent, "Dead_Tree_Atmosphere_Accents");

        CastleNatureAssets.DeadTreeWithEyes(c, dead, new Vector3(-24f, 0f, -31f), 0.85f, -18f, 4101);
        CastleNatureAssets.DeadTree(c, dead, new Vector3(25f, 0f, -34f), 0.85f, 21f);
        CastleNatureAssets.DeadTreeWithEyes(c, dead, new Vector3(-39f, 0f, 8f), 0.90f, 12f, 4102);
        CastleNatureAssets.DeadTree(c, dead, new Vector3(39f, 0f, 10f), 0.85f, -14f);
    }

    private static void BuildForestShadowPatches(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject shadows = c.Child(parent, "Forest_Shadow_Patches_Composition");

        Material shadowMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDark
            : c.NewMaterial(new Color(0.06f, 0.14f, 0.06f));

        for (int i = 0; i < 34; i++)
        {
            float x;
            float z;

            if (i < 12)
            {
                x = -78f + i * 14f;
                z = 39f + Mathf.Sin(i * 1.4f) * 4f;
            }
            else if (i < 23)
            {
                x = -52f + CastleNatureAssets.Hash01(i + 4400) * 16f;
                z = -55f + (i - 12) * 9f;
            }
            else
            {
                x = 36f + CastleNatureAssets.Hash01(i + 4500) * 18f;
                z = -55f + (i - 23) * 9f;
            }

            GameObject patch = c.Sphere(shadows, "Forest_Shadow_Patch",
                new Vector3(x, 0.018f, z),
                new Vector3(5.0f + CastleNatureAssets.Hash01(i + 4600) * 4f, 0.06f, 2.2f + CastleNatureAssets.Hash01(i + 4610) * 2.6f),
                shadowMat,
                false);

            patch.transform.rotation = Quaternion.Euler(0f, CastleNatureAssets.Hash01(i + 4620) * 180f, 0f);
        }
    }

    // ─────────────────────────────────────────────────────────
    // Лесной коридор у дороги
    // ─────────────────────────────────────────────────────────

    private static void BuildRoadsideSegment(
        CastleGenerator.CastleContext c,
        GameObject parent,
        float zStart,
        float zEnd,
        float step,
        float minDistanceFromRoad,
        float maxExtraDistance,
        float treeScaleMin,
        float treeScaleMax,
        int lod,
        int seedOffset)
    {
        GameObject segment = c.Child(parent, "Roadside_Tree_Segment_" + seedOffset);

        int index = 0;
        for (float z = zStart; z <= zEnd; z += step)
        {
            // Слева
            PlaceRoadsideTree(c, segment, -1f, z, minDistanceFromRoad, maxExtraDistance, treeScaleMin, treeScaleMax, lod, seedOffset + index);
            // Справа
            PlaceRoadsideTree(c, segment, 1f, z + 1.25f, minDistanceFromRoad, maxExtraDistance, treeScaleMin, treeScaleMax, lod, seedOffset + index + 100);

            index++;
        }
    }

    private static void PlaceRoadsideTree(
        CastleGenerator.CastleContext c,
        GameObject parent,
        float sideSign,
        float z,
        float minDistanceFromRoad,
        float maxExtraDistance,
        float treeScaleMin,
        float treeScaleMax,
        int lod,
        int seed)
    {
        // Иногда пропускаем, чтобы не было слишком механического ряда.
        if (CastleNatureAssets.Hash01(seed + 1) < 0.18f)
        {
            return;
        }

        float x = sideSign * (minDistanceFromRoad + CastleNatureAssets.Hash01(seed + 2) * maxExtraDistance);
        float zOffset = (CastleNatureAssets.Hash01(seed + 3) - 0.5f) * 2.5f;
        float scale = treeScaleMin + CastleNatureAssets.Hash01(seed + 4) * (treeScaleMax - treeScaleMin);

        CastleNatureAssets.MixedTree(c, parent, new Vector3(x, 0f, z + zOffset), scale, seed, lod);

        // Под деревом сразу маленький куст/травка — это делает посадку естественнее.
        if (CastleNatureAssets.Hash01(seed + 5) > 0.28f)
        {
            CastleNatureAssets.MixedBush(c, parent,
                new Vector3(x - sideSign * (0.6f + CastleNatureAssets.Hash01(seed + 6) * 1.4f), 0.22f, z + zOffset + CastleNatureAssets.Hash01(seed + 7) * 1.4f),
                0.45f + CastleNatureAssets.Hash01(seed + 8) * 0.35f,
                seed + 50);
        }
    }

    private static void BuildRoadsideGroundLayer(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject ground = c.Child(parent, "Roadside_Ground_Layer_Bushes_Grass");

        // Много мелкой растительности у дороги вместо ещё большего количества больших деревьев.
        for (int i = 0; i < 46; i++)
        {
            float z = -74f + i * 1.35f;
            float side = i % 2 == 0 ? -1f : 1f;

            float x = side * (5.7f + CastleNatureAssets.Hash01(i + 5000) * 4.3f);

            CastleNatureAssets.GrassTuft(c, ground,
                new Vector3(x, 0.12f, z + CastleNatureAssets.Hash01(i + 5010) * 1.2f),
                0.55f + CastleNatureAssets.Hash01(i + 5020) * 0.45f);

            if (i % 3 == 0)
            {
                CastleNatureAssets.MixedBush(c, ground,
                    new Vector3(side * (6.9f + CastleNatureAssets.Hash01(i + 5030) * 2.2f), 0.20f, z + 0.5f),
                    0.42f + CastleNatureAssets.Hash01(i + 5040) * 0.34f,
                    5100 + i);
            }

            if (i % 7 == 0)
            {
                CastleNatureAssets.MushroomGroup(c, ground,
                    new Vector3(side * (7.3f + CastleNatureAssets.Hash01(i + 5050) * 1.2f), 0.13f, z + 0.2f),
                    0.45f + CastleNatureAssets.Hash01(i + 5060) * 0.25f);
            }
        }
    }

    private static void BuildRoadsideHeroDetails(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject details = c.Child(parent, "Roadside_Hero_Details");

        // Детали не ставим в центр дороги.
        CastleNatureAssets.FallenLog(c, details, new Vector3(-12.5f, 0.32f, -42f), 1.0f, 18f);
        CastleNatureAssets.FallenLog(c, details, new Vector3(12.5f, 0.32f, -38f), 0.9f, -22f);

        CastleNatureAssets.Stump(c, details, new Vector3(-9.5f, 0f, -58f), 0.55f);
        CastleNatureAssets.Stump(c, details, new Vector3(9.8f, 0f, -54f), 0.50f);
        CastleNatureAssets.Stump(c, details, new Vector3(-12.0f, 0f, -27f), 0.60f);
        CastleNatureAssets.Stump(c, details, new Vector3(12.2f, 0f, -24f), 0.55f);

        CastleNatureAssets.RockCluster(c, details, new Vector3(-8.8f, 0.25f, -34f), 0.65f);
        CastleNatureAssets.RockCluster(c, details, new Vector3(8.9f, 0.25f, -30f), 0.65f);
        CastleNatureAssets.RockCluster(c, details, new Vector3(-10.2f, 0.25f, -19f), 0.55f);
        CastleNatureAssets.RockCluster(c, details, new Vector3(10.4f, 0.25f, -20f), 0.55f);
    }
}
