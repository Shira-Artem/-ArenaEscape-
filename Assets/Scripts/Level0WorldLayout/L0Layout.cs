using UnityEngine;

/// <summary>
/// Главный сборщик визуального layout для Level0_Castle.
/// v7: усиливает орочью деревню, добавляет безопасный support floor,
/// улучшает декоративного дракона и добавляет звуковую атмосферу орочьей зоны.
///
/// === ЗОНЫ ОТВЕТСТВЕННОСТИ (Шаг 2 LEVEL0_VISUAL_PLAN) ===
/// L0Layout: дорога (Z=-34..-132), деревня (Z≈-45), орочья арена (Z≈-142), выход (Z≈-188).
/// WorldDressingBuilder: только замок/ближняя область (Z > -35) и окружение (леса, горы).
/// </summary>
public class L0Layout : MonoBehaviour
{
    [Header("Build")]
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;

    [Header("Sections")]
    public bool createRefugeeVillage = true;
    public bool createRoadBattlefield = true;
    public bool createOrcStronghold = true;
    public bool createExitPass = true;
    public bool createAtmosphere = true;
    public bool createDragon = true;
    public bool createOrcAudio = true;
    public bool createSafetyGround = true;

    [Header("Castle / Player Reference Points")]
    public Vector3 castleGatePoint = new Vector3(0f, 0f, -16.25f);
    public Vector3 playerStartPoint = new Vector3(0f, 0f, -34f);

    [Header("World Layout Points")]
    public Vector3 refugeeVillageCenter = new Vector3(-24f, 0f, -45f);
    public Vector3 roadStart = new Vector3(0f, 0f, -34f);
    public Vector3 roadEnd = new Vector3(0f, 0f, -132f);
    public Vector3 orcStrongholdCenter = new Vector3(10f, 0f, -142f);
    public Vector3 exitPassPoint = new Vector3(0f, 0f, -188f);

    [Header("Density / Scale")]
    [Range(6, 12)] public int villageHouseCount = 9;
    [Range(12, 24)] public int orcTentCount = 20;
    [Range(3, 6)] public int orcTowerCount = 5;
    [Range(18f, 32f)] public float villageRadius = 26f;
    [Range(36f, 62f)] public float orcBaseRadius = 52f;

    [Header("Composition")]
    public bool createLargeOrcGate = true;
    public bool createArenaCore = true;
    public bool createCentralBossArea = true;
    public bool createExtraAtmosphere = true;
    public bool createVillageRoadsideCamp = true;
    public bool createRoadSightLine = true;

    [Header("Dragon Placement")]
    public Vector3 dragonOrbitOffsetFromOrcBase = new Vector3(14f, 0f, -10f);
    [Range(10f, 32f)] public float dragonOrbitRadius = 16f;
    [Range(12f, 36f)] public float dragonCruiseHeight = 20f;
    [Range(7f, 24f)] public float dragonAttackHeight = 11f;
    [Range(0.8f, 2.0f)] public float dragonVisualScale = 1.45f;
    public bool dragonBreathesFire = true;
    public bool dragonUseParticles = true;

    [Header("Support Ground")]
    [Range(-6f, -0.1f)] public float safetyGroundY = -0.6f;

    [Header("Repeatable Generation")]
    public bool useFixedSeed = true;
    public int randomSeed = 44017;

    private const string RootName = "GEN_L0_LAYOUT";
    private GameObject layoutRoot;

    private void Start()
    {
        if (buildOnStart) BuildLayout();
    }

    [ContextMenu("Build Level0 World Layout")]
    public void BuildLayout()
    {
        // A6 safety: generated Level0 layout must always start from a clean state.
        // Unity scene serialization can keep clearBeforeBuild=false even when the source default is true,
        // so do not depend on the inspector toggle for this generated world.
        ClearLayout();

        Random.State previousState = Random.state;
        if (useFixedSeed) Random.InitState(randomSeed);

        layoutRoot = new GameObject(RootName);
        layoutRoot.transform.SetParent(transform, false);

        if (createSafetyGround)
        {
            GameObject safety = CreateSection("SafetyGround");
            Color grassGreen = new Color(0.28f, 0.52f, 0.18f);
            Color darkGrass = new Color(0.18f, 0.38f, 0.12f);
            L0Props.CreateSupportFloor(new Vector3(0f, safetyGroundY, -112f), new Vector3(190f, 1.0f, 250f), safety.transform, "WorldSafetyFloor", grassGreen, true);
            L0Props.CreateSupportFloor(orcStrongholdCenter + new Vector3(0f, 0f, -2f) + Vector3.up * safetyGroundY, new Vector3(orcBaseRadius * 2.45f, 1.0f, orcBaseRadius * 2.15f), safety.transform, "OrcBaseSafetyFloor", darkGrass, true);
            L0Props.CreateSupportFloor(exitPassPoint + Vector3.up * safetyGroundY, new Vector3(34f, 1.0f, 28f), safety.transform, "ExitSafetyFloor", darkGrass, true);
            // Зелёный пол за замком — закрывает серую сцену (Z>15)
            L0Props.CreateSupportFloor(new Vector3(0f, 0.02f, 65f), new Vector3(220f, 0.5f, 100f), safety.transform, "BehindCastleGreenFloor", grassGreen, true);
            // Широкая зелёная поляна вокруг замка — поверх terrain, не заходит на арену (Z > -115)
            L0Props.CreateSupportFloor(new Vector3(0f, 0.02f, -45f), new Vector3(380f, 0.5f, 140f), safety.transform, "CastleVicinityGreenFloor", grassGreen, true);
        }

        if (createRefugeeVillage)
        {
            GameObject village = CreateSection("Village");
            L0Village.Build(village, refugeeVillageCenter, villageHouseCount, villageRadius, createAtmosphere, createExtraAtmosphere, createVillageRoadsideCamp);
        }

        if (createRoadBattlefield)
        {
            GameObject road = CreateSection("Road");
            L0Road.Build(road, castleGatePoint, roadStart, roadEnd, orcStrongholdCenter, orcBaseRadius, createAtmosphere, createExtraAtmosphere, createRoadSightLine);
        }

        if (createOrcStronghold)
        {
            GameObject stronghold = CreateSection("OrcBase");
            L0OrcBase.Build(stronghold, roadEnd, orcStrongholdCenter, orcTentCount, orcTowerCount, orcBaseRadius, createLargeOrcGate, createArenaCore, createCentralBossArea, createAtmosphere, createExtraAtmosphere, createOrcAudio);
        }

        // Dragon removed (Step 0) — createDragon flag left for inspector but build is skipped.

        if (createExitPass)
        {
            GameObject exitPass = CreateSection("Exit");
            L0Exit.Build(exitPass, orcStrongholdCenter, exitPassPoint, createAtmosphere, createExtraAtmosphere);
        }

        SetupWorldLighting();

        if (useFixedSeed) Random.state = previousState;
        Debug.Log("[L0Layout] Level 0 visual world layout v8 built.");
    }

    [ContextMenu("Clear Level0 World Layout")]
    public void ClearLayout()
    {
        GameObject[] sceneRoots = gameObject.scene.GetRootGameObjects();
        for (int rootIndex = 0; rootIndex < sceneRoots.Length; rootIndex++)
        {
            Transform[] generatedCandidates = sceneRoots[rootIndex].GetComponentsInChildren<Transform>(true);
            for (int i = generatedCandidates.Length - 1; i >= 0; i--)
            {
                Transform candidate = generatedCandidates[i];
                if (candidate == null || candidate == transform || !IsKnownGeneratedRootName(candidate.name))
                    continue;

                GameObject generatedRoot = candidate.gameObject;
                if (Application.isPlaying) Destroy(generatedRoot);
                else DestroyImmediate(generatedRoot);
            }
        }

        layoutRoot = null;
    }

    private static bool IsKnownGeneratedRootName(string objectName)
    {
        return MatchesGeneratedRootName(objectName, RootName)
            || MatchesGeneratedRootName(objectName, L0OrcArenaConfig.RootName);
    }

    private static bool MatchesGeneratedRootName(string objectName, string knownName)
    {
        return objectName == knownName
            || objectName.StartsWith(knownName + " (")
            || objectName.StartsWith(knownName + "_DUPLICATE");
    }

    private static void SetupWorldLighting()
    {
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                // Яркое тёплое солнце (почти белое, чтобы зелень оставалась зелёной)
                l.color = new Color(1f, 0.96f, 0.84f);
                l.intensity = 1.5f;
                l.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
                l.shadows = LightShadows.Soft;
                break;
            }
        }

        // Светлый насыщенный ambient — тени не уходят в серую муть
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.68f, 0.85f);
        RenderSettings.ambientEquatorColor = new Color(0.52f, 0.58f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.30f, 0.36f, 0.20f);
        RenderSettings.ambientIntensity = 1.15f;

        // Лёгкий ЛИНЕЙНЫЙ туман только на горизонте: ближние объекты остаются чёткими и цветными
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.72f, 0.80f, 0.85f);
        RenderSettings.fogStartDistance = 90f;
        RenderSettings.fogEndDistance = 340f;
    }

    private GameObject CreateSection(string sectionName)
    {
        GameObject section = new GameObject(sectionName);
        section.transform.SetParent(layoutRoot.transform, false);
        return section;
    }
}
