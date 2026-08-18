using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat pass for the orc village on Level0.
/// Four zones — gate, center, portal, surge — unlock as the player pushes deeper.
/// Each zone spawns orcs gradually when the player enters its trigger radius.
/// Next zone only opens after the current one is fully cleared.
/// Total: 92 orcs (12 + 20 + 35 + 25), max 14 alive at once.
/// Ranged ratio per zone: Z1 42%, Z2 45%, Z3 46%, Z4 44%.
/// </summary>
public class Level0OrcVillageManager : MonoBehaviour
{
    [Header("Activation")]
    public bool waitForCastleGateOpened = true;
    public Transform player;
    public Transform orcBaseAnchor;
    public float activationRadius = 38f;
    public float roadMessageDistance = 85f;

    [Header("Arena Spawn Points (override)")]
    public Transform[] spawnPointTransforms;

    [Header("Spawn Timing")]
    public float spawnInterval = 0.45f;
    public float zoneEntryDelay = 0.8f;

    [Header("Performance")]
    public int maxAliveOrcs = 14;

    [Header("State")]
    public bool showHud = false;

    private const float GATE_ZONE_RADIUS   = 45f;
    private const float CENTER_ZONE_RADIUS  = 58f;
    private const float PORTAL_ZONE_RADIUS  = 68f;
    private const float SURGE_ZONE_RADIUS   = 60f;

    private readonly List<EnemyAI> allOrcs = new List<EnemyAI>();
    private Transform fightRoot;
    private GameProgressManager progress;

    private bool fightStarted;
    private bool fightCleared;
    private int currentZone;
    private bool zoneSpawningDone;
    private int zoneTotalSpawned;
    private bool roadObjectiveSet;
    private bool roadMessageShown;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;

    private static readonly string[] ZONE_NAMES = { "", "Вход", "Центр", "Портал", "Возмездие" };

    // ════════════════════════ lifecycle ════════════════════════

    private void Start()
    {
        progress = GameProgressManager.Ensure();
        FindPlayerIfNeeded();
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        FindPlayerIfNeeded();
        UpdateRoadStoryState();

        if (!fightStarted && progress.orcVillageFightStarted && !progress.orcVillageCleared)
            StartFight();

        if (!fightStarted && CanStartFight())
            StartFight();

        if (fightStarted && !fightCleared)
            UpdateFightState();
    }

    [ContextMenu("Force Start Orc Village Fight")]
    public void ForceStartFight()
    {
        if (!fightStarted)
            StartFight();
    }

    // ════════════════════════ activation ════════════════════════

    private bool CanStartFight()
    {
        if (progress == null) return false;
        if (progress.orcVillageCleared || progress.orcVillageFightStarted) return false;
        if (waitForCastleGateOpened && !progress.castleGateOpened) return false;
        if (player == null) return false;

        float distToEntrance = Vector3.Distance(player.position, L0OrcArenaConfig.BaseEntrance);
        float distToCenter = Vector3.Distance(player.position, GetBaseCenter());
        return distToEntrance <= activationRadius || distToCenter <= activationRadius;
    }

    private void UpdateRoadStoryState()
    {
        if (progress == null || player == null || fightStarted || fightCleared) return;
        if (progress.orcVillageCleared || progress.orcVillageFightStarted) return;
        if (!progress.castleGateRaidCleared) return;
        if (waitForCastleGateOpened && !progress.castleGateOpened) return;

        if (!roadObjectiveSet)
        {
            roadObjectiveSet = true;
            L0ObjectiveGuide.Ensure().SetObjective("Иди к логову орков", GetObjectiveTarget());
        }

        float distance = Vector3.Distance(player.position, L0OrcArenaConfig.BaseEntrance);
        if (roadMessageShown || distance > roadMessageDistance) return;

        roadMessageShown = true;
        progress.MarkOrcVillageDiscovered();
        ShowBothMessages("Дым, костры и следы крови ведут к логову орков.", 5f);
        L0ObjectiveGuide.Ensure().SetObjective("Войди в логово орков", GetObjectiveTarget());
    }

    // ════════════════════════ fight start ════════════════════════

    private void StartFight()
    {
        fightStarted = true;
        progress.StartOrcVillageFight();
        Debug.Log("[OrcVillage] Fight started!");

        Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
        if (flow != null) flow.StartOrcVillageFight();

        GameObject root = new GameObject("LEVEL0_ORC_VILLAGE_FIGHT");
        fightRoot = root.transform;

        L0StoryAudio.Ensure().PlayHorn();
        ShowBothMessages("Логово орков! Пробейся через деревню к порталу!", 5f);

        StartZone(1);
    }

    // ════════════════════════ zone progression ════════════════════════

    private void UpdateFightState()
    {
        if (currentZone == 0) return;

        CleanDeadFromList();
        int alive = CountAliveOrcs();

        if (alive > 0 && zoneSpawningDone)
        {
            string zone = ZONE_NAMES[currentZone];
            L0ObjectiveGuide.Ensure().SetObjective(
                "Убей орков-защитников (" + zone + ", осталось: " + alive + ")",
                GetZoneObjectiveTarget(currentZone));
        }

        if (!zoneSpawningDone) return;
        if (alive > 0) return;

        if (currentZone < 4)
        {
            int nextZone = currentZone + 1;
            if (IsPlayerInZone(nextZone))
            {
                StartZone(nextZone);
            }
            else
            {
                string hint = nextZone == 2 ? "Продвигайся вглубь деревни!"
                            : nextZone == 3 ? "Иди к порталу!"
                            :                 "К порталу! Он почти открыт!";
                L0ObjectiveGuide.Ensure().SetObjective(hint, GetZoneObjectiveTarget(nextZone));
            }
        }
        else
        {
            CompleteFight();
        }
    }

    private bool IsPlayerInZone(int zone)
    {
        if (player == null) return false;
        Vector3 anchor = GetZoneAnchor(zone);
        float radius = GetZoneTriggerRadius(zone);
        return Vector3.Distance(player.position, anchor) <= radius;
    }

    private Vector3 GetZoneAnchor(int zone)
    {
        switch (zone)
        {
            case 1: return L0OrcArenaConfig.BaseEntrance;
            case 2: return L0OrcArenaConfig.BaseCenter;
            // Portal zone anchor: midpoint between center and shrine
            case 3: return Vector3.Lerp(L0OrcArenaConfig.BaseCenter, L0OrcArenaConfig.PortalShrine, 0.6f);
            // Zone 4: перед порталом — Колосс блокирует выход
            case 4: return L0OrcArenaConfig.PortalShrine + L0OrcArenaConfig.GateDir * 28f;
            default: return L0OrcArenaConfig.BaseCenter;
        }
    }

    private float GetZoneTriggerRadius(int zone)
    {
        switch (zone)
        {
            case 1: return GATE_ZONE_RADIUS;
            case 2: return CENTER_ZONE_RADIUS;
            case 3: return PORTAL_ZONE_RADIUS;
            case 4: return SURGE_ZONE_RADIUS;
            default: return CENTER_ZONE_RADIUS;
        }
    }

    private Transform GetZoneObjectiveTarget(int zone)
    {
        Vector3 anchor = GetZoneAnchor(zone);
        if (fightRoot == null) return transform;

        string name = "ZoneTarget_" + zone;
        Transform existing = fightRoot.Find(name);
        if (existing != null)
        {
            existing.position = anchor;
            return existing;
        }

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(fightRoot);
        obj.transform.position = anchor;
        return obj.transform;
    }

    // ════════════════════════ zone spawning ════════════════════════

    private void StartZone(int zone)
    {
        currentZone = zone;
        zoneSpawningDone = false;
        zoneTotalSpawned = 0;

        if (zone > 1)
        {
            L0StoryAudio.Ensure().PlayHorn();
            string msg = zone == 2 ? "Орки из центра деревни идут на помощь!"
                       : zone == 3 ? "Некромант пробудился! Он призовёт мёртвых!"
                       :             "КОЛОСС И ЕГО СВИТА! Последний рубеж перед порталом!";
            ShowBothMessages(msg, zone >= 3 ? 6f : 4f);
        }

        Debug.Log("[OrcVillage] Starting zone " + zone + " (" + ZONE_NAMES[zone] + ")");
        StartCoroutine(SpawnZoneOrcs(zone));
    }

    private IEnumerator SpawnZoneOrcs(int zone)
    {
        float localSpawnInterval = zone == 4 ? 0.65f
                                 : zone == 3 ? 0.32f
                                 : zone == 2 ? 0.38f
                                 :             spawnInterval;
        int   localMaxAlive      = zone == 4 ? 10 : maxAliveOrcs;
        float localEntryDelay    = zone == 4 ? 2.0f : zoneEntryDelay;

        if (zone > 1)
            yield return new WaitForSeconds(localEntryDelay);

        Vector3[] positions = GetZoneSpawnPositions(zone);
        string[]  types     = GetZoneOrcTypes(zone);
        L0OrcFactory factory = L0OrcFactory.Ensure();

        for (int i = 0; i < positions.Length; i++)
        {
            while (CountAliveOrcs() >= localMaxAlive)
                yield return new WaitForSeconds(0.5f);

            Vector3 pos = positions[i];
            pos.x += Mathf.Sin(i * 2.37f) * 2.5f;
            pos.z += Mathf.Cos(i * 3.14f) * 2.5f;
            Transform[] waypoints = CreateWaypoints(i, pos, zone);
            string label = "L0_Orc_Z" + zone + "_" + i;
            string type = (i < types.Length) ? types[i] : "warrior";

            float extraDelay = 0f;
            GameObject orc = SpawnOrcByType(factory, type, label, pos, waypoints, out extraDelay);

            EnemyAI ai = orc != null ? orc.GetComponent<EnemyAI>() : null;
            if (ai != null)
            {
                allOrcs.Add(ai);
                ai.OnNavMeshReady();
                zoneTotalSpawned++;
            }

            yield return new WaitForSeconds(localSpawnInterval + extraDelay);
        }

        zoneSpawningDone = true;
        Debug.Log("[OrcVillage] Zone " + zone + " fully spawned: " + zoneTotalSpawned + " orcs");
    }

    private GameObject SpawnOrcByType(L0OrcFactory factory, string type, string label, Vector3 pos, Transform[] wp, out float extraDelay)
    {
        extraDelay = 0f;
        switch (type)
        {
            case "colossus":     extraDelay = 1.2f; return factory.CreateColossus(label, pos, fightRoot, wp);
            case "warchief":     extraDelay = 0.8f; return factory.CreateWarchief(label, pos, fightRoot, wp);
            case "necromancer":  return factory.CreateNecromancer(label, pos, fightRoot, wp);
            case "shaman":       return factory.CreateShaman(label, pos, fightRoot, wp);
            case "tank":         return factory.CreateTank(label, pos, fightRoot, wp);
            case "berserker":    return factory.CreateBerserker(label, pos, fightRoot, wp);
            case "boss":         return factory.CreateBoss(label, pos, fightRoot, wp);
            case "poisonArcher": return factory.CreatePoisonArcher(label, pos, fightRoot, wp);
            case "archer":       return factory.CreateArcher(label, pos, fightRoot, wp);
            case "spearman":     return factory.CreateSpearman(label, pos, fightRoot, wp);
            case "wolf":         return factory.CreateWolf(label, pos, fightRoot, wp);
            case "goblin":       return factory.CreateGoblin(label, pos, fightRoot, wp);
            default:             return factory.CreateWarrior(label, pos, fightRoot, wp);
        }
    }

    // ──────────── Zone 1: Gate Ambush — 12 orcs ────────────
    private Vector3[] GetGatePositions()
    {
        Vector3 e = L0OrcArenaConfig.BaseEntrance;
        Vector3 b = L0OrcArenaConfig.BackDir;
        Vector3 s = L0OrcArenaConfig.SideDir;
        return new Vector3[]
        {
            e + b * 6f  + s * 14f,    // wide right flank
            e + b * 6f  - s * 14f,    // wide left flank
            e + b * 12f + s * 8f,     // mid-right
            e + b * 12f - s * 8f,     // mid-left
            e + b * 20f + s * 18f,    // deep right archer
            e + b * 20f - s * 18f,    // deep left archer
            e + b * 16f,              // center warrior
            e + b * 10f + s * 24f,    // far right berserker
            e + b * 10f - s * 24f,    // far left berserker
            e + b * 26f + s * 12f,    // rear right
            e + b * 26f - s * 12f,    // rear left
            e + b * 30f,              // rear center tank
        };
    }

    // ──────────── Zone 2: Central Camp — 20 orcs ────────────
    private Vector3[] GetCenterPositions()
    {
        Vector3 c = L0OrcArenaConfig.BaseCenter;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 b = L0OrcArenaConfig.BackDir;
        Vector3 s = L0OrcArenaConfig.SideDir;
        return new Vector3[]
        {
            c + g * 14f + s * 18f,    // front-right sentry
            c + g * 14f - s * 18f,    // front-left sentry
            c + g * 6f  + s * 10f,    // mid-front right
            c + g * 6f  - s * 10f,    // mid-front left
            c + s * 22f,              // wide right flank
            c - s * 22f,              // wide left flank
            c + b * 10f + s * 14f,    // rear-right
            c + b * 10f - s * 14f,    // rear-left
            c + g * 18f,              // front center
            c + b * 18f,              // rear center
            c + g * 10f + s * 28f,    // far right archer
            c + g * 10f - s * 28f,    // far left archer
            c + b * 6f  + s * 28f,    // rear-far-right archer
            c + b * 6f  - s * 28f,    // rear-far-left archer
            c + s * 8f,               // center-right berserker
            c - s * 8f,               // center-left berserker
            c + g * 20f + s * 10f,    // front-right spearman
            c + g * 20f - s * 10f,    // front-left spearman
            c + b * 16f + s * 6f,     // rear shamans
            c + b * 16f - s * 6f,     // rear shamans
        };
    }

    // ──────────── Zone 3: Portal Guard — 35 orcs ────────────
    private Vector3[] GetPortalPositions()
    {
        Vector3 shrine = L0OrcArenaConfig.PortalShrine;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 s = L0OrcArenaConfig.SideDir;
        return new Vector3[]
        {
            shrine + g * 55f + s * 14f,   // [0]  front-right warrior
            shrine + g * 55f - s * 14f,   // [1]  front-left warrior
            shrine + g * 50f + s * 28f,   // [2]  far-right warrior
            shrine + g * 50f - s * 28f,   // [3]  far-left warrior
            shrine + g * 48f,             // [4]  front-center tank
            shrine + g * 42f + s * 8f,    // [5]  poisonArcher right
            shrine + g * 42f - s * 8f,    // [6]  poisonArcher left
            shrine + g * 44f + s * 22f,   // [7]  poisonArcher far-right
            shrine + g * 44f - s * 22f,   // [8]  poisonArcher far-left
            shrine + g * 36f + s * 16f,   // [9]  spearman right
            shrine + g * 36f - s * 16f,   // [10] spearman left
            shrine + g * 34f + s * 30f,   // [11] spearman far-right
            shrine + g * 34f - s * 30f,   // [12] spearman far-left
            shrine + g * 28f + s * 10f,   // [13] shaman right
            shrine + g * 28f - s * 10f,   // [14] shaman left
            shrine + g * 24f,             // [15] shaman center
            shrine + g * 12f + s * 6f,    // [16] necromancer right
            shrine + g * 12f - s * 6f,    // [17] necromancer left
            shrine + g * 38f + s * 34f,   // [18] berserker far-right flank
            shrine + g * 38f - s * 34f,   // [19] berserker far-left flank
            shrine + g * 30f + s * 24f,   // [20] berserker mid-right
            shrine + g * 30f - s * 24f,   // [21] berserker mid-left
            shrine + g * 20f + s * 14f,   // [22] tank right
            shrine + g * 20f - s * 14f,   // [23] tank left
            shrine + g * 16f + s * 4f,    // [24] boss right
            shrine + g * 16f - s * 4f,    // [25] boss left
            shrine + g * 60f + s * 32f,   // [26] wolf far-right
            shrine + g * 60f - s * 32f,   // [27] wolf far-left
            shrine + g * 46f + s * 38f,   // [28] wolf mid-right
            shrine + g * 52f + s * 36f,   // [29] goblin right
            shrine + g * 52f - s * 36f,   // [30] goblin left
            shrine + g * 40f - s * 38f,   // [31] goblin far-left
            shrine + g * 18f,             // [32] warchief center
            shrine + g * 56f + s * 20f,   // [33] poisonArcher sniper right
            shrine + g * 56f - s * 20f,   // [34] poisonArcher sniper left
        };
    }

    // ──────────── Zone 4: Surge — 25 orcs (Colossus + армия) ────────────
    private Vector3[] GetSurgePositions()
    {
        Vector3 shrine = L0OrcArenaConfig.PortalShrine;
        Vector3 g = L0OrcArenaConfig.GateDir;
        Vector3 s = L0OrcArenaConfig.SideDir;
        return new Vector3[]
        {
            shrine + g * 22f,             // [0]  КОЛОСС
            shrine + g * 36f + s * 16f,   // [1]  berserker right
            shrine + g * 36f - s * 16f,   // [2]  berserker left
            shrine + g * 28f + s * 24f,   // [3]  berserker far-right
            shrine + g * 28f - s * 24f,   // [4]  berserker far-left
            shrine + g * 50f + s * 20f,   // [5]  poisonArcher right
            shrine + g * 50f - s * 20f,   // [6]  poisonArcher left
            shrine + g * 44f,             // [7]  poisonArcher center
            shrine + g * 30f + s * 10f,   // [8]  warchief right
            shrine + g * 30f - s * 10f,   // [9]  warchief left
            shrine + g * 40f + s * 8f,    // [10] spearman right
            shrine + g * 40f - s * 8f,    // [11] spearman left
            shrine + g * 34f,             // [12] spearman center
            shrine + g * 24f + s * 20f,   // [13] wolf right
            shrine + g * 24f - s * 20f,   // [14] wolf left
            shrine + g * 18f + s * 28f,   // [15] wolf far-right
            shrine + g * 14f + s * 4f,    // [16] necromancer right
            shrine + g * 14f - s * 4f,    // [17] necromancer left
            shrine + g * 42f + s * 14f,   // [18] shaman right
            shrine + g * 42f - s * 14f,   // [19] shaman left
            shrine + g * 46f + s * 28f,   // [20] tank right
            shrine + g * 46f - s * 28f,   // [21] tank left
            shrine + g * 54f + s * 12f,   // [22] warrior right
            shrine + g * 54f - s * 12f,   // [23] warrior left
            shrine + g * 58f,             // [24] warrior front
        };
    }

    private Vector3[] GetZoneSpawnPositions(int zone)
    {
        switch (zone)
        {
            case 1: return GetGatePositions();
            case 2: return GetCenterPositions();
            case 3: return GetPortalPositions();
            case 4: return GetSurgePositions();
            default: return new Vector3[] { L0OrcArenaConfig.BaseCenter };
        }
    }

    // Z1(12): poisonArcher shaman warrior warrior archer archer warrior berserker berserker goblin warrior tank  — 42% ranged (5/12)
    // Z2(20): warrior warrior warrior warrior poisonArcher poisonArcher archer archer archer archer berserker berserker spearman spearman tank tank shaman shaman wolf goblin  — 45% ranged (9/20)
    // Z3(35): poisonArcher shaman warrior warrior tank poisonArcher poisonArcher poisonArcher poisonArcher spearman spearman spearman spearman shaman shaman shaman necromancer necromancer berserker berserker berserker berserker tank tank boss boss wolf wolf wolf goblin goblin goblin warchief poisonArcher poisonArcher  — 46% ranged (16/35)
    // Z4(25): colossus berserker berserker shaman berserker poisonArcher poisonArcher poisonArcher warchief warchief spearman spearman spearman wolf wolf wolf necromancer necromancer shaman shaman tank tank poisonArcher poisonArcher goblin  — 44% ranged (11/25)

    private string[] GetZoneOrcTypes(int zone)
    {
        switch (zone)
        {
            case 1: return new string[]
            {
                "poisonArcher", "shaman",           // wide flanks — crossfire on entry
                "warrior", "warrior",
                "archer", "archer",
                "warrior",
                "berserker", "berserker",
                "goblin",                           // rear harassment
                "warrior", "tank"
            };
            case 2: return new string[]
            {
                "warrior", "warrior", "warrior", "warrior",
                "poisonArcher", "poisonArcher",     // wide flanks — mobile retreat shooters
                "archer", "archer", "archer", "archer",
                "berserker", "berserker",
                "spearman", "spearman",
                "tank", "tank",
                "shaman", "shaman",
                "wolf", "goblin"
            };
            case 3: return new string[]
            {
                "poisonArcher", "shaman",           // entrance fire — immediate pressure
                "warrior", "warrior",
                "tank",
                "poisonArcher", "poisonArcher", "poisonArcher", "poisonArcher",
                "spearman", "spearman", "spearman", "spearman",
                "shaman", "shaman", "shaman",
                "necromancer", "necromancer",
                "berserker", "berserker", "berserker", "berserker",
                "tank", "tank",
                "boss", "boss",
                "wolf", "wolf", "wolf",
                "goblin", "goblin", "goblin",
                "warchief",
                "poisonArcher", "poisonArcher"
            };
            case 4: return new string[]
            {
                "colossus",
                "berserker", "berserker",
                "shaman",                           // caster mixed into melee wave
                "berserker",
                "poisonArcher", "poisonArcher", "poisonArcher",
                "warchief", "warchief",
                "spearman", "spearman", "spearman",
                "wolf", "wolf", "wolf",
                "necromancer", "necromancer",
                "shaman", "shaman",
                "tank", "tank",
                "poisonArcher", "poisonArcher",     // rear flanks — sniper pressure
                "goblin"                            // fast harasser closing the wave
            };
            default: return new string[] { "warrior" };
        }
    }

    // ════════════════════════ extra orc registration ════════════════════════

    /// <summary>
    /// Вызывается из OrcSpecialAbility.RaiseDead() — регистрирует призванного зомби как живого орка.
    /// </summary>
    public void RegisterExtraOrc(EnemyAI ai)
    {
        if (ai == null) return;
        allOrcs.Add(ai);
        ai.OnNavMeshReady();
    }

    // ════════════════════════ fight complete ════════════════════════

    private void CompleteFight()
    {
        if (fightCleared) return;

        fightCleared = true;
        progress.MarkPortalGuardsDefeated();
        progress.MarkOrcVillageCleared();

        Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
        if (flow != null) flow.MarkOrcVillageCleared();

        L0StoryAudio.Ensure().PlayTask();
        ShowBothMessages("Логово очищено. Путь к порталу открыт!", 6f);

        Level0ExitPortal exit = FindFirstObjectByType<Level0ExitPortal>();
        Transform target = exit != null ? exit.transform : GetObjectiveTarget();
        L0ObjectiveGuide.Ensure().SetObjective("Путь к порталу открыт", target);

        Debug.Log("[OrcVillage] Fight complete! All zones cleared.");
    }

    // ════════════════════════ waypoints ════════════════════════

    private Transform[] CreateWaypoints(int index, Vector3 spawnPos, int zone)
    {
        Vector3 center = GetZoneAnchor(zone);
        int zoneSize = GetZoneSpawnPositions(zone).Length;
        Transform[] pts = new Transform[4];

        float goldenAngle = 2.39996f; // ≈ 137.5° in radians — golden angle for even spread
        float baseAngle = index * goldenAngle;

        for (int i = 0; i < 4; i++)
        {
            GameObject wp = new GameObject("WP_Z" + zone + "_" + index + "_" + i);
            wp.transform.SetParent(fightRoot);

            float angle = baseAngle + i * Mathf.PI * 0.55f;
            float r = 12f + i * 5f + (index % 3) * 3f;
            wp.transform.position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * r;
            pts[i] = wp.transform;
        }
        pts[0].position = spawnPos;
        return pts;
    }

    // ════════════════════════ tracking ════════════════════════

    private int CountAliveOrcs()
    {
        int alive = 0;
        foreach (EnemyAI ai in allOrcs)
            if (ai != null && ai.state != EnemyAI.State.Dead)
                alive++;
        return alive;
    }

    private void CleanDeadFromList()
    {
        for (int i = allOrcs.Count - 1; i >= 0; i--)
            if (allOrcs[i] == null)
                allOrcs.RemoveAt(i);
    }

    // ════════════════════════ helpers ════════════════════════

    private Vector3 GetBaseCenter()
    {
        if (orcBaseAnchor != null) return orcBaseAnchor.position;
        return L0OrcArenaConfig.BaseCenter;
    }

    public Transform GetObjectiveTarget()
    {
        return orcBaseAnchor != null ? orcBaseAnchor : transform;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null) return;
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
    }

    private void ShowBothMessages(string text, float duration)
    {
        GameManager.Instance?.ShowMessage(text, GameManager.MsgType.Damage, duration);
        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage(text, duration);
    }

    // ════════════════════════ HUD ════════════════════════

    private void InitStyles()
    {
        if (panelStyle != null) return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.padding = new RectOffset(10, 10, 8, 8);

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(1f, 0.72f, 0.18f);

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 13;
        textStyle.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        if (!showHud || !fightStarted || fightCleared) return;

        InitStyles();
        int alive = CountAliveOrcs();

        GUILayout.BeginArea(new Rect(Screen.width - 285f, 122f, 265f, 90f), panelStyle);
        GUILayout.Label("ЛОГОВО ОРКОВ", titleStyle);
        if (currentZone > 0)
            GUILayout.Label("Зона: " + ZONE_NAMES[currentZone] + " (" + currentZone + "/4)", textStyle);
        GUILayout.Label("Врагов: " + alive, textStyle);
        GUILayout.EndArea();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.05f, 0.35f);
        Gizmos.DrawWireSphere(L0OrcArenaConfig.BaseEntrance, GATE_ZONE_RADIUS);
        Gizmos.color = new Color(1f, 0.65f, 0.05f, 0.3f);
        Gizmos.DrawWireSphere(L0OrcArenaConfig.BaseCenter, CENTER_ZONE_RADIUS);
        Gizmos.color = new Color(0.6f, 0.1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(
            Vector3.Lerp(L0OrcArenaConfig.BaseCenter, L0OrcArenaConfig.PortalShrine, 0.6f),
            PORTAL_ZONE_RADIUS);
        Gizmos.color = new Color(1f, 0.05f, 0.05f, 0.35f);
        Gizmos.DrawWireSphere(
            L0OrcArenaConfig.PortalShrine + L0OrcArenaConfig.GateDir * 28f,
            SURGE_ZONE_RADIUS);
    }
}
