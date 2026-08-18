using UnityEngine;

/// <summary>
/// Castle Tower Detail Builder V12.
/// Шаг: реальная крепостная форма башен по референсу:
/// тяжёлое основание, каменные курсы, короткие машикули, приземлённые зубцы.
/// Без "летающего венка" и хаотичных накладок.
/// </summary>
public static class CastleTowerDetailBuilder
{
    public static void Build(CastleGenerator.CastleContext c)
    {
        GameObject parent = c.Child(c.Root, "04_CASTLE_TOWER_REFERENCE_REBUILD_V12");

        Material darkStone = c.NewMaterial(new Color(0.25f, 0.25f, 0.24f));
        Material shadowStone = c.NewMaterial(new Color(0.32f, 0.32f, 0.30f));
        Material lightEdge = c.NewMaterial(new Color(0.67f, 0.67f, 0.61f));
        Material roofDark = c.NewMaterial(new Color(0.15f, 0.09f, 0.065f));

        BuildRoundGateTower(c, parent, new Vector3(-4.6f, 0f, c.FrontZ), "Gate_Left_Reference_Tower_V12", -1f, darkStone, shadowStone, lightEdge, roofDark);
        BuildRoundGateTower(c, parent, new Vector3(4.6f, 0f, c.FrontZ), "Gate_Right_Reference_Tower_V12", 1f, darkStone, shadowStone, lightEdge, roofDark);

        BuildRoundCornerTower(c, parent, new Vector3(c.LeftX, 0f, c.FrontZ), "Corner_Front_Left_Reference_Tower_V12", 225f, darkStone, shadowStone, lightEdge, roofDark);
        BuildRoundCornerTower(c, parent, new Vector3(c.RightX, 0f, c.FrontZ), "Corner_Front_Right_Reference_Tower_V12", 135f, darkStone, shadowStone, lightEdge, roofDark);
        BuildRoundCornerTower(c, parent, new Vector3(c.LeftX, 0f, c.BackZ), "Corner_Back_Left_Reference_Tower_V12", 315f, darkStone, shadowStone, lightEdge, roofDark);
        BuildRoundCornerTower(c, parent, new Vector3(c.RightX, 0f, c.BackZ), "Corner_Back_Right_Reference_Tower_V12", 45f, darkStone, shadowStone, lightEdge, roofDark);

        BuildWallMassPolish(c, parent, darkStone, shadowStone);
    }

    private static void BuildRoundGateTower(
        CastleGenerator.CastleContext c,
        GameObject parent,
        Vector3 basePos,
        string name,
        float sideSign,
        Material darkStone,
        Material shadowStone,
        Material lightEdge,
        Material roofDark)
    {
        GameObject tower = c.Child(parent, name);

        // Тяжёлые нижние опоры у основания.
        RadialPier(c, tower, "Front_Lower_Buttress", basePos, 180f, 2.55f, 2.25f, new Vector3(0.64f, 2.75f, 0.48f), c.MatStoneLight);
        RadialPier(c, tower, "Outer_Side_Buttress", basePos, sideSign > 0f ? 270f : 90f, 2.50f, 2.10f, new Vector3(0.54f, 2.45f, 0.44f), shadowStone);

        // Имитация кладки: несколько рядов каменных пластин на теле башни.
        BuildStoneCourses(c, tower, basePos, 2.66f, 2.65f, 6.55f, 7, 10, lightEdge);

        // Глубокие бойницы с рамкой.
        RadialWindow(c, tower, "Front_Tall_Arrow_Slit", basePos, 180f, 2.70f, 4.55f, c.MatStoneLight, 0.42f, 1.72f);
        RadialWindow(c, tower, "Front_Upper_Arrow_Slit", basePos, 180f, 2.72f, 6.12f, c.MatStoneLight, 0.34f, 1.10f);
        RadialWindow(c, tower, "Side_Arrow_Slit", basePos, sideSign > 0f ? 270f : 90f, 2.62f, 4.05f, c.MatStoneLight, 0.32f, 1.15f);

        // Короткие машикули прямо под боевым верхом.
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36f;
            RadialBlock(c, tower, "Attached_Machicolation_" + i, basePos, angle, 2.70f, 7.56f,
                new Vector3(0.36f, 0.34f, 0.34f), darkStone, false);
        }

        // Низкие приземлённые зубцы — не летают над башней.
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36f + 18f;
            RadialBlock(c, tower, "Grounded_Battlement_" + i, basePos, angle, 2.47f, 8.62f,
                new Vector3(0.58f, 0.58f, 0.44f), c.MatStoneLight, false);
        }

        // Небольшой центральный тёмный шпиль, без перегруза.
        c.Cylinder(tower, "Small_Roof_Base", basePos + new Vector3(0f, 9.10f, 0f), new Vector3(0.92f, 0.10f, 0.92f), roofDark, false);
        c.Cylinder(tower, "Small_Roof_Tip", basePos + new Vector3(0f, 9.42f, 0f), new Vector3(0.32f, 0.25f, 0.32f), roofDark, false);
    }

    private static void BuildRoundCornerTower(
        CastleGenerator.CastleContext c,
        GameObject parent,
        Vector3 basePos,
        string name,
        float outwardAngle,
        Material darkStone,
        Material shadowStone,
        Material lightEdge,
        Material roofDark)
    {
        GameObject tower = c.Child(parent, name);

        // Наружная сторона угловой башни получает 3 тяжёлые опоры.
        RadialPier(c, tower, "Outer_Center_Lower_Buttress", basePos, outwardAngle, 2.38f, 2.20f, new Vector3(0.58f, 2.60f, 0.44f), c.MatStoneLight);
        RadialPier(c, tower, "Outer_Left_Lower_Buttress", basePos, outwardAngle - 42f, 2.32f, 2.00f, new Vector3(0.46f, 2.22f, 0.38f), shadowStone);
        RadialPier(c, tower, "Outer_Right_Lower_Buttress", basePos, outwardAngle + 42f, 2.32f, 2.00f, new Vector3(0.46f, 2.22f, 0.38f), shadowStone);

        BuildStoneCourses(c, tower, basePos, 2.48f, 2.62f, 6.25f, 6, 9, lightEdge);

        RadialWindow(c, tower, "Outer_Tall_Arrow_Slit", basePos, outwardAngle, 2.52f, 4.45f, c.MatStoneLight, 0.34f, 1.42f);
        RadialWindow(c, tower, "Outer_Upper_Arrow_Slit", basePos, outwardAngle, 2.54f, 5.86f, c.MatStoneLight, 0.28f, 1.00f);

        for (int i = 0; i < 9; i++)
        {
            float angle = i * 40f;
            RadialBlock(c, tower, "Attached_Machicolation_" + i, basePos, angle, 2.50f, 7.30f,
                new Vector3(0.32f, 0.30f, 0.30f), darkStone, false);
        }

        for (int i = 0; i < 9; i++)
        {
            float angle = i * 40f + 20f;
            RadialBlock(c, tower, "Grounded_Battlement_" + i, basePos, angle, 2.30f, 8.38f,
                new Vector3(0.52f, 0.52f, 0.40f), c.MatStoneLight, false);
        }

        c.Cylinder(tower, "Small_Roof_Base", basePos + new Vector3(0f, 8.82f, 0f), new Vector3(0.78f, 0.09f, 0.78f), roofDark, false);
        c.Cylinder(tower, "Small_Roof_Tip", basePos + new Vector3(0f, 9.10f, 0f), new Vector3(0.26f, 0.22f, 0.26f), roofDark, false);
    }

    private static void BuildWallMassPolish(CastleGenerator.CastleContext c, GameObject parent, Material darkStone, Material shadowStone)
    {
        GameObject walls = c.Child(parent, "Wall_Mass_Polish_V12");

        // Утолщаем сопряжение башен и стен, чтобы башни не выглядели приклеенными.
        Joint(c, walls, "Front_Left_Tower_Stone_Shoulder", new Vector3(c.LeftX + 1.0f, 1.30f, c.FrontZ + 2.82f), new Vector3(0.78f, 1.55f, 5.20f), darkStone);
        Joint(c, walls, "Front_Left_Tower_Wall_Shoulder", new Vector3(c.LeftX + 2.82f, 1.30f, c.FrontZ + 1.0f), new Vector3(5.20f, 1.55f, 0.78f), darkStone);

        Joint(c, walls, "Front_Right_Tower_Stone_Shoulder", new Vector3(c.RightX - 1.0f, 1.30f, c.FrontZ + 2.82f), new Vector3(0.78f, 1.55f, 5.20f), darkStone);
        Joint(c, walls, "Front_Right_Tower_Wall_Shoulder", new Vector3(c.RightX - 2.82f, 1.30f, c.FrontZ + 1.0f), new Vector3(5.20f, 1.55f, 0.78f), darkStone);

        // Длинная тёмная линия под боевым ярусом — стены становятся визуально тяжелее.
        c.Cube(walls, "Front_Left_Heavy_Shadow_Course", new Vector3(-11.5f, 5.78f, c.FrontZ - 0.93f), new Vector3(11.8f, 0.24f, 0.28f), shadowStone, false);
        c.Cube(walls, "Front_Right_Heavy_Shadow_Course", new Vector3(11.5f, 5.78f, c.FrontZ - 0.93f), new Vector3(11.8f, 0.24f, 0.28f), shadowStone, false);
        c.Cube(walls, "Left_Heavy_Shadow_Course", new Vector3(c.LeftX - 0.93f, 5.78f, 0f), new Vector3(0.28f, 0.24f, 31.2f), shadowStone, false);
        c.Cube(walls, "Right_Heavy_Shadow_Course", new Vector3(c.RightX + 0.93f, 5.78f, 0f), new Vector3(0.28f, 0.24f, 31.2f), shadowStone, false);
    }

    private static void BuildStoneCourses(
        CastleGenerator.CastleContext c,
        GameObject parent,
        Vector3 basePos,
        float radius,
        float startY,
        float endY,
        int rows,
        int blocksPerRow,
        Material material)
    {
        for (int r = 0; r < rows; r++)
        {
            float y = Mathf.Lerp(startY, endY, rows <= 1 ? 0f : r / (float)(rows - 1));
            float offset = (r % 2) * (180f / blocksPerRow);

            for (int i = 0; i < blocksPerRow; i++)
            {
                float angle = i * (360f / blocksPerRow) + offset;
                RadialBlock(c, parent, "Stone_Course_" + r + "_" + i, basePos, angle, radius, y,
                    new Vector3(0.34f, 0.075f, 0.045f), material, false);
            }
        }
    }

    private static void Joint(CastleGenerator.CastleContext c, GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        c.Cube(parent, name + "_Mass", pos, scale, mat, false);
        c.Cube(parent, name + "_Cap", pos + Vector3.up * 0.92f, new Vector3(scale.x + 0.15f, 0.22f, scale.z + 0.15f), c.MatStoneLight, false);
    }

    private static void RadialPier(
        CastleGenerator.CastleContext c,
        GameObject parent,
        string name,
        Vector3 basePos,
        float angleDeg,
        float radius,
        float y,
        Vector3 scale,
        Material material)
    {
        RadialBlock(c, parent, name + "_Foot", basePos, angleDeg, radius, 1.10f,
            new Vector3(scale.x + 0.20f, 0.36f, scale.z + 0.18f), c.MatStone, false);

        RadialBlock(c, parent, name + "_Body", basePos, angleDeg, radius, y, scale, material, false);

        RadialBlock(c, parent, name + "_Cap", basePos, angleDeg, radius, y + scale.y * 0.50f,
            new Vector3(scale.x + 0.12f, 0.16f, scale.z + 0.12f), c.MatStoneLight, false);
    }

    private static void RadialWindow(
        CastleGenerator.CastleContext c,
        GameObject parent,
        string name,
        Vector3 basePos,
        float angleDeg,
        float radius,
        float y,
        Material frameMaterial,
        float width,
        float height)
    {
        RadialBlock(c, parent, name + "_Frame", basePos, angleDeg, radius + 0.015f, y,
            new Vector3(width, height, 0.07f), frameMaterial, false);

        RadialBlock(c, parent, name + "_Dark", basePos, angleDeg, radius + 0.05f, y,
            new Vector3(width * 0.50f, height * 0.78f, 0.05f), c.MatDark, false);
    }

    private static Vector3 RadialPosition(Vector3 basePos, float angleDeg, float radius, float y)
    {
        float angle = angleDeg * Mathf.Deg2Rad;
        return basePos + new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius);
    }

    private static GameObject RadialBlock(
        CastleGenerator.CastleContext c,
        GameObject parent,
        string name,
        Vector3 basePos,
        float angleDeg,
        float radius,
        float y,
        Vector3 scale,
        Material material,
        bool addCollider = false)
    {
        GameObject block = c.Cube(parent, name, RadialPosition(basePos, angleDeg, radius, y), scale, material, addCollider);
        block.transform.rotation = Quaternion.Euler(0f, angleDeg, 0f);
        return block;
    }
}
