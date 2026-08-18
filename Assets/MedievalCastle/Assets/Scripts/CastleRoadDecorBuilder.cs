using UnityEngine;

/// <summary>
/// Дорога, мост, ров и мелкий уличный/природный декор.
/// Это отдельный слой окружения: не лес, не горы, а именно путь игрока и его детали.
/// </summary>
public static class CastleRoadDecorBuilder
{
    public static void BuildRoadAndBridge(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject road = c.Child(parent, "Road_To_Castle_With_Bridge");

        c.Cube(road, "Main_Road_Long_Straight",
            new Vector3(0f, 0.07f, -48f),
            new Vector3(7.8f, 0.06f, 56f),
            c.MatRoad);

        c.Cube(road, "Road_Widening_Near_Gate",
            new Vector3(0f, 0.075f, -21.5f),
            new Vector3(11.5f, 0.055f, 8.5f),
            c.MatRoad);

        c.Cube(road, "Road_Inside_Castle_To_Keep",
            new Vector3(0f, 0.085f, -5.2f),
            new Vector3(5.6f, 0.055f, 19.5f),
            c.MatRoad);

        GameObject sideRoad = c.Cube(road, "Side_Road_To_Dungeon",
            new Vector3(-7.4f, 0.09f, 5.6f),
            new Vector3(9.2f, 0.055f, 3.2f),
            c.MatRoad);
        sideRoad.transform.rotation = Quaternion.Euler(0f, -4f, 0f);

        c.Cube(road, "Wooden_Bridge_Over_Moat",
            new Vector3(0f, 0.19f, -19.2f),
            new Vector3(7.2f, 0.24f, 4.2f),
            c.MatWood);

        for (int i = -3; i <= 3; i++)
        {
            c.Cube(road, "Bridge_Plank_" + i,
                new Vector3(i * 1.05f, 0.33f, -19.2f),
                new Vector3(0.12f, 0.12f, 4.35f),
                c.MatIron,
                false);
        }

        c.Cube(road, "Bridge_Rail_Left",
            new Vector3(-3.9f, 0.75f, -19.2f),
            new Vector3(0.16f, 0.16f, 4.4f),
            c.MatWood,
            false);

        c.Cube(road, "Bridge_Rail_Right",
            new Vector3(3.9f, 0.75f, -19.2f),
            new Vector3(0.16f, 0.16f, 4.4f),
            c.MatWood,
            false);

        for (int i = 0; i < 4; i++)
        {
            float z = -21f + i * 1.2f;
            c.Cylinder(road, "Bridge_Post_Left_" + i,
                new Vector3(-3.9f, 0.55f, z),
                new Vector3(0.10f, 0.55f, 0.10f),
                c.MatWood,
                false);
            c.Cylinder(road, "Bridge_Post_Right_" + i,
                new Vector3(3.9f, 0.55f, z),
                new Vector3(0.10f, 0.55f, 0.10f),
                c.MatWood,
                false);
        }

        for (int i = 0; i < 24; i++)
        {
            float z = -72f + i * 3.8f;
            CastleNatureAssets.RoadStone(c, road, new Vector3(-4.7f + CastleNatureAssets.Hash01(i) * 0.7f, 0.13f, z), 0.32f + CastleNatureAssets.Hash01(i + 20) * 0.32f);
            CastleNatureAssets.RoadStone(c, road, new Vector3(4.7f - CastleNatureAssets.Hash01(i + 10) * 0.7f, 0.13f, z + 0.7f), 0.30f + CastleNatureAssets.Hash01(i + 30) * 0.32f);
        }
    }

    public static void BuildMoat(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject moat = c.Child(parent, "Moat_Animated_Defensive_Ring_V3_Castle_Defense");

        Material waterMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDark
            : c.MatWater;

        Material deepWaterMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDark
            : c.NewMaterial(new Color(0.055f, 0.15f, 0.22f));

        Material bankMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDirt
            : c.NewMaterial(new Color(0.30f, 0.25f, 0.17f));

        Material wetBankMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatDark
            : c.NewMaterial(new Color(0.13f, 0.16f, 0.12f));

        Material innerStoneMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.42f, 0.42f, 0.37f));

        Material foamMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.62f, 0.78f, 0.82f));

        Material reedMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatGrass
            : c.NewMaterial(new Color(0.14f, 0.27f, 0.08f));

        Material currentMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.19f, 0.43f, 0.52f));

        BuildMoatBottomAndBanks(c, moat, bankMat, wetBankMat, deepWaterMat);
        BuildMoatWaterRing(c, moat, waterMat, deepWaterMat);
        BuildInnerStoneLining(c, moat, innerStoneMat, wetBankMat);
        BuildMoatFoamAndCurrent(c, moat, foamMat, currentMat);
        BuildMoatEdgeDetails(c, moat, reedMat, bankMat, innerStoneMat);
        BuildDefensiveMoatDetails(c, moat, innerStoneMat, bankMat, wetBankMat);
    }

    private static void BuildMoatWaterRing(CastleGenerator.CastleContext c, GameObject parent, Material waterMat, Material deepWaterMat)
    {
        GameObject water = c.Child(parent, "Water_Ring_Around_Outer_Walls_Subtle_Flow");

        // Передний ров разбит на две части, чтобы дорога и мост читались чисто.
        WaterSegment(c, water, "Front_Water_Left_Channel",
            new Vector3(-12.0f, 0.044f, c.FrontZ - 4.2f),
            new Vector3(15.6f, 0.040f, 4.25f),
            waterMat,
            new Vector2(0.115f, 0.025f),
            new Color(0.08f, 0.31f, 0.42f, 0.72f),
            0.030f,
            0.11f);

        WaterSegment(c, water, "Front_Water_Right_Channel",
            new Vector3(12.0f, 0.044f, c.FrontZ - 4.2f),
            new Vector3(15.6f, 0.040f, 4.25f),
            waterMat,
            new Vector2(0.115f, 0.025f),
            new Color(0.08f, 0.31f, 0.42f, 0.72f),
            0.030f,
            0.11f);

        // Вода под мостом темнее: так мост выглядит выше, а ров глубже.
        WaterSegment(c, water, "Front_Water_Dark_Under_Bridge",
            new Vector3(0f, 0.033f, c.FrontZ - 4.2f),
            new Vector3(6.9f, 0.030f, 3.85f),
            deepWaterMat,
            new Vector2(0.08f, 0.045f),
            new Color(0.045f, 0.17f, 0.24f, 0.76f),
            0.018f,
            0.07f);

        WaterSegment(c, water, "Back_Water_Channel",
            new Vector3(0f, 0.044f, c.BackZ + 4.2f),
            new Vector3(43.5f, 0.040f, 4.25f),
            waterMat,
            new Vector2(-0.105f, 0.020f),
            new Color(0.075f, 0.29f, 0.40f, 0.70f),
            0.025f,
            0.09f);

        WaterSegment(c, water, "Left_Water_Channel",
            new Vector3(c.LeftX - 4.2f, 0.043f, 0f),
            new Vector3(4.25f, 0.040f, 36.5f),
            waterMat,
            new Vector2(0.025f, 0.125f),
            new Color(0.075f, 0.30f, 0.42f, 0.72f),
            0.027f,
            0.10f);

        WaterSegment(c, water, "Right_Water_Channel",
            new Vector3(c.RightX + 4.2f, 0.043f, 0f),
            new Vector3(4.25f, 0.040f, 36.5f),
            waterMat,
            new Vector2(-0.025f, -0.125f),
            new Color(0.075f, 0.30f, 0.42f, 0.72f),
            0.027f,
            0.10f);

        // Тёмные полосы у стен и по внешнему краю дают ощущение глубины, но не выглядят как дорожная разметка.
        c.Cube(water, "Inner_Depth_Shadow_Front", new Vector3(0f, 0.066f, c.FrontZ - 2.10f), new Vector3(39.0f, 0.016f, 0.34f), deepWaterMat, false);
        c.Cube(water, "Inner_Depth_Shadow_Back", new Vector3(0f, 0.066f, c.BackZ + 2.10f), new Vector3(39.0f, 0.016f, 0.34f), deepWaterMat, false);
        c.Cube(water, "Inner_Depth_Shadow_Left", new Vector3(c.LeftX - 2.10f, 0.066f, 0f), new Vector3(0.34f, 0.016f, 32.5f), deepWaterMat, false);
        c.Cube(water, "Inner_Depth_Shadow_Right", new Vector3(c.RightX + 2.10f, 0.066f, 0f), new Vector3(0.34f, 0.016f, 32.5f), deepWaterMat, false);

        c.Cube(water, "Outer_Depth_Shadow_Front", new Vector3(0f, 0.058f, c.FrontZ - 6.05f), new Vector3(42.0f, 0.012f, 0.22f), deepWaterMat, false);
        c.Cube(water, "Outer_Depth_Shadow_Back", new Vector3(0f, 0.058f, c.BackZ + 6.05f), new Vector3(42.0f, 0.012f, 0.22f), deepWaterMat, false);
    }

    private static GameObject WaterSegment(
        CastleGenerator.CastleContext c,
        GameObject parent,
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        Vector2 flow,
        Color color,
        float rippleStrength,
        float glitterIntensity)
    {
        GameObject segment = c.Cube(parent, name, position, scale, material, false);

        AnimatedWater animated = segment.AddComponent<AnimatedWater>();
        animated.waterColor = color;
        animated.emissionTint = new Color(0.03f, 0.11f, 0.16f);
        animated.scrollSpeed = flow;
        animated.rippleScale = Mathf.Max(1.15f, scale.x * 0.070f + scale.z * 0.030f);
        animated.rippleStrength = rippleStrength;
        animated.glitterIntensity = glitterIntensity;
        animated.glitterSpeed = 0.70f;
        animated.noiseContrast = 0.23f;
        animated.noiseBrightness = 0.45f;

        return segment;
    }

    private static void BuildMoatBottomAndBanks(CastleGenerator.CastleContext c, GameObject parent, Material bankMat, Material wetBankMat, Material deepWaterMat)
    {
        GameObject banks = c.Child(parent, "Sunken_Earth_Banks_And_Dark_Bottom");

        // Дно под прозрачной водой: ров выглядит глубже и не похож на плоский бассейн.
        c.Cube(banks, "Moat_Dark_Bottom_Front_Left", new Vector3(-12.0f, 0.014f, c.FrontZ - 4.2f), new Vector3(15.9f, 0.020f, 4.45f), deepWaterMat, false);
        c.Cube(banks, "Moat_Dark_Bottom_Front_Right", new Vector3(12.0f, 0.014f, c.FrontZ - 4.2f), new Vector3(15.9f, 0.020f, 4.45f), deepWaterMat, false);
        c.Cube(banks, "Moat_Dark_Bottom_Back", new Vector3(0f, 0.014f, c.BackZ + 4.2f), new Vector3(43.8f, 0.020f, 4.45f), deepWaterMat, false);
        c.Cube(banks, "Moat_Dark_Bottom_Left", new Vector3(c.LeftX - 4.2f, 0.014f, 0f), new Vector3(4.45f, 0.020f, 36.7f), deepWaterMat, false);
        c.Cube(banks, "Moat_Dark_Bottom_Right", new Vector3(c.RightX + 4.2f, 0.014f, 0f), new Vector3(4.45f, 0.020f, 36.7f), deepWaterMat, false);

        // Внутренний мокрый берег прямо у стен.
        c.Cube(banks, "Inner_Wet_Bank_Front_Left", new Vector3(-11.8f, 0.088f, c.FrontZ - 1.82f), new Vector3(15.7f, 0.070f, 0.52f), wetBankMat, false);
        c.Cube(banks, "Inner_Wet_Bank_Front_Right", new Vector3(11.8f, 0.088f, c.FrontZ - 1.82f), new Vector3(15.7f, 0.070f, 0.52f), wetBankMat, false);
        c.Cube(banks, "Inner_Wet_Bank_Back", new Vector3(0f, 0.088f, c.BackZ + 1.82f), new Vector3(39.2f, 0.070f, 0.52f), wetBankMat, false);
        c.Cube(banks, "Inner_Wet_Bank_Left", new Vector3(c.LeftX - 1.82f, 0.088f, 0f), new Vector3(0.52f, 0.070f, 32.5f), wetBankMat, false);
        c.Cube(banks, "Inner_Wet_Bank_Right", new Vector3(c.RightX + 1.82f, 0.088f, 0f), new Vector3(0.52f, 0.070f, 32.5f), wetBankMat, false);

        // Внешняя земляная кромка. Она чуть шире, чтобы ров ощущался вырытым в земле.
        c.Cube(banks, "Outer_Earth_Bank_Front", new Vector3(0f, 0.100f, c.FrontZ - 6.72f), new Vector3(43.6f, 0.090f, 0.85f), bankMat, false);
        c.Cube(banks, "Outer_Earth_Bank_Back", new Vector3(0f, 0.100f, c.BackZ + 6.72f), new Vector3(43.6f, 0.090f, 0.85f), bankMat, false);
        c.Cube(banks, "Outer_Earth_Bank_Left", new Vector3(c.LeftX - 6.72f, 0.100f, 0f), new Vector3(0.85f, 0.090f, 38.8f), bankMat, false);
        c.Cube(banks, "Outer_Earth_Bank_Right", new Vector3(c.RightX + 6.72f, 0.100f, 0f), new Vector3(0.85f, 0.090f, 38.8f), bankMat, false);

        // Неровные дополнительные участки земли — ров уже не выглядит идеально прямоугольной ванной.
        for (int i = 0; i < 10; i++)
        {
            float x = -19.0f + i * 4.2f;
            if (Mathf.Abs(x) < 5.5f) continue;

            c.Cube(banks, "Irregular_Front_Bank_Lump_" + i,
                new Vector3(x + Mathf.Sin(i * 1.3f) * 0.45f, 0.118f, c.FrontZ - 6.12f),
                new Vector3(1.55f, 0.055f, 0.58f),
                bankMat,
                false).transform.rotation = Quaternion.Euler(0f, Mathf.Sin(i) * 7f, 0f);
        }

        // Углы закрывают стыки воды и берегов, чтобы контур читался как единый ров.
        c.Cube(banks, "Earth_Corner_Front_Left", new Vector3(c.LeftX - 4.35f, 0.095f, c.FrontZ - 4.35f), new Vector3(5.15f, 0.075f, 5.15f), bankMat, false);
        c.Cube(banks, "Earth_Corner_Front_Right", new Vector3(c.RightX + 4.35f, 0.095f, c.FrontZ - 4.35f), new Vector3(5.15f, 0.075f, 5.15f), bankMat, false);
        c.Cube(banks, "Earth_Corner_Back_Left", new Vector3(c.LeftX - 4.35f, 0.095f, c.BackZ + 4.35f), new Vector3(5.15f, 0.075f, 5.15f), bankMat, false);
        c.Cube(banks, "Earth_Corner_Back_Right", new Vector3(c.RightX + 4.35f, 0.095f, c.BackZ + 4.35f), new Vector3(5.15f, 0.075f, 5.15f), bankMat, false);
    }

    private static void BuildInnerStoneLining(CastleGenerator.CastleContext c, GameObject parent, Material stoneMat, Material wetBankMat)
    {
        GameObject lining = c.Child(parent, "Inner_Stone_Lining_By_Castle_Walls");

        // Каменная облицовка у стен связывает воду именно с крепостью, а не с обычным ручьём.
        c.Cube(lining, "Stone_Lining_Front_Left", new Vector3(-11.8f, 0.145f, c.FrontZ - 1.38f), new Vector3(15.6f, 0.16f, 0.18f), stoneMat, false);
        c.Cube(lining, "Stone_Lining_Front_Right", new Vector3(11.8f, 0.145f, c.FrontZ - 1.38f), new Vector3(15.6f, 0.16f, 0.18f), stoneMat, false);
        c.Cube(lining, "Stone_Lining_Back", new Vector3(0f, 0.145f, c.BackZ + 1.38f), new Vector3(39.0f, 0.16f, 0.18f), stoneMat, false);
        c.Cube(lining, "Stone_Lining_Left", new Vector3(c.LeftX - 1.38f, 0.145f, 0f), new Vector3(0.18f, 0.16f, 32.5f), stoneMat, false);
        c.Cube(lining, "Stone_Lining_Right", new Vector3(c.RightX + 1.38f, 0.145f, 0f), new Vector3(0.18f, 0.16f, 32.5f), stoneMat, false);

        // Мелкие тёмные пятна сырости под стенами.
        for (int i = 0; i < 12; i++)
        {
            float x = -16.5f + i * 3.0f;
            if (Mathf.Abs(x) < 4.8f) continue;

            c.Cube(lining, "Moisture_Stain_Front_" + i,
                new Vector3(x, 0.172f, c.FrontZ - 1.23f),
                new Vector3(0.65f, 0.045f, 0.035f),
                wetBankMat,
                false);
        }
    }

    private static void BuildMoatFoamAndCurrent(CastleGenerator.CastleContext c, GameObject parent, Material foamMat, Material currentMat)
    {
        GameObject details = c.Child(parent, "Subtle_Foam_Short_Currents_And_Ripples");

        // Короткие светлые штрихи, не длинные белые полосы: вода больше не похожа на дорогу.
        for (int i = 0; i < 9; i++)
        {
            float x = -17f + i * 4.2f;
            if (Mathf.Abs(x) < 5.0f) continue;

            GameObject current = c.Cube(details, "Front_Short_Current_" + i,
                new Vector3(x, 0.092f, c.FrontZ - 4.15f + Mathf.Sin(i * 1.7f) * 0.55f),
                new Vector3(0.62f + (i % 3) * 0.16f, 0.010f, 0.045f),
                currentMat,
                false);
            current.transform.rotation = Quaternion.Euler(0f, -8f + Mathf.Sin(i * 0.9f) * 16f, 0f);
        }

        for (int i = 0; i < 8; i++)
        {
            float z = -13.0f + i * 3.6f;

            GameObject left = c.Cube(details, "Left_Short_Current_" + i,
                new Vector3(c.LeftX - 4.25f + Mathf.Sin(i * 1.2f) * 0.35f, 0.092f, z),
                new Vector3(0.045f, 0.010f, 0.70f),
                currentMat,
                false);
            left.transform.rotation = Quaternion.Euler(0f, -8f + Mathf.Sin(i) * 10f, 0f);

            GameObject right = c.Cube(details, "Right_Short_Current_" + i,
                new Vector3(c.RightX + 4.25f + Mathf.Sin(i * 1.4f) * 0.35f, 0.092f, z),
                new Vector3(0.045f, 0.010f, 0.70f),
                currentMat,
                false);
            right.transform.rotation = Quaternion.Euler(0f, 8f + Mathf.Sin(i) * 10f, 0f);
        }

        // Пена только у моста и у углов, чтобы не было визуального мусора.
        Vector3[] foamSpots =
        {
            new Vector3(-4.15f, 0.118f, c.FrontZ - 3.05f),
            new Vector3(4.15f, 0.118f, c.FrontZ - 3.05f),
            new Vector3(-5.85f, 0.118f, c.FrontZ - 4.82f),
            new Vector3(5.85f, 0.118f, c.FrontZ - 4.82f),
            new Vector3(c.LeftX - 2.55f, 0.118f, c.FrontZ - 1.25f),
            new Vector3(c.RightX + 2.55f, 0.118f, c.FrontZ - 1.25f),
            new Vector3(c.LeftX - 2.55f, 0.118f, c.BackZ + 1.25f),
            new Vector3(c.RightX + 2.55f, 0.118f, c.BackZ + 1.25f),
        };

        for (int i = 0; i < foamSpots.Length; i++)
        {
            GameObject foam = c.Cube(details, "Small_Foam_Patch_" + i, foamSpots[i], new Vector3(0.80f, 0.012f, 0.095f), foamMat, false);
            foam.transform.rotation = Quaternion.Euler(0f, i * 21f, 0f);
        }

        for (int i = 0; i < 18; i++)
        {
            float t = i / 17f;
            float x = Mathf.Lerp(-17.5f, 17.5f, t);
            if (Mathf.Abs(x) < 5.2f) continue;

            c.Sphere(details, "Tiny_Surface_Bubble_" + i,
                new Vector3(x, 0.108f, c.FrontZ - 2.6f + Mathf.Sin(i * 2.1f) * 0.18f),
                new Vector3(0.040f, 0.014f, 0.040f),
                foamMat,
                false);
        }
    }

    private static void BuildMoatEdgeDetails(CastleGenerator.CastleContext c, GameObject parent, Material reedMat, Material bankMat, Material stoneMat)
    {
        GameObject edge = c.Child(parent, "Grouped_Reeds_Rocks_And_Natural_Bank_Details");

        // Камыш теперь группами, а не ровной частой линией.
        for (int i = 0; i < 12; i++)
        {
            float x = -18.5f + i * 3.35f;
            if (Mathf.Abs(x) < 5.6f) continue;

            if (i % 2 == 0)
                ReedCluster(c, edge, new Vector3(x, 0.10f, c.FrontZ - 6.00f), reedMat, i);

            if (i % 3 != 0)
                ReedCluster(c, edge, new Vector3(x + Mathf.Sin(i) * 0.35f, 0.10f, c.BackZ + 6.00f), reedMat, i + 100);
        }

        for (int i = 0; i < 8; i++)
        {
            float z = -13.5f + i * 3.70f;

            if (i % 2 == 0)
                ReedCluster(c, edge, new Vector3(c.LeftX - 6.00f, 0.10f, z), reedMat, i + 200);
            else
                ReedCluster(c, edge, new Vector3(c.RightX + 6.00f, 0.10f, z), reedMat, i + 300);
        }

        // Камни по берегу, особенно у моста и углов.
        for (int i = 0; i < 18; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float z = -17.0f + CastleNatureAssets.Hash01(i + 900) * 34f;
            float x = side < 0 ? c.LeftX - 6.15f + CastleNatureAssets.Hash01(i + 910) * 1.10f : c.RightX + 6.15f - CastleNatureAssets.Hash01(i + 920) * 1.10f;
            float s = 0.25f + CastleNatureAssets.Hash01(i + 930) * 0.24f;

            c.Sphere(edge, "Moat_Bank_Rock_" + i,
                new Vector3(x, 0.15f, z),
                new Vector3(s * 1.55f, s * 0.52f, s),
                i % 3 == 0 ? stoneMat : bankMat,
                false);
        }

        Vector3[] heroRocks =
        {
            new Vector3(-5.55f, 0.18f, c.FrontZ - 5.65f),
            new Vector3(5.70f, 0.18f, c.FrontZ - 5.50f),
            new Vector3(-6.35f, 0.16f, c.FrontZ - 2.45f),
            new Vector3(6.45f, 0.16f, c.FrontZ - 2.55f),
        };

        for (int i = 0; i < heroRocks.Length; i++)
        {
            c.Sphere(edge, "Bridge_Area_Wet_Rock_" + i,
                heroRocks[i],
                new Vector3(0.55f + i * 0.06f, 0.16f, 0.34f + i * 0.03f),
                stoneMat,
                false);
        }
    }

    private static void BuildDefensiveMoatDetails(CastleGenerator.CastleContext c, GameObject parent, Material stoneMat, Material bankMat, Material wetMat)
    {
        GameObject defense = c.Child(parent, "Defensive_Details_Stakes_Drains_And_Bridge_Accents");

        // Деревянные сваи/колья по внешней стороне — ров становится оборонительным, а не декоративным каналом.
        for (int i = 0; i < 14; i++)
        {
            float x = -18.5f + i * 2.85f;
            if (Mathf.Abs(x) < 5.8f) continue;
            WoodenStake(c, defense, new Vector3(x, 0.32f, c.FrontZ - 5.75f), i * 9f);
        }

        for (int i = 0; i < 9; i++)
        {
            float z = -13.0f + i * 3.15f;
            WoodenStake(c, defense, new Vector3(c.LeftX - 5.75f, 0.32f, z), -8f + i * 4f);
            WoodenStake(c, defense, new Vector3(c.RightX + 5.75f, 0.32f, z + 0.6f), 8f - i * 3f);
        }

        // Водостоки из стены — маленькая деталь, которая сильно привязывает воду к замку.
        DrainSpout(c, defense, new Vector3(-13.2f, 2.00f, c.FrontZ - 0.95f), true);
        DrainSpout(c, defense, new Vector3(13.2f, 2.05f, c.FrontZ - 0.95f), true);
        DrainSpout(c, defense, new Vector3(c.LeftX - 0.95f, 1.90f, -7.0f), false);
        DrainSpout(c, defense, new Vector3(c.RightX + 0.95f, 1.90f, 7.0f), false);

        // Под мостом темнее и грязнее — главный вход выглядит крепче.
        c.Cube(defense, "Bridge_Dark_Moisture_Shadow", new Vector3(0f, 0.132f, c.FrontZ - 2.25f), new Vector3(7.6f, 0.035f, 0.34f), wetMat, false);
        c.Cube(defense, "Bridge_Stone_Threshold_Left", new Vector3(-4.25f, 0.26f, c.FrontZ - 1.95f), new Vector3(0.40f, 0.42f, 1.35f), stoneMat, false);
        c.Cube(defense, "Bridge_Stone_Threshold_Right", new Vector3(4.25f, 0.26f, c.FrontZ - 1.95f), new Vector3(0.40f, 0.42f, 1.35f), stoneMat, false);
    }

    private static void WoodenStake(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY)
    {
        GameObject stake = c.Child(parent, "Outer_Wooden_Defense_Stake");
        stake.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cylinder(stake, "Stake_Post", pos, new Vector3(0.055f, 0.42f, 0.055f), c.MatWood, false);
        c.Cube(stake, "Stake_Tip", pos + new Vector3(0f, 0.45f, 0f), new Vector3(0.13f, 0.17f, 0.13f), c.MatWood, false)
            .transform.rotation = Quaternion.Euler(0f, rotY, 45f);
    }

    private static void DrainSpout(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool frontWall)
    {
        Vector3 scale = frontWall ? new Vector3(0.18f, 0.18f, 1.15f) : new Vector3(1.15f, 0.18f, 0.18f);
        Vector3 waterDropOffset = frontWall ? new Vector3(0f, -0.85f, -0.48f) : new Vector3(pos.x < 0f ? -0.48f : 0.48f, -0.85f, 0f);

        c.Cube(parent, "Stone_Drain_Spout", pos, scale, c.MatStoneLight, false);
        c.Cube(parent, "Small_Drain_Water_Streak", pos + waterDropOffset, frontWall ? new Vector3(0.08f, 0.72f, 0.04f) : new Vector3(0.04f, 0.72f, 0.08f), c.MatWater, false);
    }

    private static void ReedCluster(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, Material mat, int seed)
    {
        GameObject cluster = c.Child(parent, "Reed_Cluster_Grouped_" + seed);

        int count = 3 + Mathf.Abs(seed) % 4;
        for (int i = 0; i < count; i++)
        {
            float ox = (CastleNatureAssets.Hash01(seed * 17 + i * 11) - 0.5f) * 0.54f;
            float oz = (CastleNatureAssets.Hash01(seed * 19 + i * 13) - 0.5f) * 0.54f;
            float h = 0.42f + CastleNatureAssets.Hash01(seed * 23 + i * 7) * 0.42f;

            GameObject reed = c.Cylinder(cluster, "Reed_Stem_" + i,
                basePos + new Vector3(ox, h * 0.50f, oz),
                new Vector3(0.023f, h * 0.50f, 0.023f),
                mat,
                false);

            reed.transform.rotation = Quaternion.Euler(
                CastleNatureAssets.Hash01(seed + i + 30) * 9f - 4.5f,
                CastleNatureAssets.Hash01(seed + i + 40) * 360f,
                CastleNatureAssets.Hash01(seed + i + 50) * 12f - 6f);
        }
    }

    public static void BuildNatureDecor(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject nature = c.Child(parent, "Nature_Decor_Rocks_Bushes_Stumps");

        CastleNatureAssets.Hill(c, nature, new Vector3(-42f, 0.05f, 34f), new Vector3(18f, 2.2f, 7f));
        CastleNatureAssets.Hill(c, nature, new Vector3(43f, 0.06f, 35f), new Vector3(20f, 2.4f, 8f));
        CastleNatureAssets.Hill(c, nature, new Vector3(-46f, 0.05f, -24f), new Vector3(13f, 1.7f, 6f));
        CastleNatureAssets.Hill(c, nature, new Vector3(47f, 0.05f, -24f), new Vector3(14f, 1.7f, 6f));
        CastleNatureAssets.Hill(c, nature, new Vector3(0f, 0.03f, 52f), new Vector3(25f, 2.1f, 6f));

        CastleNatureAssets.RockCluster(c, nature, new Vector3(-36f, 0.25f, -38f), 1.0f);
        CastleNatureAssets.RockCluster(c, nature, new Vector3(37f, 0.25f, -37f), 0.95f);
        CastleNatureAssets.RockCluster(c, nature, new Vector3(-42f, 0.25f, 24f), 1.1f);
        CastleNatureAssets.RockCluster(c, nature, new Vector3(43f, 0.25f, 23f), 1.05f);

        CastleNatureAssets.Rock(c, nature, new Vector3(-12f, 0.35f, -29f), new Vector3(1.8f, 0.7f, 1.2f), 12f);
        CastleNatureAssets.Rock(c, nature, new Vector3(13f, 0.35f, -31f), new Vector3(2.2f, 0.8f, 1.4f), -18f);
        CastleNatureAssets.Rock(c, nature, new Vector3(-23f, 0.35f, -18f), new Vector3(2.0f, 0.9f, 1.5f), 30f);
        CastleNatureAssets.Rock(c, nature, new Vector3(24f, 0.35f, -17f), new Vector3(2.4f, 0.8f, 1.6f), -25f);

        for (int i = 0; i < 34; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float z = -56f + i * 2.5f;
            float x = side * (7.6f + CastleNatureAssets.Hash01(i + 600) * 4.2f);
            CastleNatureAssets.MixedBush(c, nature, new Vector3(x, 0.25f, z), 0.55f + CastleNatureAssets.Hash01(i + 610) * 0.55f, i);
        }

        CastleNatureAssets.Stump(c, nature, new Vector3(-17f, 0f, -36f), 0.8f);
        CastleNatureAssets.Stump(c, nature, new Vector3(17f, 0f, -34f), 0.7f);
        CastleNatureAssets.Stump(c, nature, new Vector3(-31f, 0f, 2f), 0.75f);
        CastleNatureAssets.Stump(c, nature, new Vector3(32f, 0f, 5f), 0.75f);

        CastleNatureAssets.FallenLog(c, nature, new Vector3(-23f, 0.35f, 15f), 1.1f, -12f);
        CastleNatureAssets.FallenLog(c, nature, new Vector3(24f, 0.35f, 14f), 1.0f, 18f);
    }

    public static void BuildRoadDetails(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject details = c.Child(parent, "Road_Details_Posts_Fences_Signs");

        for (int i = 0; i < 11; i++)
        {
            float z = -61f + i * 5.0f;
            CastleNatureAssets.RoadPost(c, details, new Vector3(-5.6f, 0f, z));
            CastleNatureAssets.RoadPost(c, details, new Vector3(5.6f, 0f, z + 1.2f));
        }

        CastleNatureAssets.SignPost(c, details, new Vector3(-6.9f, 0f, -34f), "Left_Sign");
        CastleNatureAssets.SignPost(c, details, new Vector3(6.9f, 0f, -26f), "Right_Sign");

        CastleNatureAssets.FenceSegment(c, details, new Vector3(-7.2f, 0f, -46f), 0f);
        CastleNatureAssets.FenceSegment(c, details, new Vector3(7.2f, 0f, -43f), 0f);
        CastleNatureAssets.FenceSegment(c, details, new Vector3(-7.2f, 0f, -28f), 0f);
        CastleNatureAssets.FenceSegment(c, details, new Vector3(7.2f, 0f, -19f), 0f);
    }
}
