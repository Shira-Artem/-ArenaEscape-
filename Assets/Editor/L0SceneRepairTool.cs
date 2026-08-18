#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class L0SceneRepairTool
{
    [MenuItem("Tools/ArenaEscape/Level0/Repair Story Scene")]
    public static void RepairStoryScene()
    {
        RemoveMissingScriptsInOpenScene();

        GameObject castleGenerator = EnsureObject("CastleGenerator");
        EnsureComponentByName(castleGenerator, "CastleGenerator");

        GameObject builder = EnsureObject("LEVEL0_CASTLE_BUILDER");
        builder.transform.position = Vector3.zero;

        CastlePrologueBuilder prologue = EnsureComponent<CastlePrologueBuilder>(builder);
        prologue.buildOnStart = true;
        prologue.clearBeforeBuild = true;
        prologue.resetProgressOnStart = true;
        prologue.addLevel0Setup = true;
        prologue.lockWeaponsUntilFound = true;
        prologue.autoStartGateRaidAfterIntro = true;

        Level0CastleSetup setup = EnsureComponent<Level0CastleSetup>(builder);
        setup.setupOnStart = false;
        setup.playerStartPosition = new Vector3(0f, 1.2f, -34f);
        setup.cameraLocalPosition = new Vector3(0f, 1.62f, 0f);
        setup.disableHeadBob = true;

        Level0GameFlow flow = EnsureComponent<Level0GameFlow>(builder);
        flow.showFlowHud = false;
        flow.enableDebugKeys = false;

        Level0OrcRaidManager raid = EnsureComponent<Level0OrcRaidManager>(builder);
        raid.enabled = true;
        raid.showRaidHud = false;
        raid.totalOrcs = 7;
        raid.wave1Orcs = 3;
        raid.wave2Orcs = 4;
        raid.wave2Delay = 2.5f;
        raid.killsBeforeGateOpens = 3;
        raid.allyCount = 6;
        raid.openGateMidRaid = true;
        raid.spawnAlliesWhenGateOpens = true;
        raid.includeArchers = false;
        raid.includeBerserker = false;
        raid.buildNavMeshBeforeRaid = false;
        raid.spawnInterval = 0.35f;
        raid.maxAliveAtOnce = 4;
        raid.showDebugSpawnMarkers = false;
        raid.spawnRaidFromDistance = true;
        raid.raidSpawnDistanceFromGate = 35f;
        raid.raidSpawnSpread = 8f;

        GameObject alliesObj = EnsureObject("L0_GATE_ALLIES");
        L0GateAllies allies = EnsureComponent<L0GateAllies>(alliesObj);
        allies.allyCount = 6;

        Level0OrcVillageManager village = EnsureComponent<Level0OrcVillageManager>(builder);
        village.enabled = true;
        village.showHud = false;
        village.activationRadius = 38f;
        village.maxAliveOrcs = 5;
        village.spawnInterval = 0.8f;
        village.roadMessageDistance = 85f;

        GameObject gateObj = EnsureObject("LEVEL0_EXIT_GATE");
        gateObj.transform.position = new Vector3(0f, 2.05f, -16.25f);

        CastleIntroGate gate = EnsureComponent<CastleIntroGate>(gateObj);
        gate.autoPlaceAtCastleFrontGate = true;
        gate.castleFrontGatePosition = new Vector3(0f, 2.05f, -16.25f);
        gate.createVisibleBlocker = true;
        gate.showDebugBarrier = false;
        gate.loadNextScene = false;

        GameObject guideObj = EnsureObject("L0_OBJECTIVE_GUIDE");
        L0ObjectiveGuide guide = EnsureComponent<L0ObjectiveGuide>(guideObj);
        guide.drawScreenMarker = true;
        guide.createMarker = false;

        GameObject hudObj = EnsureObject("L0_HUD_CLEANER");
        L0HudCleaner cleaner = EnsureComponent<L0HudCleaner>(hudObj);
        cleaner.cleanOnStart = true;
        cleaner.keepGameManagerMessages = true;
        cleaner.hideGameManagerHud = true;
        cleaner.hidePlayerHealthHud = true;
        cleaner.ensureLevel0PlayerHud = true;
        cleaner.hideStoryWorldLabels = true;

        GameObject playerHudObj = EnsureObject("L0_PLAYER_HUD");
        L0PlayerHud playerHud = EnsureComponent<L0PlayerHud>(playerHudObj);
        playerHud.showHud = true;

        GameObject audioObj = EnsureObject("L0_STORY_AUDIO");
        EnsureComponent<L0StoryAudio>(audioObj);

        GameObject exitObj = EnsureObject("LEVEL0_FINAL_EXIT");
        exitObj.transform.position = L0OrcArenaConfig.PortalShrine + Vector3.up * 0.75f;

        LevelTransitionPortal transition = EnsureComponent<LevelTransitionPortal>(exitObj);
        transition.nextSceneName = "two";
        transition.loadSceneOnUse = false;
        transition.requireUnlock = true;
        transition.isUnlocked = false;
        transition.interactionRadius = 4f;
        transition.createVisualMarker = true;

        Level0ExitPortal exit = EnsureComponent<Level0ExitPortal>(exitObj);
        exit.nextSceneName = "two";
        exit.loadNextScene = true;
        exit.requireOrcVillageCleared = true;
        exit.debugAllowAfterGateRaidCleared = false;
        exit.loadSceneWhenCompleted = true;
        exit.hideUntilUnlocked = true;

        GameObject quickExitObj = EnsureObject("LEVEL0_QUICK_EXIT_TO_ONE");
        quickExitObj.transform.position = new Vector3(10f, 1f, -178f);

        LevelTransitionPortal quickTransition = EnsureComponent<LevelTransitionPortal>(quickExitObj);
        quickTransition.nextSceneName = "two";
        quickTransition.loadSceneOnUse = false;
        quickTransition.requireUnlock = true;
        quickTransition.interactionRadius = 5f;
        quickTransition.createVisualMarker = false;

        Level0ExitPortal quickExit = EnsureComponent<Level0ExitPortal>(quickExitObj);
        quickExit.nextSceneName = "two";
        quickExit.loadNextScene = true;
        quickExit.requireOrcVillageCleared = false;
        quickExit.debugAllowAfterGateRaidCleared = true;
        quickExit.hideUntilUnlocked = true;

        L0QuickExitToOne quickExitHelper = EnsureComponent<L0QuickExitToOne>(quickExitObj);
        quickExitHelper.nextSceneName = "two";
        quickExitHelper.debugAlwaysShowPortal = false;
        quickExitHelper.fallbackPosition = new Vector3(10f, 1f, -178f);
        quickExitHelper.interactionRadius = 5f;
        quickExitHelper.buildTemporaryRoad = false;
        quickExitHelper.roadWidth = 10f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[L0SceneRepairTool] Level0 scene repaired.");
        EditorUtility.DisplayDialog("Level0 repaired", "Готово. Запусти Play и проверь управление/жителей/сюжет.", "OK");
    }

    [MenuItem("Tools/ArenaEscape/Level0/Remove Missing Scripts")]
    public static void RemoveMissingScriptsInOpenScene()
    {
        int removed = 0;
        GameObject[] objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in objects)
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[L0SceneRepairTool] Removed missing scripts: " + removed);
        }
    }

    private static GameObject EnsureObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
            obj = new GameObject(name);
        return obj;
    }

    private static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
            component = obj.AddComponent<T>();
        return component;
    }

    private static Component EnsureComponentByName(GameObject obj, string typeName)
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return new Type[0]; }
            })
            .FirstOrDefault(t => t.Name == typeName);

        if (type == null)
        {
            Debug.LogWarning("[L0SceneRepairTool] Type not found: " + typeName);
            return null;
        }

        Component component = obj.GetComponent(type);
        if (component == null)
            component = obj.AddComponent(type);

        return component;
    }
}
#endif
