using UnityEngine;

/// <summary>
/// Environment Refactor v2 — PRO Nature Assets.
/// Библиотека собственных природных ассетов: деревья, кусты, камни, грибы, пни, брёвна.
/// Улучшения: стабильный Xorshift, вторичные ветки, улучшенные материалы, LOD, анимация.
/// </summary>
public static class CastleNatureAssets
{
    // ─────────────────────────────────────────────────────────
    // 1. Быстрый генератор случайных чисел (Xorshift32)
    //    Заменяет старый Hash01, более равномерный и быстрый.
    // ─────────────────────────────────────────────────────────
    private static class FastRandom
    {
        public static float NextFloat(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= 0x9E3779B9u;
                x *= 0x85EBCA6Bu;
                x ^= x >> 13;
                x *= 0xC2B2AE35u;
                x ^= x >> 16;

                // Xorshift finalization
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;

                return (x & 0x00FFFFFFu) / 16777216f;
            }
        }
    }

    // Совместимость: оставляем старую Hash01, но внутри используем FastRandom
    public static float Hash01(int seed)
    {
        return FastRandom.NextFloat(seed);
    }

    private static float SignedHash(int seed)
    {
        return Hash01(seed) * 2f - 1f;
    }

    // Кэшированные массивы цветов – без аллокаций каждый раз
    private static readonly Color[] BLOSSOM_COLORS = new Color[]
    {
        new Color(1f, 0.82f, 0.88f),
        new Color(1f, 0.68f, 0.78f),
        new Color(0.95f, 0.62f, 0.74f),
        new Color(1f, 0.74f, 0.84f)
    };

    // ─────────────────────────────────────────────────────────
    // 2. Улучшенные материалы с вариациями оттенков
    // ─────────────────────────────────────────────────────────
    private static Material LeafMaterial(CastleGenerator.CastleContext c, int seed)
    {
        if (c.Mode == CastleGenerator.LabMode.Lab3_Blockout)
            return c.MatLeaves;

        float r = Hash01(seed);
        Color baseColor;
        if (r < 0.62f) baseColor = c.MatLeaves.color; // используем базовый цвет MatLeaves
        else if (r < 0.78f) baseColor = new Color(0.10f, 0.28f, 0.12f);
        else if (r < 0.90f) baseColor = new Color(0.19f, 0.42f, 0.16f);
        else baseColor = new Color(0.42f, 0.34f, 0.13f);

        // Случайный оттенок
        float variation = (Hash01(seed + 5) - 0.5f) * 0.08f;
        baseColor.r = Mathf.Clamp01(baseColor.r + variation);
        baseColor.g = Mathf.Clamp01(baseColor.g + variation * 0.6f);
        baseColor.b = Mathf.Clamp01(baseColor.b + variation * 0.2f);
        return c.NewMaterial(baseColor);
    }

    private static Material BlossomMaterial(CastleGenerator.CastleContext c, int seed)
    {
        if (c.Mode == CastleGenerator.LabMode.Lab3_Blockout)
            return c.MatLeaves;
        return c.NewMaterial(BLOSSOM_COLORS[Mathf.Abs(seed) % BLOSSOM_COLORS.Length]);
    }

    // ─────────────────────────────────────────────────────────
    // 3. Методы генерации деревьев (с LOD перегрузками)
    // ─────────────────────────────────────────────────────────

    // Главный выбор – старая сигнатура без LOD
    public static void MixedTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed)
    {
        MixedTree(c, parent, basePos, scale, seed, 0);
    }

    // Новая перегрузка с LOD (0 – полно, 1 – упрощённо)
    public static void MixedTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed, int lod)
    {
        int type = Mathf.Abs(seed) % 9;
        float rot = Hash01(seed + 500) * 360f;

        // В LOD1 используем только простые формы (первые 3 типа)
        if (lod > 0) type = type % 3;

        if (type == 0) PineTree(c, parent, basePos, scale, seed, lod);
        else if (type == 1) TallThinTree(c, parent, basePos, scale, seed, lod);
        else if (type == 2) SmallRoundTree(c, parent, basePos, scale * 0.9f, seed, lod);
        else if (type == 3) BroadTree(c, parent, basePos, scale, seed, lod);
        else if (type == 4) OakTree(c, parent, basePos, scale, rot, seed, lod);
        else if (type == 5) FloweringTree(c, parent, basePos, scale * 0.85f, rot, seed, lod);
        else if (type == 6) TallPine(c, parent, basePos, scale, rot, seed, lod);
        else if (type == 7) WeepingWillow(c, parent, basePos, scale * 0.95f, rot, seed, lod);
        else AncientOakWithHollow(c, parent, basePos, scale, rot, seed, lod);
    }

    // ─────────────────────────────────────────────────────────
    // 4. Старые базовые деревья – теперь с поддержкой LOD и дополнительными деталями
    //    Сигнатуры оставлены совместимыми (lod = 0 по умолчанию)
    // ─────────────────────────────────────────────────────────

    public static void BroadTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Broad_LowPoly");
        Material leaf = LeafMaterial(c, seed);
        float trunkHeight = 3.0f * scale;

        c.Cylinder(tree, "Trunk",
            basePos + new Vector3(0f, trunkHeight * 0.5f, 0f),
            new Vector3(0.26f * scale, trunkHeight * 0.5f, 0.26f * scale),
            c.MatTrunk, false);

        if (lod == 0)
        {
            // Полная детализация
            c.Sphere(tree, "Crown_Main",
                basePos + new Vector3(0f, trunkHeight + 1.0f * scale, 0f),
                new Vector3(2.05f * scale, 1.35f * scale, 2.05f * scale),
                leaf, false);

            c.Sphere(tree, "Crown_Top",
                basePos + new Vector3(0.2f * scale, trunkHeight + 2.05f * scale, -0.1f * scale),
                new Vector3(1.5f * scale, 1.05f * scale, 1.5f * scale),
                leaf, false);

            c.Sphere(tree, "Crown_Side",
                basePos + new Vector3(-0.75f * scale, trunkHeight + 1.35f * scale, 0.45f * scale),
                new Vector3(1.25f * scale, 0.9f * scale, 1.25f * scale),
                leaf, false);
        }
        else
        {
            // LOD1: одна большая сфера
            c.Sphere(tree, "Crown_LOD",
                basePos + new Vector3(0f, trunkHeight + 1.5f * scale, 0f),
                new Vector3(2.3f * scale, 1.8f * scale, 2.3f * scale),
                leaf, false);
        }

        // Добавляем аниматор (если не блокнот)
        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void PineTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Pine_LowPoly");
        Material leaf = LeafMaterial(c, seed + 10);
        float trunkHeight = 3.8f * scale;

        c.Cylinder(tree, "Pine_Trunk",
            basePos + new Vector3(0f, trunkHeight * 0.45f, 0f),
            new Vector3(0.22f * scale, trunkHeight * 0.45f, 0.22f * scale),
            c.MatTrunk, false);

        if (lod == 0)
        {
            c.Sphere(tree, "Pine_Crown_Low",
                basePos + new Vector3(0f, trunkHeight * 0.85f, 0f),
                new Vector3(1.9f * scale, 0.75f * scale, 1.9f * scale),
                leaf, false);

            c.Sphere(tree, "Pine_Crown_Mid",
                basePos + new Vector3(0f, trunkHeight * 1.05f, 0f),
                new Vector3(1.45f * scale, 0.68f * scale, 1.45f * scale),
                leaf, false);

            c.Sphere(tree, "Pine_Crown_Top",
                basePos + new Vector3(0f, trunkHeight * 1.24f, 0f),
                new Vector3(0.95f * scale, 0.62f * scale, 0.95f * scale),
                leaf, false);
        }
        else
        {
            c.Sphere(tree, "Pine_Crown_LOD",
                basePos + new Vector3(0f, trunkHeight * 1.05f, 0f),
                new Vector3(2.1f * scale, 1.1f * scale, 2.1f * scale),
                leaf, false);
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void TallThinTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Tall_Thin");
        Material leaf = LeafMaterial(c, seed + 20);
        float trunkHeight = 4.5f * scale;

        c.Cylinder(tree, "Thin_Trunk",
            basePos + new Vector3(0f, trunkHeight * 0.5f, 0f),
            new Vector3(0.18f * scale, trunkHeight * 0.5f, 0.18f * scale),
            c.MatTrunk, false);

        if (lod == 0)
        {
            c.Sphere(tree, "High_Crown",
                basePos + new Vector3(0f, trunkHeight + 1.0f * scale, 0f),
                new Vector3(1.35f * scale, 1.55f * scale, 1.35f * scale),
                leaf, false);

            c.Sphere(tree, "High_Crown_Side",
                basePos + new Vector3(0.45f * scale, trunkHeight + 1.5f * scale, -0.2f * scale),
                new Vector3(0.95f * scale, 0.85f * scale, 0.95f * scale),
                leaf, false);
        }
        else
        {
            c.Sphere(tree, "High_Crown_LOD",
                basePos + new Vector3(0f, trunkHeight + 1.25f * scale, 0f),
                new Vector3(1.6f * scale, 1.8f * scale, 1.6f * scale),
                leaf, false);
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void SmallRoundTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Small_Round");
        Material leaf = LeafMaterial(c, seed + 30);
        float trunkHeight = 2.3f * scale;

        c.Cylinder(tree, "Small_Trunk",
            basePos + new Vector3(0f, trunkHeight * 0.5f, 0f),
            new Vector3(0.22f * scale, trunkHeight * 0.5f, 0.22f * scale),
            c.MatTrunk, false);

        c.Sphere(tree, "Round_Crown",
            basePos + new Vector3(0f, trunkHeight + 0.85f * scale, 0f),
            new Vector3(1.65f * scale, 1.15f * scale, 1.65f * scale),
            leaf, false);

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    // ─────────────────────────────────────────────────────────
    // 5. PRO-деревья – улучшенные, с вторичными ветками и LOD
    // ─────────────────────────────────────────────────────────

    public static void OakTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotationY, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Oak_Pro");
        Material leaf = LeafMaterial(c, seed + 100);

        // Корни (только в полном LOD)
        if (lod == 0)
        {
            Vector3[] rootOffsets =
            {
                new Vector3(0.6f, 0f, 0.4f),
                new Vector3(-0.5f, 0f, 0.7f),
                new Vector3(0.4f, 0f, -0.7f),
                new Vector3(-0.7f, 0f, -0.3f)
            };

            for (int i = 0; i < rootOffsets.Length; i++)
            {
                GameObject root = c.Cylinder(tree, "Oak_Root_" + i,
                    basePos + rootOffsets[i] * scale + new Vector3(0f, 0.18f * scale, 0f),
                    new Vector3(0.16f * scale, 0.45f * scale, 0.16f * scale),
                    c.MatTrunk, false);
                root.transform.rotation = Quaternion.Euler(70f + SignedHash(seed + i) * 10f, rotationY + i * 85f, 15f + SignedHash(seed + i + 20) * 20f);
            }
        }

        float trunkH = 3.2f * scale;

        c.Cylinder(tree, "Oak_Trunk_Lower",
            basePos + new Vector3(0f, trunkH * 0.30f, 0f),
            new Vector3(0.52f * scale, trunkH * 0.30f, 0.52f * scale),
            c.MatTrunk, false);

        c.Cylinder(tree, "Oak_Trunk_Upper",
            basePos + new Vector3(0.08f * scale, trunkH * 0.73f, -0.05f * scale),
            new Vector3(0.36f * scale, trunkH * 0.42f, 0.36f * scale),
            c.MatTrunk, false);

        // Основные ветки + вторичные (только в полном LOD)
        for (int i = 0; i < 5; i++)
        {
            float angle = rotationY + i * 72f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0.28f + i * 0.04f, Mathf.Sin(rad)).normalized;
            Vector3 start = basePos + new Vector3(0f, trunkH * (0.55f + i * 0.07f), 0f);
            Vector3 branchPos = start + dir * (0.55f * scale);
            Vector3 branchEnd = branchPos + dir * (0.65f * scale);

            GameObject branch = c.Cylinder(tree, "Oak_Branch_" + i,
                branchPos,
                new Vector3(0.14f * scale, 0.65f * scale, 0.14f * scale),
                c.MatTrunk, false);
            branch.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

            // Вторичные мелкие ветки (только LOD0)
            if (lod == 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    float angle2 = branch.transform.eulerAngles.y + (j == 0 ? 45f : -45f);
                    Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * Vector3.forward * 0.3f;
                    c.Cylinder(tree, "Oak_SubBranch_" + i + "_" + j,
                        branchEnd + dir2 * 0.2f * scale,
                        new Vector3(0.06f * scale, 0.3f * scale, 0.06f * scale),
                        c.MatTrunk, false);
                }
            }
        }

        Vector3 crownCenter = basePos + new Vector3(0.08f * scale, trunkH + 0.95f * scale, -0.08f * scale);

        if (lod == 0)
        {
            c.Sphere(tree, "Crown_Core", crownCenter, new Vector3(2.25f * scale, 1.55f * scale, 2.05f * scale), leaf, false);
            c.Sphere(tree, "Crown_Top", crownCenter + new Vector3(0f, 0.9f * scale, 0.2f * scale), new Vector3(1.45f * scale, 1.05f * scale, 1.35f * scale), leaf, false);
            c.Sphere(tree, "Crown_Left", crownCenter + new Vector3(-1.25f * scale, 0.15f * scale, 0.4f * scale), new Vector3(1.25f * scale, 0.95f * scale, 1.1f * scale), leaf, false);
            c.Sphere(tree, "Crown_Right", crownCenter + new Vector3(1.35f * scale, 0.25f * scale, -0.3f * scale), new Vector3(1.35f * scale, 1.0f * scale, 1.15f * scale), leaf, false);
            c.Sphere(tree, "Crown_Front", crownCenter + new Vector3(0.25f * scale, -0.15f * scale, 1.15f * scale), new Vector3(1.15f * scale, 0.85f * scale, 1.05f * scale), leaf, false);
            c.Sphere(tree, "Crown_Back", crownCenter + new Vector3(-0.2f * scale, 0.35f * scale, -1.1f * scale), new Vector3(1.05f * scale, 0.8f * scale, 0.95f * scale), leaf, false);

            // Жёлуди
            Material acornMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatWood
                : c.NewMaterial(new Color(0.48f, 0.24f, 0.06f));

            for (int i = 0; i < 8; i++)
            {
                Vector3 acornPos = crownCenter + new Vector3(
                    SignedHash(seed + i * 11) * 1.25f * scale,
                    SignedHash(seed + i * 13) * 0.55f * scale,
                    SignedHash(seed + i * 17) * 1.15f * scale);
                c.Sphere(tree, "Acorn_" + i, acornPos, new Vector3(0.10f * scale, 0.12f * scale, 0.10f * scale), acornMat, false);
            }
        }
        else
        {
            // LOD1: одна большая крона
            c.Sphere(tree, "Crown_LOD", crownCenter, new Vector3(2.6f * scale, 1.8f * scale, 2.4f * scale), leaf, false);
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void FloweringTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotationY, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_Sakura_Pro");
        float trunkH = 2.8f * scale;

        c.Cylinder(tree, "Trunk_Low",
            basePos + new Vector3(0f, trunkH * 0.30f, 0f),
            new Vector3(0.26f * scale, trunkH * 0.30f, 0.26f * scale),
            c.MatTrunk, false);

        if (lod == 0)
        {
            GameObject midTrunk = c.Cylinder(tree, "Trunk_Mid",
                basePos + new Vector3(0.12f * scale, trunkH * 0.68f, -0.08f * scale),
                new Vector3(0.20f * scale, trunkH * 0.35f, 0.20f * scale),
                c.MatTrunk, false);
            midTrunk.transform.rotation = Quaternion.Euler(8f, rotationY, 5f);

            GameObject topTrunk = c.Cylinder(tree, "Trunk_Top",
                basePos + new Vector3(0.26f * scale, trunkH * 1.02f, -0.17f * scale),
                new Vector3(0.14f * scale, trunkH * 0.25f, 0.14f * scale),
                c.MatTrunk, false);
            topTrunk.transform.rotation = Quaternion.Euler(15f, rotationY + 10f, 8f);

            for (int i = 0; i < 5; i++)
            {
                float yOffset = trunkH + 0.25f * scale + i * 0.45f * scale;
                float xOffset = Mathf.Sin(i * 1.2f) * 0.45f * scale;
                float zOffset = Mathf.Cos(i * 0.9f) * 0.35f * scale;
                c.Sphere(tree, "Blossom_Layer_" + i,
                    basePos + new Vector3(xOffset, yOffset, zOffset),
                    new Vector3(1.25f * scale - i * 0.08f * scale, 0.82f * scale - i * 0.04f * scale, 1.08f * scale - i * 0.06f * scale),
                    BlossomMaterial(c, seed + i),
                    false);
            }

            // Свисающие ветки
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f + rotationY;
                Vector3 dir = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), -0.7f, Mathf.Cos(angle * Mathf.Deg2Rad)).normalized;
                Vector3 start = basePos + new Vector3(0.4f * scale, trunkH + 0.85f * scale, 0f);
                GameObject hanging = c.Cylinder(tree, "Hanging_Branch_" + i,
                    start + dir * 0.55f * scale,
                    new Vector3(0.055f * scale, 0.55f * scale, 0.055f * scale),
                    c.MatTrunk, false);
                hanging.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            }

            // Лепестки
            Material petalMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatLeaves
                : c.NewMaterial(new Color(1f, 0.80f, 0.86f));
            for (int i = 0; i < 12; i++)
            {
                Vector3 petalPos = basePos + new Vector3(
                    SignedHash(seed + i * 3) * 1.45f * scale,
                    0.7f + Hash01(seed + i * 5) * 1.4f * scale,
                    SignedHash(seed + i * 7) * 1.35f * scale);
                GameObject petal = c.Cube(tree, "Petal_" + i,
                    petalPos,
                    new Vector3(0.06f * scale, 0.02f * scale, 0.08f * scale),
                    petalMat, false);
                petal.transform.rotation = Quaternion.Euler(Hash01(seed + i) * 360f, Hash01(seed + i + 30) * 360f, Hash01(seed + i + 60) * 360f);
            }
        }
        else
        {
            // LOD1: просто несколько сфер
            for (int i = 0; i < 3; i++)
            {
                float yOff = trunkH + 0.4f * scale + i * 0.7f * scale;
                c.Sphere(tree, "Blossom_LOD_" + i,
                    basePos + new Vector3(0f, yOff, 0f),
                    new Vector3(1.5f * scale, 0.9f * scale, 1.5f * scale),
                    BlossomMaterial(c, seed + i),
                    false);
            }
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void TallPine(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotationY, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_TallPine_Pro");
        Material leaf = LeafMaterial(c, seed + 200);
        float trunkH = 4.5f * scale;

        c.Cylinder(tree, "Pine_Trunk",
            basePos + new Vector3(0f, trunkH * 0.45f, 0f),
            new Vector3(0.24f * scale, trunkH * 0.45f, 0.24f * scale),
            c.MatTrunk, false);

        float[] layerHeights = { 0.65f, 0.85f, 1.05f, 1.22f, 1.38f, 1.52f };
        float[] layerRadii = { 1.9f, 1.7f, 1.5f, 1.2f, 0.9f, 0.6f };

        if (lod == 0)
        {
            for (int i = 0; i < layerHeights.Length; i++)
            {
                float y = trunkH * layerHeights[i];
                float rad = layerRadii[i] * scale;
                float heightRad = (1.05f - i * 0.08f) * scale;
                c.Sphere(tree, "Pine_Layer_" + i,
                    basePos + new Vector3(0f, y, 0f),
                    new Vector3(rad, heightRad, rad),
                    leaf, false);

                if (i < 3)
                {
                    for (int b = 0; b < 4; b++)
                    {
                        float angle = b * 90f + rotationY + i * 15f;
                        Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.2f, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                        GameObject branch = c.Cylinder(tree, "Pine_Branch_" + i + "_" + b,
                            basePos + new Vector3(dir.x * rad * 0.62f, y - 0.1f * scale, dir.z * rad * 0.62f),
                            new Vector3(0.07f * scale, 0.42f * scale, 0.07f * scale),
                            c.MatTrunk, false);
                        branch.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
                    }
                }
            }
        }
        else
        {
            // LOD1: три больших сферы
            c.Sphere(tree, "Pine_LOD_Low", basePos + new Vector3(0f, trunkH * 0.75f, 0f), new Vector3(2.2f * scale, 1.0f * scale, 2.2f * scale), leaf, false);
            c.Sphere(tree, "Pine_LOD_Mid", basePos + new Vector3(0f, trunkH * 1.05f, 0f), new Vector3(1.8f * scale, 0.9f * scale, 1.8f * scale), leaf, false);
            c.Sphere(tree, "Pine_LOD_Top", basePos + new Vector3(0f, trunkH * 1.3f, 0f), new Vector3(1.1f * scale, 0.7f * scale, 1.1f * scale), leaf, false);
        }

        // Шишки (только LOD0)
        if (lod == 0)
        {
            Material coneMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatWood
                : c.NewMaterial(new Color(0.34f, 0.17f, 0.04f));
            for (int i = 0; i < 8; i++)
            {
                float yOff = trunkH * (0.72f + Hash01(seed + i) * 0.55f);
                float angle = Hash01(seed + i + 90) * 360f * Mathf.Deg2Rad;
                float rad = 0.9f * scale + Hash01(seed + i + 120) * 0.45f * scale;
                Vector3 conePos = basePos + new Vector3(Mathf.Cos(angle) * rad, yOff, Mathf.Sin(angle) * rad);
                c.Sphere(tree, "Cone_" + i, conePos, new Vector3(0.10f * scale, 0.16f * scale, 0.10f * scale), coneMat, false);
            }
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void WeepingWillow(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotationY, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_WeepingWillow");
        Material leaf = LeafMaterial(c, seed + 300);
        float trunkH = 3.5f * scale;

        GameObject trunk = c.Cylinder(tree, "Willow_Trunk",
            basePos + new Vector3(0f, trunkH * 0.48f, 0f),
            new Vector3(0.40f * scale, trunkH * 0.48f, 0.40f * scale),
            c.MatTrunk, false);
        trunk.transform.rotation = Quaternion.Euler(5f, rotationY, 3f);

        c.Sphere(tree, "Willow_Crown",
            basePos + new Vector3(0.18f * scale, trunkH + 0.55f * scale, -0.1f * scale),
            new Vector3(2.15f * scale, 1.25f * scale, 2.0f * scale),
            leaf, false);

        // Свисающие ветки (только LOD0)
        if (lod == 0)
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = Hash01(seed + i * 10) * 360f * Mathf.Deg2Rad;
                float rad = 0.55f * scale + Hash01(seed + i * 12) * 1.15f * scale;
                Vector3 start = basePos + new Vector3(Mathf.Cos(angle) * rad, trunkH + 0.55f * scale, Mathf.Sin(angle) * rad);
                Vector3 end = start + Vector3.down * (0.9f * scale + Hash01(seed + i * 13) * 0.85f * scale);
                Vector3 dir = (end - start).normalized;
                GameObject branch = c.Cylinder(tree, "Weeping_Branch_" + i,
                    (start + end) * 0.5f,
                    new Vector3(0.035f * scale, Vector3.Distance(start, end) * 0.5f, 0.035f * scale),
                    c.MatTrunk, false);
                branch.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            }
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    public static void AncientOakWithHollow(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotationY, int seed = 0, int lod = 0)
    {
        GameObject tree = c.Child(parent, "Asset_Tree_AncientOak_Hollow");
        Material leaf = LeafMaterial(c, seed + 400);
        float trunkH = 3.8f * scale;

        c.Cylinder(tree, "Old_Trunk",
            basePos + new Vector3(0f, trunkH * 0.48f, 0f),
            new Vector3(0.58f * scale, trunkH * 0.48f, 0.58f * scale),
            c.MatTrunk, false);

        if (lod == 0)
        {
            Material darkMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatDark
                : c.NewMaterial(new Color(0.07f, 0.035f, 0.015f));
            c.Sphere(tree, "Hollow",
                basePos + new Vector3(0.45f * scale, trunkH * 0.55f, 0.30f * scale),
                new Vector3(0.28f * scale, 0.34f * scale, 0.22f * scale),
                darkMat, false);

            Material ivyMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
                ? c.MatLeaves
                : c.NewMaterial(new Color(0.14f, 0.38f, 0.12f));
            for (int i = 0; i < 12; i++)
            {
                float y = trunkH * Hash01(seed + i * 4);
                float angle = Hash01(seed + i * 9) * 360f * Mathf.Deg2Rad;
                float rad = 0.58f * scale;
                Vector3 ivyPos = basePos + new Vector3(Mathf.Cos(angle) * rad, y, Mathf.Sin(angle) * rad);
                c.Cube(tree, "Ivy_" + i, ivyPos, new Vector3(0.09f * scale, 0.09f * scale, 0.045f * scale), ivyMat, false);
            }
        }

        Vector3 crownPos = basePos + new Vector3(0f, trunkH + 0.95f * scale, 0f);
        c.Sphere(tree, "Crown_Main", crownPos, new Vector3(2.45f * scale, 1.55f * scale, 2.25f * scale), leaf, false);
        if (lod == 0)
        {
            c.Sphere(tree, "Crown_Side1", crownPos + new Vector3(-1.15f * scale, 0.15f * scale, 0.5f * scale), new Vector3(1.25f * scale, 0.95f * scale, 1.05f * scale), leaf, false);
            c.Sphere(tree, "Crown_Side2", crownPos + new Vector3(1.25f * scale, 0.35f * scale, -0.4f * scale), new Vector3(1.35f * scale, 1.05f * scale, 1.15f * scale), leaf, false);
        }

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    // ─────────────────────────────────────────────────────────
    // 6. Мёртвое дерево с глазами (готическое, по желанию)
    //    Используй DeadTreeWithEyes вместо DeadTree для особого шарма
    // ─────────────────────────────────────────────────────────
    public static void DeadTree(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotY)
    {
        // Оставляем старую версию для совместимости
        GameObject tree = c.Child(parent, "Asset_Dead_Tree");
        GameObject trunk = c.Cylinder(tree, "Dead_Trunk",
            basePos + new Vector3(0f, 2.0f * scale, 0f),
            new Vector3(0.20f * scale, 2.0f * scale, 0.20f * scale),
            c.MatTrunk, false);
        trunk.transform.rotation = Quaternion.Euler(0f, rotY, 4f);

        GameObject branch1 = c.Cube(tree, "Dead_Branch_1",
            basePos + new Vector3(0.65f * scale, 3.0f * scale, 0f),
            new Vector3(1.4f * scale, 0.12f * scale, 0.12f * scale),
            c.MatTrunk, false);
        branch1.transform.rotation = Quaternion.Euler(0f, rotY, 25f);

        GameObject branch2 = c.Cube(tree, "Dead_Branch_2",
            basePos + new Vector3(-0.55f * scale, 2.6f * scale, 0f),
            new Vector3(1.1f * scale, 0.10f * scale, 0.10f * scale),
            c.MatTrunk, false);
        branch2.transform.rotation = Quaternion.Euler(0f, rotY, -28f);
    }

    // Опциональное готическое мёртвое дерево с дуплом-глазом
    public static void DeadTreeWithEyes(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float scale, float rotY, int seed = 0)
    {
        GameObject tree = c.Child(parent, "Asset_DeadTree_Eyes");
        // Искривлённый ствол (можно сделать из нескольких цилиндров)
        GameObject trunk = c.Cylinder(tree, "Dead_Trunk",
            basePos + new Vector3(0f, 1.5f * scale, 0f),
            new Vector3(0.28f * scale, 1.5f * scale, 0.28f * scale),
            c.MatTrunk, false);
        trunk.transform.rotation = Quaternion.Euler(0f, rotY, 10f);

        // Глаз-дупло (светящийся)
        Material eyeMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDark
            : c.NewMaterial(new Color(1f, 0.3f, 0.1f)); // красноватый
        c.Sphere(tree, "Eye", basePos + new Vector3(0.35f * scale, 2.4f * scale, 0.2f * scale),
            new Vector3(0.18f * scale, 0.22f * scale, 0.14f * scale), eyeMat, false);

        // Ветки как у мёртвого дерева
        GameObject branch1 = c.Cube(tree, "Branch1",
            basePos + new Vector3(0.65f * scale, 3.0f * scale, 0f),
            new Vector3(1.4f * scale, 0.12f * scale, 0.12f * scale),
            c.MatTrunk, false);
        branch1.transform.rotation = Quaternion.Euler(0f, rotY, 25f);

        GameObject branch2 = c.Cube(tree, "Branch2",
            basePos + new Vector3(-0.55f * scale, 2.6f * scale, 0f),
            new Vector3(1.1f * scale, 0.10f * scale, 0.10f * scale),
            c.MatTrunk, false);
        branch2.transform.rotation = Quaternion.Euler(0f, rotY, -28f);

        if (c.Mode != CastleGenerator.LabMode.Lab3_Blockout)
            tree.AddComponent<TreeAnimator>();
    }

    // ─────────────────────────────────────────────────────────
    // 7. Кусты, камни, грибы и прочее – без изменений, всё работает
    // ─────────────────────────────────────────────────────────

    public static void MixedBush(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale, int seed)
    {
        if (seed % 3 == 0) TallBush(c, parent, pos, scale);
        else if (seed % 3 == 1) LowBush(c, parent, pos, scale);
        else RoundBush(c, parent, pos, scale);
    }

    public static void RoundBush(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject bush = c.Child(parent, "Asset_Bush_Round");
        c.Sphere(bush, "Bush_Main", pos, new Vector3(1.4f * scale, 0.55f * scale, 1.0f * scale), c.MatLeaves, false);
        c.Sphere(bush, "Bush_Side", pos + new Vector3(0.55f * scale, 0.05f, -0.2f * scale), new Vector3(0.95f * scale, 0.45f * scale, 0.85f * scale), c.MatLeaves, false);
    }

    public static void LowBush(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject bush = c.Child(parent, "Asset_Bush_Low");
        c.Sphere(bush, "Bush_Low_A", pos, new Vector3(1.8f * scale, 0.38f * scale, 1.25f * scale), c.MatLeaves, false);
        c.Sphere(bush, "Bush_Low_B", pos + new Vector3(-0.55f * scale, 0.03f, 0.25f * scale), new Vector3(1.15f * scale, 0.32f * scale, 0.95f * scale), c.MatLeaves, false);
    }

    public static void TallBush(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject bush = c.Child(parent, "Asset_Bush_Tall");
        c.Sphere(bush, "Bush_Tall_Base", pos + new Vector3(0f, 0.15f * scale, 0f), new Vector3(1.0f * scale, 0.65f * scale, 0.85f * scale), c.MatLeaves, false);
        c.Sphere(bush, "Bush_Tall_Top", pos + new Vector3(0.1f * scale, 0.65f * scale, 0f), new Vector3(0.75f * scale, 0.55f * scale, 0.75f * scale), c.MatLeaves, false);
    }

    public static void GrassTuft(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject tuft = c.Child(parent, "Asset_Grass_Tuft");
        Material mat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatGrass
            : c.NewMaterial(new Color(0.12f, 0.34f, 0.11f));

        for (int i = 0; i < 5; i++)
        {
            GameObject blade = c.Cube(tuft, "Grass_Blade_" + i,
                pos + new Vector3((i - 2) * 0.08f * scale, 0.18f * scale, Hash01(i + 50) * 0.16f * scale),
                new Vector3(0.035f * scale, 0.35f * scale, 0.035f * scale),
                mat, false);
            blade.transform.rotation = Quaternion.Euler(0f, Hash01(i + 70) * 70f, -18f + i * 8f);
        }
    }

    public static void MushroomGroup(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject group = c.Child(parent, "Asset_Mushroom_Group");
        Material stemMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout ? c.MatWood : c.NewMaterial(new Color(0.80f, 0.74f, 0.60f));
        Material capMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout ? c.MatDark : c.NewMaterial(new Color(0.55f, 0.12f, 0.08f));

        for (int i = 0; i < 4; i++)
        {
            Vector3 p = pos + new Vector3((Hash01(i + 4) - 0.5f) * 0.8f * scale, 0f, (Hash01(i + 9) - 0.5f) * 0.8f * scale);
            float s = scale * (0.55f + Hash01(i + 16) * 0.35f);
            c.Cylinder(group, "Mushroom_Stem_" + i, p + new Vector3(0f, 0.12f * s, 0f), new Vector3(0.06f * s, 0.12f * s, 0.06f * s), stemMat, false);
            c.Sphere(group, "Mushroom_Cap_" + i, p + new Vector3(0f, 0.29f * s, 0f), new Vector3(0.20f * s, 0.08f * s, 0.20f * s), capMat, false);
        }
    }

    public static void FallenLog(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale, float rotY)
    {
        GameObject log = c.Child(parent, "Asset_Fallen_Log");
        GameObject body = c.Cylinder(log, "Log_Body", pos, new Vector3(0.28f * scale, 1.45f * scale, 0.28f * scale), c.MatTrunk, false);
        body.transform.rotation = Quaternion.Euler(0f, rotY, 90f);
        c.Cylinder(log, "Log_Cut_Left", pos + Quaternion.Euler(0f, rotY, 0f) * new Vector3(-1.45f * scale, 0f, 0f), new Vector3(0.29f * scale, 0.025f * scale, 0.29f * scale), c.MatWood, false).transform.rotation = Quaternion.Euler(0f, rotY, 90f);
        c.Cylinder(log, "Log_Cut_Right", pos + Quaternion.Euler(0f, rotY, 0f) * new Vector3(1.45f * scale, 0f, 0f), new Vector3(0.29f * scale, 0.025f * scale, 0.29f * scale), c.MatWood, false).transform.rotation = Quaternion.Euler(0f, rotY, 90f);
    }

    public static void RockCluster(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject cluster = c.Child(parent, "Asset_Rock_Cluster");
        Rock(c, cluster, pos + new Vector3(0f, 0f, 0f), new Vector3(1.4f * scale, 0.55f * scale, 1.0f * scale), 10f);
        Rock(c, cluster, pos + new Vector3(1.1f * scale, 0f, 0.3f * scale), new Vector3(0.9f * scale, 0.35f * scale, 0.75f * scale), -18f);
        Rock(c, cluster, pos + new Vector3(-0.9f * scale, 0f, -0.2f * scale), new Vector3(0.8f * scale, 0.32f * scale, 0.65f * scale), 28f);
    }

    public static void Stump(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float scale)
    {
        GameObject stump = c.Child(parent, "Asset_Tree_Stump");
        c.Cylinder(stump, "Stump_Body", pos + new Vector3(0f, 0.35f * scale, 0f), new Vector3(0.36f * scale, 0.35f * scale, 0.36f * scale), c.MatTrunk, false);
        c.Cylinder(stump, "Stump_Top", pos + new Vector3(0f, 0.73f * scale, 0f), new Vector3(0.38f * scale, 0.035f * scale, 0.38f * scale), c.MatWood, false);
    }

    public static void SignPost(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, string name)
    {
        GameObject sign = c.Child(parent, "Asset_Road_Sign_" + name);
        c.Cylinder(sign, "Sign_Post", pos + new Vector3(0f, 0.85f, 0f), new Vector3(0.12f, 0.85f, 0.12f), c.MatWood, false);
        GameObject board = c.Cube(sign, "Sign_Board", pos + new Vector3(0.45f, 1.45f, 0f), new Vector3(1.15f, 0.35f, 0.12f), c.MatWood, false);
        board.transform.rotation = Quaternion.Euler(0f, 10f, 0f);
    }

    public static void FenceSegment(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject fence = c.Child(parent, "Asset_Small_Road_Fence");
        fence.transform.position = pos;
        fence.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        c.Cylinder(fence, "Fence_Post_A", pos + new Vector3(-1.1f, 0.55f, 0f), new Vector3(0.10f, 0.55f, 0.10f), c.MatWood, false);
        c.Cylinder(fence, "Fence_Post_B", pos + new Vector3(1.1f, 0.55f, 0f), new Vector3(0.10f, 0.55f, 0.10f), c.MatWood, false);
        c.Cube(fence, "Fence_Rail_Top", pos + new Vector3(0f, 0.9f, 0f), new Vector3(2.4f, 0.10f, 0.10f), c.MatWood, false);
        c.Cube(fence, "Fence_Rail_Bottom", pos + new Vector3(0f, 0.55f, 0f), new Vector3(2.4f, 0.10f, 0.10f), c.MatWood, false);
    }

    public static void Rock(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, Vector3 scale, float rotationY)
    {
        GameObject rock = c.Sphere(parent, "Asset_Rock", pos, scale, c.MatRock, false);
        rock.transform.rotation = Quaternion.Euler(0f, rotationY, -8f);
    }

    public static void Hill(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, Vector3 scale)
    {
        GameObject hill = c.Sphere(parent, "Background_Hill", pos, scale, c.MatGrass, false);
        hill.transform.rotation = Quaternion.identity;
    }

    public static void RoadStone(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float size)
    {
        c.Sphere(parent, "Small_Road_Stone", pos, new Vector3(size, 0.18f, size * 0.75f), c.MatRock, false);
    }

    public static void RoadPost(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cylinder(parent, "Small_Road_Post", pos + new Vector3(0f, 0.55f, 0f), new Vector3(0.12f, 0.55f, 0.12f), c.MatWood, false);
        c.Cube(parent, "Small_Road_Post_Top", pos + new Vector3(0f, 1.15f, 0f), new Vector3(0.35f, 0.12f, 0.35f), c.MatWood, false);
    }
}