using UnityEngine;

public class Level0GameFlow : MonoBehaviour
{
    public static Level0GameFlow Instance { get; private set; }

    public bool autoStartGateRaidAfterPrologue = true;
    public bool showFlowHud = false;
    public bool enableDebugKeys = false;
    public KeyCode debugClearGateRaidKey = KeyCode.F6;
    public KeyCode debugClearOrcVillageKey = KeyCode.F7;

    private GameProgressManager progress;
    private string message;
    private float messageUntil;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        progress = GameProgressManager.Ensure();
        EnsureOrcVillageManager();
        EnsureExitPortal();
    }

    private void EnsureOrcVillageManager()
    {
        if (FindFirstObjectByType<Level0OrcVillageManager>() != null)
            return;

        GameObject obj = new GameObject("L0_ORC_VILLAGE_MANAGER");
        obj.AddComponent<Level0OrcVillageManager>();
        Debug.Log("[Level0GameFlow] Created Level0OrcVillageManager at runtime");
    }

    private void EnsureExitPortal()
    {
        if (FindFirstObjectByType<Level0ExitPortal>() != null)
            return;

        // Portal must be at PortalShrine — the far end of the orc village
        Vector3 portalPos = L0OrcArenaConfig.PortalShrine + Vector3.up * 0.75f;

        GameObject obj = new GameObject("LEVEL0_FINAL_EXIT");
        obj.transform.position = portalPos;

        LevelTransitionPortal transition = obj.AddComponent<LevelTransitionPortal>();
        transition.nextSceneName = "two";
        transition.loadSceneOnUse = false;
        transition.requireUnlock = true;
        transition.isUnlocked = false;
        transition.interactionRadius = 5f;
        transition.createVisualMarker = true;

        Level0ExitPortal exit = obj.AddComponent<Level0ExitPortal>();
        exit.nextSceneName = "two";
        exit.loadNextScene = true;
        exit.requireOrcVillageCleared = true;
        exit.hideUntilUnlocked = true;

        Debug.Log("[Level0GameFlow] Created exit portal at " + portalPos);
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (autoStartGateRaidAfterPrologue && progress.IsCastleIntroCompleted() && !progress.castleGateRaidStarted)
            StartGateRaid();

        if (enableDebugKeys)
        {
            if (Input.GetKeyDown(debugClearGateRaidKey))
                MarkGateRaidCleared();

            if (Input.GetKeyDown(debugClearOrcVillageKey))
                MarkOrcVillageCleared();
        }
    }

    public void StartGateRaid()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (progress.castleGateRaidStarted)
            return;

        progress.StartGateRaid();

        L0StoryAudio.Ensure().PlayHorn();

        ShowMessage("Тревога! Убей первых орков у ворот!", 6f);
        Level0OrcRaidManager raid = FindFirstObjectByType<Level0OrcRaidManager>();
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), raid != null ? raid.transform : transform);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage("Тревога! Убей первых орков у ворот!", 6f);
    }

    public void MarkCastleGateOpenedMidRaid()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.MarkCastleGateOpened();

        ShowMessage("Удержи ворота вместе с защитниками.", 6f);
        CastleIntroGate gate = FindFirstObjectByType<CastleIntroGate>();
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), gate != null ? gate.transform : transform);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage("Ворота открыты. Защитники помогают удержать проход.", 6f);
    }

    public void MarkGateRaidCleared()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.MarkGateRaidCleared();
        L0StoryAudio.Ensure().PlayTask();

        ShowMessage(progress.castleGateOpened ? "Мы удержим замок. Иди к логову орков." : "Атака отбита. Теперь проверь ворота.", 6f);
        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        CastleIntroGate gate = FindFirstObjectByType<CastleIntroGate>();
        Transform target = progress.castleGateOpened && village != null ? village.GetObjectiveTarget() : (gate != null ? gate.transform : transform);
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), target);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage(progress.castleGateOpened ? "Мы удержим замок. Иди к логову орков." : "Атака отбита. Теперь проверь ворота.", 6f);
    }

    public void MarkCastleGateOpened()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.MarkCastleGateOpened();
        L0StoryAudio.Ensure().PlayGate();

        ShowMessage(progress.castleGateRaidCleared ? "Иди к логову орков." : "Удержи ворота вместе с защитниками.", 6f);
        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        CastleIntroGate gate = FindFirstObjectByType<CastleIntroGate>();
        Transform target = progress.castleGateRaidCleared && village != null ? village.GetObjectiveTarget() : (gate != null ? gate.transform : transform);
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), target);
    }

    public void StartOrcVillageFight()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.StartOrcVillageFight();
        ShowMessage("Орочья деревня впереди. Зачисти её.", 6f);
        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), village != null ? village.GetObjectiveTarget() : transform);
    }

    public void MarkOrcVillageCleared()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.MarkOrcVillageCleared();
        ShowMessage("Орочья деревня очищена. Путь дальше открыт.", 7f);
        Level0ExitPortal exit = FindFirstObjectByType<Level0ExitPortal>();
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), exit != null ? exit.transform : transform);
    }

    public void ShowMessage(string text, float duration)
    {
        message = text;
        messageUntil = Time.time + duration;
        Debug.Log("[Level0GameFlow] " + text);
    }

    private void OnGUI()
    {
        if (!showFlowHud)
            return;

        if (!string.IsNullOrEmpty(message) && Time.time < messageUntil)
            GUI.Box(new Rect(Screen.width / 2f - 330f, Screen.height - 180f, 660f, 42f), message);
    }
}
