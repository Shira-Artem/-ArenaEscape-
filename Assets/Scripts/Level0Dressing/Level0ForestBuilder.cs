using UnityEngine;

/// <summary>
/// Шаг 4A: Большой лес из PP деревьев вокруг Level0.
/// Заполняет пустоту между замком и деревней, по бокам дороги, на горизонте.
/// Создаёт визуальный барьер — деревня не видна сразу от замка.
/// </summary>
public class Level0ForestBuilder : MonoBehaviour
{
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;

    [Header("Toggle Zones")]
    public bool createCastleVillageForest = true;
    public bool createVillagePerimeter = true;
    public bool createRoadForest = true;
    public bool createHorizonTrees = true;
    public bool createRocks = true;
    public bool createGroundcover = true;
    public bool createMountainRing = true;
    public bool createTransitionForest = true;

    [Header("Reference Points")]
    public Vector3 castleGatePoint = new Vector3(0f, 0f, -16.25f);
    public Vector3 villageCenter = new Vector3(-24f, 0f, -45f);
    public Vector3 roadStart = new Vector3(0f, 0f, -34f);
    public Vector3 roadEnd = new Vector3(0f, 0f, -132f);

    // PP prefab paths
    private static readonly string[] TreePrefabs = {
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_02.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Tree_10.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_05.prefab",
        "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Birch_Tree_06.prefab"
    };
    private static readonly string RockMoss09 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_09.prefab";
    private static readonly string RockMoss11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Moss_Grown_11.prefab";
    private static readonly string RockPile05 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Rock_Pile_Forest_Moss_05.prefab";
    private static readonly string Grass11 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_11.prefab";
    private static readonly string Grass15 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Grass_15.prefab";
    private static readonly string Mountain01 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Forest_Mountain_Moss_01.prefab";
    private static readonly string Mountain02 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Forest_Mountain_Moss_02.prefab";
    private static readonly string DeadTree = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_dead.prefab";
    private static readonly string DeadStump = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_stump.prefab";
    private static readonly string Daffodil = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Daffodil_03.prefab";
    private static readonly string Sunflower = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Sunflower_04.prefab";
    private static readonly string Hyacinth = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Hyacinth_04.prefab";
    private static readonly string Meadow07 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_07.prefab";
    private static readonly string Meadow08 = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_08.prefab";
    private static readonly string MeadowPath = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs/PP_Meadow_Path_05.prefab";
    private static readonly string RPGGrass01 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Grass/rpgpp_lt_grass_small_01a.prefab";
    private static readonly string RPGGrass02 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Grass/rpgpp_lt_grass_small_01b.prefab";
    private static readonly string RPGBush01 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Bushes/rpgpp_lt_bush_01.prefab";
    private static readonly string RPGBush02 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Bushes/rpgpp_lt_bush_02.prefab";
    private static readonly string RPGPlant01 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Plants/rpgpp_lt_plant_01.prefab";
    private static readonly string RPGPlant02 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Plants/rpgpp_lt_plant_02.prefab";
    private static readonly string RPGTerrainGrass01 = "Assets/RPGPP_LT/Prefabs/Nature/Terrain/rpgpp_lt_terrain_grass_01.prefab";
    private static readonly string RPGTerrainGrass02 = "Assets/RPGPP_LT/Prefabs/Nature/Terrain/rpgpp_lt_terrain_grass_02.prefab";
    private static readonly string RPGHillSmall01 = "Assets/RPGPP_LT/Prefabs/Nature/Terrain/rpgpp_lt_hill_small_01.prefab";
    private static readonly string RPGHillSmall02 = "Assets/RPGPP_LT/Prefabs/Nature/Terrain/rpgpp_lt_hill_small_02.prefab";
    private static readonly string RPGFlower01 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Flowers/rpgpp_lt_flower_01.prefab";
    private static readonly string RPGFlower02 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Flowers/rpgpp_lt_flower_02.prefab";
    private static readonly string RPGFlower03 = "Assets/RPGPP_LT/Prefabs/Nature/Vegetation/Flowers/rpgpp_lt_flower_03.prefab";

    private const string ContainerName = "Generated_Level0_Forest";
    private GameObject forestContainer;

    private void Start()
    {
        if (buildOnStart) BuildForest();
    }

    [ContextMenu("Build Forest")]
    public void BuildForest()
    {
        if (clearBeforeBuild) ClearForest();

        Random.State prevState = Random.state;
        Random.InitState(44018);

        forestContainer = new GameObject(ContainerName);
        forestContainer.transform.SetParent(transform, false);
        Transform root = forestContainer.transform;

        if (createCastleVillageForest) BuildCastleVillageForest(root);
        if (createVillagePerimeter) BuildVillagePerimeter(root);
        if (createRoadForest) BuildRoadForest(root);
        if (createHorizonTrees) BuildHorizonTrees(root);
        if (createRocks) BuildRocks(root);
        if (createGroundcover) BuildGroundcover(root);
        if (createMountainRing) BuildMountainRing(root);
        if (createTransitionForest) BuildTransitionForest(root);

        Random.state = prevState;
        Debug.Log("[Level0ForestBuilder] Forest built — step 4A.");
    }

    /// <summary>
    /// Плотный лес между замком и деревней (Z=-18 до Z=-42).
    /// Деревья по бокам от дороги (|X| > 7), чтобы дорога оставалась проходимой.
    /// Это главный визуальный барьер.
    /// </summary>
    private void BuildCastleVillageForest(Transform root)
    {
        // Левая сторона (X от -8 до -32) — ПЛОТНЫЙ лес, 3 ряда
        // Ряд 1 — ближний к дороге (X -8..-14)
        PlaceTree(-10f, -22f, root, "CVForest_L01", 1.1f);
        PlaceTree(-9f,  -26f, root, "CVForest_L02", 1.3f);
        PlaceTree(-11f, -30f, root, "CVForest_L03", 1.0f);
        PlaceTree(-8f,  -34f, root, "CVForest_L04", 1.2f);
        PlaceTree(-10f, -38f, root, "CVForest_L05", 0.95f);
        PlaceTree(-12f, -19f, root, "CVForest_L06", 1.05f);
        PlaceTree(-9f,  -42f, root, "CVForest_L07", 1.1f);
        // Ряд 2 — средний (X -14..-22)
        PlaceTree(-14f, -20f, root, "CVForest_L08", 1.25f);
        PlaceTree(-16f, -24f, root, "CVForest_L09", 1.15f);
        PlaceTree(-18f, -28f, root, "CVForest_L10", 1.0f);
        PlaceTree(-15f, -32f, root, "CVForest_L11", 1.3f);
        PlaceTree(-17f, -36f, root, "CVForest_L12", 1.1f);
        PlaceTree(-20f, -22f, root, "CVForest_L13", 0.95f);
        PlaceTree(-14f, -40f, root, "CVForest_L14", 1.2f);
        PlaceTree(-19f, -31f, root, "CVForest_L15", 1.05f);
        // Ряд 3 — дальний (X -22..-32)
        PlaceTree(-22f, -21f, root, "CVForest_L16", 1.2f);
        PlaceTree(-25f, -26f, root, "CVForest_L17", 1.35f);
        PlaceTree(-24f, -32f, root, "CVForest_L18", 0.9f);
        PlaceTree(-28f, -24f, root, "CVForest_L19", 1.15f);
        PlaceTree(-26f, -36f, root, "CVForest_L20", 1.0f);
        PlaceTree(-30f, -29f, root, "CVForest_L21", 1.25f);
        PlaceTree(-23f, -39f, root, "CVForest_L22", 1.1f);
        PlaceTree(-27f, -19f, root, "CVForest_L23", 1.0f);

        // Правая сторона (X от +8 до +30) — тоже 3 ряда
        // Ряд 1 — ближний к дороге
        PlaceTree(10f,  -20f, root, "CVForest_R01", 1.15f);
        PlaceTree(9f,   -25f, root, "CVForest_R02", 1.3f);
        PlaceTree(11f,  -29f, root, "CVForest_R03", 1.0f);
        PlaceTree(8f,   -33f, root, "CVForest_R04", 1.2f);
        PlaceTree(10f,  -37f, root, "CVForest_R05", 0.95f);
        PlaceTree(9f,   -41f, root, "CVForest_R06", 1.1f);
        PlaceTree(12f,  -19f, root, "CVForest_R07", 1.05f);
        // Ряд 2 — средний
        PlaceTree(14f,  -22f, root, "CVForest_R08", 1.25f);
        PlaceTree(16f,  -27f, root, "CVForest_R09", 1.1f);
        PlaceTree(15f,  -32f, root, "CVForest_R10", 1.3f);
        PlaceTree(18f,  -25f, root, "CVForest_R11", 1.0f);
        PlaceTree(14f,  -36f, root, "CVForest_R12", 0.95f);
        PlaceTree(17f,  -39f, root, "CVForest_R13", 1.15f);
        PlaceTree(13f,  -42f, root, "CVForest_R14", 1.05f);
        // Ряд 3 — дальний
        PlaceTree(20f,  -21f, root, "CVForest_R15", 1.2f);
        PlaceTree(22f,  -27f, root, "CVForest_R16", 1.35f);
        PlaceTree(25f,  -31f, root, "CVForest_R17", 0.9f);
        PlaceTree(24f,  -36f, root, "CVForest_R18", 1.15f);
        PlaceTree(28f,  -24f, root, "CVForest_R19", 1.0f);
        PlaceTree(21f,  -40f, root, "CVForest_R20", 1.25f);
        PlaceTree(26f,  -20f, root, "CVForest_R21", 1.1f);
    }

    /// <summary>
    /// Деревья вокруг деревни — обрамляют поселение, создают уют.
    /// Не ставим на площади и дорожном коридоре.
    /// </summary>
    private void BuildVillagePerimeter(Transform root)
    {
        float vx = villageCenter.x; // -24
        float vz = villageCenter.z; // -45

        // Дальняя (южная) сторона деревни — плотная стена
        PlaceTree(vx - 18f, vz - 14f, root, "VPeri_S01", 1.1f);
        PlaceTree(vx - 12f, vz - 16f, root, "VPeri_S02", 1.25f);
        PlaceTree(vx - 5f,  vz - 13f, root, "VPeri_S03", 0.95f);
        PlaceTree(vx + 2f,  vz - 15f, root, "VPeri_S04", 1.15f);
        PlaceTree(vx + 8f,  vz - 12f, root, "VPeri_S05", 1.05f);
        PlaceTree(vx - 15f, vz - 18f, root, "VPeri_S06", 1.2f);
        PlaceTree(vx - 8f,  vz - 20f, root, "VPeri_S07", 1.0f);
        PlaceTree(vx + 5f,  vz - 18f, root, "VPeri_S08", 1.3f);

        // Левая (западная) сторона деревни — густая
        PlaceTree(vx - 20f, vz - 5f,  root, "VPeri_W01", 1.2f);
        PlaceTree(vx - 22f, vz + 3f,  root, "VPeri_W02", 1.05f);
        PlaceTree(vx - 18f, vz + 8f,  root, "VPeri_W03", 1.3f);
        PlaceTree(vx - 24f, vz - 1f,  root, "VPeri_W04", 1.15f);
        PlaceTree(vx - 21f, vz + 10f, root, "VPeri_W05", 0.95f);
        PlaceTree(vx - 26f, vz + 5f,  root, "VPeri_W06", 1.1f);

        // Ближняя (северная) к замку
        PlaceTree(vx - 12f, vz + 12f, root, "VPeri_N01", 1.0f);
        PlaceTree(vx + 8f,  vz + 10f, root, "VPeri_N02", 1.1f);
        PlaceTree(vx - 6f,  vz + 14f, root, "VPeri_N03", 1.15f);
        PlaceTree(vx + 2f,  vz + 13f, root, "VPeri_N04", 0.95f);

        // Правая (восточная) — между деревней и дорогой
        PlaceTree(vx + 14f, vz - 3f,  root, "VPeri_E01", 1.15f);
        PlaceTree(vx + 16f, vz - 10f, root, "VPeri_E02", 0.95f);
        PlaceTree(vx + 12f, vz + 5f,  root, "VPeri_E03", 1.2f);
        PlaceTree(vx + 18f, vz - 6f,  root, "VPeri_E04", 1.0f);
        PlaceTree(vx + 15f, vz + 2f,  root, "VPeri_E05", 1.1f);
    }

    /// <summary>
    /// Деревья вдоль дороги от деревни до орочьей зоны (Z=-45 до Z=-90).
    /// Редеют ближе к оркам — мир становится мрачнее.
    /// </summary>
    private void BuildRoadForest(Transform root)
    {
        // Левая сторона дороги — ряд 1 (ближний, X -7..-12)
        PlaceTree(-8f,  -46f, root, "Road_L01", 1.1f);
        PlaceTree(-9f,  -51f, root, "Road_L02", 1.2f);
        PlaceTree(-7f,  -56f, root, "Road_L03", 1.0f);
        PlaceTree(-10f, -61f, root, "Road_L04", 0.95f);
        PlaceTree(-8f,  -66f, root, "Road_L05", 1.15f);
        PlaceTree(-11f, -71f, root, "Road_L06", 0.9f);
        PlaceTree(-9f,  -77f, root, "Road_L07", 0.85f);
        PlaceTree(-10f, -83f, root, "Road_L08", 0.8f);
        PlaceTree(-8f,  -89f, root, "Road_L09", 0.75f);

        // Левая — ряд 2 (средний, X -14..-20)
        PlaceTree(-15f, -48f, root, "Road_L2_01", 1.25f);
        PlaceTree(-17f, -54f, root, "Road_L2_02", 1.15f);
        PlaceTree(-14f, -60f, root, "Road_L2_03", 1.0f);
        PlaceTree(-18f, -67f, root, "Road_L2_04", 0.95f);
        PlaceTree(-16f, -74f, root, "Road_L2_05", 0.9f);
        PlaceTree(-19f, -80f, root, "Road_L2_06", 0.85f);

        // Левая — ряд 3 (дальний, X -22..-30)
        PlaceTree(-23f, -50f, root, "Road_L3_01", 1.3f);
        PlaceTree(-26f, -58f, root, "Road_L3_02", 1.2f);
        PlaceTree(-24f, -66f, root, "Road_L3_03", 1.1f);
        PlaceTree(-28f, -74f, root, "Road_L3_04", 0.95f);

        // Правая сторона дороги — ряд 1 (ближний, X 8..12)
        PlaceTree(9f,   -48f, root, "Road_R01", 1.15f);
        PlaceTree(10f,  -53f, root, "Road_R02", 1.1f);
        PlaceTree(8f,   -58f, root, "Road_R03", 1.0f);
        PlaceTree(11f,  -63f, root, "Road_R04", 1.2f);
        PlaceTree(9f,   -68f, root, "Road_R05", 0.9f);
        PlaceTree(12f,  -74f, root, "Road_R06", 0.85f);
        PlaceTree(10f,  -80f, root, "Road_R07", 0.8f);
        PlaceTree(11f,  -86f, root, "Road_R08", 0.75f);

        // Правая — ряд 2 (средний, X 14..20)
        PlaceTree(15f,  -50f, root, "Road_R2_01", 1.2f);
        PlaceTree(17f,  -56f, root, "Road_R2_02", 1.1f);
        PlaceTree(14f,  -62f, root, "Road_R2_03", 1.0f);
        PlaceTree(18f,  -69f, root, "Road_R2_04", 0.95f);
        PlaceTree(16f,  -76f, root, "Road_R2_05", 0.85f);
        PlaceTree(19f,  -82f, root, "Road_R2_06", 0.8f);

        // Правая — ряд 3 (дальний, X 22..30)
        PlaceTree(23f,  -52f, root, "Road_R3_01", 1.3f);
        PlaceTree(26f,  -60f, root, "Road_R3_02", 1.15f);
        PlaceTree(24f,  -68f, root, "Road_R3_03", 1.05f);
        PlaceTree(28f,  -76f, root, "Road_R3_04", 0.9f);
    }

    /// <summary>
    /// Большие деревья на горизонте (X=±35-60) — закрывают пустое небо.
    /// Масштаб крупнее, чтобы силуэты были видны издалека.
    /// </summary>
    private void BuildHorizonTrees(Transform root)
    {
        // Далёкий левый горизонт — плотная линия (X -35..-55)
        PlaceTree(-36f, -10f, root, "Horiz_FL01", 1.8f);
        PlaceTree(-40f, -18f, root, "Horiz_FL02", 2.0f);
        PlaceTree(-44f, -26f, root, "Horiz_FL03", 1.7f);
        PlaceTree(-38f, -34f, root, "Horiz_FL04", 1.9f);
        PlaceTree(-42f, -42f, root, "Horiz_FL05", 2.1f);
        PlaceTree(-46f, -50f, root, "Horiz_FL06", 1.6f);
        PlaceTree(-40f, -58f, root, "Horiz_FL07", 1.85f);
        PlaceTree(-44f, -66f, root, "Horiz_FL08", 2.0f);
        PlaceTree(-48f, -74f, root, "Horiz_FL09", 1.7f);
        PlaceTree(-42f, -82f, root, "Horiz_FL10", 1.9f);
        // Второй ряд дальше (X -50..-60)
        PlaceTree(-52f, -14f, root, "Horiz_FL11", 2.2f);
        PlaceTree(-55f, -30f, root, "Horiz_FL12", 2.0f);
        PlaceTree(-50f, -46f, root, "Horiz_FL13", 2.3f);
        PlaceTree(-54f, -62f, root, "Horiz_FL14", 1.8f);
        PlaceTree(-52f, -78f, root, "Horiz_FL15", 2.1f);

        // Далёкий правый горизонт — плотная линия (X 35..55)
        PlaceTree(35f,  -12f, root, "Horiz_FR01", 1.9f);
        PlaceTree(39f,  -20f, root, "Horiz_FR02", 1.7f);
        PlaceTree(43f,  -28f, root, "Horiz_FR03", 2.0f);
        PlaceTree(37f,  -36f, root, "Horiz_FR04", 1.8f);
        PlaceTree(41f,  -44f, root, "Horiz_FR05", 2.1f);
        PlaceTree(45f,  -52f, root, "Horiz_FR06", 1.6f);
        PlaceTree(39f,  -60f, root, "Horiz_FR07", 1.85f);
        PlaceTree(43f,  -68f, root, "Horiz_FR08", 2.0f);
        PlaceTree(47f,  -76f, root, "Horiz_FR09", 1.7f);
        PlaceTree(41f,  -84f, root, "Horiz_FR10", 1.9f);
        // Второй ряд дальше (X 50..60)
        PlaceTree(51f,  -16f, root, "Horiz_FR11", 2.2f);
        PlaceTree(54f,  -32f, root, "Horiz_FR12", 2.0f);
        PlaceTree(50f,  -48f, root, "Horiz_FR13", 2.3f);
        PlaceTree(53f,  -64f, root, "Horiz_FR14", 1.8f);
        PlaceTree(51f,  -80f, root, "Horiz_FR15", 2.1f);

        // === ЗАДНИК ЗА ЗАМКОМ — ГУСТЕЙШИЙ ЛЕС (Z от -5 до +35) ===
        // Замок ~X±15, Z±16 — ближние ряды только |X|>20!
        // Ряд 1 — ближний к замку (Z -5..+5), ТОЛЬКО за пределами замка
        PlaceTree(-40f, -4f,  root, "Back1_01", 1.8f);
        PlaceTree(-32f, -2f,  root, "Back1_02", 1.6f);
        PlaceTree(-24f, 0f,   root, "Back1_03", 1.7f);
        PlaceTree(24f,  1f,   root, "Back1_09", 1.7f);
        PlaceTree(32f,  -3f,  root, "Back1_10", 1.6f);
        PlaceTree(40f,  -1f,  root, "Back1_11", 1.9f);

        // Ряд 2 (Z +5..+12) — центр (|X|<18) только от Z>8
        PlaceTree(-45f, 6f,   root, "Back2_01", 2.0f);
        PlaceTree(-36f, 8f,   root, "Back2_02", 1.7f);
        PlaceTree(-28f, 5f,   root, "Back2_03", 1.9f);
        PlaceTree(-20f, 9f,   root, "Back2_04", 1.6f);
        PlaceTree(20f,  9f,   root, "Back2_09", 1.6f);
        PlaceTree(28f,  7f,   root, "Back2_10", 1.8f);
        PlaceTree(36f,  5f,   root, "Back2_11", 2.0f);
        PlaceTree(44f,  8f,   root, "Back2_12", 1.7f);

        // Ряд 3 (Z +12..+20)
        PlaceTree(-48f, 14f,  root, "Back3_01", 2.1f);
        PlaceTree(-40f, 16f,  root, "Back3_02", 1.8f);
        PlaceTree(-32f, 13f,  root, "Back3_03", 2.0f);
        PlaceTree(-24f, 17f,  root, "Back3_04", 1.7f);
        PlaceTree(-16f, 15f,  root, "Back3_05", 1.9f);
        PlaceTree(-8f,  18f,  root, "Back3_06", 1.6f);
        PlaceTree(0f,   14f,  root, "Back3_07", 2.0f);
        PlaceTree(8f,   16f,  root, "Back3_08", 1.8f);
        PlaceTree(16f,  13f,  root, "Back3_09", 1.7f);
        PlaceTree(24f,  17f,  root, "Back3_10", 1.9f);
        PlaceTree(32f,  15f,  root, "Back3_11", 2.1f);
        PlaceTree(40f,  14f,  root, "Back3_12", 1.8f);
        PlaceTree(48f,  16f,  root, "Back3_13", 2.0f);

        // Ряд 4 — самый дальний (Z +20..+30)
        PlaceTree(-50f, 22f,  root, "Back4_01", 2.2f);
        PlaceTree(-42f, 25f,  root, "Back4_02", 1.9f);
        PlaceTree(-34f, 23f,  root, "Back4_03", 2.1f);
        PlaceTree(-26f, 26f,  root, "Back4_04", 1.8f);
        PlaceTree(-18f, 22f,  root, "Back4_05", 2.0f);
        PlaceTree(-10f, 27f,  root, "Back4_06", 1.7f);
        PlaceTree(-2f,  24f,  root, "Back4_07", 2.2f);
        PlaceTree(6f,   26f,  root, "Back4_08", 1.9f);
        PlaceTree(14f,  22f,  root, "Back4_09", 2.0f);
        PlaceTree(22f,  25f,  root, "Back4_10", 1.8f);
        PlaceTree(30f,  23f,  root, "Back4_11", 2.1f);
        PlaceTree(38f,  27f,  root, "Back4_12", 1.9f);
        PlaceTree(46f,  22f,  root, "Back4_13", 2.2f);

        // Ряд 5 — заполнение промежутков (центр только Z>16)
        PlaceTree(-44f, 10f,  root, "Back5_01", 1.7f);
        PlaceTree(-36f, 20f,  root, "Back5_02", 2.0f);
        PlaceTree(-28f, 11f,  root, "Back5_03", 1.6f);
        PlaceTree(-20f, 19f,  root, "Back5_04", 1.9f);
        PlaceTree(-12f, 22f,  root, "Back5_05", 1.8f);
        PlaceTree(-4f,  20f,  root, "Back5_06", 1.5f);
        PlaceTree(4f,   20f,  root, "Back5_07", 2.0f);
        PlaceTree(12f,  22f,  root, "Back5_08", 1.7f);
        PlaceTree(20f,  19f,  root, "Back5_09", 1.6f);
        PlaceTree(28f,  19f,  root, "Back5_10", 1.9f);
        PlaceTree(36f,  11f,  root, "Back5_11", 1.8f);
        PlaceTree(44f,  20f,  root, "Back5_12", 2.0f);

        // Углы — заполняем дальние диагонали
        PlaceTree(-55f, 10f,  root, "BackCorner_01", 2.3f);
        PlaceTree(-55f, 20f,  root, "BackCorner_02", 2.1f);
        PlaceTree(-52f, 28f,  root, "BackCorner_03", 2.4f);
        PlaceTree(55f,  10f,  root, "BackCorner_04", 2.3f);
        PlaceTree(55f,  20f,  root, "BackCorner_05", 2.1f);
        PlaceTree(52f,  28f,  root, "BackCorner_06", 2.4f);
        PlaceTree(-48f, 3f,   root, "BackCorner_07", 2.0f);
        PlaceTree(48f,  3f,   root, "BackCorner_08", 2.0f);

        // === ДЕРЕВЬЯ ПРЯМО ЗА ЗАМКОМ (Z>26 — за задней стеной) ===
        // IsInsideCastle пропускает их т.к. z>=26
        PlaceTree(-10f, 28f, root, "BehCastle_01", 1.6f);
        PlaceTree(10f,  28f, root, "BehCastle_02", 1.6f);
        PlaceTree(-5f,  32f, root, "BehCastle_03", 1.7f);
        PlaceTree(5f,   32f, root, "BehCastle_04", 1.7f);
        PlaceTree(0f,   30f, root, "BehCastle_05", 1.8f);
        PlaceTree(-15f, 30f, root, "BehCastle_06", 1.7f);
        PlaceTree(15f,  30f, root, "BehCastle_07", 1.7f);
        PlaceTree(-8f,  38f, root, "BehCastle_08", 1.9f);
        PlaceTree(8f,   38f, root, "BehCastle_09", 1.9f);
        PlaceTree(0f,   44f, root, "BehCastle_10", 2.0f);
        PlaceTree(-18f, 36f, root, "BehCastle_11", 1.8f);
        PlaceTree(18f,  36f, root, "BehCastle_12", 1.8f);
        PlaceTree(-12f, 48f, root, "BehCastle_13", 1.7f);
        PlaceTree(12f,  48f, root, "BehCastle_14", 1.7f);
        PlaceTree(-4f,  52f, root, "BehCastle_15", 1.9f);
        PlaceTree(4f,   52f, root, "BehCastle_16", 1.9f);
    }

    /// <summary>
    /// Камни PP_Rock_Moss между деревьями — добавляют разнообразие.
    /// </summary>
    private void BuildRocks(Transform root)
    {
        PlaceRock(RockMoss09, -13f, -25f, 0.6f, root, "ForRock_01");
        PlaceRock(RockMoss11, 13f,  -27f, 0.5f, root, "ForRock_02");
        PlaceRock(RockPile05, -17f, -35f, 0.7f, root, "ForRock_03");
        PlaceRock(RockMoss09, 17f,  -40f, 0.55f, root, "ForRock_04");
        PlaceRock(RockMoss11, -11f, -55f, 0.6f, root, "ForRock_05");
        PlaceRock(RockPile05, 12f,  -58f, 0.5f, root, "ForRock_06");
        PlaceRock(RockMoss09, -15f, -70f, 0.45f, root, "ForRock_07");
        PlaceRock(RockMoss11, 14f,  -73f, 0.5f, root, "ForRock_08");

        // У деревни
        PlaceRock(RockPile05, villageCenter.x - 16f, villageCenter.z - 8f, 0.65f, root, "VilRock_01");
        PlaceRock(RockMoss09, villageCenter.x + 10f, villageCenter.z - 11f, 0.55f, root, "VilRock_02");
    }

    private void BuildGroundcover(Transform root)
    {
        // === PP GRASS — густая трава вдоль всей дороги ===
        PlaceGrass(Grass11, -12f, -23f, 0.9f, root, "FGrass_01");
        PlaceGrass(Grass15, 11f,  -24f, 0.8f, root, "FGrass_02");
        PlaceGrass(Grass11, -16f, -32f, 0.85f, root, "FGrass_03");
        PlaceGrass(Grass15, 15f,  -35f, 0.9f, root, "FGrass_04");
        PlaceGrass(Grass11, -10f, -50f, 0.8f, root, "FGrass_05");
        PlaceGrass(Grass15, 12f,  -53f, 0.75f, root, "FGrass_06");
        PlaceGrass(Grass11, -13f, -65f, 0.7f, root, "FGrass_07");
        PlaceGrass(Grass15, 14f,  -68f, 0.7f, root, "FGrass_08");

        // === ГУСТАЯ ТРАВА — заполняем пустоту между деревьями ===
        string[] grassPaths = { Grass11, Grass15, RPGGrass01, RPGGrass02 };
        for (int i = 0; i < 60; i++)
        {
            float z = Mathf.Lerp(-18f, -95f, i / 59f);
            float x = (i % 2 == 0) ? -(7f + (i % 8) * 3.5f) : (7f + (i % 8) * 3.5f);
            float sc = 0.6f + (i % 5) * 0.15f;
            PlaceGrass(grassPaths[i % grassPaths.Length], x, z, sc, root, "DenseGrass_" + i);
        }

        // === ЛУГА (PP_Meadow) — ТОЛЬКО вдоль дороги, НЕ в деревне (деревня = -24,-45 r26) ===
        // Дорога идёт по X≈0..8, деревня слева (X<-10). Луга держим у дороги |X|<24 и подальше от деревни.
        float vx = villageCenter.x;
        float vz = villageCenter.z;
        PlaceGrass(Meadow08, 18f,  -16f, 0.7f, root, "Meadow_Gate02");
        PlaceGrass(Meadow07, 20f,  -42f, 0.7f, root, "Meadow_Road02");
        PlaceGrass(MeadowPath, 22f, -55f, 0.5f, root, "Meadow_Road03");
        PlaceGrass(Meadow08, 24f,  -60f, 0.6f, root, "Meadow_Road04");
        PlaceGrass(Meadow07, 20f,  -72f, 0.5f, root, "Meadow_Road05");
        PlaceGrass(Meadow08, 22f,  -78f, 0.6f, root, "Meadow_Road06");

        // === RPGPP КУСТЫ — вдоль дороги (правая сторона + дальние, НЕ в деревне) ===
        string[] bushPaths = { RPGBush01, RPGBush02 };
        PlaceGrass(bushPaths[0], -9f,  -28f, 1.2f, root, "Bush_01");
        PlaceGrass(bushPaths[1], 10f,  -31f, 1.0f, root, "Bush_02");
        PlaceGrass(bushPaths[1], 16f,  -48f, 1.1f, root, "Bush_04");
        PlaceGrass(bushPaths[0], -11f, -58f, 1.0f, root, "Bush_05");
        PlaceGrass(bushPaths[1], 13f,  -62f, 1.2f, root, "Bush_06");
        PlaceGrass(bushPaths[0], -18f, -74f, 1.1f, root, "Bush_07");
        PlaceGrass(bushPaths[1], 20f,  -80f, 1.0f, root, "Bush_08");

        // === RPGPP РАСТЕНИЯ — вдоль дороги ===
        PlaceGrass(RPGPlant01, -7f,  -25f, 1.0f, root, "Plant_01");
        PlaceGrass(RPGPlant02, 8f,   -33f, 1.1f, root, "Plant_02");
        PlaceGrass(RPGPlant02, 14f,  -55f, 1.0f, root, "Plant_04");

        // === RPGPP TERRAIN GRASS — мелкие патчи, ТОЛЬКО правая сторона (слева деревня!) ===
        PlaceGrass(RPGTerrainGrass02, 22f,  -30f, 0.4f,  root, "TerrGrass_02");
        PlaceGrass(RPGTerrainGrass02, 28f,  -55f, 0.4f,  root, "TerrGrass_04");
        PlaceGrass(RPGTerrainGrass01, 25f,  -80f, 0.35f, root, "TerrGrass_06");
        PlaceGrass(RPGTerrainGrass01, 30f,  -100f, 0.4f, root, "TerrGrass_07");

        // === RPGPP ХОЛМИКИ — мелкие, ТОЛЬКО правая сторона ===
        PlaceGrass(RPGHillSmall02, 38f,  -28f, 0.35f, root, "Hill_02");
        PlaceGrass(RPGHillSmall02, 40f,  -58f, 0.35f, root, "Hill_04");
        PlaceGrass(RPGHillSmall01, 42f,  -90f, 0.4f,  root, "Hill_05");

        // === ЦВЕТЫ — яркие акценты ===
        PlaceFlower(Daffodil,  vx - 10f, vz + 8f,  0.8f, root, "VFlower_01");
        PlaceFlower(Sunflower, vx + 6f,  vz + 6f,  0.9f, root, "VFlower_02");
        PlaceFlower(Hyacinth,  vx - 14f, vz - 5f,  0.85f, root, "VFlower_03");
        PlaceFlower(Daffodil,  vx + 3f,  vz - 10f, 0.75f, root, "VFlower_04");
        PlaceFlower(Sunflower, -7f, -42f, 0.7f, root, "RFlower_01");
        PlaceFlower(Daffodil,  8f,  -46f, 0.8f, root, "RFlower_02");
        PlaceFlower(Sunflower, vx + 12f, vz - 2f,  0.85f, root, "VFlower_05");
        PlaceFlower(Daffodil,  vx + 15f, vz - 7f,  0.9f,  root, "VFlower_06");
        PlaceFlower(Hyacinth,  vx + 10f, vz + 4f,  0.75f, root, "VFlower_07");
        PlaceFlower(Sunflower, vx - 8f,  vz + 10f, 0.8f,  root, "VFlower_08");
        PlaceFlower(Daffodil,  vx + 8f,  vz - 13f, 0.7f,  root, "VFlower_09");
        PlaceFlower(Hyacinth,  vx - 16f, vz + 3f,  0.85f, root, "VFlower_10");

        // RPGPP цветы — дополнительное разнообразие
        string[] rpgFlowers = { RPGFlower01, RPGFlower02, RPGFlower03 };
        for (int i = 0; i < 15; i++)
        {
            float z = Mathf.Lerp(-20f, -85f, i / 14f);
            float x = (i % 2 == 0) ? -(8f + (i % 6) * 2.5f) : (8f + (i % 6) * 2.5f);
            PlaceFlower(rpgFlowers[i % rpgFlowers.Length], x, z, 0.8f + (i % 4) * 0.1f, root, "RPGFlower_" + i);
        }

        // === ЛУГА И ТРАВА ЗА ЗАМКОМ (Z>0) — закрывают серую землю позади ===
        PlaceGrass(Meadow07, -25f,  8f,  0.7f, root, "BehCMeadow_01");
        PlaceGrass(Meadow08,  25f,  8f,  0.7f, root, "BehCMeadow_02");
        PlaceGrass(Meadow07, -30f, 18f,  0.65f, root, "BehCMeadow_03");
        PlaceGrass(Meadow08,  30f, 18f,  0.65f, root, "BehCMeadow_04");
        PlaceGrass(Meadow07, -22f, 28f,  0.7f, root, "BehCMeadow_05");
        PlaceGrass(Meadow08,  22f, 28f,  0.7f, root, "BehCMeadow_06");
        PlaceGrass(MeadowPath,-28f, 35f, 0.6f, root, "BehCMeadow_07");
        PlaceGrass(MeadowPath, 28f, 35f, 0.6f, root, "BehCMeadow_08");
        // За центром замка (Z>26 — безопасно снаружи стен)
        PlaceGrass(Meadow07,  -8f, 32f,  0.7f, root, "BehCMeadow_09");
        PlaceGrass(Meadow08,   8f, 32f,  0.7f, root, "BehCMeadow_10");
        PlaceGrass(Meadow07,   0f, 40f,  0.75f, root, "BehCMeadow_11");
        PlaceGrass(Meadow08, -15f, 44f,  0.65f, root, "BehCMeadow_12");
        PlaceGrass(Meadow07,  15f, 44f,  0.65f, root, "BehCMeadow_13");
        PlaceGrass(MeadowPath, -6f, 50f, 0.6f, root, "BehCMeadow_14");
        PlaceGrass(MeadowPath,  6f, 50f, 0.6f, root, "BehCMeadow_15");
        PlaceGrass(Meadow07, -20f, 55f,  0.7f, root, "BehCMeadow_16");
        PlaceGrass(Meadow08,  20f, 55f,  0.7f, root, "BehCMeadow_17");
        PlaceGrass(Meadow07,   0f, 60f,  0.75f, root, "BehCMeadow_18");
        // RPGPP terrain за замком — текстурные патчи
        PlaceGrass(RPGTerrainGrass01, -32f, 12f, 0.4f, root, "BehCTerr_01");
        PlaceGrass(RPGTerrainGrass02,  32f, 12f, 0.4f, root, "BehCTerr_02");
        PlaceGrass(RPGTerrainGrass01, -35f, 28f, 0.45f, root, "BehCTerr_03");
        PlaceGrass(RPGTerrainGrass02,  35f, 28f, 0.45f, root, "BehCTerr_04");
        PlaceGrass(RPGTerrainGrass01,   0f, 38f, 0.4f, root, "BehCTerr_05");
        PlaceGrass(RPGTerrainGrass02, -20f, 48f, 0.4f, root, "BehCTerr_06");
        PlaceGrass(RPGTerrainGrass01,  20f, 48f, 0.4f, root, "BehCTerr_07");
        PlaceGrass(RPGTerrainGrass01,   0f, 58f, 0.45f, root, "BehCTerr_08");
        // Холмики за замком
        PlaceGrass(RPGHillSmall01, -42f, 18f, 0.4f, root, "BehCHill_01");
        PlaceGrass(RPGHillSmall02,  42f, 18f, 0.4f, root, "BehCHill_02");
        PlaceGrass(RPGHillSmall01, -38f, 38f, 0.4f, root, "BehCHill_03");
        PlaceGrass(RPGHillSmall02,  38f, 38f, 0.4f, root, "BehCHill_04");
        PlaceGrass(RPGHillSmall01, -30f, 55f, 0.45f, root, "BehCHill_05");
        PlaceGrass(RPGHillSmall02,  30f, 55f, 0.45f, root, "BehCHill_06");
    }

    /// <summary>
    /// Кольцо гор PP_Forest_Mountain ДАЛЕКО по периметру карты.
    /// PP_Forest_Mountain при scale 1.0 уже ~20м в высоту, поэтому scale 2-3 достаточно.
    /// Горы стоят на |X|>150 и Z>120 / Z&lt;-380 — ДАЛЕКО от игровой зоны.
    /// Игрок видит красивые горные силуэты на горизонте, не стену перед носом.
    /// </summary>
    private void BuildMountainRing(Transform root)
    {
        // === ЗА ЗАМКОМ (Z > 120) — далёкий горный хребет ===
        PlaceMountain(Mountain01, -120f, 160f, 3.0f, 10f,  root, "Mt_Back_01");
        PlaceMountain(Mountain02, -60f,  170f, 3.5f, 30f,  root, "Mt_Back_02");
        PlaceMountain(Mountain01,  0f,   180f, 4.0f, 0f,   root, "Mt_Back_03");
        PlaceMountain(Mountain02,  60f,  170f, 3.5f, 50f,  root, "Mt_Back_04");
        PlaceMountain(Mountain01,  120f, 160f, 3.0f, 80f,  root, "Mt_Back_05");
        PlaceMountain(Mountain02, -90f,  200f, 4.0f, 120f, root, "Mt_Back_06");
        PlaceMountain(Mountain01,  30f,  210f, 4.5f, 60f,  root, "Mt_Back_07");
        PlaceMountain(Mountain02,  90f,  195f, 3.5f, 150f, root, "Mt_Back_08");

        // === ЛЕВАЯ СТЕНА (X = -160...-200) — от замка до портала ===
        PlaceMountain(Mountain01, -170f,  60f,  3.0f, 200f, root, "Mt_Left_01");
        PlaceMountain(Mountain02, -180f,  10f,  3.5f, 240f, root, "Mt_Left_02");
        PlaceMountain(Mountain01, -175f, -40f,  3.0f, 170f, root, "Mt_Left_03");
        PlaceMountain(Mountain02, -180f, -90f,  3.5f, 280f, root, "Mt_Left_04");
        PlaceMountain(Mountain01, -175f, -140f, 3.0f, 310f, root, "Mt_Left_05");
        PlaceMountain(Mountain02, -180f, -190f, 3.5f, 130f, root, "Mt_Left_06");
        PlaceMountain(Mountain01, -175f, -240f, 3.0f, 220f, root, "Mt_Left_07");
        PlaceMountain(Mountain02, -180f, -290f, 3.5f, 50f,  root, "Mt_Left_08");
        PlaceMountain(Mountain01, -175f, -340f, 3.0f, 180f, root, "Mt_Left_09");
        // Дальний ряд
        PlaceMountain(Mountain02, -220f,  30f,  4.0f, 320f, root, "Mt_Left2_01");
        PlaceMountain(Mountain01, -230f, -60f,  4.5f, 70f,  root, "Mt_Left2_02");
        PlaceMountain(Mountain02, -220f, -160f, 4.0f, 140f, root, "Mt_Left2_03");
        PlaceMountain(Mountain01, -230f, -260f, 4.5f, 250f, root, "Mt_Left2_04");

        // === ПРАВАЯ СТЕНА (X = 160...200) — от замка до портала ===
        PlaceMountain(Mountain02, 170f,  60f,  3.0f, 15f,  root, "Mt_Right_01");
        PlaceMountain(Mountain01, 180f,  10f,  3.5f, 55f,  root, "Mt_Right_02");
        PlaceMountain(Mountain02, 175f, -40f,  3.0f, 100f, root, "Mt_Right_03");
        PlaceMountain(Mountain01, 180f, -90f,  3.5f, 160f, root, "Mt_Right_04");
        PlaceMountain(Mountain02, 175f, -140f, 3.0f, 210f, root, "Mt_Right_05");
        PlaceMountain(Mountain01, 180f, -190f, 3.5f, 270f, root, "Mt_Right_06");
        PlaceMountain(Mountain02, 175f, -240f, 3.0f, 330f, root, "Mt_Right_07");
        PlaceMountain(Mountain01, 180f, -290f, 3.5f, 40f,  root, "Mt_Right_08");
        PlaceMountain(Mountain02, 175f, -340f, 3.0f, 300f, root, "Mt_Right_09");
        // Дальний ряд
        PlaceMountain(Mountain01, 220f,  30f,  4.0f, 25f,  root, "Mt_Right2_01");
        PlaceMountain(Mountain02, 230f, -60f,  4.5f, 95f,  root, "Mt_Right2_02");
        PlaceMountain(Mountain01, 220f, -160f, 4.0f, 175f, root, "Mt_Right2_03");
        PlaceMountain(Mountain02, 230f, -260f, 4.5f, 235f, root, "Mt_Right2_04");

        // === ЗА ПОРТАЛОМ (Z < -380) — далёкий хребет ===
        PlaceMountain(Mountain01, -100f, -400f, 3.5f, 45f,  root, "Mt_Far_01");
        PlaceMountain(Mountain02, -30f,  -420f, 4.0f, 120f, root, "Mt_Far_02");
        PlaceMountain(Mountain01,  40f,  -430f, 4.5f, 200f, root, "Mt_Far_03");
        PlaceMountain(Mountain02,  110f, -410f, 3.5f, 280f, root, "Mt_Far_04");
        PlaceMountain(Mountain01, -60f,  -450f, 4.0f, 170f, root, "Mt_Far_05");
        PlaceMountain(Mountain02,  70f,  -445f, 4.0f, 90f,  root, "Mt_Far_06");

        // === УГЛОВЫЕ ГОРЫ — заполняют диагонали ===
        PlaceMountain(Mountain02, -200f,  130f,  3.5f, 110f, root, "Mt_Corner_01");
        PlaceMountain(Mountain01,  200f,  130f,  3.5f, 230f, root, "Mt_Corner_02");
        PlaceMountain(Mountain02, -200f, -380f,  3.5f, 160f, root, "Mt_Corner_03");
        PlaceMountain(Mountain01,  200f, -380f,  3.5f, 300f, root, "Mt_Corner_04");
    }

    /// <summary>
    /// Лес в переходной зоне Z=-90...-120 (между живым лесом и ареной).
    /// Деревья редеют и переходят в мёртвые ближе к арене.
    /// Также мёртвые деревья-силуэты вокруг арены на горизонте.
    /// </summary>
    private void BuildTransitionForest(Transform root)
    {
        // === Живые деревья (Z=-90...-105) — редкие, завершение леса ===
        PlaceTree(-12f, -92f,  root, "Trans_L01", 0.7f);
        PlaceTree(-18f, -96f,  root, "Trans_L02", 0.65f);
        PlaceTree(-25f, -93f,  root, "Trans_L03", 0.8f);
        PlaceTree(-14f, -100f, root, "Trans_L04", 0.6f);
        PlaceTree(-22f, -103f, root, "Trans_L05", 0.55f);
        PlaceTree(14f,  -94f,  root, "Trans_R01", 0.7f);
        PlaceTree(20f,  -97f,  root, "Trans_R02", 0.75f);
        PlaceTree(26f,  -91f,  root, "Trans_R03", 0.8f);
        PlaceTree(16f,  -101f, root, "Trans_R04", 0.6f);
        PlaceTree(23f,  -105f, root, "Trans_R05", 0.55f);

        // === Мёртвые деревья (Z=-105...-120) — переход к орочьей территории ===
        PlaceDeadTree(-10f, -108f, 1.5f, root, "TransDead_L01");
        PlaceDeadTree(-20f, -112f, 1.8f, root, "TransDead_L02");
        PlaceDeadTree(-30f, -106f, 1.4f, root, "TransDead_L03");
        PlaceDeadTree(-15f, -116f, 1.6f, root, "TransDead_L04");
        PlaceDeadTree(12f,  -110f, 1.5f, root, "TransDead_R01");
        PlaceDeadTree(22f,  -107f, 1.7f, root, "TransDead_R02");
        PlaceDeadTree(32f,  -113f, 1.4f, root, "TransDead_R03");
        PlaceDeadTree(18f,  -118f, 1.6f, root, "TransDead_R04");

        // Пни в переходной зоне
        PlaceDeadStump(-16f, -110f, 1.2f, root, "TransStump_01");
        PlaceDeadStump(15f,  -115f, 1.0f, root, "TransStump_02");
        PlaceDeadStump(-25f, -114f, 1.3f, root, "TransStump_03");
        PlaceDeadStump(28f,  -109f, 1.1f, root, "TransStump_04");

        // === Мёртвые деревья-силуэты вокруг арены на горизонте ===
        // Левая сторона арены (X=-50...-70)
        PlaceDeadTree(-55f, -130f, 2.2f, root, "ArenaSil_L01");
        PlaceDeadTree(-60f, -155f, 2.5f, root, "ArenaSil_L02");
        PlaceDeadTree(-52f, -180f, 2.0f, root, "ArenaSil_L03");
        PlaceDeadTree(-58f, -205f, 2.3f, root, "ArenaSil_L04");
        PlaceDeadTree(-55f, -235f, 2.1f, root, "ArenaSil_L05");
        PlaceDeadTree(-60f, -260f, 2.4f, root, "ArenaSil_L06");
        // Правая сторона арены (X=50...70)
        PlaceDeadTree(55f,  -135f, 2.2f, root, "ArenaSil_R01");
        PlaceDeadTree(62f,  -160f, 2.5f, root, "ArenaSil_R02");
        PlaceDeadTree(53f,  -185f, 2.0f, root, "ArenaSil_R03");
        PlaceDeadTree(60f,  -210f, 2.3f, root, "ArenaSil_R04");
        PlaceDeadTree(56f,  -240f, 2.1f, root, "ArenaSil_R05");
        PlaceDeadTree(62f,  -265f, 2.4f, root, "ArenaSil_R06");
        // За порталом (Z < -280)
        PlaceDeadTree(-30f, -285f, 2.0f, root, "ArenaSil_B01");
        PlaceDeadTree(0f,   -290f, 2.3f, root, "ArenaSil_B02");
        PlaceDeadTree(30f,  -288f, 2.1f, root, "ArenaSil_B03");
        PlaceDeadTree(-15f, -295f, 1.8f, root, "ArenaSil_B04");
        PlaceDeadTree(20f,  -293f, 1.9f, root, "ArenaSil_B05");

        // Горизонтальные ряды деревьев дальше по бокам (заполнение пустоты)
        PlaceTree(-45f, -100f, root, "TransHoriz_L01", 1.5f);
        PlaceTree(-50f, -110f, root, "TransHoriz_L02", 1.7f);
        PlaceTree(-48f, -120f, root, "TransHoriz_L03", 1.4f);
        PlaceTree(45f,  -102f, root, "TransHoriz_R01", 1.5f);
        PlaceTree(50f,  -112f, root, "TransHoriz_R02", 1.6f);
        PlaceTree(48f,  -118f, root, "TransHoriz_R03", 1.4f);
    }

    // --- Placement helpers ---

    // Тинты против серости
    private static readonly Color MossRockTint = new Color(0.78f, 0.86f, 0.62f);
    private static readonly Color ForestMountainTint = new Color(0.62f, 0.78f, 0.60f);

    // Зона замка: |X|<17 AND -12<Z<26 — сюда деревья НЕ ставить
    private static bool IsInsideCastle(float x, float z)
    {
        return Mathf.Abs(x) < 17f && z > -12f && z < 26f;
    }

    private static bool IsInsideBlacksmith(float x, float z)
    {
        // Зона расширена: учитываем большие кроны PP-деревьев, чтобы они не свисали
        // на платформу кузни (даже если ствол стоит чуть за её краем).
        return x > -3f && x < 13.5f && z > -32f && z < -17f;
    }

    private void PlaceTree(float x, float z, Transform parent, string name, float scale)
    {
        if (IsInsideCastle(x, z) || IsInsideBlacksmith(x, z)) return;
        string prefabPath = TreePrefabs[Mathf.Abs(name.GetHashCode()) % TreePrefabs.Length];
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject t = PlacePrefab(prefabPath, pos, scale, parent, name, true); // keepColliders=true
        if (t != null) L0Util.TintTree(t);
    }

    private void PlaceRock(string path, float x, float z, float scale, Transform parent, string name)
    {
        if (IsInsideBlacksmith(x, z)) return;
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject r = PlacePrefab(path, pos, scale, parent, name, true); // keepColliders=true
        if (r != null) L0Util.TintMaterials(r, MossRockTint);
    }

    private static readonly Color GrassTint = new Color(0.70f, 1.0f, 0.55f); // сочная зелень

    private void PlaceGrass(string path, float x, float z, float scale, Transform parent, string name)
    {
        if (IsInsideBlacksmith(x, z)) return;
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject g = PlacePrefab(path, pos, scale, parent, name, false);
        if (g != null) L0Util.TintMaterials(g, GrassTint);
    }

    private void PlaceFlower(string path, float x, float z, float scale, Transform parent, string name)
    {
        Vector3 pos = new Vector3(x, 0.02f, z);
        PlacePrefab(path, pos, scale, parent, name);
    }

    private void PlaceMountain(string path, float x, float z, float scale, float yRot, Transform parent, string name)
    {
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject prefab = L0Util.LoadPrefab(path);
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, yRot, 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.TintMaterials(obj, ForestMountainTint); // горы — лесные, не серый камень
        L0Util.RemoveAllColliders(obj);
    }

    private void PlaceDeadTree(float x, float z, float scale, Transform parent, string name)
    {
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject prefab = L0Util.LoadPrefab(DeadTree);
        if (prefab == null)
        {
            L0Util.CreateFallbackTree(pos, scale, parent, name);
            return;
        }
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.RemoveAllColliders(obj);
    }

    private void PlaceDeadStump(float x, float z, float scale, Transform parent, string name)
    {
        Vector3 pos = new Vector3(x, 0f, z);
        GameObject prefab = L0Util.LoadPrefab(DeadStump);
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        L0Util.RemoveAllColliders(obj);
    }

    private GameObject PlacePrefab(string path, Vector3 pos, float scale, Transform parent, string name, bool keepColliders = false)
    {
        GameObject prefab = L0Util.LoadPrefab(path);
        if (prefab == null)
        {
            L0Util.CreateFallbackTree(pos, scale, parent, name);
            return null;
        }
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), parent);
        obj.name = name;
        obj.transform.localScale = Vector3.one * scale;
        L0Util.FixMaterials(obj);
        if (!keepColliders) L0Util.RemoveAllColliders(obj);
        return obj;
    }

    [ContextMenu("Clear Forest")]
    public void ClearForest()
    {
        if (forestContainer != null)
        {
            L0Util.SafeDestroy(forestContainer);
            forestContainer = null;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name == ContainerName)
                L0Util.SafeDestroy(child.gameObject);
        }
    }
}
