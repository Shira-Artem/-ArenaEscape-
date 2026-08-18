using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// Сюжетная битва у ворот для 0 уровня.
/// Не трогает WeaponController и PlayerController.
/// Использует существующий EnemyAI, EnemyHitDetector, GameManager, FeedbackManager.
/// Сценарий:
/// пролог 8/8 -> тревога -> спавн орков -> убийство всех -> ворота можно открыть.
/// </summary>
public class Level0OrcRaidManager : MonoBehaviour
{
    [Header("Start Conditions")]
    public bool startRaidWhenIntroCompleted = true;
    public float raidStartDelay = 1.0f;
    public bool buildNavMeshBeforeRaid = false;

    [Header("Raid Settings")]
    public int totalOrcs = 18;
    public int wave1Orcs = 5;
    public int wave2Orcs = 7;
    public int wave3Orcs = 7;
    public float wave2Delay = 2.5f;
    public float wave3Delay = 2.5f;
    public bool includeArchers = true;
    public bool includeBerserker = true;
    public float spawnInterval = 0.30f;
    public int maxAliveAtOnce = 9;
    public int killsBeforeGateOpens = 3;
    public int allyCount = 6;
    public bool openGateMidRaid = true;
    public bool spawnAlliesWhenGateOpens = true;

    [Header("Spawn Area")]
    public bool spawnRaidFromDistance = true;
    public float raidSpawnDistanceFromGate = 35f;
    public float raidSpawnSpread = 8f;
    public Vector3[] spawnPoints = new Vector3[]
    {
        new Vector3(-7f, 0.2f, -23f),
        new Vector3( 7f, 0.2f, -23f),
        new Vector3(-9f, 0.2f, -28f),
        new Vector3( 9f, 0.2f, -28f),
        new Vector3(-4f, 0.2f, -20f),
        new Vector3( 4f, 0.2f, -20f),
        new Vector3(-11f, 0.2f, -31f),
        new Vector3( 11f, 0.2f, -31f)
    };

    [Header("Waypoint Center")]
    public Vector3 gateDefensePoint = new Vector3(0f, 0.2f, -25f);
    public float waypointSpread = 5f;

    [Header("Visuals")]
    public bool createWarningMarkers = true;
    public bool showDebugSpawnMarkers = false;
    public bool showRaidHud = false;

    private readonly List<EnemyAI> spawnedOrcs = new List<EnemyAI>();
    private Transform raidRoot;
    private bool startingRaid;
    private bool raidActive;
    private bool raidCleared;
    private bool raidSpawnStarted;
    private bool raidSpawningComplete;
    private bool gateOpenedMidRaid;
    private int lastKilledCount = -1;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle dangerStyle;
    private static readonly Vector3 ApproximateOrcBasePosition = new Vector3(8f, 0.2f, -135f);

    private struct RaidSpawnPoint
    {
        public Vector3 position;
        public bool navMeshSampleSucceeded;
        public bool usedFallback;
    }

    private void Start()
    {
        GameProgressManager.Ensure();
        // Лаба №7(1): удержание ворот + вышка для лука.
        L0GateDefense.Ensure();
        L0GateTower.Ensure();
    }

    private void Update()
    {
        GameProgressManager progress = GameProgressManager.Instance;

        if (progress == null)
            return;

        bool introCompletedAndRaidNotCleared = startRaidWhenIntroCompleted
            && progress.IsCastleIntroCompleted()
            && !progress.castleGateRaidCleared;

        bool progressRaidStartedButNoSpawn = progress.castleGateRaidStarted
            && !progress.castleGateRaidCleared
            && !raidSpawnStarted;

        if (((introCompletedAndRaidNotCleared && !raidSpawnStarted) || progressRaidStartedButNoSpawn)
            && !startingRaid
            && !raidActive
            && !raidCleared)
        {
            StartCoroutine(StartRaidRoutine());
        }

        if (raidActive)
            UpdateRaidState();
    }

    [ContextMenu("Force Start Raid")]
    public void ForceStartRaid()
    {
        if (!startingRaid && !raidActive && !raidCleared)
            StartCoroutine(StartRaidRoutine());
    }

    private IEnumerator StartRaidRoutine()
    {
        startingRaid = true;
        raidSpawnStarted = true;
        raidCleared = false;
        raidSpawningComplete = false;
        spawnRaidFromDistance = true;
        buildNavMeshBeforeRaid = false;
        maxAliveAtOnce = Mathf.Max(4, maxAliveAtOnce);
        raidSpawnDistanceFromGate = 35f;
        raidSpawnSpread = 8f;
        wave1Orcs = Mathf.Max(1, wave1Orcs);
        wave2Orcs = Mathf.Max(1, wave2Orcs);
        wave3Orcs = Mathf.Max(0, wave3Orcs);
        totalOrcs = wave1Orcs + wave2Orcs + wave3Orcs;
        wave2Delay = Mathf.Max(0f, wave2Delay);
        wave3Delay = Mathf.Max(0f, wave3Delay);
        spawnInterval = Mathf.Max(0.05f, spawnInterval);

        GameProgressManager progress = GameProgressManager.Ensure();
        if (!progress.castleGateRaidStarted)
            progress.MarkOrcRaidStarted();

        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), transform);

        GameManager.Instance?.ShowMessage("Тревога! Орки прорвались к главным воротам!", GameManager.MsgType.Damage, 4f);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage("Орки у ворот! Защити замок перед выходом.", 5f);

        yield return new WaitForSeconds(raidStartDelay);

        BuildRaidRoot();

        if (showDebugSpawnMarkers && createWarningMarkers)
            BuildWarningMarkers();

        if (buildNavMeshBeforeRaid)
            yield return BakeRaidNavMesh();

        // Союзники спавнятся сразу — чтобы GateDefense (шкала удержания) была активна с 1-й волны.
        L0GateAllies allies = L0GateAllies.Ensure();
        allies.allyCount = allyCount;
        allies.SpawnAllies();

        raidActive = true;
        startingRaid = false;

        GameManager.Instance?.ShowMessage("Держи ворота! Стражники встали в строй.", GameManager.MsgType.Damage, 4f);
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), transform);

        yield return SpawnRaidOrcsRoutine();
    }

    private void BuildRaidRoot()
    {
        if (raidRoot != null)
            Destroy(raidRoot.gameObject);

        GameObject root = new GameObject("LEVEL0_ORC_RAID");
        raidRoot = root.transform;
    }

    private void BuildWarningMarkers()
    {
        int markerCount = Mathf.Max(4, totalOrcs);
        for (int i = 0; i < markerCount; i++)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "RaidSpawn_DebugMarker";
            marker.transform.SetParent(raidRoot);
            marker.transform.position = GetRaidSpawnPosition(i) + Vector3.up * 0.04f;
            marker.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);

            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = new Color(0.8f, 0.05f, 0.02f, 0.45f);
            }

            Collider col = marker.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
        }
    }

    private IEnumerator SpawnRaidOrcsRoutine()
    {
        spawnedOrcs.Clear();
        raidSpawningComplete = false;
        gateOpenedMidRaid = false;

        int firstWaveCount = Mathf.Max(1, wave1Orcs);
        int secondWaveCount = Mathf.Max(1, wave2Orcs);
        int thirdWaveCount = Mathf.Max(0, wave3Orcs);
        totalOrcs = firstWaveCount + secondWaveCount + thirdWaveCount;

        yield return SpawnRaidWaveRoutine(1, firstWaveCount, 0);

        while (GetKilledCount() < firstWaveCount)
            yield return null;

        OpenGateAfterWaveOne();

        if (wave2Delay > 0f)
            yield return new WaitForSeconds(wave2Delay);

        yield return SpawnRaidWaveRoutine(2, secondWaveCount, firstWaveCount);

        // 3-я волна — финальный натиск (лаба №7(1): пик давления на ворота).
        if (thirdWaveCount > 0)
        {
            if (wave3Delay > 0f)
                yield return new WaitForSeconds(wave3Delay);

            yield return SpawnRaidWaveRoutine(3, thirdWaveCount, firstWaveCount + secondWaveCount);
        }

        raidSpawningComplete = true;
    }

    private IEnumerator SpawnRaidWaveRoutine(int waveNumber, int count, int spawnIndexOffset)
    {
        for (int i = 0; i < count; i++)
        {
            while (CountAliveOrcs() >= Mathf.Max(1, maxAliveAtOnce))
                yield return new WaitForSeconds(spawnInterval);

            int spawnIndex = spawnIndexOffset + i;
            RaidSpawnPoint spawnPoint = GetRaidSpawnPoint(spawnIndex);
            Vector3 pos = spawnPoint.position;

            if (showDebugSpawnMarkers)
                CreateRaidSpawnDebugMarker(pos);

            Vector3[] wps = MakeWaypointsForOrc(spawnIndex);
            GameObject orc = CreateRaidWaveOrc(waveNumber, i, count, pos, wps);

            if (orc != null)
            {
                orc.transform.SetParent(raidRoot);
                EnemyAI ai = orc.GetComponent<EnemyAI>();
                NavMeshAgent nav = orc.GetComponent<NavMeshAgent>();
                L0RaidOrcRunner runner = orc.GetComponent<L0RaidOrcRunner>();
                if (runner == null)
                    runner = orc.AddComponent<L0RaidOrcRunner>();

                if (runner != null)
                {
                    runner.gateTarget = gateDefensePoint;
                    runner.moveSpeed = 3.2f;
                    runner.stopDistance = 6f;
                }

                bool agentIsOnNavMesh = nav != null && nav.enabled && nav.isOnNavMesh;
                bool runnerAdded = runner != null;

                Debug.Log("[Level0OrcRaidManager] Spawned " + orc.name
                    + " at " + pos
                    + ", navMeshSampleSucceeded=" + spawnPoint.navMeshSampleSucceeded
                    + ", usedFallback=" + spawnPoint.usedFallback
                    + ", hasNavMeshAgent=" + (nav != null)
                    + ", agent.isOnNavMesh=" + agentIsOnNavMesh
                    + ", hasEnemyAI=" + (ai != null)
                    + ", hasL0RaidOrcRunner=" + runnerAdded);

                if (!spawnPoint.navMeshSampleSucceeded)
                    Debug.Log("[Level0OrcRaidManager] NavMesh sample failed, using fallback road point");

                if (nav == null)
                    Debug.LogWarning("[Level0OrcRaidManager] Spawned orc has no NavMeshAgent: " + orc.name);

                if (ai == null)
                    Debug.LogWarning("[Level0OrcRaidManager] Spawned orc has no EnemyAI: " + orc.name);

                if (ai != null)
                {
                    spawnedOrcs.Add(ai);
                    ai.OnNavMeshReady();
                }
            }
            else
            {
                Debug.LogWarning("[Level0OrcRaidManager] Orc creation returned null at " + pos
                    + ", navMeshSampleSucceeded=" + spawnPoint.navMeshSampleSucceeded
                    + ", usedFallback=" + spawnPoint.usedFallback);
            }

            if (i < count - 1)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private GameObject CreateRaidWaveOrc(int waveNumber, int index, int waveCount, Vector3 pos, Vector3[] wps)
    {
        string prefix = "Level0_Raid_W" + waveNumber + "_";

        if (includeBerserker && waveNumber >= 2 && index == waveCount - 1 && waveCount >= 4)
            return CreateOrcBerserker(prefix + "Berserker_" + index, pos, wps);

        if (includeArchers && waveNumber >= 2 && (index == 1 || index == 3))
            return CreateOrcArcher(prefix + "Archer_" + index, pos, wps);

        return CreateOrcWarrior(prefix + "Orc_" + index, pos, wps);
    }

    private Vector3[] MakeWaypointsForOrc(int index)
    {
        float side = index % 2 == 0 ? -1f : 1f;
        float back = (index % 3) * 1.6f;

        return new Vector3[]
        {
            gateDefensePoint + new Vector3(side * waypointSpread, 0f, -back),
            gateDefensePoint + new Vector3(-side * waypointSpread * 0.5f, 0f, -2f - back),
            gateDefensePoint + new Vector3(0f, 0f, -1f - back)
        };
    }

    private Vector3 GetRaidSpawnPosition(int index)
    {
        if (!spawnRaidFromDistance)
        {
            Vector3 nearGate = spawnPoints != null && spawnPoints.Length > 0
                ? spawnPoints[index % spawnPoints.Length]
                : gateDefensePoint + new Vector3(Random.Range(-6f, 6f), 0.2f, Random.Range(-3f, 3f));

            return SampleSpawnPositionOrFallback(nearGate, index).position;
        }

        return GetRaidSpawnPoint(index).position;
    }

    private RaidSpawnPoint GetRaidSpawnPoint(int index)
    {
        if (!spawnRaidFromDistance)
        {
            Vector3 nearGate = spawnPoints != null && spawnPoints.Length > 0
                ? spawnPoints[index % spawnPoints.Length]
                : gateDefensePoint + new Vector3(0f, 0f, -30f);

            return SampleSpawnPositionOrFallback(nearGate, index);
        }

        return GetRoadSpawnPoint(index);
    }

    private RaidSpawnPoint GetRoadSpawnPoint(int index)
    {
        Vector3 directionToOrcBase = ApproximateOrcBasePosition - gateDefensePoint;
        directionToOrcBase.y = 0f;

        if (directionToOrcBase.sqrMagnitude < 0.01f)
            directionToOrcBase = Vector3.back;

        directionToOrcBase.Normalize();

        Vector3 side = Vector3.Cross(Vector3.up, directionToOrcBase).normalized;
        float spawnDistance = Mathf.Clamp(raidSpawnDistanceFromGate, 30f, 45f);
        Vector3 center = gateDefensePoint + directionToOrcBase * spawnDistance;
        float angle = index * Mathf.PI * 2f / Mathf.Max(4, totalOrcs);
        Vector3 offset = side * (Mathf.Sin(angle) * raidSpawnSpread)
            + directionToOrcBase * (Mathf.Cos(angle) * raidSpawnSpread * 0.35f);

        Vector3 pos = center + offset;
        pos.y = gateDefensePoint.y;
        return SampleSpawnPositionOrFallback(pos, index);
    }

    private RaidSpawnPoint SampleSpawnPositionOrFallback(Vector3 pos, int index)
    {
        bool usedFallback = false;
        if (!IsValidPosition(pos))
        {
            pos = GetFallbackRoadPosition(index);
            usedFallback = true;
        }

        NavMeshHit hit;
        bool navMeshSampleSucceeded = NavMesh.SamplePosition(pos, out hit, 12f, NavMesh.AllAreas);
        if (navMeshSampleSucceeded)
            pos = hit.position;
        else
        {
            pos = GetFallbackRoadPosition(index);
            usedFallback = true;
        }

        if (!IsValidPosition(pos))
        {
            pos = GetFallbackRoadPosition(index);
            usedFallback = true;
        }

        RaidSpawnPoint result = new RaidSpawnPoint();
        result.position = pos;
        result.navMeshSampleSucceeded = navMeshSampleSucceeded;
        result.usedFallback = usedFallback;
        return result;
    }

    private Vector3 GetFallbackRoadPosition(int index)
    {
        Vector3 directionToOrcBase = ApproximateOrcBasePosition - gateDefensePoint;
        directionToOrcBase.y = 0f;

        if (directionToOrcBase.sqrMagnitude < 0.01f)
            directionToOrcBase = Vector3.back;

        directionToOrcBase.Normalize();

        Vector3 side = Vector3.Cross(Vector3.up, directionToOrcBase).normalized;
        float t = (index % 4) / 3f;
        float distance = Mathf.Lerp(30f, 45f, t);
        float sideOffset = ((index % 3) - 1) * (raidSpawnSpread * 0.45f);
        Vector3 pos = gateDefensePoint + directionToOrcBase * distance + side * sideOffset;
        pos.y = gateDefensePoint.y;
        return pos;
    }

    private void CreateRaidSpawnDebugMarker(Vector3 position)
    {
        if (raidRoot == null)
            return;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "RaidSpawn_DebugMarker";
        marker.transform.SetParent(raidRoot);
        marker.transform.position = position + Vector3.up * 0.08f;
        marker.transform.localScale = new Vector3(0.7f, 0.08f, 0.7f);

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0f, 0f, 0.85f);
        }

        Collider col = marker.GetComponent<Collider>();
        if (col != null)
            Destroy(col);
    }

    private bool IsValidPosition(Vector3 pos)
    {
        return !float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z)
            && !float.IsInfinity(pos.x) && !float.IsInfinity(pos.y) && !float.IsInfinity(pos.z);
    }

    private IEnumerator BakeRaidNavMesh()
    {
        yield return null;

        GameObject old = GameObject.Find("Runtime_Level0_Raid_NavMeshSurface");

        if (old != null)
            Destroy(old);

        GameObject surfGO = new GameObject("Runtime_Level0_Raid_NavMeshSurface");
        surfGO.transform.SetParent(raidRoot);

        NavMeshSurface surf = surfGO.AddComponent<NavMeshSurface>();
        surf.collectObjects = CollectObjects.All;
        surf.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surf.BuildNavMesh();

        yield return new WaitForSeconds(0.2f);
    }

    private void UpdateRaidState()
    {
        int killed = GetKilledCount();

        if (killed != lastKilledCount)
        {
            lastKilledCount = killed;

            if (killed > 0)
                GameManager.Instance?.ShowMessage("Орки у ворот: " + killed + "/" + spawnedOrcs.Count, GameManager.MsgType.Kill, 1.2f);
        }

        if (raidSpawningComplete && spawnedOrcs.Count > 0 && killed >= spawnedOrcs.Count)
        {
            ClearRaid();
        }
    }

    private void OpenGateAfterWaveOne()
    {
        if (!openGateMidRaid || gateOpenedMidRaid)
            return;

        gateOpenedMidRaid = true;

        GameProgressManager progress = GameProgressManager.Ensure();
        CastleIntroGate gate = FindFirstObjectByType<CastleIntroGate>();

        if (gate != null)
            gate.OpenGateFromRaid();
        else
            progress.MarkCastleGateOpened();

        Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
        if (flow != null)
            flow.MarkCastleGateOpenedMidRaid();

        // Союзники уже заспавнены в StartRaidRoutine — повторный вызов ничего не сделает (guards spawned=true).

        GameManager.Instance?.ShowMessage("Ворота открываются! Держите строй!", GameManager.MsgType.Kill, 4f);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage("Ворота открываются! Подкрепление идёт!", 4f);

        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), gate != null ? gate.transform : transform);
    }

    private int CountAliveOrcs()
    {
        int alive = 0;

        for (int i = 0; i < spawnedOrcs.Count; i++)
        {
            EnemyAI ai = spawnedOrcs[i];
            if (ai != null && ai.state != EnemyAI.State.Dead)
                alive++;
        }

        return alive;
    }

    private int GetKilledCount()
    {
        int killed = 0;

        for (int i = 0; i < spawnedOrcs.Count; i++)
        {
            EnemyAI ai = spawnedOrcs[i];

            if (ai == null || ai.state == EnemyAI.State.Dead)
                killed++;
        }

        return killed;
    }

    private void ClearRaid()
    {
        if (raidCleared)
            return;

        if (spawnedOrcs.Count == 0)
            return;

        raidCleared = true;
        raidActive = false;

        GameProgressManager progress = GameProgressManager.Ensure();
        Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
        if (flow != null)
            flow.MarkGateRaidCleared();
        else
            progress.MarkOrcRaidCleared();

        GameManager.Instance?.ShowMessage("Мы удержим замок. Иди к логову орков.", GameManager.MsgType.Kill, 5f);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage("Орки разбиты. Союзники удержат ворота. Иди к логову орков.", 6f);

        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        Transform target = village != null ? village.GetObjectiveTarget() : transform;
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), target);
    }

    private void InitGuiStyles()
    {
        if (panelStyle != null)
            return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.padding = new RectOffset(10, 10, 8, 8);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(1f, 0.2f, 0.08f);

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 14;
        textStyle.normal.textColor = Color.white;

        dangerStyle = new GUIStyle(GUI.skin.label);
        dangerStyle.fontSize = 14;
        dangerStyle.fontStyle = FontStyle.Bold;
        dangerStyle.normal.textColor = new Color(1f, 0.7f, 0.12f);
    }

    private void OnGUI()
    {
        if (!showRaidHud)
            return;

        if (!raidActive && !startingRaid)
            return;

        InitGuiStyles();

        int killed = GetKilledCount();
        int total = Mathf.Max(1, spawnedOrcs.Count);

        GUILayout.BeginArea(new Rect(Screen.width - 285f, 18f, 265f, 95f), panelStyle);
        GUILayout.Label("БИТВА У ВОРОТ", titleStyle);

        if (startingRaid)
            GUILayout.Label("Орки приближаются...", dangerStyle);
        else
            GUILayout.Label("Орков убито: " + killed + "/" + total, textStyle);

        GUILayout.Label("Не дай им прорваться к замку.", textStyle);
        GUILayout.EndArea();
    }

    // ---------------------------------------------------------------------
    // ORC FACTORY — маленькая независимая версия под Level0.
    // Боёвку даёт существующий EnemyAI.
    // ---------------------------------------------------------------------

    private GameObject CreateOrcWarrior(string name, Vector3 pos, Vector3[] wps)
    {
        GameObject root = MakeOrcRoot(name, pos, wps, 3.8f, 18, 95, 20f, 2.2f);
        root.transform.localScale = Vector3.one * 1.5f;

        // R1: тёмная болотно-серая кожа (читается на зелёной траве) + красная боевая раскраска.
        Color skin = new Color(0.22f, 0.27f, 0.19f);
        Color armor = new Color(0.14f, 0.14f, 0.16f);
        Color leather = new Color(0.26f, 0.14f, 0.07f);
        Color warpaint = new Color(0.82f, 0.10f, 0.06f);

        GameObject torso = Prim(PrimitiveType.Cube, root.transform, "Warrior_Torso", new Vector3(0f, 0.72f, 0.08f), new Vector3(0.82f, 0.68f, 0.56f), Quaternion.Euler(5f, 0f, 0f), skin);
        GameObject head = Prim(PrimitiveType.Cube, root.transform, "Warrior_Head", new Vector3(0f, 1.25f, 0.10f), new Vector3(0.58f, 0.52f, 0.54f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Chest_Armor", new Vector3(0f, 0.74f, 0.42f), new Vector3(0.68f, 0.46f, 0.12f), Quaternion.identity, armor);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(0.22f, 0.10f, 0f), new Vector3(0.18f, 0.36f, 0.18f), Quaternion.identity, new Color(0.13f, 0.10f, 0.07f));
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-0.22f, 0.10f, 0f), new Vector3(0.18f, 0.36f, 0.18f), Quaternion.identity, new Color(0.13f, 0.10f, 0.07f));
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(0.58f, 0.72f, 0.08f), new Vector3(0.14f, 0.38f, 0.14f), Quaternion.Euler(0f, 0f, -14f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-0.58f, 0.72f, 0.08f), new Vector3(0.14f, 0.38f, 0.14f), Quaternion.Euler(0f, 0f, 14f), skin);
        Prim(PrimitiveType.Cube, root.transform, "WarPaint_Face", new Vector3(0f, 1.27f, 0.37f), new Vector3(0.5f, 0.10f, 0.06f), Quaternion.Euler(5f, 0f, 0f), warpaint);
        Prim(PrimitiveType.Cube, root.transform, "Shoulder_R", new Vector3(-0.58f, 0.98f, 0.08f), new Vector3(0.30f, 0.16f, 0.34f), Quaternion.identity, warpaint);
        AddTusks(root.transform, 1.07f, 0.38f, 0.06f);
        AddAxe(root.transform, new Vector3(-0.78f, 0.62f, 0.26f), 0.9f, leather, armor);
        SetBodyParts(root, torso, head);

        return root;
    }

    private GameObject CreateOrcArcher(string name, Vector3 pos, Vector3[] wps)
    {
        GameObject root = MakeOrcRoot(name, pos, wps, 3.6f, 12, 70, 24f, 2.2f);
        root.transform.localScale = Vector3.one * 1.3f;

        // R1: тёмная кожа для контраста с травой.
        Color skin = new Color(0.22f, 0.27f, 0.19f);
        Color leather = new Color(0.30f, 0.16f, 0.07f);
        Color glow = new Color(1f, 0.70f, 0.12f);

        GameObject torso = Prim(PrimitiveType.Cube, root.transform, "Archer_Torso", new Vector3(0f, 0.65f, 0.08f), new Vector3(0.70f, 0.56f, 0.48f), Quaternion.Euler(5f, 0f, 0f), skin);
        GameObject head = Prim(PrimitiveType.Cube, root.transform, "Archer_Head", new Vector3(0f, 1.12f, 0.08f), new Vector3(0.50f, 0.46f, 0.48f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Cube, root.transform, "Quiver", new Vector3(0.34f, 0.82f, -0.28f), new Vector3(0.18f, 0.65f, 0.14f), Quaternion.Euler(-18f, 0f, 20f), leather);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(0.18f, 0.10f, 0f), new Vector3(0.14f, 0.30f, 0.14f), Quaternion.identity, new Color(0.13f, 0.10f, 0.07f));
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-0.18f, 0.10f, 0f), new Vector3(0.14f, 0.30f, 0.14f), Quaternion.identity, new Color(0.13f, 0.10f, 0.07f));
        AddTusks(root.transform, 0.94f, 0.34f, 0.045f);
        AddBow(root.transform, new Vector3(-0.62f, 0.62f, 0.22f), leather);
        SetBodyParts(root, torso, head);

        EnemyAI ai = root.GetComponent<EnemyAI>();

        if (ai != null)
        {
            ai.useRangedAttack = true;
            ai.rangedDamage = 12;
            ai.rangedRange = 20f;
            ai.rangedCooldown = 1.8f;
            ai.rangedProjectileSpeed = 18f;
            ai.rangedProjectileColor = glow;
            ai.rangedAttackLabel = "ВЫСТРЕЛ!";
            ai.chaseRange = 24f;
            ai.attackRange = 2.2f;
        }

        NavMeshAgent nav = root.GetComponent<NavMeshAgent>();

        if (nav != null)
            nav.stoppingDistance = 0.85f;

        return root;
    }

    private GameObject CreateOrcBerserker(string name, Vector3 pos, Vector3[] wps)
    {
        GameObject root = MakeOrcRoot(name, pos, wps, 4.2f, 24, 120, 20f, 2.2f);
        root.transform.localScale = Vector3.one * 1.9f;

        Color skin = new Color(0.58f, 0.14f, 0.08f);
        Color dark = new Color(0.35f, 0.06f, 0.04f);
        Color iron = new Color(0.22f, 0.22f, 0.24f);

        GameObject torso = Prim(PrimitiveType.Cube, root.transform, "Berserker_Torso", new Vector3(0f, 0.72f, 0.08f), new Vector3(0.96f, 0.74f, 0.64f), Quaternion.Euler(5f, 0f, 0f), skin);
        GameObject head = Prim(PrimitiveType.Cube, root.transform, "Berserker_Head", new Vector3(0f, 1.30f, 0.10f), new Vector3(0.70f, 0.62f, 0.64f), Quaternion.Euler(5f, 0f, 0f), skin);
        Prim(PrimitiveType.Sphere, root.transform, "Berserker_Belly", new Vector3(0f, 0.50f, 0.15f), new Vector3(0.58f, 0.38f, 0.48f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cube, root.transform, "Metal_Brow", new Vector3(0f, 1.53f, 0.11f), new Vector3(0.72f, 0.12f, 0.58f), Quaternion.identity, iron);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_L", new Vector3(0.25f, 0.05f, 0f), new Vector3(0.22f, 0.38f, 0.22f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cylinder, root.transform, "Leg_R", new Vector3(-0.25f, 0.05f, 0f), new Vector3(0.22f, 0.38f, 0.22f), Quaternion.identity, dark);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_L", new Vector3(0.70f, 0.68f, 0.08f), new Vector3(0.18f, 0.40f, 0.18f), Quaternion.Euler(0f, 0f, -16f), skin);
        Prim(PrimitiveType.Cylinder, root.transform, "Arm_R", new Vector3(-0.70f, 0.68f, 0.08f), new Vector3(0.18f, 0.40f, 0.18f), Quaternion.Euler(0f, 0f, 16f), skin);
        AddTusks(root.transform, 1.06f, 0.42f, 0.09f);
        AddAxe(root.transform, new Vector3(-0.95f, 0.55f, 0.30f), 1.2f, dark, iron);
        SetBodyParts(root, torso, head);

        return root;
    }

    private GameObject MakeOrcRoot(string name, Vector3 pos, Vector3[] wps, float speed, int dmg, int hp, float chase, float atk)
    {
        GameObject g = new GameObject(name);
        g.tag = "Enemy";
        g.transform.position = pos;

        CapsuleCollider col = g.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.45f;
        col.center = new Vector3(0f, 0.9f, 0f);

        NavMeshAgent nav = g.AddComponent<NavMeshAgent>();
        nav.speed = speed;
        nav.angularSpeed = 300f;
        nav.acceleration = 18f;
        nav.stoppingDistance = 0.45f;
        nav.height = 1.8f;
        nav.radius = 0.4f;
        nav.avoidancePriority = Random.Range(30, 70);
        nav.autoRepath = true;

        EnemyAI ai = g.AddComponent<EnemyAI>();
        ai.damage = dmg;
        ai.maxHp = hp;
        ai.chaseRange = chase;
        ai.attackRange = atk;

        g.AddComponent<EnemyHitDetector>();

        if (wps != null && wps.Length > 0)
        {
            ai.waypoints = new Transform[wps.Length];

            for (int i = 0; i < wps.Length; i++)
            {
                GameObject wp = new GameObject(name + "_WP_" + i);
                wp.transform.position = wps[i];
                wp.transform.SetParent(raidRoot);
                ai.waypoints[i] = wp.transform;
            }
        }
        else
        {
            ai.waypoints = new Transform[0];
        }

        return g;
    }

    private GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos, Vector3 scale, Quaternion rot, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = rot;
        obj.transform.localScale = scale;

        Collider col = obj.GetComponent<Collider>();

        if (col != null)
            Destroy(col);

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        return obj;
    }

    private void SetBodyParts(GameObject root, params GameObject[] parts)
    {
        EnemyAI ai = root.GetComponent<EnemyAI>();

        if (ai != null)
            ai.bodyParts = parts;
    }

    private void AddTusks(Transform root, float y, float z, float size)
    {
        Prim(PrimitiveType.Cube, root, "Tusk_L", new Vector3(0.16f, y, z), new Vector3(size, size * 2.4f, size), Quaternion.Euler(-12f, 0f, 10f), Color.white);
        Prim(PrimitiveType.Cube, root, "Tusk_R", new Vector3(-0.16f, y, z), new Vector3(size, size * 2.4f, size), Quaternion.Euler(-12f, 0f, -10f), Color.white);
    }

    private void AddAxe(Transform root, Vector3 pos, float size, Color wood, Color iron)
    {
        GameObject handle = Prim(PrimitiveType.Cylinder, root, "Axe_Handle", pos, new Vector3(0.04f, 0.55f * size, 0.04f), Quaternion.Euler(0f, 0f, -25f), wood);
        Prim(PrimitiveType.Cube, handle.transform, "Axe_Head", new Vector3(0f, 0.58f, 0f), new Vector3(4f, 0.65f, 3f), Quaternion.identity, iron);
    }

    private void AddBow(Transform root, Vector3 pos, Color wood)
    {
        GameObject bow = Prim(PrimitiveType.Cylinder, root, "Orc_Bow", pos, new Vector3(0.035f, 0.62f, 0.035f), Quaternion.Euler(0f, 0f, 20f), wood);
        Prim(PrimitiveType.Cube, bow.transform, "Bow_String", new Vector3(0.28f, 0f, 0f), new Vector3(0.03f, 1.8f, 0.03f), Quaternion.identity, new Color(0.9f, 0.82f, 0.65f));
    }
}
