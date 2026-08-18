using UnityEngine;

/// <summary>
/// Castle Detail Upgrade v1.
/// Подробная геометрия замка для ЛР3/ЛР4:
/// стены, башни, ворота, двор, донжон, тронный зал и подземелье.
/// 
/// Файл отвечает за крупную архитектуру. Мелкий декор лежит в CastleDecorBuilder.cs.
/// </summary>
public static class CastleBlockoutBuilder
{
    public static void Build(CastleGenerator.CastleContext c)
    {
        GameObject parent = c.Child(c.Root, "01_CASTLE_DETAIL_UPGRADE_V1_STRUCTURE");

        BuildOuterWalls(c, parent);
        BuildGatehouse(c, parent);
        BuildWallWalkwaysAndStairs(c, parent);
        BuildCourtyard(c, parent);
        BuildKeepAndThroneHall(c, parent);
        BuildDungeonEntrance(c, parent);
    }

    // ─────────────────────────────────────────────────────────
    // Внешняя крепостная стена
    // ─────────────────────────────────────────────────────────

    private static void BuildOuterWalls(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject walls = c.Child(parent, "Outer_Walls_Heavy_Fortress_Silhouette_V11");

        // Более тяжёлые стены: не просто плоскость, а толстый оборонительный пояс.
        c.Cube(walls, "Front_Wall_Left_Base", new Vector3(-11.5f, 0.55f, c.FrontZ), new Vector3(11.8f, 1.10f, 1.85f), c.MatStone);
        c.Cube(walls, "Front_Wall_Right_Base", new Vector3(11.5f, 0.55f, c.FrontZ), new Vector3(11.8f, 1.10f, 1.85f), c.MatStone);

        c.Cube(walls, "Front_Wall_Left_Main", new Vector3(-11.5f, 3.55f, c.FrontZ), new Vector3(11.45f, 6.15f, 1.35f), c.MatStone);
        c.Cube(walls, "Front_Wall_Right_Main", new Vector3(11.5f, 3.55f, c.FrontZ), new Vector3(11.45f, 6.15f, 1.35f), c.MatStone);

        c.Cube(walls, "Back_Wall_Base", new Vector3(0f, 0.55f, c.BackZ), new Vector3(36.2f, 1.10f, 1.85f), c.MatStone);
        c.Cube(walls, "Back_Wall_Main", new Vector3(0f, 3.55f, c.BackZ), new Vector3(35.7f, 6.15f, 1.35f), c.MatStone);

        c.Cube(walls, "Left_Wall_Base", new Vector3(c.LeftX, 0.55f, 0f), new Vector3(1.85f, 1.10f, 32.2f), c.MatStone);
        c.Cube(walls, "Right_Wall_Base", new Vector3(c.RightX, 0.55f, 0f), new Vector3(1.85f, 1.10f, 32.2f), c.MatStone);
        c.Cube(walls, "Left_Wall_Main", new Vector3(c.LeftX, 3.55f, 0f), new Vector3(1.35f, 6.15f, 31.7f), c.MatStone);
        c.Cube(walls, "Right_Wall_Main", new Vector3(c.RightX, 3.55f, 0f), new Vector3(1.35f, 6.15f, 31.7f), c.MatStone);

        // Нижний тёмный цоколь — визуально делает стены тяжелее.
        Material heavyBase = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStone
            : c.NewMaterial(new Color(0.29f, 0.30f, 0.29f));

        c.Cube(walls, "Front_Left_Dark_Footing", new Vector3(-11.5f, 0.25f, c.FrontZ - 0.05f), new Vector3(12.0f, 0.50f, 2.05f), heavyBase);
        c.Cube(walls, "Front_Right_Dark_Footing", new Vector3(11.5f, 0.25f, c.FrontZ - 0.05f), new Vector3(12.0f, 0.50f, 2.05f), heavyBase);
        c.Cube(walls, "Back_Dark_Footing", new Vector3(0f, 0.25f, c.BackZ + 0.05f), new Vector3(36.4f, 0.50f, 2.05f), heavyBase);
        c.Cube(walls, "Left_Dark_Footing", new Vector3(c.LeftX - 0.05f, 0.25f, 0f), new Vector3(2.05f, 0.50f, 32.4f), heavyBase);
        c.Cube(walls, "Right_Dark_Footing", new Vector3(c.RightX + 0.05f, 0.25f, 0f), new Vector3(2.05f, 0.50f, 32.4f), heavyBase);

        // Верхний боевой ярус: широкий карниз + внутренняя полка.
        c.Cube(walls, "Front_Left_Battle_Coping", new Vector3(-11.5f, 6.75f, c.FrontZ), new Vector3(11.9f, 0.32f, 1.95f), c.MatStoneLight, false);
        c.Cube(walls, "Front_Right_Battle_Coping", new Vector3(11.5f, 6.75f, c.FrontZ), new Vector3(11.9f, 0.32f, 1.95f), c.MatStoneLight, false);
        c.Cube(walls, "Back_Battle_Coping", new Vector3(0f, 6.75f, c.BackZ), new Vector3(36.0f, 0.32f, 1.95f), c.MatStoneLight, false);
        c.Cube(walls, "Left_Battle_Coping", new Vector3(c.LeftX, 6.75f, 0f), new Vector3(1.95f, 0.32f, 32.0f), c.MatStoneLight, false);
        c.Cube(walls, "Right_Battle_Coping", new Vector3(c.RightX, 6.75f, 0f), new Vector3(1.95f, 0.32f, 32.0f), c.MatStoneLight, false);

        // Наружная линия под зубцами — добавляет глубину стены.
        c.Cube(walls, "Front_Left_Upper_Shadow_Line", new Vector3(-11.5f, 6.35f, c.FrontZ - 0.78f), new Vector3(11.9f, 0.22f, 0.22f), heavyBase, false);
        c.Cube(walls, "Front_Right_Upper_Shadow_Line", new Vector3(11.5f, 6.35f, c.FrontZ - 0.78f), new Vector3(11.9f, 0.22f, 0.22f), heavyBase, false);
        c.Cube(walls, "Back_Upper_Shadow_Line", new Vector3(0f, 6.35f, c.BackZ + 0.78f), new Vector3(36.0f, 0.22f, 0.22f), heavyBase, false);
        c.Cube(walls, "Left_Upper_Shadow_Line", new Vector3(c.LeftX - 0.78f, 6.35f, 0f), new Vector3(0.22f, 0.22f, 32.0f), heavyBase, false);
        c.Cube(walls, "Right_Upper_Shadow_Line", new Vector3(c.RightX + 0.78f, 6.35f, 0f), new Vector3(0.22f, 0.22f, 32.0f), heavyBase, false);

        // Зубцы крупнее и реже — крепость выглядит тяжелее, без мелкой каши.
        MerlonsX(c, walls, "Front_Left_Merlons_Heavy", -16f, -6f, c.FrontZ - 0.02f, 7.35f, 0.95f);
        MerlonsX(c, walls, "Front_Right_Merlons_Heavy", 6f, 16f, c.FrontZ - 0.02f, 7.35f, 0.95f);
        MerlonsX(c, walls, "Back_Merlons_Heavy", -16f, 16f, c.BackZ + 0.02f, 7.35f, 0.95f);
        MerlonsZ(c, walls, "Left_Merlons_Heavy", c.LeftX - 0.02f, -14f, 14f, 7.35f, 0.95f);
        MerlonsZ(c, walls, "Right_Merlons_Heavy", c.RightX + 0.02f, -14f, 14f, 7.35f, 0.95f);

        // Контрфорсы и каменные панели.
        BuildWallButtresses(c, walls);
        BuildWallWindowsAndStonePattern(c, walls);

        // Угловые башни.
        CornerTower(c, walls, new Vector3(c.LeftX, 0f, c.FrontZ), "Corner_Tower_Front_Left");
        CornerTower(c, walls, new Vector3(c.RightX, 0f, c.FrontZ), "Corner_Tower_Front_Right");
        CornerTower(c, walls, new Vector3(c.LeftX, 0f, c.BackZ), "Corner_Tower_Back_Left");
        CornerTower(c, walls, new Vector3(c.RightX, 0f, c.BackZ), "Corner_Tower_Back_Right");
    }

    private static void BuildWallButtresses(CastleGenerator.CastleContext c, GameObject walls)
    {
        // Передняя стена: опоры крупнее, чтобы стена не была плоским серым листом.
        float[] frontXs = { -15f, -12f, -8.5f, 8.5f, 12f, 15f };
        for (int i = 0; i < frontXs.Length; i++)
        {
            Buttress(c, walls, new Vector3(frontXs[i], 2.75f, cFront(c) - 0.86f), 0f, "Front_Heavy_Buttress_" + i);
        }

        // Задняя стена.
        for (int i = 0; i < 7; i++)
        {
            float x = -13.5f + i * 4.5f;
            Buttress(c, walls, new Vector3(x, 2.75f, c.BackZ + 0.86f), 180f, "Back_Heavy_Buttress_" + i);
        }

        // Боковые стены.
        for (int i = 0; i < 5; i++)
        {
            float z = -10.5f + i * 5.2f;
            ButtressSide(c, walls, new Vector3(c.LeftX - 0.86f, 2.75f, z), "Left_Heavy_Buttress_" + i);
            ButtressSide(c, walls, new Vector3(c.RightX + 0.86f, 2.75f, z), "Right_Heavy_Buttress_" + i);
        }
    }

    private static float cFront(CastleGenerator.CastleContext c)
    {
        return c.FrontZ;
    }

    private static void Buttress(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float rotY, string name)
    {
        GameObject b = c.Child(parent, name);
        b.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        c.Cube(b, "Buttress_Foot", pos + new Vector3(0f, -2.22f, -0.04f), new Vector3(1.25f, 0.72f, 1.35f), c.MatStone);
        c.Cube(b, "Buttress_Body", pos, new Vector3(0.82f, 5.15f, 0.95f), c.MatStoneLight);
        c.Cube(b, "Buttress_Cap", pos + new Vector3(0f, 2.72f, -0.04f), new Vector3(1.08f, 0.38f, 1.18f), c.MatStoneLight, false);
    }

    private static void ButtressSide(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, string name)
    {
        GameObject b = c.Child(parent, name);
        c.Cube(b, "Buttress_Foot", pos + new Vector3(0f, -2.22f, 0f), new Vector3(1.35f, 0.72f, 1.25f), c.MatStone);
        c.Cube(b, "Buttress_Body", pos, new Vector3(0.95f, 5.15f, 0.82f), c.MatStoneLight);
        c.Cube(b, "Buttress_Cap", pos + new Vector3(0f, 2.72f, 0f), new Vector3(1.18f, 0.38f, 1.08f), c.MatStoneLight, false);
    }

    private static void BuildWallWindowsAndStonePattern(CastleGenerator.CastleContext c, GameObject parent)
    {
        // Бойницы на внешних стенах.
        float[] frontSlits = { -14f, -10.5f, -7.2f, 7.2f, 10.5f, 14f };
        for (int i = 0; i < frontSlits.Length; i++)
        {
            c.Cube(parent, "Front_Arrow_Slit_" + i,
                new Vector3(frontSlits[i], 4.3f, c.FrontZ - 0.53f),
                new Vector3(0.18f, 1.15f, 0.08f),
                c.MatDark,
                false);
        }

        for (int i = 0; i < 7; i++)
        {
            float x = -13.5f + i * 4.5f;
            c.Cube(parent, "Back_Arrow_Slit_" + i,
                new Vector3(x, 4.25f, c.BackZ + 0.53f),
                new Vector3(0.18f, 1.1f, 0.08f),
                c.MatDark,
                false);
        }

        for (int i = 0; i < 5; i++)
        {
            float z = -10.5f + i * 5.2f;
            c.Cube(parent, "Left_Arrow_Slit_" + i,
                new Vector3(c.LeftX - 0.53f, 4.25f, z),
                new Vector3(0.08f, 1.1f, 0.18f),
                c.MatDark,
                false);

            c.Cube(parent, "Right_Arrow_Slit_" + i,
                new Vector3(c.RightX + 0.53f, 4.25f, z),
                new Vector3(0.08f, 1.1f, 0.18f),
                c.MatDark,
                false);
        }

        // Небольшая имитация каменной кладки: редкие плоские блоки, без коллайдеров.
        Material brickMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.41f, 0.40f, 0.36f));

        for (int row = 0; row < 4; row++)
        {
            float y = 1.45f + row * 1.05f;
            for (int i = 0; i < 8; i++)
            {
                float x = -15.2f + i * 4.0f + (row % 2) * 0.7f;
                if (Mathf.Abs(x) < 4.5f) continue;

                c.Cube(parent, "Front_Stone_Block_" + row + "_" + i,
                    new Vector3(x, y, c.FrontZ - 0.535f),
                    new Vector3(1.35f, 0.18f, 0.04f),
                    brickMat,
                    false);
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    // Ворота и надвратная часть
    // ─────────────────────────────────────────────────────────

    private static void BuildGatehouse(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject gate = c.Child(parent, "Gatehouse_Detailed_Main_Entrance");

        GateTower(c, gate, new Vector3(-4.6f, 0f, c.FrontZ), "Gate_Tower_Left");
        GateTower(c, gate, new Vector3(4.6f, 0f, c.FrontZ), "Gate_Tower_Right");

        // Основная надвратная постройка.
        // ВАЖНО: раньше здесь был один сплошной Gatehouse_Back_Block,
        // он перекрывал проход физическим коллайдером. Теперь блок разбит на
        // левую/правую стойку и верхнюю часть, поэтому игрок проходит внутрь.
        c.Cube(gate, "Gatehouse_Back_Left_Block", new Vector3(-3.0f, 4.0f, c.FrontZ + 0.55f), new Vector3(1.0f, 7.4f, 1.6f), c.MatStone);
        c.Cube(gate, "Gatehouse_Back_Right_Block", new Vector3(3.0f, 4.0f, c.FrontZ + 0.55f), new Vector3(1.0f, 7.4f, 1.6f), c.MatStone);
        c.Cube(gate, "Gatehouse_Back_Top_Block", new Vector3(0f, 6.35f, c.FrontZ + 0.55f), new Vector3(6.6f, 2.7f, 1.6f), c.MatStone);
        c.Cube(gate, "Gatehouse_Front_Lintel", new Vector3(0f, 6.25f, c.FrontZ - 0.22f), new Vector3(6.5f, 1.4f, 1.55f), c.MatStoneLight);
        c.Cube(gate, "Gatehouse_Capstone", new Vector3(0f, 7.2f, c.FrontZ - 0.18f), new Vector3(7.1f, 0.32f, 1.8f), c.MatStoneLight, false);

        // Псевдо-арка: стойки, верх и скругление цилиндрами.
        c.Cube(gate, "Gate_Left_Pillar", new Vector3(-2.65f, 2.75f, c.FrontZ - 0.55f), new Vector3(0.9f, 5.5f, 1.35f), c.MatStoneLight);
        c.Cube(gate, "Gate_Right_Pillar", new Vector3(2.65f, 2.75f, c.FrontZ - 0.55f), new Vector3(0.9f, 5.5f, 1.35f), c.MatStoneLight);
        c.Cube(gate, "Gate_Arch_Top", new Vector3(0f, 5.55f, c.FrontZ - 0.55f), new Vector3(5.3f, 1.05f, 1.35f), c.MatStoneLight);

        GameObject archLeft = c.Cylinder(gate, "Gate_Arch_Round_Left",
            new Vector3(-1.85f, 5.1f, c.FrontZ - 0.58f),
            new Vector3(0.42f, 0.32f, 0.42f),
            c.MatStoneLight,
            false);
        archLeft.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        GameObject archRight = c.Cylinder(gate, "Gate_Arch_Round_Right",
            new Vector3(1.85f, 5.1f, c.FrontZ - 0.58f),
            new Vector3(0.42f, 0.32f, 0.42f),
            c.MatStoneLight,
            false);
        archRight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Открытые деревянные створки ворот.
        // Они стоят по бокам, поэтому визуально проход открыт.
        GameObject leftDoor = c.Cube(gate, "Wooden_Gate_Left_Door_Open",
            new Vector3(-2.25f, 2.1f, c.FrontZ - 0.92f),
            new Vector3(1.55f, 3.5f, 0.16f),
            c.MatWood,
            false);
        leftDoor.transform.rotation = Quaternion.Euler(0f, -28f, 0f);

        GameObject rightDoor = c.Cube(gate, "Wooden_Gate_Right_Door_Open",
            new Vector3(2.25f, 2.1f, c.FrontZ - 0.92f),
            new Vector3(1.55f, 3.5f, 0.16f),
            c.MatWood,
            false);
        rightDoor.transform.rotation = Quaternion.Euler(0f, 28f, 0f);

        c.Cube(gate, "Gate_Door_Left_Iron_Beam", new Vector3(-2.25f, 2.1f, c.FrontZ - 1.04f), new Vector3(0.14f, 3.6f, 0.18f), c.MatIron, false);
        c.Cube(gate, "Gate_Door_Right_Iron_Beam", new Vector3(2.25f, 2.1f, c.FrontZ - 1.04f), new Vector3(0.14f, 3.6f, 0.18f), c.MatIron, false);

        // Поднятая решётка: она видна наверху, но проход снизу свободный.
        for (int i = -4; i <= 4; i++)
        {
            c.Cube(gate, "Raised_Portcullis_Vertical_Bar_" + i,
                new Vector3(i * 0.38f, 5.25f, c.FrontZ - 1.18f),
                new Vector3(0.07f, 2.1f, 0.07f), c.MatIron, false);
        }

        c.Cube(gate, "Raised_Portcullis_Horizontal_Bar_1", new Vector3(0f, 4.35f, c.FrontZ - 1.18f), new Vector3(3.5f, 0.08f, 0.08f), c.MatIron, false);
        c.Cube(gate, "Raised_Portcullis_Horizontal_Bar_2", new Vector3(0f, 5.25f, c.FrontZ - 1.18f), new Vector3(3.5f, 0.08f, 0.08f), c.MatIron, false);
        c.Cube(gate, "Raised_Portcullis_Horizontal_Bar_3", new Vector3(0f, 6.10f, c.FrontZ - 1.18f), new Vector3(3.5f, 0.08f, 0.08f), c.MatIron, false);

        // Мостик / подъемная площадка.
        c.Cube(gate, "Drawbridge_Wooden_Platform", new Vector3(0f, 0.27f, c.FrontZ - 2.2f), new Vector3(7.1f, 0.28f, 3.6f), c.MatWood);
        c.Cube(gate, "Drawbridge_Left_Iron_Edge", new Vector3(-3.65f, 0.46f, c.FrontZ - 2.2f), new Vector3(0.12f, 0.16f, 3.7f), c.MatIron, false);
        c.Cube(gate, "Drawbridge_Right_Iron_Edge", new Vector3(3.65f, 0.46f, c.FrontZ - 2.2f), new Vector3(0.12f, 0.16f, 3.7f), c.MatIron, false);

        for (int i = -3; i <= 3; i++)
        {
            c.Cube(gate, "Drawbridge_Plank_" + i,
                new Vector3(i * 0.95f, 0.5f, c.FrontZ - 2.2f),
                new Vector3(0.08f, 0.08f, 3.55f),
                c.MatIron,
                false);
        }

        // Надвратные зубцы.
        MerlonsX(c, gate, "Gatehouse_Top_Merlons", -3.1f, 3.1f, c.FrontZ - 0.25f, 7.85f, 0.65f);
    }

    private static void GateTower(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, string name)
    {
        GameObject tower = c.Child(parent, name);

        // V12: башня строится как монолитная масса.
        // Здесь только основная геометрия, без хаотичных декоративных блоков.
        c.Cylinder(tower, "Tower_Wide_Foundation", basePos + new Vector3(0f, 0.45f, 0f), new Vector3(3.18f, 0.45f, 3.18f), c.MatStone);
        c.Cylinder(tower, "Tower_Sloped_Base", basePos + new Vector3(0f, 1.15f, 0f), new Vector3(2.92f, 0.48f, 2.92f), c.MatStone);
        c.Cylinder(tower, "Tower_Main_Drum", basePos + new Vector3(0f, 4.35f, 0f), new Vector3(2.55f, 3.75f, 2.55f), c.MatStoneLight);

        // Крупные пояса — меньше мелочи, больше архитектурной логики.
        c.Cylinder(tower, "Tower_Base_Belt", basePos + new Vector3(0f, 2.28f, 0f), new Vector3(2.86f, 0.14f, 2.86f), c.MatStone, false);
        c.Cylinder(tower, "Tower_Middle_Belt", basePos + new Vector3(0f, 4.95f, 0f), new Vector3(2.78f, 0.13f, 2.78f), c.MatStone, false);
        c.Cylinder(tower, "Tower_Upper_Belt", basePos + new Vector3(0f, 6.95f, 0f), new Vector3(2.88f, 0.16f, 2.88f), c.MatStone, false);

        // Основной боевой верх. Детальный венец добавляет CastleTowerDetailBuilder.
        c.Cylinder(tower, "Tower_Battle_Deck", basePos + new Vector3(0f, 7.72f, 0f), new Vector3(3.03f, 0.24f, 3.03f), c.MatStone);
        c.Cylinder(tower, "Tower_Solid_Parapet", basePos + new Vector3(0f, 8.12f, 0f), new Vector3(2.73f, 0.22f, 2.73f), c.MatStoneLight, false);

        // Чистые бойницы без лишних накладок.
        c.Cube(tower, "Window_Slit_Front", basePos + new Vector3(0f, 4.65f, -2.60f), new Vector3(0.24f, 1.55f, 0.08f), c.MatDark, false);
        c.Cube(tower, "Window_Slit_Left", basePos + new Vector3(-2.60f, 3.70f, 0f), new Vector3(0.08f, 1.18f, 0.24f), c.MatDark, false);
        c.Cube(tower, "Window_Slit_Right", basePos + new Vector3(2.60f, 3.70f, 0f), new Vector3(0.08f, 1.18f, 0.24f), c.MatDark, false);
    }

    private static void CornerTower(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, string name)
    {
        GameObject tower = c.Child(parent, name);

        // V12: угловая башня массивная, но без летающих элементов.
        c.Cylinder(tower, "Tower_Wide_Foundation", basePos + new Vector3(0f, 0.43f, 0f), new Vector3(3.00f, 0.43f, 3.00f), c.MatStone);
        c.Cylinder(tower, "Tower_Sloped_Base", basePos + new Vector3(0f, 1.10f, 0f), new Vector3(2.72f, 0.46f, 2.72f), c.MatStone);
        c.Cylinder(tower, "Tower_Main_Drum", basePos + new Vector3(0f, 4.25f, 0f), new Vector3(2.38f, 3.65f, 2.38f), c.MatStoneLight);

        c.Cylinder(tower, "Tower_Base_Belt", basePos + new Vector3(0f, 2.22f, 0f), new Vector3(2.66f, 0.13f, 2.66f), c.MatStone, false);
        c.Cylinder(tower, "Tower_Middle_Belt", basePos + new Vector3(0f, 4.82f, 0f), new Vector3(2.58f, 0.12f, 2.58f), c.MatStone, false);
        c.Cylinder(tower, "Tower_Upper_Belt", basePos + new Vector3(0f, 6.82f, 0f), new Vector3(2.68f, 0.15f, 2.68f), c.MatStone, false);

        c.Cylinder(tower, "Tower_Battle_Deck", basePos + new Vector3(0f, 7.52f, 0f), new Vector3(2.82f, 0.22f, 2.82f), c.MatStone);
        c.Cylinder(tower, "Tower_Solid_Parapet", basePos + new Vector3(0f, 7.92f, 0f), new Vector3(2.52f, 0.20f, 2.52f), c.MatStoneLight, false);

        c.Cube(tower, "Arrow_Slit_1", basePos + new Vector3(0f, 4.52f, -2.42f), new Vector3(0.22f, 1.25f, 0.08f), c.MatDark, false);
        c.Cube(tower, "Arrow_Slit_2", basePos + new Vector3(2.42f, 3.58f, 0f), new Vector3(0.08f, 1.10f, 0.22f), c.MatDark, false);
        c.Cube(tower, "Arrow_Slit_3", basePos + new Vector3(-2.42f, 3.58f, 0f), new Vector3(0.08f, 1.10f, 0.22f), c.MatDark, false);
    }

    private static void TowerRoofStack(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, float radius, float height)
    {
        Material roofMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.18f, 0.13f, 0.10f));

        c.Cylinder(parent, "Tower_Roof_Layer_1", pos + new Vector3(0f, 0.15f, 0f), new Vector3(radius, 0.15f, radius), roofMat, false);
        c.Cylinder(parent, "Tower_Roof_Layer_2", pos + new Vector3(0f, 0.45f, 0f), new Vector3(radius * 0.72f, 0.17f, radius * 0.72f), roofMat, false);
        c.Cylinder(parent, "Tower_Roof_Layer_3", pos + new Vector3(0f, 0.78f, 0f), new Vector3(radius * 0.45f, 0.18f, radius * 0.45f), roofMat, false);
        c.Cylinder(parent, "Tower_Roof_Tip", pos + new Vector3(0f, 1.12f, 0f), new Vector3(radius * 0.18f, 0.28f, radius * 0.18f), roofMat, false);
    }

    // ─────────────────────────────────────────────────────────
    // Проходы по стенам и лестницы
    // ─────────────────────────────────────────────────────────

    private static void BuildWallWalkwaysAndStairs(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject walk = c.Child(parent, "Wall_Walkways_And_Stairs_Clean_V6");

        // Основные проходы по стенам.
        c.Cube(walk, "Front_Left_Walkway", new Vector3(-11.5f, 5.95f, c.FrontZ + 0.8f), new Vector3(10.5f, 0.28f, 1.15f), c.MatFloor);
        c.Cube(walk, "Front_Right_Walkway", new Vector3(11.5f, 5.95f, c.FrontZ + 0.8f), new Vector3(10.5f, 0.28f, 1.15f), c.MatFloor);
        c.Cube(walk, "Back_Walkway", new Vector3(0f, 5.95f, c.BackZ - 0.8f), new Vector3(32f, 0.28f, 1.15f), c.MatFloor);
        c.Cube(walk, "Left_Walkway", new Vector3(c.LeftX + 0.8f, 0f, 0f), new Vector3(1.15f, 0.28f, 28f), c.MatFloor).transform.position = new Vector3(c.LeftX + 0.8f, 5.95f, 0f);
        c.Cube(walk, "Right_Walkway", new Vector3(c.RightX - 0.8f, 5.95f, 0f), new Vector3(1.15f, 0.28f, 28f), c.MatFloor);

        // Две аккуратные лестницы к стенам — не торчат в центре двора.
        BuildCompactWallStair(c, walk, "Left_Compact_Wall_Stair", new Vector3(-15.1f, 0f, -8.9f), 1f);
        BuildCompactWallStair(c, walk, "Right_Compact_Wall_Stair", new Vector3(15.1f, 0f, -8.9f), -1f);

        // Маленькие площадки наверху.
        c.Cube(walk, "Left_Stair_Top_Landing", new Vector3(-15.1f, 5.95f, -2.5f), new Vector3(1.55f, 0.24f, 2.2f), c.MatFloor);
        c.Cube(walk, "Right_Stair_Top_Landing", new Vector3(15.1f, 5.95f, -2.5f), new Vector3(1.55f, 0.24f, 2.2f), c.MatFloor);

        // Внутренние галереи делаем тоньше и выше, чтобы они не давили на камеру.
        c.Cube(walk, "Left_Inner_Wooden_Gallery_Clean", new Vector3(-15.25f, 3.95f, -0.4f), new Vector3(1.15f, 0.16f, 13.4f), c.MatWood, false);
        c.Cube(walk, "Right_Inner_Wooden_Gallery_Clean", new Vector3(15.25f, 3.95f, -0.4f), new Vector3(1.15f, 0.16f, 13.4f), c.MatWood, false);

        c.Cube(walk, "Left_Gallery_Thin_Rail", new Vector3(-14.62f, 4.45f, -0.4f), new Vector3(0.10f, 0.62f, 13.4f), c.MatWood, false);
        c.Cube(walk, "Right_Gallery_Thin_Rail", new Vector3(14.62f, 4.45f, -0.4f), new Vector3(0.10f, 0.62f, 13.4f), c.MatWood, false);

        // Редкие опоры, чтобы не было частокола из столбов.
        for (int i = 0; i < 4; i++)
        {
            float z = -6.4f + i * 3.9f;
            c.Cube(walk, "Left_Gallery_Post_Clean_" + i, new Vector3(-15.25f, 2.0f, z), new Vector3(0.16f, 3.75f, 0.16f), c.MatWood);
            c.Cube(walk, "Right_Gallery_Post_Clean_" + i, new Vector3(15.25f, 2.0f, z), new Vector3(0.16f, 3.75f, 0.16f), c.MatWood);
        }
    }

    private static void BuildCompactWallStair(CastleGenerator.CastleContext c, GameObject parent, string name, Vector3 start, float sideSign)
    {
        GameObject stair = c.Child(parent, name);

        for (int i = 0; i < 15; i++)
        {
            float y = 0.16f + i * 0.38f;
            float z = start.z + i * 0.44f;

            c.Cube(stair, "Step_" + i,
                new Vector3(start.x, y, z),
                new Vector3(1.45f, 0.24f, 0.58f),
                c.MatStoneLight);
        }

        c.Cube(stair, "Outer_Rail",
            new Vector3(start.x + sideSign * 0.78f, 2.9f, start.z + 3.1f),
            new Vector3(0.10f, 4.5f, 6.4f),
            c.MatWood,
            false);

        c.Cube(stair, "Inner_Rail",
            new Vector3(start.x - sideSign * 0.78f, 2.9f, start.z + 3.1f),
            new Vector3(0.10f, 4.5f, 6.4f),
            c.MatWood,
            false);
    }


    // ─────────────────────────────────────────────────────────
    // Двор
    // ─────────────────────────────────────────────────────────

    private static void BuildCourtyard(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject yard = c.Child(parent, "Courtyard_Detailed_Inner_Space");

        c.Cube(yard, "Courtyard_Stone_Floor", new Vector3(0f, 0.05f, -2f), new Vector3(28f, 0.1f, 22f), c.MatFloor);

        // Каменная плитка на площади.
        BuildPavingStones(c, yard);

        // Чистый центральный маршрут и зоны двора.
        BuildCourtyardClearPathAndZones(c, yard);

        // Низкая внутренняя стена.
        c.Cube(yard, "Inner_Divider_Wall_Left", new Vector3(-8.5f, 2f, 2.2f), new Vector3(7f, 4f, 0.7f), c.MatStone);
        c.Cube(yard, "Inner_Divider_Wall_Right", new Vector3(8.5f, 2f, 2.2f), new Vector3(7f, 4f, 0.7f), c.MatStone);
        c.Cube(yard, "Inner_Divider_Gate_Top", new Vector3(0f, 4.2f, 2.2f), new Vector3(5f, 0.8f, 0.7f), c.MatStoneLight);
        c.Cube(yard, "Inner_Divider_Coping_Left", new Vector3(-8.5f, 4.15f, 2.2f), new Vector3(7.2f, 0.28f, 0.9f), c.MatStoneLight, false);
        c.Cube(yard, "Inner_Divider_Coping_Right", new Vector3(8.5f, 4.15f, 2.2f), new Vector3(7.2f, 0.28f, 0.9f), c.MatStoneLight, false);

        MerlonsX(c, yard, "Inner_Left_Merlons", -12f, -5.5f, 2.2f, 4.75f, 0.55f);
        MerlonsX(c, yard, "Inner_Right_Merlons", 5.5f, 12f, 2.2f, 4.75f, 0.55f);

        BuildWell(c, yard);
        BuildSmallStableBlockout(c, yard);
        BuildForgeBlockout(c, yard);
    }

    private static void BuildPavingStones(CastleGenerator.CastleContext c, GameObject parent)
    {
        Material slabMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.48f, 0.45f, 0.38f));

        for (int x = -5; x <= 5; x++)
        {
            for (int z = -4; z <= 4; z++)
            {
                if ((x + z) % 3 == 0)
                {
                    c.Cube(parent, "Courtyard_Paving_" + x + "_" + z,
                        new Vector3(x * 2.2f, 0.13f, -6.4f + z * 1.85f),
                        new Vector3(1.55f, 0.035f, 1.10f),
                        slabMat,
                        false);
                }
            }
        }
    }

    private static void BuildCourtyardClearPathAndZones(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject zones = c.Child(parent, "Courtyard_Clear_Path_And_Zones_V4");

        Material pathMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatFloor
            : c.NewMaterial(new Color(0.50f, 0.45f, 0.35f));

        Material borderMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.36f, 0.34f, 0.30f));

        // Главный чистый проход: ворота -> двор -> тронный зал.
        c.Cube(zones, "Main_Courtyard_Clean_Path",
            new Vector3(0f, 0.155f, -5.2f),
            new Vector3(5.9f, 0.055f, 16.8f),
            pathMat,
            false);

        // Боковые площадки: слева хозяйственная, справа кузница/стража.
        c.Cube(zones, "Left_Service_Yard_Patch",
            new Vector3(-10.5f, 0.16f, -4.4f),
            new Vector3(6.4f, 0.045f, 9.6f),
            borderMat,
            false);

        c.Cube(zones, "Right_Service_Yard_Patch",
            new Vector3(10.5f, 0.16f, -4.4f),
            new Vector3(6.4f, 0.045f, 9.6f),
            borderMat,
            false);

        // Низкие бордюры, которые читают маршрут, но не мешают ходьбе.
        c.Cube(zones, "Main_Path_Left_Low_Border",
            new Vector3(-3.15f, 0.30f, -5.2f),
            new Vector3(0.18f, 0.20f, 15.6f),
            c.MatStoneLight,
            false);

        c.Cube(zones, "Main_Path_Right_Low_Border",
            new Vector3(3.15f, 0.30f, -5.2f),
            new Vector3(0.18f, 0.20f, 15.6f),
            c.MatStoneLight,
            false);

        // Пустой центр вокруг игрока — ничего крупного сюда не ставим.
        c.Cube(zones, "Courtyard_Center_Visual_Clear_Area",
            new Vector3(0f, 0.165f, -3.2f),
            new Vector3(4.6f, 0.03f, 6.4f),
            pathMat,
            false);
    }

    private static void BuildWell(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject well = c.Child(parent, "Detailed_Well_Center");

        Vector3 center = new Vector3(-5.5f, 0f, -4.5f);

        c.Cylinder(well, "Well_Stone_Ring", center + new Vector3(0f, 0.55f, 0f), new Vector3(1.25f, 0.55f, 1.25f), c.MatStoneLight);
        c.Cylinder(well, "Well_Dark_Hole", center + new Vector3(0f, 1.12f, 0f), new Vector3(0.85f, 0.06f, 0.85f), c.MatDark, false);
        c.Cylinder(well, "Well_Water_Glimmer", center + new Vector3(0f, 1.16f, 0f), new Vector3(0.70f, 0.035f, 0.70f), c.MatWater, false);

        c.Cube(well, "Well_Post_Left", center + new Vector3(-0.95f, 1.75f, 0f), new Vector3(0.16f, 1.9f, 0.16f), c.MatWood);
        c.Cube(well, "Well_Post_Right", center + new Vector3(0.95f, 1.75f, 0f), new Vector3(0.16f, 1.9f, 0.16f), c.MatWood);
        c.Cube(well, "Well_Top_Beam", center + new Vector3(0f, 2.65f, 0f), new Vector3(2.4f, 0.2f, 0.2f), c.MatWood);
        c.Cylinder(well, "Well_Rope_Roll", center + new Vector3(0f, 2.42f, 0f), new Vector3(0.23f, 0.52f, 0.23f), c.MatWood, false).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        c.Cube(well, "Well_Rope_Down", center + new Vector3(0f, 1.83f, 0f), new Vector3(0.05f, 1.05f, 0.05f), c.MatIron, false);
        c.Cube(well, "Well_Bucket", center + new Vector3(0f, 1.2f, 0.18f), new Vector3(0.42f, 0.34f, 0.32f), c.MatWood, false);
    }

    private static void BuildSmallStableBlockout(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject stable = c.Child(parent, "Stable_Blockout_Left_Side_Reworked");

        // Конюшня теперь прижата к левой стене и не перекрывает центральный проход.
        Vector3 pos = new Vector3(-13.2f, 0f, -4.8f);

        c.Cube(stable, "Stable_Back_Wall", pos + new Vector3(-0.55f, 1.25f, 0f), new Vector3(0.35f, 2.5f, 5.8f), c.MatWood);
        c.Cube(stable, "Stable_Rear_Low_Wall", pos + new Vector3(1.35f, 0.72f, 2.65f), new Vector3(3.7f, 1.25f, 0.24f), c.MatWood);
        c.Cube(stable, "Stable_Front_Beam", pos + new Vector3(1.35f, 2.32f, -2.65f), new Vector3(3.7f, 0.25f, 0.22f), c.MatWood);
        c.Cube(stable, "Stable_Post_1", pos + new Vector3(-0.55f, 1.35f, -2.7f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);
        c.Cube(stable, "Stable_Post_2", pos + new Vector3(1.35f, 1.35f, -2.7f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);
        c.Cube(stable, "Stable_Post_3", pos + new Vector3(3.25f, 1.35f, -2.7f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);

        PitchedRoof(c, stable, "Stable_Side_Roof",
            pos + new Vector3(1.35f, 2.65f, 0f),
            4.9f, 6.3f, 1.05f, c.MatWood);

        // Перегородки стойл вдоль стены.
        for (int i = 0; i < 3; i++)
        {
            float z = -1.75f + i * 1.75f;
            c.Cube(stable, "Stable_Stall_Divider_" + i,
                pos + new Vector3(1.4f, 0.85f, z),
                new Vector3(3.2f, 1.45f, 0.12f),
                c.MatWood,
                false);
        }
    }

    private static void BuildForgeBlockout(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject forge = c.Child(parent, "Blacksmith_Forge_Right_Side_Reworked");

        // Кузница теперь прижата к правой стене и не создаёт хаос в центре двора.
        Vector3 pos = new Vector3(13.1f, 0f, -4.3f);

        c.Cube(forge, "Forge_Back_Wall", pos + new Vector3(0.55f, 1.35f, 0f), new Vector3(0.35f, 2.7f, 5.6f), c.MatStone);
        c.Cube(forge, "Forge_Rear_Low_Wall", pos + new Vector3(-1.45f, 0.72f, 2.55f), new Vector3(3.8f, 1.25f, 0.25f), c.MatStone);
        c.Cube(forge, "Forge_Front_Beam", pos + new Vector3(-1.45f, 2.35f, -2.55f), new Vector3(3.8f, 0.25f, 0.22f), c.MatWood);
        c.Cube(forge, "Forge_Post_1", pos + new Vector3(0.55f, 1.35f, -2.55f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);
        c.Cube(forge, "Forge_Post_2", pos + new Vector3(-1.45f, 1.35f, -2.55f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);
        c.Cube(forge, "Forge_Post_3", pos + new Vector3(-3.35f, 1.35f, -2.55f), new Vector3(0.24f, 2.7f, 0.24f), c.MatWood);

        PitchedRoof(c, forge, "Forge_Side_Roof",
            pos + new Vector3(-1.45f, 2.7f, 0f),
            5.0f, 6.1f, 1.1f, c.MatWood);

        c.Cube(forge, "Forge_Stone_Furnace",
            pos + new Vector3(-0.35f, 0.6f, 0.75f),
            new Vector3(1.45f, 1.2f, 1.1f),
            c.MatStoneLight);

        c.Cube(forge, "Forge_Dark_Mouth",
            pos + new Vector3(-0.35f, 0.68f, 0.15f),
            new Vector3(0.95f, 0.45f, 0.08f),
            c.MatDark,
            false);

        c.Cube(forge, "Forge_Chimney",
            pos + new Vector3(-0.35f, 2.45f, 1.05f),
            new Vector3(0.6f, 2.15f, 0.6f),
            c.MatStone);
    }

    // ─────────────────────────────────────────────────────────
    // Донжон и тронный зал
    // ─────────────────────────────────────────────────────────

    private static void BuildKeepAndThroneHall(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject keep = c.Child(parent, "Keep_Throne_Hall_Detailed_Main_Building");

        // Основной каменный объём.
        c.Cube(keep, "Keep_Floor", new Vector3(0f, 0.12f, 9f), new Vector3(16.5f, 0.22f, 11.5f), c.MatFloor);
        c.Cube(keep, "Keep_Back_Wall", new Vector3(0f, 3.8f, 14.35f), new Vector3(16.5f, 7.6f, 0.8f), c.MatStone);
        c.Cube(keep, "Keep_Left_Wall", new Vector3(-8.25f, 3.8f, 9f), new Vector3(0.8f, 7.6f, 11.4f), c.MatStone);
        c.Cube(keep, "Keep_Right_Wall", new Vector3(8.25f, 3.8f, 9f), new Vector3(0.8f, 7.6f, 11.4f), c.MatStone);

        c.Cube(keep, "Keep_Front_Left", new Vector3(-5.25f, 3.8f, 3.65f), new Vector3(5.8f, 7.6f, 0.8f), c.MatStone);
        c.Cube(keep, "Keep_Front_Right", new Vector3(5.25f, 3.8f, 3.65f), new Vector3(5.8f, 7.6f, 0.8f), c.MatStone);
        c.Cube(keep, "Keep_Front_Top", new Vector3(0f, 6.35f, 3.65f), new Vector3(4.8f, 2.1f, 0.8f), c.MatStone);

        c.Cube(keep, "Keep_Front_Cornice", new Vector3(0f, 7.7f, 3.6f), new Vector3(17.4f, 0.35f, 1.1f), c.MatStoneLight, false);
        c.Cube(keep, "Keep_Back_Cornice", new Vector3(0f, 7.7f, 14.4f), new Vector3(17.4f, 0.35f, 1.1f), c.MatStoneLight, false);
        c.Cube(keep, "Keep_Left_Cornice", new Vector3(-8.3f, 7.7f, 9f), new Vector3(1.1f, 0.35f, 11.8f), c.MatStoneLight, false);
        c.Cube(keep, "Keep_Right_Cornice", new Vector3(8.3f, 7.7f, 9f), new Vector3(1.1f, 0.35f, 11.8f), c.MatStoneLight, false);

        // Крыша донжона.
        PitchedRoof(c, keep, "Keep_Pitched_Roof", new Vector3(0f, 7.95f, 9f), 17.4f, 12.1f, 2.4f,
            c.Mode == CastleGenerator.LabMode.Lab3_Blockout ? c.MatStoneLight : c.NewMaterial(new Color(0.20f, 0.13f, 0.09f)));

        MerlonsX(c, keep, "Keep_Roof_Front_Merlons", -7f, 7f, 3.6f, 8.25f, 0.55f);
        MerlonsX(c, keep, "Keep_Roof_Back_Merlons", -7f, 7f, 14.4f, 8.25f, 0.55f);

        // Боковые башенки у донжона.
        MiniKeepTower(c, keep, new Vector3(-8.4f, 0f, 3.7f), "Keep_Front_Left_Turret");
        MiniKeepTower(c, keep, new Vector3(8.4f, 0f, 3.7f), "Keep_Front_Right_Turret");
        MiniKeepTower(c, keep, new Vector3(-8.4f, 0f, 14.3f), "Keep_Back_Left_Turret");
        MiniKeepTower(c, keep, new Vector3(8.4f, 0f, 14.3f), "Keep_Back_Right_Turret");

        // Главная центральная башня донжона — теперь замок выше и силуэт мощнее.
        CentralKeepTower(c, keep, new Vector3(0f, 0f, 14.85f));

        // Новые элементы масштаба и величия.
        BuildKeepGrandPlinth(c, keep);
        BuildGrandEntranceStair(c, keep);
        BuildFrontCentralTowerMass(c, keep);
        BuildMajesticUpperExpansionV4(c, keep);
        BuildFortressSilhouettePolishV6(c, keep);
        BuildCastleScaleV7(c, keep);

        // Окна с рамками.
        WindowWithFrame(c, keep, new Vector3(-3.4f, 4.8f, 3.2f), true, "Keep_Front_Window_Left");
        WindowWithFrame(c, keep, new Vector3(3.4f, 4.8f, 3.2f), true, "Keep_Front_Window_Right");
        WindowWithFrame(c, keep, new Vector3(-7.86f, 4.5f, 9f), false, "Keep_Left_Window");
        WindowWithFrame(c, keep, new Vector3(7.86f, 4.5f, 9f), false, "Keep_Right_Window");

        // Колонны и внутренние арки.
        Pillar(c, keep, new Vector3(-5.2f, 0f, 6.6f));
        Pillar(c, keep, new Vector3(5.2f, 0f, 6.6f));
        Pillar(c, keep, new Vector3(-5.2f, 0f, 11.6f));
        Pillar(c, keep, new Vector3(5.2f, 0f, 11.6f));

        // Тонкие высокие арочные линии вместо тяжёлых перекладин в поле зрения.
        c.Cube(keep, "Left_High_Interior_Rib", new Vector3(-5.2f, 6.15f, 9.1f), new Vector3(0.28f, 0.28f, 5.8f), c.MatStoneLight, false);
        c.Cube(keep, "Right_High_Interior_Rib", new Vector3(5.2f, 6.15f, 9.1f), new Vector3(0.28f, 0.28f, 5.8f), c.MatStoneLight, false);
        c.Cube(keep, "Back_High_Interior_Rib", new Vector3(0f, 6.28f, 11.6f), new Vector3(10.9f, 0.28f, 0.28f), c.MatStoneLight, false);

        // Тронная зона.
        Material carpetMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout ? c.MatStoneLight : c.MatRed;
        c.Cube(keep, "Throne_Carpet_Long", new Vector3(0f, 0.28f, 8.2f), new Vector3(3.1f, 0.05f, 8.8f), carpetMat);
        c.Cube(keep, "Throne_Carpet_Border_Left", new Vector3(-1.72f, 0.31f, 8.2f), new Vector3(0.13f, 0.05f, 8.8f), c.MatStoneLight, false);
        c.Cube(keep, "Throne_Carpet_Border_Right", new Vector3(1.72f, 0.31f, 8.2f), new Vector3(0.13f, 0.05f, 8.8f), c.MatStoneLight, false);

        c.Cube(keep, "Throne_Platform_Low", new Vector3(0f, 0.55f, 12.15f), new Vector3(5.2f, 0.55f, 2.9f), c.MatStoneLight);
        c.Cube(keep, "Throne_Platform_High", new Vector3(0f, 1.02f, 12.55f), new Vector3(4.1f, 0.38f, 2.2f), c.MatStoneLight);
        c.Cube(keep, "Throne_Seat", new Vector3(0f, 1.55f, 12.55f), new Vector3(1.75f, 0.35f, 1.35f), c.MatWood);
        c.Cube(keep, "Throne_Back", new Vector3(0f, 2.75f, 13.08f), new Vector3(2.0f, 2.6f, 0.25f), c.MatWood);
        c.Cube(keep, "Throne_Left_Arm", new Vector3(-1.1f, 1.85f, 12.55f), new Vector3(0.25f, 0.95f, 1.25f), c.MatWood);
        c.Cube(keep, "Throne_Right_Arm", new Vector3(1.1f, 1.85f, 12.55f), new Vector3(0.25f, 0.95f, 1.25f), c.MatWood);

        // Верхняя галерея и лестница к центральной башне — чтобы интерьер смотрелся богаче.
        BuildThroneRearGallery(c, keep);
        BuildTowerSpiralSteps(c, keep, new Vector3(0f, 0f, 14.85f));
    }

    private static void CentralKeepTower(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos)
    {
        GameObject tower = c.Child(parent, "Central_Keep_Tower_Tall_Donjon");

        c.Cylinder(tower, "Central_Tower_Base",
            basePos + new Vector3(0f, 0.65f, 0f),
            new Vector3(3.45f, 0.65f, 3.45f),
            c.MatStone);

        c.Cylinder(tower, "Central_Tower_Body_Lower",
            basePos + new Vector3(0f, 5.6f, 0f),
            new Vector3(2.85f, 4.9f, 2.85f),
            c.MatStoneLight);

        c.Cylinder(tower, "Central_Tower_Body_Middle",
            basePos + new Vector3(0f, 11.35f, 0f),
            new Vector3(2.35f, 4.4f, 2.35f),
            c.MatStoneLight);

        c.Cylinder(tower, "Central_Tower_Body_Upper",
            basePos + new Vector3(0f, 16.25f, 0f),
            new Vector3(1.95f, 3.6f, 1.95f),
            c.MatStoneLight);

        c.Cylinder(tower, "Central_Tower_Belt_Lower",
            basePos + new Vector3(0f, 8.75f, 0f),
            new Vector3(3.05f, 0.18f, 3.05f),
            c.MatStone,
            false);

        c.Cylinder(tower, "Central_Tower_Belt_Middle",
            basePos + new Vector3(0f, 14.15f, 0f),
            new Vector3(2.55f, 0.18f, 2.55f),
            c.MatStone,
            false);

        c.Cylinder(tower, "Central_Tower_Top_Platform",
            basePos + new Vector3(0f, 19.45f, 0f),
            new Vector3(2.35f, 0.30f, 2.35f),
            c.MatStone);

        RingMerlons(c, tower, basePos + new Vector3(0f, 19.95f, 0f), 2.15f, 14);
        TowerRoofStack(c, tower, basePos + new Vector3(0f, 20.75f, 0f), 1.55f, 1.75f);

        // Вход с уровня верхней галереи.
        c.Cube(tower, "Central_Tower_Door_Frame",
            basePos + new Vector3(0f, 4.0f, -2.82f),
            new Vector3(1.95f, 2.6f, 0.22f),
            c.MatStoneLight,
            false);

        c.Cube(tower, "Central_Tower_Door_Opening",
            basePos + new Vector3(0f, 3.6f, -2.94f),
            new Vector3(1.05f, 1.8f, 0.08f),
            c.MatDark,
            false);

        // Окна центральной башни.
        for (int level = 0; level < 4; level++)
        {
            float y = 6.0f + level * 3.2f;

            c.Cube(tower, "Central_Tower_Window_Front_" + level,
                basePos + new Vector3(0f, y, -2.88f),
                new Vector3(0.28f, 1.15f, 0.08f),
                c.MatDark,
                false);

            c.Cube(tower, "Central_Tower_Window_Left_" + level,
                basePos + new Vector3(-2.88f, y + 0.35f, 0f),
                new Vector3(0.08f, 1.15f, 0.28f),
                c.MatDark,
                false);

            c.Cube(tower, "Central_Tower_Window_Right_" + level,
                basePos + new Vector3(2.88f, y + 0.15f, 0f),
                new Vector3(0.08f, 1.15f, 0.28f),
                c.MatDark,
                false);
        }

        // Флаг на вершине.
        c.Cube(tower, "Central_Tower_Flag_Pole",
            basePos + new Vector3(0f, 23.05f, 0f),
            new Vector3(0.09f, 2.5f, 0.09f),
            c.MatWood,
            false);

        c.Cube(tower, "Central_Tower_Red_Flag",
            basePos + new Vector3(0.72f, 23.9f, 0f),
            new Vector3(1.45f, 0.62f, 0.06f),
            c.MatRed,
            false);
    }

    private static void MiniKeepTower(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, string name)
    {
        GameObject tower = c.Child(parent, name);

        c.Cylinder(tower, "Mini_Turret_Body", basePos + new Vector3(0f, 4.1f, 0f), new Vector3(0.75f, 4.1f, 0.75f), c.MatStoneLight);
        c.Cylinder(tower, "Mini_Turret_Top", basePos + new Vector3(0f, 8.3f, 0f), new Vector3(0.9f, 0.16f, 0.9f), c.MatStone, false);
        TowerRoofStack(c, tower, basePos + new Vector3(0f, 8.55f, 0f), 0.65f, 0.7f);
    }

    private static void WindowWithFrame(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front, string name)
    {
        GameObject window = c.Child(parent, name);

        Vector3 glassScale = front ? new Vector3(0.8f, 1.25f, 0.08f) : new Vector3(0.08f, 1.25f, 0.8f);
        Vector3 frameH = front ? new Vector3(1.15f, 0.12f, 0.10f) : new Vector3(0.10f, 0.12f, 1.15f);
        Vector3 frameV = front ? new Vector3(0.12f, 1.45f, 0.10f) : new Vector3(0.10f, 1.45f, 0.12f);

        c.Cube(window, "Dark_Window_Glass", pos, glassScale, c.MatDark, false);
        c.Cube(window, "Frame_Top", pos + new Vector3(0f, 0.72f, 0f), frameH, c.MatStoneLight, false);
        c.Cube(window, "Frame_Bottom", pos + new Vector3(0f, -0.72f, 0f), frameH, c.MatStoneLight, false);

        if (front)
        {
            c.Cube(window, "Frame_Left", pos + new Vector3(-0.53f, 0f, 0f), frameV, c.MatStoneLight, false);
            c.Cube(window, "Frame_Right", pos + new Vector3(0.53f, 0f, 0f), frameV, c.MatStoneLight, false);
        }
        else
        {
            c.Cube(window, "Frame_Left", pos + new Vector3(0f, 0f, -0.53f), frameV, c.MatStoneLight, false);
            c.Cube(window, "Frame_Right", pos + new Vector3(0f, 0f, 0.53f), frameV, c.MatStoneLight, false);
        }
    }


    private static void BuildMajesticUpperExpansionV4(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject upper = c.Child(keep, "Majestic_Upper_Expansion_PRO_V7");

        Material roofMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.17f, 0.09f, 0.06f));

        // Задняя королевская масса донжона — крупная, но упорядоченная.
        c.Cube(upper, "Rear_High_Keep_Block",
            new Vector3(0f, 11.85f, 15.3f),
            new Vector3(13.8f, 7.1f, 3.4f),
            c.MatStone);

        c.Cube(upper, "Rear_High_Keep_Cornice",
            new Vector3(0f, 15.55f, 15.3f),
            new Vector3(14.8f, 0.32f, 3.85f),
            c.MatStoneLight,
            false);

        PitchedRoof(c, upper, "Rear_High_Keep_Roof",
            new Vector3(0f, 15.9f, 15.3f),
            14.8f, 4.45f, 2.65f, roofMat);

        TallSquareKeepTower(c, upper, new Vector3(-8.3f, 0f, 14.8f), "Left_Rear_Square_Keep_Tower_PRO");
        TallSquareKeepTower(c, upper, new Vector3(8.3f, 0f, 14.8f), "Right_Rear_Square_Keep_Tower_PRO");

        // Центральная корона с отдельной крышей.
        c.Cube(upper, "Rear_Crown_Block",
            new Vector3(0f, 19.3f, 15.35f),
            new Vector3(5.8f, 3.1f, 2.35f),
            c.MatStoneLight);

        c.Cube(upper, "Rear_Crown_Window",
            new Vector3(0f, 19.25f, 14.05f),
            new Vector3(0.95f, 1.85f, 0.08f),
            c.MatDark,
            false);

        c.Cube(upper, "Rear_Crown_Cornice",
            new Vector3(0f, 21.0f, 15.35f),
            new Vector3(6.4f, 0.28f, 2.72f),
            c.MatStone,
            false);

        PitchedRoof(c, upper, "Rear_Crown_Roof",
            new Vector3(0f, 21.35f, 15.35f),
            6.4f, 3.0f, 1.65f, roofMat);

        for (int i = -3; i <= 3; i++)
        {
            if (i == 0) continue;
            c.Cube(upper, "Upper_Gothic_Window_" + i,
                new Vector3(i * 1.55f, 12.35f, 13.55f),
                new Vector3(0.32f, 1.75f, 0.08f),
                c.MatDark,
                false);
        }

        MerlonsX(c, upper, "Upper_Keep_Merlons_Front", -6.2f, 6.2f, 13.75f, 16.35f, 0.55f);
        MerlonsX(c, upper, "Upper_Keep_Merlons_Back", -6.2f, 6.2f, 16.85f, 16.35f, 0.55f);
    }

    private static void TallSquareKeepTower(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, string name)
    {
        GameObject tower = c.Child(parent, name);

        c.Cube(tower, "Square_Tower_Base",
            basePos + new Vector3(0f, 0.55f, 0f),
            new Vector3(2.8f, 0.65f, 2.8f),
            c.MatStone);

        c.Cube(tower, "Square_Tower_Body",
            basePos + new Vector3(0f, 8.1f, 0f),
            new Vector3(2.35f, 14.4f, 2.35f),
            c.MatStoneLight);

        c.Cube(tower, "Square_Tower_Top_Cornice",
            basePos + new Vector3(0f, 15.45f, 0f),
            new Vector3(3.0f, 0.28f, 3.0f),
            c.MatStone,
            false);

        MerlonsX(c, tower, name + "_Merlons_Front", basePos.x - 1.05f, basePos.x + 1.05f, basePos.z - 1.35f, 16.05f, 0.42f);
        MerlonsX(c, tower, name + "_Merlons_Back", basePos.x - 1.05f, basePos.x + 1.05f, basePos.z + 1.35f, 16.05f, 0.42f);

        TowerRoofStack(c, tower,
            basePos + new Vector3(0f, 16.25f, 0f),
            0.85f, 1.0f);

        for (int level = 0; level < 4; level++)
        {
            c.Cube(tower, "Square_Tower_Slit_" + level,
                basePos + new Vector3(0f, 4.8f + level * 2.45f, -1.22f),
                new Vector3(0.18f, 1.05f, 0.06f),
                c.MatDark,
                false);
        }
    }

    private static void BuildFortressSilhouettePolishV6(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject polish = c.Child(keep, "Fortress_Silhouette_Polish_V6");

        Material roofMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.16f, 0.09f, 0.06f));

        // Боковые шпили вокруг главного объёма — добавляют величие без расширения карты.
        SlenderSpire(c, polish, new Vector3(-8.9f, 0f, 5.0f), 13.4f, "Front_Left_High_Spire", roofMat);
        SlenderSpire(c, polish, new Vector3(8.9f, 0f, 5.0f), 13.4f, "Front_Right_High_Spire", roofMat);
        SlenderSpire(c, polish, new Vector3(-8.9f, 0f, 13.7f), 14.4f, "Back_Left_High_Spire", roofMat);
        SlenderSpire(c, polish, new Vector3(8.9f, 0f, 13.7f), 14.4f, "Back_Right_High_Spire", roofMat);

        // Длинные декоративные карнизы делают фасад богаче.
        c.Cube(polish, "Front_Keep_Upper_Stone_Line",
            new Vector3(0f, 9.35f, 3.08f),
            new Vector3(15.8f, 0.20f, 0.18f),
            c.MatStoneLight,
            false);

        c.Cube(polish, "Front_Keep_Lower_Stone_Line",
            new Vector3(0f, 6.95f, 3.06f),
            new Vector3(15.8f, 0.20f, 0.18f),
            c.MatStoneLight,
            false);

        // Небольшие декоративные окна, чтобы фасад не был плоским.
        for (int i = -3; i <= 3; i++)
        {
            if (i == 0) continue;

            c.Cube(polish, "Keep_Facade_Narrow_Window_" + i,
                new Vector3(i * 1.9f, 7.85f, 3.03f),
                new Vector3(0.26f, 1.15f, 0.07f),
                c.MatDark,
                false);
        }
    }

    private static void SlenderSpire(CastleGenerator.CastleContext c, GameObject parent, Vector3 basePos, float height, string name, Material roofMat)
    {
        GameObject spire = c.Child(parent, name);

        c.Cylinder(spire, "Spire_Base",
            basePos + new Vector3(0f, height - 1.6f, 0f),
            new Vector3(0.55f, 0.22f, 0.55f),
            c.MatStone,
            false);

        c.Cylinder(spire, "Spire_Body",
            basePos + new Vector3(0f, height - 0.85f, 0f),
            new Vector3(0.34f, 0.72f, 0.34f),
            c.MatStoneLight,
            false);

        c.Cylinder(spire, "Spire_Tip",
            basePos + new Vector3(0f, height + 0.05f, 0f),
            new Vector3(0.16f, 0.75f, 0.16f),
            roofMat,
            false);
    }

    private static void BuildCastleScaleV7(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject scale = c.Child(keep, "Castle_Scale_And_Silhouette_PRO_V7");

        Material roofMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.15f, 0.08f, 0.055f));

        // Высокие шпили у донжона — добавляют силуэт, но не мешают проходам.
        SlenderSpire(c, scale, new Vector3(-9.2f, 0f, 4.8f), 15.4f, "Front_Left_Royal_Spire", roofMat);
        SlenderSpire(c, scale, new Vector3(9.2f, 0f, 4.8f), 15.4f, "Front_Right_Royal_Spire", roofMat);
        SlenderSpire(c, scale, new Vector3(-9.2f, 0f, 14.1f), 16.4f, "Back_Left_Royal_Spire", roofMat);
        SlenderSpire(c, scale, new Vector3(9.2f, 0f, 14.1f), 16.4f, "Back_Right_Royal_Spire", roofMat);

        // Вертикальные фасадные линии — замок выглядит выше.
        for (int i = -3; i <= 3; i++)
        {
            if (i == 0) continue;

            c.Cube(scale, "Keep_Facade_Vertical_Rib_" + i,
                new Vector3(i * 1.85f, 6.5f, 3.02f),
                new Vector3(0.16f, 4.7f, 0.08f),
                c.MatStoneLight,
                false);
        }

        c.Cube(scale, "Keep_Facade_Upper_Line",
            new Vector3(0f, 9.55f, 3.0f),
            new Vector3(16.4f, 0.18f, 0.16f),
            c.MatStoneLight,
            false);

        c.Cube(scale, "Keep_Facade_Lower_Line",
            new Vector3(0f, 6.75f, 3.0f),
            new Vector3(16.4f, 0.18f, 0.16f),
            c.MatStoneLight,
            false);
    }

    private static void BuildKeepGrandPlinth(CastleGenerator.CastleContext c, GameObject keep)
    {
        c.Cube(keep, "Keep_Grand_Plinth",
            new Vector3(0f, 0.42f, 9.0f),
            new Vector3(19.8f, 0.62f, 13.9f),
            c.MatStone);

        c.Cube(keep, "Keep_Grand_Plinth_Front_Lip",
            new Vector3(0f, 0.72f, 2.15f),
            new Vector3(9.0f, 0.18f, 0.85f),
            c.MatStoneLight,
            false);

        c.Cube(keep, "Keep_Grand_Plinth_Left_Lip",
            new Vector3(-9.55f, 0.72f, 9f),
            new Vector3(0.85f, 0.18f, 11.9f),
            c.MatStoneLight,
            false);

        c.Cube(keep, "Keep_Grand_Plinth_Right_Lip",
            new Vector3(9.55f, 0.72f, 9f),
            new Vector3(0.85f, 0.18f, 11.9f),
            c.MatStoneLight,
            false);
    }

    private static void BuildGrandEntranceStair(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject stair = c.Child(keep, "Grand_Keep_Entrance_Stair");

        c.Cube(stair, "Entrance_Landing",
            new Vector3(0f, 0.62f, 2.85f),
            new Vector3(5.8f, 0.24f, 1.55f),
            c.MatStoneLight);

        for (int i = 0; i < 7; i++)
        {
            float y = 0.08f + i * 0.15f;
            float z = 0.85f + i * 0.32f;
            float depth = 2.95f - i * 0.18f;

            c.Cube(stair, "Grand_Stair_Step_" + i,
                new Vector3(0f, y, z),
                new Vector3(5.4f, 0.15f, depth),
                c.MatStone);
        }

        c.Cube(stair, "Stair_Left_Parapet",
            new Vector3(-3.1f, 0.72f, 1.85f),
            new Vector3(0.35f, 0.75f, 2.55f),
            c.MatStoneLight,
            false);

        c.Cube(stair, "Stair_Right_Parapet",
            new Vector3(3.1f, 0.72f, 1.85f),
            new Vector3(0.35f, 0.75f, 2.55f),
            c.MatStoneLight,
            false);
    }

    private static void BuildFrontCentralTowerMass(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject mass = c.Child(keep, "Front_Central_Tower_Mass_PRO_V7");

        Material roofMat = c.Mode == CastleGenerator.LabMode.Lab3_Blockout
            ? c.MatStoneLight
            : c.NewMaterial(new Color(0.18f, 0.10f, 0.07f));

        // Главный фасадный донжон: высокий, широкий, читаемый с дороги.
        c.Cube(mass, "Front_Tower_Lower_Block",
            new Vector3(0f, 11.6f, 4.0f),
            new Vector3(10.8f, 6.9f, 4.0f),
            c.MatStone);

        c.Cube(mass, "Front_Tower_Lower_Cornice",
            new Vector3(0f, 15.05f, 4.0f),
            new Vector3(11.8f, 0.34f, 4.42f),
            c.MatStoneLight,
            false);

        c.Cube(mass, "Front_Tower_Middle_Block",
            new Vector3(0f, 18.0f, 4.0f),
            new Vector3(7.4f, 4.9f, 3.25f),
            c.MatStoneLight);

        c.Cube(mass, "Front_Tower_Middle_Cornice",
            new Vector3(0f, 20.55f, 4.0f),
            new Vector3(8.15f, 0.30f, 3.65f),
            c.MatStone,
            false);

        c.Cube(mass, "Front_Tower_Upper_Block",
            new Vector3(0f, 23.1f, 4.0f),
            new Vector3(5.2f, 4.0f, 2.75f),
            c.MatStoneLight);

        c.Cube(mass, "Front_Tower_Upper_Cornice",
            new Vector3(0f, 25.2f, 4.0f),
            new Vector3(6.0f, 0.30f, 3.15f),
            c.MatStone,
            false);

        // Мощные вертикальные контрфорсы по бокам.
        c.Cube(mass, "Front_Tower_Left_Buttress",
            new Vector3(-5.0f, 11.0f, 3.18f),
            new Vector3(0.95f, 6.1f, 1.45f),
            c.MatStoneLight);

        c.Cube(mass, "Front_Tower_Right_Buttress",
            new Vector3(5.0f, 11.0f, 3.18f),
            new Vector3(0.95f, 6.1f, 1.45f),
            c.MatStoneLight);

        c.Cube(mass, "Front_Tower_Left_Upper_Buttress",
            new Vector3(-3.55f, 18.0f, 3.05f),
            new Vector3(0.72f, 4.35f, 1.05f),
            c.MatStone);

        c.Cube(mass, "Front_Tower_Right_Upper_Buttress",
            new Vector3(3.55f, 18.0f, 3.05f),
            new Vector3(0.72f, 4.35f, 1.05f),
            c.MatStone);

        // Большие окна и рамки, чтобы башня не была пустой коробкой.
        c.Cube(mass, "Front_Tower_Main_Window_Frame",
            new Vector3(0f, 12.05f, 1.94f),
            new Vector3(2.05f, 3.65f, 0.14f),
            c.MatStoneLight,
            false);

        c.Cube(mass, "Front_Tower_Main_Window_Dark",
            new Vector3(0f, 12.05f, 1.84f),
            new Vector3(1.25f, 2.85f, 0.08f),
            c.MatDark,
            false);

        c.Cube(mass, "Front_Tower_Middle_Window_Dark",
            new Vector3(0f, 18.0f, 2.28f),
            new Vector3(1.1f, 2.3f, 0.08f),
            c.MatDark,
            false);

        for (int i = -2; i <= 2; i++)
        {
            if (i == 0) continue;
            c.Cube(mass, "Front_Tower_Slit_" + i,
                new Vector3(i * 1.55f, 18.1f, 2.30f),
                new Vector3(0.30f, 1.55f, 0.08f),
                c.MatDark,
                false);
        }

        MerlonsX(c, mass, "Front_Tower_Merlons_Front", -2.2f, 2.2f, 2.65f, 25.8f, 0.58f);
        MerlonsX(c, mass, "Front_Tower_Merlons_Back", -2.2f, 2.2f, 5.35f, 25.8f, 0.58f);

        PitchedRoof(c, mass, "Front_Tower_Main_Roof",
            new Vector3(0f, 26.1f, 4.0f),
            6.4f,
            3.7f,
            2.25f,
            roofMat);

        c.Cube(mass, "Front_Tower_Flag_Pole",
            new Vector3(0f, 28.75f, 4.0f),
            new Vector3(0.09f, 2.55f, 0.09f),
            c.MatWood,
            false);

        c.Cube(mass, "Front_Tower_Flag",
            new Vector3(0.85f, 29.55f, 4.0f),
            new Vector3(1.65f, 0.64f, 0.06f),
            c.MatRed,
            false);
    }

    private static void BuildThroneRearGallery(CastleGenerator.CastleContext c, GameObject keep)
    {
        GameObject gallery = c.Child(keep, "Throne_Hall_Clean_Side_Galleries_PRO_V7");

        // Центральный вид на трон свободный: галереи только вдоль стен.
        c.Cube(gallery, "Left_Side_Gallery_Floor",
            new Vector3(-7.05f, 4.55f, 9.3f),
            new Vector3(0.92f, 0.15f, 6.4f),
            c.MatStoneLight,
            false);

        c.Cube(gallery, "Right_Side_Gallery_Floor",
            new Vector3(7.05f, 4.55f, 9.3f),
            new Vector3(0.92f, 0.15f, 6.4f),
            c.MatStoneLight,
            false);

        c.Cube(gallery, "Left_Side_Gallery_Rail",
            new Vector3(-6.48f, 5.05f, 9.3f),
            new Vector3(0.10f, 0.62f, 6.4f),
            c.MatWood,
            false);

        c.Cube(gallery, "Right_Side_Gallery_Rail",
            new Vector3(6.48f, 5.05f, 9.3f),
            new Vector3(0.10f, 0.62f, 6.4f),
            c.MatWood,
            false);

        // Задний балкон только над троном, без тяжёлой поперечной балки.
        c.Cube(gallery, "Rear_Royal_Balcony_Floor",
            new Vector3(0f, 4.85f, 13.28f),
            new Vector3(5.0f, 0.15f, 0.95f),
            c.MatStoneLight,
            false);

        c.Cube(gallery, "Rear_Royal_Balcony_Rail",
            new Vector3(0f, 5.32f, 12.63f),
            new Vector3(5.0f, 0.56f, 0.10f),
            c.MatWood,
            false);

        // Четыре опорные колонны по углам, без частокола.
        Pillar(c, gallery, new Vector3(-7.35f, 0f, 6.7f));
        Pillar(c, gallery, new Vector3(-7.35f, 0f, 11.8f));
        Pillar(c, gallery, new Vector3(7.35f, 0f, 6.7f));
        Pillar(c, gallery, new Vector3(7.35f, 0f, 11.8f));
    }

    private static void BuildTowerSpiralSteps(CastleGenerator.CastleContext c, GameObject keep, Vector3 towerBase)
    {
        GameObject stair = c.Child(keep, "Royal_Interior_Wall_Stairs_PRO_V7");

        // Боковые марши вдоль стен, а не в центральной зоне.
        for (int i = 0; i < 10; i++)
        {
            c.Cube(stair, "Left_Gallery_Stair_Step_" + i,
                new Vector3(-7.22f, 0.15f + i * 0.40f, 5.25f + i * 0.54f),
                new Vector3(1.05f, 0.15f, 0.60f),
                c.MatStoneLight,
                false);

            c.Cube(stair, "Right_Gallery_Stair_Step_" + i,
                new Vector3(7.22f, 0.15f + i * 0.40f, 5.25f + i * 0.54f),
                new Vector3(1.05f, 0.15f, 0.60f),
                c.MatStoneLight,
                false);
        }

        c.Cube(stair, "Left_Gallery_Stair_Rail",
            new Vector3(-6.64f, 2.25f, 8.0f),
            new Vector3(0.09f, 3.5f, 5.2f),
            c.MatWood,
            false);

        c.Cube(stair, "Right_Gallery_Stair_Rail",
            new Vector3(6.64f, 2.25f, 8.0f),
            new Vector3(0.09f, 3.5f, 5.2f),
            c.MatWood,
            false);

        // Короткий подъём к дверце центральной башни.
        for (int i = 0; i < 4; i++)
        {
            c.Cube(stair, "Tower_Door_Short_Step_" + i,
                towerBase + new Vector3(0f, 4.25f + i * 0.22f, -2.1f + i * 0.32f),
                new Vector3(1.2f, 0.12f, 0.45f),
                c.MatStoneLight,
                false);
        }
    }

    // ─────────────────────────────────────────────────────────
    // Подземелье
    // ─────────────────────────────────────────────────────────

    private static void BuildDungeonEntrance(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject dungeon = c.Child(parent, "Dungeon_Detailed_Entrance_And_Cell");

        c.Cube(dungeon, "Dungeon_Dark_Opening", new Vector3(-12f, 0.12f, 7.8f), new Vector3(4.4f, 0.08f, 3.2f), c.MatDark, false);
        c.Cube(dungeon, "Dungeon_Frame_Back", new Vector3(-12f, 1.35f, 9.25f), new Vector3(4.8f, 2.7f, 0.35f), c.MatStone);
        c.Cube(dungeon, "Dungeon_Frame_Left", new Vector3(-14.35f, 1.35f, 7.8f), new Vector3(0.35f, 2.7f, 3.2f), c.MatStone);
        c.Cube(dungeon, "Dungeon_Frame_Right", new Vector3(-9.65f, 1.35f, 7.8f), new Vector3(0.35f, 2.7f, 3.2f), c.MatStone);
        c.Cube(dungeon, "Dungeon_Frame_Top", new Vector3(-12f, 2.75f, 7.8f), new Vector3(4.9f, 0.35f, 3.2f), c.MatStoneLight);

        for (int i = 0; i < 9; i++)
        {
            c.Cube(dungeon, "Dungeon_Stair_" + i,
                new Vector3(-12f, 0.08f - i * 0.18f, 6.2f + i * 0.42f),
                new Vector3(3.5f, 0.16f, 0.42f), c.MatWood);
        }

        // Камера-тюрьма.
        c.Cube(dungeon, "Small_Cell_Floor", new Vector3(-12f, -1.35f, 11.8f), new Vector3(5.5f, 0.22f, 4.2f), c.MatFloor);
        c.Cube(dungeon, "Small_Cell_Back_Wall", new Vector3(-12f, 0.2f, 13.8f), new Vector3(5.5f, 3.1f, 0.35f), c.MatStone);
        c.Cube(dungeon, "Small_Cell_Left_Wall", new Vector3(-14.75f, 0.2f, 11.8f), new Vector3(0.35f, 3.1f, 4.2f), c.MatStone);
        c.Cube(dungeon, "Small_Cell_Right_Wall", new Vector3(-9.25f, 0.2f, 11.8f), new Vector3(0.35f, 3.1f, 4.2f), c.MatStone);
        c.Cube(dungeon, "Small_Cell_Ceiling", new Vector3(-12f, 1.85f, 11.8f), new Vector3(5.5f, 0.28f, 4.2f), c.MatStone);
        c.Cube(dungeon, "Prisoner_Blockout_Marker", new Vector3(-12f, -0.35f, 12.3f), new Vector3(0.55f, 1.8f, 0.55f), c.MatStoneLight);
    }

    // ─────────────────────────────────────────────────────────
    // Общие помощники
    // ─────────────────────────────────────────────────────────

    private static void Pillar(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        GameObject pillar = c.Child(parent, "Stone_Pillar_Detailed");

        c.Cube(pillar, "Pillar_Base", pos + new Vector3(0f, 0.22f, 0f), new Vector3(1.05f, 0.42f, 1.05f), c.MatStoneLight);
        c.Cylinder(pillar, "Pillar_Shaft", pos + new Vector3(0f, 2.05f, 0f), new Vector3(0.38f, 1.75f, 0.38f), c.MatStoneLight);
        c.Cube(pillar, "Pillar_Cap", pos + new Vector3(0f, 3.9f, 0f), new Vector3(1.05f, 0.35f, 1.05f), c.MatStoneLight);
    }

    private static void MerlonsX(CastleGenerator.CastleContext c, GameObject parent, string prefix, float x1, float x2, float z, float y, float size)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(x2 - x1) / 1.45f));

        for (int i = 0; i <= count; i++)
        {
            float t = i / (float)count;
            float x = Mathf.Lerp(x1, x2, t);
            c.Cube(parent, prefix + "_" + i, new Vector3(x, y, z), new Vector3(size, 0.9f, 0.9f), c.MatStone);
        }
    }

    private static void MerlonsZ(CastleGenerator.CastleContext c, GameObject parent, string prefix, float x, float z1, float z2, float y, float size)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(z2 - z1) / 1.45f));

        for (int i = 0; i <= count; i++)
        {
            float t = i / (float)count;
            float z = Mathf.Lerp(z1, z2, t);
            c.Cube(parent, prefix + "_" + i, new Vector3(x, y, z), new Vector3(0.9f, 0.9f, size), c.MatStone);
        }
    }

    private static void RingMerlons(CastleGenerator.CastleContext c, GameObject parent, Vector3 center, float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            GameObject merlon = c.Cube(parent, "Round_Merlon_" + i, pos, new Vector3(0.55f, 0.82f, 0.55f), c.MatStone);
            merlon.transform.LookAt(center);
        }
    }

    private static void PitchedRoof(CastleGenerator.CastleContext c, GameObject parent, string name, Vector3 baseCenter, float width, float depth, float height, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.position = baseCenter;

        Mesh mesh = new Mesh();
        float w = width * 0.5f;
        float d = depth * 0.5f;

        Vector3[] vertices =
        {
            new Vector3(-w, 0f, -d),
            new Vector3(w, 0f, -d),
            new Vector3(0f, height, -d),

            new Vector3(-w, 0f, d),
            new Vector3(w, 0f, d),
            new Vector3(0f, height, d)
        };

        int[] triangles =
        {
            0, 2, 1, // front gable
            3, 4, 5, // back gable
            0, 3, 5, 0, 5, 2, // left roof side
            1, 2, 5, 1, 5, 4, // right roof side
            0, 1, 4, 0, 4, 3  // bottom
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = material;

        // Крыша без коллайдера: она визуальная и не мешает игроку.
    }
}
