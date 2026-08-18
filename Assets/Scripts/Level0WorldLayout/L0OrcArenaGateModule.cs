using UnityEngine;

/// <summary>
/// Builds the entrance into the orc arena — v2 with real assets.
/// Composition: gate frame, dead forest, menhirs, real smoke, scattered weapons, fence walls.
/// </summary>
public static class L0OrcArenaGateModule
{
    private const string LairGateLabel = "ЛОГОВО ОРКОВ";

    // Asset paths
    static readonly string DeadTree = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_dead.prefab";
    static readonly string Stump = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_stump.prefab";
    static readonly string DeadShrub = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Shrubs/PT_Generic_Shrub_01_dead.prefab";
    static readonly string Menhir = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Menhir_Rock_02.prefab";
    static readonly string GenericRock = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Generic_Rock_01.prefab";
    static readonly string RockMoss09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    static readonly string RockPile05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_05.prefab";
    static readonly string FenceWood01 = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab";
    static readonly string FenceWood02 = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_02.prefab";
    static readonly string PurpleMushroom05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Mushroom_Fantasy_Purple_05.prefab";
    static readonly string PurpleMushroom08 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Mushroom_Fantasy_Purple_08.prefab";
    static readonly string OrangeMushroom09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Mushroom_Fantasy_Orange_09.prefab";
    static readonly string Smoke1 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab";
    static readonly string Smoke3 = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 3.prefab";
    static readonly string WeaponSpear = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/spear-1.prefab";
    static readonly string WeaponAxe = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/axe-1.prefab";
    static readonly string WeaponSword = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/sword-1.prefab";
    static readonly string WeaponShield = "Assets/New Folder/Low Poly Modular Medieval Weapons/Prefabs/shield/shield 1.prefab";
    static readonly string Weapon2HAxe = "Assets/New Folder/Low Poly Modular Medieval Weapons/Sample Weapons/2HandedAxe.prefab";

    public static void Build(Transform gateZone, Transform outerCampRing, bool createAtmosphere)
    {
        if (gateZone == null || outerCampRing == null)
            return;

        Vector3 entrance = L0OrcArenaConfig.BaseEntrance;
        Vector3 center = L0OrcArenaConfig.BaseCenter;
        Vector3 gateDir = L0OrcArenaConfig.GateDir;
        Vector3 sideDir = L0OrcArenaConfig.SideDir;
        Quaternion faceGate = L0OrcArenaConfig.FaceGate;
        // === GROUND PAD ===
        L0OrcArenaPrimitiveKit.CreateGroundPatchWalkable(gateZone, "GateZoneOpenPad",
            entrance + gateDir * 1.5f, faceGate,
            new Vector3(L0OrcArenaConfig.RoadWidth + 14f, 0.10f, 32f), L0OrcArenaMaterials.GateGround);

        // Выжженная земля перед воротами
        L0Props.CreateGroundPatch(entrance + gateDir * -8f, new Vector3(28f, 0.06f, 18f),
            new Color(0.18f, 0.14f, 0.1f), gateZone, "GateBurnedGround", faceGate);

        // === GATE FRAME (примитивы — это ок, это рукотворная структура) ===
        CreateOpenGate(gateZone, entrance, faceGate);

        // === ЗНАКИ ===
        Vector3 roadDir = entrance - L0OrcArenaConfig.RoadStart;
        roadDir.y = 0f;
        if (roadDir.sqrMagnitude < 0.01f) roadDir = Vector3.forward;
        roadDir.Normalize();
        Vector3 roadSide = Vector3.Cross(Vector3.up, roadDir).normalized;
        L0Props.CreateRouteSign("ВХОД В ЛОГОВО", entrance + roadSide * -11f + roadDir * 2.5f + Vector3.up * 0.05f,
            faceGate * Quaternion.Euler(0f, 16f, 0f), gateZone);
        L0Props.CreateRouteSign(LairGateLabel, entrance + sideDir * 16.5f + gateDir * 8.5f + Vector3.up * 0.05f,
            faceGate * Quaternion.Euler(0f, -14f, 0f), gateZone);

        // === БАШНИ (оставляем — они далеко и масштабные) ===
        Vector3 leftTower = entrance + sideDir * -32f + gateDir * -1.5f;
        Vector3 rightTower = entrance + sideDir * 32f + gateDir * -1.5f;
        L0OrcArenaPrimitiveKit.CreateGroundPatchWalkable(outerCampRing, "GateTowerPad_L", leftTower, faceGate,
            new Vector3(14f, 0.10f, 14f), L0OrcArenaMaterials.GateGround);
        L0OrcArenaPrimitiveKit.CreateGroundPatchWalkable(outerCampRing, "GateTowerPad_R", rightTower, faceGate,
            new Vector3(14f, 0.10f, 14f), L0OrcArenaMaterials.GateGround);
        L0Props.CreateOrcTower(leftTower, Quaternion.LookRotation(center - leftTower, Vector3.up), 2.70f, outerCampRing);
        L0Props.CreateOrcTower(rightTower, Quaternion.LookRotation(center - rightTower, Vector3.up), 2.70f, outerCampRing);

        // === БИВНИ + ФАКЕЛЫ У ВОРОТ (оставляем) ===
        L0Props.CreateOrcTuskPillar(entrance + sideDir * -14.2f + gateDir * 2.2f, faceGate, 1.7f, gateZone);
        L0Props.CreateOrcTuskPillar(entrance + sideDir * 14.2f + gateDir * 2.2f, faceGate, 1.7f, gateZone);
        L0Props.CreateTorchPost(entrance + sideDir * -15.8f + gateDir * 3.2f, faceGate, 1.25f, gateZone);
        L0Props.CreateTorchPost(entrance + sideDir * 15.8f + gateDir * 3.2f, faceGate, 1.25f, gateZone);

        // === ЗНАМЁНА (оставляем) ===
        L0OrcArenaPrimitiveKit.CreateSideFlag(gateZone, "GateFlag_L", entrance + sideDir * -21f + gateDir * 4f, faceGate, 1.4f);
        L0OrcArenaPrimitiveKit.CreateSideFlag(gateZone, "GateFlag_R", entrance + sideDir * 21f + gateDir * 4f, faceGate, 1.4f);
        L0OrcArenaPrimitiveKit.CreateBannerBar(gateZone, "GateLeftWarBanner",
            entrance + sideDir * -24.5f + gateDir * 7.5f + Vector3.up * 5.8f,
            Quaternion.LookRotation(sideDir, Vector3.up), 8.5f);
        L0OrcArenaPrimitiveKit.CreateBannerBar(gateZone, "GateRightWarBanner",
            entrance + sideDir * 24.5f + gateDir * 7.5f + Vector3.up * 5.8f,
            Quaternion.LookRotation(-sideDir, Vector3.up), 8.5f);

        // =====================================================
        // === V2: НАСТОЯЩИЕ АССЕТЫ ===
        // =====================================================

        // --- МЁРТВЫЙ ЛЕС вокруг ворот — орки вырубили всё ---
        // Большие мёртвые деревья — силуэты на фоне неба, обрамляют вход
        L0Util.Place(DeadTree, entrance + sideDir * -20f + gateDir * -10f, Quaternion.Euler(0f, 45f, 0f), 1.6f, gateZone, "GateDeadTree_L1");
        L0Util.Place(DeadTree, entrance + sideDir * -26f + gateDir * 4f, Quaternion.Euler(0f, 120f, 0f), 1.8f, gateZone, "GateDeadTree_L2");
        L0Util.Place(DeadTree, entrance + sideDir * -18f + gateDir * 12f, Quaternion.Euler(0f, 200f, 0f), 1.4f, gateZone, "GateDeadTree_L3");
        L0Util.Place(DeadTree, entrance + sideDir * 22f + gateDir * -8f, Quaternion.Euler(0f, 160f, 0f), 1.7f, gateZone, "GateDeadTree_R1");
        L0Util.Place(DeadTree, entrance + sideDir * 28f + gateDir * 6f, Quaternion.Euler(0f, 280f, 0f), 1.5f, gateZone, "GateDeadTree_R2");
        L0Util.Place(DeadTree, entrance + sideDir * 19f + gateDir * 14f, Quaternion.Euler(0f, 70f, 0f), 1.9f, gateZone, "GateDeadTree_R3");

        // Пни — орки рубили лес для построек
        L0Util.Place(Stump, entrance + sideDir * -12f + gateDir * -14f, Quaternion.Euler(0f, 30f, 0f), 1.2f, gateZone, "GateStump_L1");
        L0Util.Place(Stump, entrance + sideDir * -24f + gateDir * -4f, Quaternion.Euler(0f, 150f, 0f), 0.9f, gateZone, "GateStump_L2");
        L0Util.Place(Stump, entrance + sideDir * 15f + gateDir * -12f, Quaternion.Euler(0f, 260f, 0f), 1.1f, gateZone, "GateStump_R1");
        L0Util.Place(Stump, entrance + sideDir * 25f + gateDir * -2f, Quaternion.Euler(0f, 90f, 0f), 1.0f, gateZone, "GateStump_R2");

        // --- ДЕРЕВЯННЫЕ ЗАБОРЫ — настоящие, от ворот к периметру ---
        float fenceX = L0OrcArenaConfig.EntranceClearWidth * 0.5f + 5f;
        Quaternion fenceRotL = Quaternion.LookRotation(gateDir, Vector3.up);
        Quaternion fenceRotR = Quaternion.LookRotation(gateDir, Vector3.up);
        for (int i = 0; i < 3; i++)
        {
            float zOff = 5f + i * 6f;
            string fencePath = (i % 2 == 0) ? FenceWood01 : FenceWood02;
            L0Util.Place(fencePath, entrance + sideDir * -fenceX + gateDir * zOff, fenceRotL, 1.3f, gateZone, "GateFence_L" + i);
            L0Util.Place(fencePath, entrance + sideDir * fenceX + gateDir * zOff, fenceRotR, 1.3f, gateZone, "GateFence_R" + i);
        }

        // --- КАМНИ-МЕНГИРЫ — ритуальные маркеры входа ---
        L0Util.Place(Menhir, entrance + sideDir * -10f + gateDir * -4f, Quaternion.Euler(0f, 15f, -5f), 1.4f, gateZone, "GateMenhir_L");
        L0Util.Place(Menhir, entrance + sideDir * 10f + gateDir * -3f, Quaternion.Euler(0f, -20f, 4f), 1.3f, gateZone, "GateMenhir_R");

        // Валуны между деревьями
        L0Util.Place(GenericRock, entrance + sideDir * -16f + gateDir * -7f, Quaternion.Euler(0f, 55f, 0f), 1.0f, gateZone, "GateRock_L1");
        L0Util.Place(RockMoss09, entrance + sideDir * 20f + gateDir * -5f, Quaternion.Euler(0f, 130f, 0f), 0.7f, gateZone, "GateRock_R1");
        L0Util.Place(RockPile05, entrance + sideDir * -22f + gateDir * 10f, Quaternion.Euler(0f, 80f, 0f), 0.8f, gateZone, "GateRock_L2");
        L0Util.Place(GenericRock, entrance + sideDir * 16f + gateDir * 10f, Quaternion.Euler(0f, 200f, 0f), 0.6f, gateZone, "GateRock_R2");

        // --- ОРУЖИЕ НА ЗЕМЛЕ — настоящие модели, следы битв ---
        // Копья воткнуты в землю (наклон ~60° от горизонтали)
        L0Util.Place(WeaponSpear, entrance + sideDir * -6f + gateDir * -6f, Quaternion.Euler(-70f, 25f, 0f), 1.2f, gateZone, "GateSpear_1");
        L0Util.Place(WeaponSpear, entrance + sideDir * 5f + gateDir * -8f, Quaternion.Euler(-65f, -30f, 0f), 1.1f, gateZone, "GateSpear_2");
        L0Util.Place(WeaponSpear, entrance + sideDir * -3f + gateDir * -12f, Quaternion.Euler(-75f, 50f, 0f), 1.3f, gateZone, "GateSpear_3");

        // Топоры и мечи лежат на земле (плоско)
        L0Util.Place(WeaponAxe, entrance + sideDir * -8f + gateDir * -9f + Vector3.up * 0.1f, Quaternion.Euler(90f, 40f, 0f), 1.5f, gateZone, "GateAxe_1");
        L0Util.Place(Weapon2HAxe, entrance + sideDir * 7f + gateDir * -11f + Vector3.up * 0.1f, Quaternion.Euler(90f, -25f, 0f), 1.3f, gateZone, "GateAxe_2");
        L0Util.Place(WeaponSword, entrance + sideDir * 2f + gateDir * -5f + Vector3.up * 0.1f, Quaternion.Euler(90f, 70f, 0f), 1.4f, gateZone, "GateSword_1");

        // Щиты прислонены / лежат
        L0Util.Place(WeaponShield, entrance + sideDir * -4f + gateDir * -7f + Vector3.up * 0.15f, Quaternion.Euler(75f, 30f, 0f), 2.0f, gateZone, "GateShield_1");
        L0Util.Place(WeaponShield, entrance + sideDir * 8f + gateDir * -4f + Vector3.up * 0.15f, Quaternion.Euler(70f, -50f, 10f), 1.8f, gateZone, "GateShield_2");

        // --- МЁРТВЫЕ КУСТЫ — заполняют пустоту между деревьями ---
        L0Util.Place(DeadShrub, entrance + sideDir * -14f + gateDir * -2f, Quaternion.Euler(0f, 40f, 0f), 1.2f, gateZone, "GateDeadShrub_1");
        L0Util.Place(DeadShrub, entrance + sideDir * 17f + gateDir * 2f, Quaternion.Euler(0f, 150f, 0f), 1.0f, gateZone, "GateDeadShrub_2");
        L0Util.Place(DeadShrub, entrance + sideDir * -8f + gateDir * 8f, Quaternion.Euler(0f, 250f, 0f), 0.9f, gateZone, "GateDeadShrub_3");
        L0Util.Place(DeadShrub, entrance + sideDir * 12f + gateDir * 12f, Quaternion.Euler(0f, 320f, 0f), 1.1f, gateZone, "GateDeadShrub_4");

        // --- ФИОЛЕТОВЫЕ ГРИБЫ — мрачная деталь, контраст ---
        L0Util.Place(PurpleMushroom05, entrance + sideDir * -18f + gateDir * -6f + Vector3.up * 0.05f, Quaternion.Euler(0f, 60f, 0f), 1.5f, gateZone, "GateMush_1");
        L0Util.Place(PurpleMushroom08, entrance + sideDir * 24f + gateDir * 2f + Vector3.up * 0.05f, Quaternion.Euler(0f, 180f, 0f), 1.3f, gateZone, "GateMush_2");
        L0Util.Place(OrangeMushroom09, entrance + sideDir * -23f + gateDir * 8f + Vector3.up * 0.05f, Quaternion.Euler(0f, 100f, 0f), 1.2f, gateZone, "GateMush_3");
        L0Util.Place(PurpleMushroom05, entrance + sideDir * 14f + gateDir * -14f + Vector3.up * 0.05f, Quaternion.Euler(0f, 270f, 0f), 1.0f, gateZone, "GateMush_4");

        // --- КОСТРИЩА (примитивные, но с огнём — ок) ---
        L0Props.CreateCampfire(entrance + sideDir * -10f + gateDir * -6f, 1.4f, gateZone);
        L0Props.CreateCampfire(entrance + sideDir * 10f + gateDir * -6f, 1.4f, gateZone);

        // --- НАСТОЯЩИЙ ДЫМ (msVFX ParticleSystem!) ---
        L0Util.PlaceSmoke(Smoke1, entrance + gateDir * 6f + Vector3.up * 1f, 0.8f, gateZone, "GateSmoke_Center");
        L0Util.PlaceSmoke(Smoke3, entrance + sideDir * -14f + gateDir * 8f + Vector3.up * 0.5f, 0.6f, gateZone, "GateSmoke_L");
        L0Util.PlaceSmoke(Smoke1, entrance + sideDir * 15f + gateDir * 10f + Vector3.up * 0.5f, 0.5f, gateZone, "GateSmoke_R");
        L0Util.PlaceSmoke(Smoke3, entrance + sideDir * -10f + gateDir * -6f + Vector3.up * 0.8f, 0.4f, gateZone, "GateSmoke_FireL");
        L0Util.PlaceSmoke(Smoke3, entrance + sideDir * 10f + gateDir * -6f + Vector3.up * 0.8f, 0.4f, gateZone, "GateSmoke_FireR");

        // --- ОСВЕЩЕНИЕ ---
        if (createAtmosphere)
        {
            L0Props.CreatePointLight("GateFire_L", entrance + sideDir * -15.8f + Vector3.up * 2.4f,
                L0OrcArenaMaterials.Fire, 1.15f, 13f, gateZone);
            L0Props.CreatePointLight("GateFire_R", entrance + sideDir * 15.8f + Vector3.up * 2.4f,
                L0OrcArenaMaterials.Fire, 1.15f, 13f, gateZone);
            L0Props.CreatePointLight("GateBonfire_L", entrance + sideDir * -10f + gateDir * -6f + Vector3.up * 1.5f,
                L0OrcArenaMaterials.Fire, 0.9f, 10f, gateZone);
            L0Props.CreatePointLight("GateBonfire_R", entrance + sideDir * 10f + gateDir * -6f + Vector3.up * 1.5f,
                L0OrcArenaMaterials.Fire, 0.9f, 10f, gateZone);
            // Мрачный фиолетовый свет от грибов
            L0Props.CreatePointLight("GateMushroomGlow", entrance + sideDir * -18f + gateDir * -5f + Vector3.up * 0.5f,
                new Color(0.5f, 0.2f, 0.7f), 0.4f, 6f, gateZone);
        }
    }

    private static void CreateOpenGate(Transform parent, Vector3 position, Quaternion rotation)
    {
        GameObject root = L0OrcArenaPrimitiveKit.CreateGroup("OpenOrcStrongholdGate", parent);
        root.transform.position = position;
        root.transform.rotation = rotation;

        float postX = L0OrcArenaConfig.EntranceClearWidth * 0.5f + 3.2f;
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "LeftGatePost", PrimitiveType.Cylinder, new Vector3(-postX, 3.35f, 0f), Quaternion.identity, new Vector3(0.62f, 3.35f, 0.62f), L0OrcArenaMaterials.DarkWood, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RightGatePost", PrimitiveType.Cylinder, new Vector3(postX, 3.35f, 0f), Quaternion.identity, new Vector3(0.62f, 3.35f, 0.62f), L0OrcArenaMaterials.DarkWood, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "LeftOuterGatePost", PrimitiveType.Cylinder, new Vector3(-postX - 2.7f, 2.85f, -0.7f), Quaternion.Euler(0f, 0f, -5f), new Vector3(0.36f, 2.85f, 0.36f), L0OrcArenaMaterials.Wood, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RightOuterGatePost", PrimitiveType.Cylinder, new Vector3(postX + 2.7f, 2.85f, -0.7f), Quaternion.Euler(0f, 0f, 5f), new Vector3(0.36f, 2.85f, 0.36f), L0OrcArenaMaterials.Wood, true);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "HighTopBeam_NoCollider", PrimitiveType.Cube, new Vector3(0f, 6.85f, 0f), Quaternion.identity, new Vector3(postX * 2f + 7.4f, 0.42f, 0.44f), L0OrcArenaMaterials.Wood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "BackTopBeam_NoCollider", PrimitiveType.Cube, new Vector3(0f, 6.35f, -0.9f), Quaternion.identity, new Vector3(postX * 2f + 5.4f, 0.26f, 0.34f), L0OrcArenaMaterials.DarkWood, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "RedWarning_NoCollider", PrimitiveType.Cube, new Vector3(0f, 5.48f, -0.06f), Quaternion.identity, new Vector3(L0OrcArenaConfig.EntranceClearWidth * 0.58f, 0.70f, 0.08f), L0OrcArenaMaterials.OrcRed, false);
        L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "SkullMark_NoCollider", PrimitiveType.Sphere, new Vector3(0f, 6.18f, -0.26f), Quaternion.identity, new Vector3(0.72f, 0.52f, 0.16f), L0OrcArenaMaterials.Bone, false);

        for (int i = 0; i < 5; i++)
        {
            float x = Mathf.Lerp(-postX + 1.2f, postX - 1.2f, i / 4f);
            L0OrcArenaPrimitiveKit.CreatePrimitive(root.transform, "TopSpike_NoCollider_" + i, PrimitiveType.Cylinder, new Vector3(x, 7.55f, 0f), Quaternion.identity, new Vector3(0.09f, 0.72f, 0.09f), L0OrcArenaMaterials.Bone, false);
        }
    }
}
