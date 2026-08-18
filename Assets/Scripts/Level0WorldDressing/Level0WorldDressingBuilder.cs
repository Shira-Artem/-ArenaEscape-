using UnityEngine;

/// <summary>
/// Билдер визуального наполнения замковой/ближней зоны Level0.
///
/// === ЗОНЫ ОТВЕТСТВЕННОСТИ (Шаг 2 LEVEL0_VISUAL_PLAN) ===
/// WorldDressingBuilder: замок и ближняя область (Z > -35), окружение (леса, горы — далеко от дороги).
/// L0Layout: дорога (Z=-34..-132), деревня беженцев (Z≈-45), орочья арена (Z≈-142), портал, выход.
///
/// ВСЕ МОДУЛИ ОТКЛЮЧЕНЫ по умолчанию:
/// - RuinedVillage / OrcSiegeCamp / RoadMarkers — зоны (Z=-34..-92) перекрываются с L0Layout.
/// - Environment (EnvironmentAtmosphere) — горы и валуны перекрывают игровую территорию.
/// Включайте только если L0Layout выключен.
/// </summary>
public class Level0WorldDressingBuilder : MonoBehaviour
{
    [Header("Общие настройки")]
    public bool buildOnStart = true;
    public bool clearBeforeBuild = true;
    public bool useFixedSeed = true;
    public int randomSeed = 12345;

    [Header("Включаемые части (Village/Camp/Road ОТКЛЮЧЕНЫ — конфликт с L0Layout)")]
    public bool createRuinedVillage = false;
    public bool createOrcSiegeCamp = false;
    public bool createRoadMarkers = false;
    public bool createAtmosphere = false;

    [Header("Опорные точки (можно менять в инспекторе)")]
    public Vector3 castleGatePoint = new Vector3(0f, 0f, -16.25f);
    public Vector3 playerRoadPoint = new Vector3(0f, 0f, -34f);
    public Vector3 orcSiegeCampCenter = new Vector3(14f, 0f, -54f);
    public Vector3 ruinedVillageCenter = new Vector3(-16f, 0f, -68f);
    public Vector3 nextLevelRoadEnd = new Vector3(0f, 0f, -92f);

    [Header("Количество объектов")]
    public int villageHouseCount = 8;
    public int orcTentCount = 5;

    [Header("Окружение (ОТКЛЮЧЕНО — горы/валуны перекрывают игровую зону L0Layout)")]
    public bool createEnvironment = false;

    private GameObject dressingRoot;

    private void Start()
    {
        if (buildOnStart) BuildDressing();
    }

    [ContextMenu("Build Level0 World Dressing")]
    public void BuildDressing()
    {
        if (clearBeforeBuild) ClearDressing();
        if (useFixedSeed) Random.InitState(randomSeed);

        villageHouseCount = Mathf.Clamp(villageHouseCount, 1, 8);
        orcTentCount = Mathf.Clamp(orcTentCount, 1, 5);

        dressingRoot = new GameObject("GENERATED_LEVEL0_WORLD_DRESSING");
        dressingRoot.transform.SetParent(transform, false);
        dressingRoot.transform.localPosition = Vector3.zero;
        dressingRoot.transform.localRotation = Quaternion.identity;
        dressingRoot.transform.localScale = Vector3.one;

        // Создаём дочерние контейнеры для каждой части
        if (createRuinedVillage)
        {
            GameObject villageContainer = new GameObject("RuinedVillage");
            villageContainer.transform.SetParent(dressingRoot.transform, false);
            Level0RuinedVillageDressing.Build(villageContainer, ruinedVillageCenter, villageHouseCount);
        }

        if (createOrcSiegeCamp)
        {
            GameObject campContainer = new GameObject("OrcSiegeCamp");
            campContainer.transform.SetParent(dressingRoot.transform, false);
            Level0OrcSiegeCampDressing.Build(campContainer, orcSiegeCampCenter, orcTentCount);
        }

        if (createRoadMarkers || createAtmosphere)
        {
            GameObject roadContainer = new GameObject("RoadAndMarkers");
            roadContainer.transform.SetParent(dressingRoot.transform, false);
            Level0RoadAndRouteDressing.Build(roadContainer, castleGatePoint, playerRoadPoint, nextLevelRoadEnd, orcSiegeCampCenter, ruinedVillageCenter);
        }

        if (createEnvironment)
        {
            GameObject envContainer = new GameObject("Environment");
            envContainer.transform.SetParent(dressingRoot.transform, false);
            Level0EnvironmentAtmosphere.Build(envContainer, castleGatePoint, nextLevelRoadEnd,
                ruinedVillageCenter, orcSiegeCampCenter);
        }

        Debug.Log("[Level0WorldDressingBuilder] Визуальное наполнение 0 уровня построено.");
    }

    [ContextMenu("Clear Level0 World Dressing")]
    public void ClearDressing()
    {
        if (dressingRoot == null)
        {
            Transform existing = transform.Find("GENERATED_LEVEL0_WORLD_DRESSING");
            if (existing != null) dressingRoot = existing.gameObject;
        }

        if (dressingRoot != null)
        {
            if (Application.isPlaying) Destroy(dressingRoot);
            else DestroyImmediate(dressingRoot);
            dressingRoot = null;
        }
    }
}
