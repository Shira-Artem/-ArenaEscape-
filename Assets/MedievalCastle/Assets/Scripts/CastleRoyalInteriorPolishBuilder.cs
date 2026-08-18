using UnityEngine;

/// <summary>
/// Castle Royal Interior Polish V7.
/// Дополнительная полировка замка без перегруза базовых файлов.
/// Этот builder вызывается из CastleDecorBuilder.Build(c).
/// </summary>
public static class CastleRoyalInteriorPolishBuilder
{
    public static void Build(CastleGenerator.CastleContext c)
    {
        GameObject parent = c.Child(c.Root, "03_CASTLE_ROYAL_POLISH_V7");

        BuildExteriorFacadePolish(c, parent);
        BuildCourtyardCompositionPolish(c, parent);
        BuildThroneHallCompositionPolish(c, parent);
        BuildNavigationMarkers(c, parent);
    }

    private static void BuildExteriorFacadePolish(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject facade = c.Child(parent, "Exterior_Facade_Polish");

        Material gold = c.NewMaterial(new Color(0.92f, 0.68f, 0.16f));
        Material ivy = c.NewMaterial(new Color(0.08f, 0.30f, 0.10f));

        // Глубокий парадный портал перед донжоном.
        c.Cube(facade, "Keep_Entrance_Outer_Arch_Left",
            new Vector3(-2.05f, 2.55f, 2.52f),
            new Vector3(0.55f, 4.1f, 0.22f),
            c.MatStoneLight,
            false);

        c.Cube(facade, "Keep_Entrance_Outer_Arch_Right",
            new Vector3(2.05f, 2.55f, 2.52f),
            new Vector3(0.55f, 4.1f, 0.22f),
            c.MatStoneLight,
            false);

        c.Cube(facade, "Keep_Entrance_Outer_Arch_Top",
            new Vector3(0f, 4.65f, 2.52f),
            new Vector3(4.65f, 0.55f, 0.22f),
            c.MatStoneLight,
            false);

        c.Cube(facade, "Keep_Entrance_Dark_Depth",
            new Vector3(0f, 2.35f, 2.42f),
            new Vector3(2.75f, 3.65f, 0.10f),
            c.MatDark,
            false);

        // Герб над входом.
        c.Cube(facade, "Royal_Crest_Shield",
            new Vector3(0f, 5.65f, 2.40f),
            new Vector3(1.25f, 1.45f, 0.08f),
            c.MatRed,
            false);

        c.Cube(facade, "Royal_Crest_Gold_Vertical",
            new Vector3(0f, 5.65f, 2.33f),
            new Vector3(0.22f, 1.25f, 0.05f),
            gold,
            false);

        c.Cube(facade, "Royal_Crest_Gold_Horizontal",
            new Vector3(0f, 5.65f, 2.32f),
            new Vector3(1.02f, 0.22f, 0.05f),
            gold,
            false);

        // Аккуратный плющ по краям фасада.
        for (int i = 0; i < 12; i++)
        {
            float y = 3.0f + i * 0.35f;
            c.Cube(facade, "Ivy_Left_" + i,
                new Vector3(-7.78f + Mathf.Sin(i * 1.4f) * 0.28f, y, 3.02f),
                new Vector3(0.22f, 0.16f, 0.04f),
                ivy,
                false);

            c.Cube(facade, "Ivy_Right_" + i,
                new Vector3(7.78f + Mathf.Sin(i * 1.6f) * 0.28f, y, 3.02f),
                new Vector3(0.22f, 0.16f, 0.04f),
                ivy,
                false);
        }
    }

    private static void BuildCourtyardCompositionPolish(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject yard = c.Child(parent, "Courtyard_Composition_Polish");

        Material cleanStone = c.NewMaterial(new Color(0.47f, 0.44f, 0.37f));
        Material darkStone = c.NewMaterial(new Color(0.28f, 0.29f, 0.28f));

        // Центральная ось двора: визуально чистая дорога.
        c.Cube(yard, "Royal_Courtyard_Main_Axis",
            new Vector3(0f, 0.19f, -4.0f),
            new Vector3(4.8f, 0.04f, 12.8f),
            cleanStone,
            false);

        c.Cube(yard, "Royal_Courtyard_Left_Border",
            new Vector3(-2.55f, 0.25f, -4.0f),
            new Vector3(0.13f, 0.16f, 12.8f),
            darkStone,
            false);

        c.Cube(yard, "Royal_Courtyard_Right_Border",
            new Vector3(2.55f, 0.25f, -4.0f),
            new Vector3(0.13f, 0.16f, 12.8f),
            darkStone,
            false);

        // Маленькие стражевые тумбы по маршруту.
        GuardPost(c, yard, new Vector3(-3.3f, 0f, -8.8f));
        GuardPost(c, yard, new Vector3(3.3f, 0f, -8.8f));
        GuardPost(c, yard, new Vector3(-3.3f, 0f, -2.2f));
        GuardPost(c, yard, new Vector3(3.3f, 0f, -2.2f));
    }

    private static void BuildThroneHallCompositionPolish(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject hall = c.Child(parent, "Throne_Hall_Composition_Polish");

        Material gold = c.NewMaterial(new Color(0.92f, 0.68f, 0.16f));
        Material darkRed = c.NewMaterial(new Color(0.42f, 0.04f, 0.04f));
        Material darkWood = c.NewMaterial(new Color(0.18f, 0.10f, 0.06f));

        // Чистый ковёр с золотой рамкой.
        c.Cube(hall, "Royal_Carpet_Overlay",
            new Vector3(0f, 0.36f, 8.25f),
            new Vector3(2.55f, 0.035f, 8.9f),
            darkRed,
            false);

        c.Cube(hall, "Royal_Carpet_Left_Gold_Border",
            new Vector3(-1.42f, 0.39f, 8.25f),
            new Vector3(0.10f, 0.035f, 8.9f),
            gold,
            false);

        c.Cube(hall, "Royal_Carpet_Right_Gold_Border",
            new Vector3(1.42f, 0.39f, 8.25f),
            new Vector3(0.10f, 0.035f, 8.9f),
            gold,
            false);

        // Королевский балдахин над троном.
        c.Cube(hall, "Throne_Canopy_Back",
            new Vector3(0f, 3.55f, 13.42f),
            new Vector3(3.4f, 3.6f, 0.18f),
            darkRed,
            false);

        c.Cube(hall, "Throne_Canopy_Top",
            new Vector3(0f, 5.55f, 12.82f),
            new Vector3(4.1f, 0.18f, 1.55f),
            darkWood,
            false);

        c.Cube(hall, "Throne_Canopy_Gold_Line",
            new Vector3(0f, 5.18f, 13.28f),
            new Vector3(3.4f, 0.12f, 0.08f),
            gold,
            false);

        // Статуи/рыцарские фигуры по бокам трона.
        KnightStatue(c, hall, new Vector3(-4.45f, 0f, 12.25f), "Left_Royal_Guard_Statue");
        KnightStatue(c, hall, new Vector3(4.45f, 0f, 12.25f), "Right_Royal_Guard_Statue");

        // Высокие стеновые панели делают интерьер дороже.
        WallPanel(c, hall, new Vector3(-7.86f, 3.25f, 7.2f), false, "Left_Wall_Panel_A");
        WallPanel(c, hall, new Vector3(-7.86f, 3.25f, 10.4f), false, "Left_Wall_Panel_B");
        WallPanel(c, hall, new Vector3(7.86f, 3.25f, 7.2f), false, "Right_Wall_Panel_A");
        WallPanel(c, hall, new Vector3(7.86f, 3.25f, 10.4f), false, "Right_Wall_Panel_B");

        // Верхний свет — высоко, без визуальной каши.
        c.Cube(hall, "High_Chandelier_Chain",
            new Vector3(0f, 7.55f, 9.4f),
            new Vector3(0.06f, 1.7f, 0.06f),
            c.MatIron,
            false);

        c.Cylinder(hall, "High_Chandelier_Ring",
            new Vector3(0f, 6.55f, 9.4f),
            new Vector3(0.65f, 0.04f, 0.65f),
            c.MatIron,
            false);

        c.PointLight(hall, "Royal_Hall_Upper_Warm_Light",
            new Vector3(0f, 6.35f, 9.4f),
            new Color(1f, 0.55f, 0.18f),
            0.75f,
            5.5f,
            true);
    }

    private static void BuildNavigationMarkers(CastleGenerator.CastleContext c, GameObject parent)
    {
        GameObject nav = c.Child(parent, "Subtle_Navigation_Markers");

        Material gold = c.NewMaterial(new Color(0.85f, 0.62f, 0.16f));

        // Ненавязчивые маркеры маршрута, чтобы скрины были понятнее.
        c.Cube(nav, "Route_Marker_To_Throne",
            new Vector3(0f, 0.44f, 1.7f),
            new Vector3(1.8f, 0.05f, 0.22f),
            gold,
            false);

        c.Cube(nav, "Route_Marker_To_Dungeon",
            new Vector3(-9.4f, 0.44f, 7.0f),
            new Vector3(1.4f, 0.05f, 0.20f),
            gold,
            false);
    }

    private static void GuardPost(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos)
    {
        c.Cube(parent, "Guard_Post_Base",
            pos + new Vector3(0f, 0.28f, 0f),
            new Vector3(0.38f, 0.56f, 0.38f),
            c.MatStoneLight,
            false);

        c.Cube(parent, "Guard_Post_Fire",
            pos + new Vector3(0f, 0.72f, 0f),
            new Vector3(0.22f, 0.28f, 0.22f),
            c.MatFire,
            false);

        c.PointLight(parent, "Guard_Post_Light",
            pos + new Vector3(0f, 0.95f, 0f),
            new Color(1f, 0.45f, 0.10f),
            0.38f,
            2.8f,
            true);
    }

    private static void KnightStatue(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, string name)
    {
        GameObject statue = c.Child(parent, name);

        Material armor = c.NewMaterial(new Color(0.48f, 0.50f, 0.50f));

        c.Cube(statue, "Statue_Base",
            pos + new Vector3(0f, 0.25f, 0f),
            new Vector3(0.9f, 0.50f, 0.75f),
            c.MatStoneLight,
            false);

        c.Cube(statue, "Statue_Body",
            pos + new Vector3(0f, 1.05f, 0f),
            new Vector3(0.55f, 1.2f, 0.35f),
            armor,
            false);

        c.Cube(statue, "Statue_Head",
            pos + new Vector3(0f, 1.85f, 0f),
            new Vector3(0.38f, 0.38f, 0.38f),
            armor,
            false);

        c.Cube(statue, "Statue_Sword",
            pos + new Vector3(0.48f, 1.05f, -0.05f),
            new Vector3(0.08f, 1.45f, 0.08f),
            c.MatIron,
            false);
    }

    private static void WallPanel(CastleGenerator.CastleContext c, GameObject parent, Vector3 pos, bool front, string name)
    {
        GameObject panel = c.Child(parent, name);

        Material panelMat = c.NewMaterial(new Color(0.32f, 0.31f, 0.29f));

        Vector3 panelScale = front ? new Vector3(1.2f, 2.7f, 0.06f) : new Vector3(0.06f, 2.7f, 1.2f);
        Vector3 lineH = front ? new Vector3(1.3f, 0.08f, 0.07f) : new Vector3(0.07f, 0.08f, 1.3f);
        Vector3 lineV = front ? new Vector3(0.08f, 2.8f, 0.07f) : new Vector3(0.07f, 2.8f, 0.08f);

        c.Cube(panel, "Panel_Back", pos, panelScale, panelMat, false);
        c.Cube(panel, "Panel_Top", pos + Vector3.up * 1.4f, lineH, c.MatStoneLight, false);
        c.Cube(panel, "Panel_Bottom", pos + Vector3.down * 1.4f, lineH, c.MatStoneLight, false);
        c.Cube(panel, "Panel_Center", pos, lineV, c.MatStoneLight, false);
    }
}
