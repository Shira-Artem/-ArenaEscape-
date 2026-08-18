using UnityEngine;

/// <summary>
/// Owns village ground, camp identity and the perimeter palisade. Primary combat
/// composition deliberately lives in L0OrcArenaCombatLayoutModule.
/// </summary>
public static class L0OrcArenaCampModule
{
    // Real asset paths for arena dressing
    static readonly string DeadTree = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_dead.prefab";
    static readonly string Stump = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_stump.prefab";
    static readonly string DeadShrub = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Shrubs/PT_Generic_Shrub_01_dead.prefab";
    // STEP 7.6 — cohesive dark-fortress dressing: Synty rocks + Dungeon props (replaces PT/PP rocks, fantasy mushrooms, wooden fences)
    static readonly string SyntyRockA = "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Rock_03.prefab";
    static readonly string SyntyRockB = "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Rock_05.prefab";
    static readonly string SyntyRockC = "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Rock_07.prefab";
    static readonly string SyntyRockD = "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Rock_09.prefab";
    static readonly string SyntyPebbles = "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Rock_Pebbles_02.prefab";
    static readonly string DunBarrel = "Assets/DungeonAssetPack/Prefabs/Props/Barrel.prefab";
    static readonly string DunBox = "Assets/DungeonAssetPack/Prefabs/Props/Box.prefab";
    static readonly string DunOpenBox = "Assets/DungeonAssetPack/Prefabs/Props/Opened_Box.prefab";
    static readonly string DunBrokenColumn = "Assets/DungeonAssetPack/Prefabs/Props/Broken_Column.prefab";
    static readonly string DunBrickPile = "Assets/DungeonAssetPack/Prefabs/Props/Brick_Pile.prefab";
    static readonly string DunStone = "Assets/DungeonAssetPack/Prefabs/Props/Stone.prefab";
    static readonly string DunBanner = "Assets/DungeonAssetPack/Prefabs/Props/Banner.prefab";
    static readonly string DunCandelabrum = "Assets/DungeonAssetPack/Prefabs/Props/Candelabrum.prefab";
    static readonly string DunSwordStand = "Assets/DungeonAssetPack/Prefabs/Props/SwordStand_1.prefab";
    static readonly string Smoke1 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab";
    static readonly string Smoke2 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2.prefab";
    static readonly string Smoke4 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 4.prefab";
    static readonly string WeaponSpear = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/spear-1.prefab";
    static readonly string WeaponAxe = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Axe.prefab";
    static readonly string WeaponSword = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Sword.prefab";
    static readonly string WeaponShield = "Assets/New Folder/Low Poly Modular Medieval Weapons/Prefabs/shield/shield 2.prefab";
    static readonly string WeaponMaul = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/Maul.prefab";

    public static void Build(Transform battleZone, Transform outerCampRing, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        if (battleZone == null || outerCampRing == null || placementGuard == null)
            return;

        BuildGround(battleZone);
        BuildCampIdentity(outerCampRing, createAtmosphere, placementGuard);
        BuildArenaFortifications(outerCampRing, createAtmosphere, placementGuard);
        BuildPalisade(outerCampRing, placementGuard);
        BuildRealAssetDressing(battleZone, createAtmosphere);
    }

    public static Vector3[] GetFireAudioPoints()
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 entrance = L0OrcArenaConfig.BaseEntrance;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 side = L0OrcArenaConfig.SideDir;

        return new[]
        {
            center,
            entrance + side * -15.8f,
            entrance + side * 15.8f,
            portal + side * -12f,
            portal + side * 12f,
        };
    }

    /// <summary>
    /// STEP 7.6 — cohesive dark-fortress dressing spread across the arena: dead trees, Synty boulders,
    /// Dungeon rubble (broken columns / brick piles), supply stacks (crates+barrels), banners,
    /// candelabra, sword stands, scattered stones, weapons and smoke. Purely decorative (no colliders),
    /// ignores PlacementGuard — visual fill. Replaces the old PT/PP rocks, fantasy mushrooms and wood fences.
    /// </summary>
    private static void BuildRealAssetDressing(Transform parent, bool createAtmosphere)
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 s = L0OrcArenaConfig.SideDir;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 b = L0OrcArenaConfig.BackDir;

        // === МЁРТВЫЕ ДЕРЕВЬЯ — скелеты по периметру арены, создают силуэт ===
        // Левая сторона
        L0Util.Place(DeadTree, c + s * -75f + g * 30f, Q(0f, 30f, 0f), 2.0f, parent, "ArenaDead_L1");
        L0Util.Place(DeadTree, c + s * -85f + g * -5f, Q(0f, 150f, 0f), 2.2f, parent, "ArenaDead_L2");
        L0Util.Place(DeadTree, c + s * -70f + b * 40f, Q(0f, 240f, 0f), 1.8f, parent, "ArenaDead_L3");
        L0Util.Place(DeadTree, c + s * -60f + b * 70f, Q(0f, 80f, 0f), 2.1f, parent, "ArenaDead_L4");
        // Правая сторона
        L0Util.Place(DeadTree, c + s * 78f + g * 25f, Q(0f, 200f, 0f), 1.9f, parent, "ArenaDead_R1");
        L0Util.Place(DeadTree, c + s * 88f + g * -8f, Q(0f, 320f, 0f), 2.0f, parent, "ArenaDead_R2");
        L0Util.Place(DeadTree, c + s * 72f + b * 35f, Q(0f, 110f, 0f), 2.3f, parent, "ArenaDead_R3");
        L0Util.Place(DeadTree, c + s * 65f + b * 65f, Q(0f, 260f, 0f), 1.7f, parent, "ArenaDead_R4");
        // Задняя сторона (за порталом)
        L0Util.Place(DeadTree, c + b * 90f + s * -30f, Q(0f, 50f, 0f), 2.4f, parent, "ArenaDead_B1");
        L0Util.Place(DeadTree, c + b * 95f + s * 25f, Q(0f, 170f, 0f), 2.0f, parent, "ArenaDead_B2");
        L0Util.Place(DeadTree, c + b * 85f, Q(0f, 300f, 0f), 2.5f, parent, "ArenaDead_B3");

        // Пни ближе к центру — орки расчистили бой-зону
        L0Util.Place(Stump, c + s * -35f + g * 20f, Q(0f, 60f, 0f), 1.0f, parent, "ArenaStump_1");
        L0Util.Place(Stump, c + s * 40f + g * 15f, Q(0f, 180f, 0f), 0.9f, parent, "ArenaStump_2");
        L0Util.Place(Stump, c + s * -30f + b * 25f, Q(0f, 270f, 0f), 1.1f, parent, "ArenaStump_3");
        L0Util.Place(Stump, c + s * 35f + b * 30f, Q(0f, 120f, 0f), 0.8f, parent, "ArenaStump_4");
        L0Util.Place(Stump, c + s * -50f + b * 55f, Q(0f, 45f, 0f), 1.0f, parent, "ArenaStump_5");
        L0Util.Place(Stump, c + s * 55f + b * 50f, Q(0f, 210f, 0f), 0.9f, parent, "ArenaStump_6");

        // === КАМНИ/ВАЛУНЫ Synty — крупные тёмные валуны, сочетаются со стенами крепости ===
        L0Util.Place(SyntyRockA, c + s * -55f + g * 15f, Q(0f, 40f, 0f), 2.6f, parent, "ArenaRock_1");
        L0Util.Place(SyntyRockC, c + s * 60f + g * 10f, Q(0f, 160f, 0f), 2.4f, parent, "ArenaRock_2");
        L0Util.Place(SyntyRockB, c + s * -45f + b * 20f, Q(0f, 90f, 0f), 2.0f, parent, "ArenaRock_3");
        L0Util.Place(SyntyRockD, c + s * 50f + b * 25f, Q(0f, 250f, 0f), 2.2f, parent, "ArenaRock_4");
        L0Util.Place(SyntyRockA, c + s * -90f + b * 50f, Q(0f, 310f, 0f), 3.0f, parent, "ArenaRock_5");
        L0Util.Place(SyntyRockC, c + s * 92f + b * 55f, Q(0f, 130f, 0f), 2.8f, parent, "ArenaRock_6");
        L0Util.Place(SyntyRockB, c + b * 95f + s * -15f, Q(0f, 200f, 0f), 2.5f, parent, "ArenaRock_7");
        L0Util.Place(SyntyRockD, c + b * 88f + s * 20f, Q(0f, 60f, 0f), 2.3f, parent, "ArenaRock_8");
        L0Util.Place(SyntyRockA, c + s * 84f + g * 30f, Q(0f, 20f, 0f), 2.7f, parent, "ArenaRock_9");
        L0Util.Place(SyntyRockC, c + s * -82f + g * 35f, Q(0f, 280f, 0f), 2.5f, parent, "ArenaRock_10");
        // Галька-россыпь у валунов
        L0Util.Place(SyntyPebbles, c + s * -52f + g * 18f, Q(0f, 70f, 0f), 2.0f, parent, "ArenaPebble_1");
        L0Util.Place(SyntyPebbles, c + s * 56f + b * 28f, Q(0f, 200f, 0f), 1.8f, parent, "ArenaPebble_2");
        L0Util.Place(SyntyPebbles, c + b * 90f + s * 10f, Q(0f, 120f, 0f), 2.2f, parent, "ArenaPebble_3");
        L0Util.Place(SyntyPebbles, c + s * 80f + b * 50f, Q(0f, 330f, 0f), 1.9f, parent, "ArenaPebble_4");

        // === ОБЛОМКИ Dungeon вместо деревянных заборов — рухнувшие колонны и кучи кирпича ===
        L0Util.Place(DunBrokenColumn, c + s * -62f + g * 2f, Q(0f, 30f, 0f), 2.6f, parent, "ArenaRubble_1");
        L0Util.Place(DunBrickPile, c + s * 65f + g * -3f + Vector3.up * 0.5f, Q(0f, 160f, 0f), 1.3f, parent, "ArenaRubble_2");
        L0Util.Place(DunBrokenColumn, c + s * -55f + b * 45f, Q(0f, 220f, 0f), 2.4f, parent, "ArenaRubble_3");
        L0Util.Place(DunBrickPile, c + s * 58f + b * 42f + Vector3.up * 0.5f, Q(0f, 310f, 0f), 1.2f, parent, "ArenaRubble_4");
        L0Util.Place(DunBrokenColumn, c + b * 82f + s * 35f, Q(0f, 100f, 0f), 2.8f, parent, "ArenaRubble_5");
        L0Util.Place(DunBrickPile, c + b * 78f + s * -35f + Vector3.up * 0.5f, Q(0f, 40f, 0f), 1.4f, parent, "ArenaRubble_6");

        // === ЯЩИКИ/БОЧКИ Dungeon — припасы орочьего лагеря (заполняют пустоту) ===
        BuildSupplyStack(parent, "Supply_1", c + s * -68f + g * 24f, 2.6f);
        BuildSupplyStack(parent, "Supply_2", c + s * 72f + g * 20f, 2.4f);
        BuildSupplyStack(parent, "Supply_3", c + s * -60f + b * 58f, 2.5f);
        BuildSupplyStack(parent, "Supply_4", c + s * 64f + b * 60f, 2.6f);
        BuildSupplyStack(parent, "Supply_5", c + b * 92f + s * 8f, 2.3f);

        // === ЗНАМЁНА/КАНДЕЛЯБРЫ/СТОЙКИ ОРУЖИЯ — вертикальные акценты по периметру ===
        L0Util.Place(DunBanner, c + s * -78f + g * 10f + Vector3.up * 2.5f, Q(0f, 90f, 0f), 3.0f, parent, "ArenaBanner_1");
        L0Util.Place(DunBanner, c + s * 82f + g * 6f + Vector3.up * 2.5f, Q(0f, 270f, 0f), 3.0f, parent, "ArenaBanner_2");
        L0Util.Place(DunBanner, c + b * 86f + s * -22f + Vector3.up * 2.5f, Q(0f, 0f, 0f), 3.2f, parent, "ArenaBanner_3");
        L0Util.Place(DunCandelabrum, c + s * -50f + g * 28f + Vector3.up * 0.9f, Q(0f, 0f, 0f), 2.6f, parent, "ArenaCandel_1");
        L0Util.Place(DunCandelabrum, c + s * 52f + b * 30f + Vector3.up * 0.9f, Q(0f, 0f, 0f), 2.6f, parent, "ArenaCandel_2");
        L0Util.Place(DunSwordStand, c + s * -44f + b * 18f + Vector3.up * 0.75f, Q(0f, 120f, 0f), 2.6f, parent, "ArenaSwordStand_1");
        L0Util.Place(DunSwordStand, c + s * 46f + g * 14f + Vector3.up * 0.75f, Q(0f, 200f, 0f), 2.6f, parent, "ArenaSwordStand_2");

        // === МЕЛКАЯ КАМЕННАЯ РОССЫПЬ Dungeon — следы боя ===
        L0Util.Place(DunStone, c + s * -36f + g * 22f, Q(0f, 30f, 0f), 3.0f, parent, "ArenaStone_1");
        L0Util.Place(DunStone, c + s * 34f + g * 26f, Q(0f, 200f, 8f), 2.6f, parent, "ArenaStone_2");
        L0Util.Place(DunStone, c + s * -28f + b * 34f, Q(0f, 90f, 0f), 2.8f, parent, "ArenaStone_3");
        L0Util.Place(DunStone, c + s * 30f + b * 32f, Q(0f, 300f, 12f), 2.4f, parent, "ArenaStone_4");
        L0Util.Place(DunStone, c + s * -48f + g * 6f, Q(0f, 150f, 0f), 2.7f, parent, "ArenaStone_5");
        L0Util.Place(DunStone, c + s * 50f + b * 8f, Q(0f, 60f, 6f), 2.5f, parent, "ArenaStone_6");

        // === МЁРТВЫЕ КУСТЫ — заполняют промежутки ===
        L0Util.Place(DeadShrub, c + s * -48f + g * 8f, Q(0f, 30f, 0f), 1.2f, parent, "ArenaShrub_1");
        L0Util.Place(DeadShrub, c + s * 52f + g * 5f, Q(0f, 140f, 0f), 1.0f, parent, "ArenaShrub_2");
        L0Util.Place(DeadShrub, c + s * -40f + b * 35f, Q(0f, 220f, 0f), 1.3f, parent, "ArenaShrub_3");
        L0Util.Place(DeadShrub, c + s * 45f + b * 40f, Q(0f, 90f, 0f), 1.1f, parent, "ArenaShrub_4");
        L0Util.Place(DeadShrub, c + s * -70f + b * 60f, Q(0f, 300f, 0f), 0.9f, parent, "ArenaShrub_5");
        L0Util.Place(DeadShrub, c + s * 68f + b * 45f, Q(0f, 180f, 0f), 1.0f, parent, "ArenaShrub_6");
        L0Util.Place(DeadShrub, c + s * -80f + g * 20f, Q(0f, 75f, 0f), 1.2f, parent, "ArenaShrub_7");
        L0Util.Place(DeadShrub, c + s * 82f + g * 18f, Q(0f, 260f, 0f), 1.1f, parent, "ArenaShrub_8");

        // === ОРУЖИЕ разбросано по арене — следы постоянных боёв ===
        // Рядом с лагерными зонами
        L0Util.Place(WeaponSpear, c + s * -52f + g * 10f, Q(-70f, 40f, 0f), 1.3f, parent, "ArenaWeapon_1");
        L0Util.Place(WeaponAxe, c + s * 55f + g * 8f + Vector3.up * 0.1f, Q(90f, 60f, 0f), 1.5f, parent, "ArenaWeapon_2");
        L0Util.Place(WeaponSword, c + s * -30f + b * 15f + Vector3.up * 0.1f, Q(90f, 130f, 0f), 1.4f, parent, "ArenaWeapon_3");
        L0Util.Place(WeaponShield, c + s * 38f + b * 20f + Vector3.up * 0.15f, Q(75f, -30f, 0f), 2.0f, parent, "ArenaWeapon_4");
        L0Util.Place(WeaponMaul, c + s * -60f + b * 45f + Vector3.up * 0.1f, Q(90f, 200f, 0f), 1.6f, parent, "ArenaWeapon_5");
        L0Util.Place(WeaponSpear, c + s * 65f + b * 50f, Q(-65f, 290f, 0f), 1.2f, parent, "ArenaWeapon_6");
        // Ближе к центру
        L0Util.Place(WeaponAxe, c + s * -18f + g * 25f + Vector3.up * 0.1f, Q(90f, 15f, 0f), 1.4f, parent, "ArenaWeapon_7");
        L0Util.Place(WeaponShield, c + s * 20f + g * 22f + Vector3.up * 0.15f, Q(80f, 100f, 0f), 1.8f, parent, "ArenaWeapon_8");
        L0Util.Place(WeaponSword, c + s * -15f + b * 30f + Vector3.up * 0.1f, Q(90f, 250f, 0f), 1.3f, parent, "ArenaWeapon_9");
        L0Util.Place(WeaponMaul, c + s * 22f + b * 35f + Vector3.up * 0.1f, Q(90f, 340f, 0f), 1.5f, parent, "ArenaWeapon_10");

        // === НАСТОЯЩИЙ ДЫМ — от костров лагерей и с периметра ===
        if (createAtmosphere)
        {
            L0Util.PlaceSmoke(Smoke1, c + s * -58f + g * 5f + Vector3.up * 1f, 0.7f, parent, "ArenaSmoke_1");
            L0Util.PlaceSmoke(Smoke2, c + s * 58f + g * 5f + Vector3.up * 1f, 0.6f, parent, "ArenaSmoke_2");
            L0Util.PlaceSmoke(Smoke4, c + s * -48f + b * 40f + Vector3.up * 1f, 0.5f, parent, "ArenaSmoke_3");
            L0Util.PlaceSmoke(Smoke1, c + s * 48f + b * 40f + Vector3.up * 1f, 0.6f, parent, "ArenaSmoke_4");
            L0Util.PlaceSmoke(Smoke2, c + b * 80f + Vector3.up * 1.5f, 0.8f, parent, "ArenaSmoke_5");
            // Дым от центра арены
            L0Util.PlaceSmoke(Smoke4, c + Vector3.up * 0.5f, 0.4f, parent, "ArenaSmoke_Center");
        }
    }

    private static Quaternion Q(float x, float y, float z) { return Quaternion.Euler(x, y, z); }

    /// <summary>
    /// Small Dungeon supply stack — a couple of crates/barrels clustered. Decorative fill (no colliders).
    /// </summary>
    private static void BuildSupplyStack(Transform parent, string id, Vector3 at, float scale)
    {
        L0Util.Place(DunBox, at + Vector3.up * (0.34f * scale), Q(0f, 18f, 0f), scale, parent, id + "_Box");
        L0Util.Place(DunOpenBox, at + new Vector3(0.55f * scale, 0.34f * scale, 0.2f * scale), Q(0f, -40f, 0f), scale * 0.9f, parent, id + "_OpenBox");
        L0Util.Place(DunBarrel, at + new Vector3(-0.45f * scale, 0.4f * scale, 0.5f * scale), Q(0f, 0f, 0f), scale, parent, id + "_Barrel");
        L0Util.Place(DunBox, at + new Vector3(0.1f * scale, 1.0f * scale, -0.1f * scale), Q(0f, 55f, 0f), scale * 0.85f, parent, id + "_BoxTop");
    }

    private static void BuildGround(Transform parent)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 entrance = L0OrcArenaConfig.BaseEntrance;
        Vector3 portal = L0OrcArenaConfig.PortalShrine;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;
        Vector3 flatColliderCenter = new Vector3(center.x, center.y + 0.035f, center.z);

        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "ArenaOuterFloor_NoCollider", PrimitiveType.Cylinder, center, Quaternion.identity, new Vector3(L0OrcArenaConfig.ArenaRadius * 2f, 0.025f, L0OrcArenaConfig.ArenaDepthRadius * 2f), L0OrcArenaMaterials.ArenaDirt, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "ArenaAshOuterMood_NoCollider", PrimitiveType.Cylinder, center + Vector3.up * 0.018f, Quaternion.identity, new Vector3(L0OrcArenaConfig.ArenaRadius * 1.82f, 0.012f, L0OrcArenaConfig.ArenaDepthRadius * 1.76f), new Color(0.075f, 0.047f, 0.034f), false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, "BattleZoneOpenFloor_NoCollider", PrimitiveType.Cylinder, center + Vector3.up * 0.035f, Quaternion.identity, new Vector3(L0OrcArenaConfig.InnerBattleRadius * 2.02f, 0.015f, L0OrcArenaConfig.InnerBattleRadius * 2.02f), L0OrcArenaMaterials.ArenaInner, false);
        L0OrcArenaPrimitiveKit.CreateWalkablePlaneCollider(parent, "ArenaFlatWalkableCollider", flatColliderCenter, Quaternion.identity, new Vector2(L0OrcArenaConfig.ArenaRadius * 2.04f, L0OrcArenaConfig.ArenaDepthRadius * 2.04f));

        Vector3 entranceStart = entrance + gate * L0OrcArenaConfig.EntranceForwardClear;
        Vector3 entranceEnd = center + gate * 4f;
        float entranceLength = L0OrcArenaConfig.FlatDistance(entranceStart, entranceEnd);
        L0OrcArenaPrimitiveKit.CreateGroundPatchWalkable(parent, "GateToBattleLane", (entranceStart + entranceEnd) * 0.5f, L0OrcArenaConfig.FaceGate, new Vector3(L0OrcArenaConfig.RoadWidth + 8f, 0.08f, entranceLength), L0OrcArenaMaterials.Road);
        CreateLaneEdge(parent, "GateLaneLeftAshEdge", entranceStart, entranceEnd, -(L0OrcArenaConfig.RoadWidth * 0.5f + 2.2f));
        CreateLaneEdge(parent, "GateLaneRightAshEdge", entranceStart, entranceEnd, L0OrcArenaConfig.RoadWidth * 0.5f + 2.2f);

        Vector3 portalStart = center + back * 8f;
        Vector3 portalEnd = portal + gate * 3f;
        float portalLength = L0OrcArenaConfig.FlatDistance(portalStart, portalEnd);
        L0OrcArenaPrimitiveKit.CreateGroundPatchWalkable(parent, "BattleToPortalLane", (portalStart + portalEnd) * 0.5f, L0OrcArenaConfig.FaceBack, new Vector3(L0OrcArenaConfig.RoadWidth + 6f, 0.08f, portalLength), L0OrcArenaMaterials.Road);
        CreateLaneEdge(parent, "PortalLaneLeftRuneEdge", portalStart, portalEnd, -(L0OrcArenaConfig.RoadWidth * 0.5f + 1.8f), true);
        CreateLaneEdge(parent, "PortalLaneRightRuneEdge", portalStart, portalEnd, L0OrcArenaConfig.RoadWidth * 0.5f + 1.8f, true);

    }

    private static void BuildPalisade(Transform parent, L0OrcArenaPlacementGuard placementGuard)
    {
        const int count = 64;
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;

        for (int i = 0; i < count; i++)
        {
            float deg = i * (360f / count);
            Vector3 dir = Quaternion.Euler(0f, deg, 0f) * Vector3.forward;
            float gateDot = Vector3.Dot(dir, gate);
            float backDot = Vector3.Dot(dir, back);
            float sideDot = Mathf.Abs(Vector3.Dot(dir, side));

            if (gateDot > 0.61f && sideDot < 0.64f)
                continue;

            if (backDot > 0.67f && sideDot < 0.55f)
                continue;

            Vector3 pos = center + new Vector3(dir.x * L0OrcArenaConfig.ArenaRadius, 0f, dir.z * L0OrcArenaConfig.ArenaDepthRadius);
            if (L0OrcArenaConfig.IsInsideEntranceClearZone(pos) || L0OrcArenaConfig.IsInsidePortalPath(pos))
                continue;

            const float palisadeFootprintRadius = 4.75f;
            if (!placementGuard.IsFootprintInsideZone(pos, palisadeFootprintRadius, L0OrcArenaZone.PerimeterRing))
                continue;

            if (!placementGuard.IsFootprintOutsideCriticalLanes(pos, palisadeFootprintRadius))
                continue;

            if (placementGuard.IntersectsReserved(
                pos,
                palisadeFootprintRadius,
                L0OrcArenaPlacementCategory.Critical | L0OrcArenaPlacementCategory.Combat | L0OrcArenaPlacementCategory.SetDressing))
                continue;

            CreatePalisadeSegment(parent, pos, Quaternion.LookRotation(dir, Vector3.up), 2.25f + (i % 3) * 0.12f);
        }
    }

    private static void BuildCampIdentity(Transform parent, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        CreateCampIdentityCluster(parent, "CAMP_IDENTITY_LeftFrontHut", L0OrcArenaConfig.LeftFrontCampIdentity, -1f, true, createAtmosphere, placementGuard);
        CreateCampIdentityCluster(parent, "CAMP_IDENTITY_RightFrontDrumYard", L0OrcArenaConfig.RightFrontCampIdentity, 1f, false, createAtmosphere, placementGuard);
        CreateCampIdentityCluster(parent, "CAMP_IDENTITY_LeftRearHut", L0OrcArenaConfig.LeftRearCampIdentity, -1f, true, createAtmosphere, placementGuard);
        CreateCampIdentityCluster(parent, "CAMP_IDENTITY_RightRearWarYard", L0OrcArenaConfig.RightRearCampIdentity, 1f, false, createAtmosphere, placementGuard);
    }

    private static void CreateCampIdentityCluster(Transform parent, string name, Vector3 anchor, float sideSign, bool createHut, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        if (!placementGuard.TryReserve(L0OrcArenaPlacementCategory.Camp, name, anchor, L0OrcArenaConfig.CampIdentityRadius, L0OrcArenaZone.CampRing))
            return;

        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup(name, parent);
        Quaternion faceCenter = FaceCenter(anchor);
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        L0OrcArenaPrimitiveKit.CreateGroundPatchDecorative(root.transform, name + "_TrampledGround_NoCollider", anchor + Vector3.up * 0.065f, faceCenter, new Vector3(13f, 0.03f, 9f), L0OrcArenaMaterials.GateGround);

        if (createHut)
            L0Props.CreateHugeOrcHut(anchor + side * (sideSign * 1.2f), faceCenter, 0.78f, root.transform);
        else
            L0Props.CreateOrcWarDrum(anchor + gate * -0.8f, faceCenter, 1.15f, root.transform);

        L0Props.CreateBonePile(anchor + side * (sideSign * 3.8f) + gate * 2.5f, 0.78f, root.transform);
        L0OrcArenaPrimitiveKit.CreateSideFlag(root.transform, name + "_ClaimFlag", anchor + side * (sideSign * 4.6f) + gate * -1.6f, faceCenter, 0.92f);

        if (createAtmosphere)
            L0Props.CreateCampfire(anchor + side * (sideSign * -3.4f) + gate * 2.0f, 0.82f, root.transform);
    }

    private static void CreateLaneEdge(Transform parent, string name, Vector3 start, Vector3 end, float sideOffset, bool emissive = false)
    {
        Vector3 dir = end - start;
        dir.y = 0f;
        float length = dir.magnitude;
        if (length < 0.01f)
            return;

        dir /= length;
        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 pos = (start + end) * 0.5f + side * sideOffset + Vector3.up * 0.105f;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        Color color = emissive ? new Color(0.40f, 0.025f, 0.018f) : new Color(0.055f, 0.035f, 0.025f);
        L0OrcArenaPrimitiveKit.CreatePrimitive(parent, name + "_NoCollider", PrimitiveType.Cube, pos, rot, new Vector3(0.62f, 0.035f, length), color, false, emissive);
    }


    private static void CreatePalisadeSegment(Transform parent, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup("CleanPalisadeSegment", parent);
        root.transform.position = position;
        root.transform.rotation = rotation;

        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 0.62f * scale;
            float height = (1.52f + (i % 2) * 0.22f) * scale;
            L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "Stake_" + i, PrimitiveType.Cylinder, new Vector3(x, height, 0f), Quaternion.Euler(0f, 0f, (i - 1) * 4f), new Vector3(0.15f * scale, height, 0.15f * scale), L0OrcArenaMaterials.DarkWood, true);
            L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "Tip_NoCollider_" + i, PrimitiveType.Cube, new Vector3(x, height * 2f + 0.18f * scale, 0f), Quaternion.Euler(45f, 0f, 45f), new Vector3(0.28f * scale, 0.36f * scale, 0.28f * scale), L0OrcArenaMaterials.DarkWood, false);
        }

        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "Rail", PrimitiveType.Cube, new Vector3(0f, 1.60f * scale, 0.14f * scale), Quaternion.identity, new Vector3(4.1f * scale, 0.16f * scale, 0.16f * scale), L0OrcArenaMaterials.Wood, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RedWarningRag_NoCollider", PrimitiveType.Cube, new Vector3(0f, 2.05f * scale, 0.21f * scale), Quaternion.identity, new Vector3(1.5f * scale, 0.42f * scale, 0.07f * scale), L0OrcArenaMaterials.OrcRed, false);
    }

    private static Quaternion FaceCenter(Vector3 position)
    {
        Vector3 toCenter = L0OrcArenaConfig.BaseCenter - position;
        toCenter.y = 0f;
        return toCenter.sqrMagnitude < 0.01f ? L0OrcArenaConfig.FaceGate : Quaternion.LookRotation(toCenter, Vector3.up);
    }

    /// <summary>
    /// STEP 7.5 — rings the open battle center with Dungeon-pack fortification nests (dark stone
    /// walls / jail cells / barricades), not dwellings. Every candidate is gated by the
    /// PlacementGuard (Camp category, CampRing zone), so a nest can never land in the gate vista,
    /// the portal aisle, the open battle pit or on top of an existing reservation. The ring doubles
    /// as combat cover for the future orc spawn battle. Builds via L0DungeonFort.BuildCluster.
    /// </summary>
    private static void BuildArenaFortifications(Transform parent, bool createAtmosphere, L0OrcArenaPlacementGuard placementGuard)
    {
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 side = L0OrcArenaConfig.SideDir;
        Vector3 gate = L0OrcArenaConfig.GateDir;
        Vector3 back = L0OrcArenaConfig.BackDir;

        Random.State savedState = Random.state;
        Random.InitState(70451);

        const int candidates = 16;
        const float footprint = 9f; // big fortress nests: barrier box ~8.4m half — fits CampRing (50-74) at r~62

        for (int i = 0; i < candidates; i++)
        {
            float deg = i * (360f / candidates) + 9f;
            Vector3 dir = Quaternion.Euler(0f, deg, 0f) * Vector3.forward;
            float gateDot = Vector3.Dot(dir, gate);
            float backDot = Vector3.Dot(dir, back);
            float sideDot = Mathf.Abs(Vector3.Dot(dir, side));

            // Keep the gate vista (player's first read) and the portal aisle (mandatory exit) open.
            if (gateDot > 0.5f && sideDot < 0.62f)
                continue;
            if (backDot > 0.55f && sideDot < 0.5f)
                continue;

            float radius = 61f + (i % 2) * 2f; // 61 / 63 — footprint 9 stays inside CampRing (50-74)
            Vector3 pos = center + dir * radius;

            string id = "FORT_" + i;
            if (!placementGuard.TryReserve(L0OrcArenaPlacementCategory.Camp, id, pos, footprint, L0OrcArenaZone.CampRing))
                continue;

            float scale = 3.7f + (i % 3) * 0.3f; // 3.7 / 4.0 / 4.3 — tall fortress walls, readable from afar
            L0DungeonFort.BuildCluster(parent, id, pos, i, scale, createAtmosphere);
        }

        Random.state = savedState;
    }

}
