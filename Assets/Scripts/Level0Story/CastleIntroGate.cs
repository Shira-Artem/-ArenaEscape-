using UnityEngine;
using UnityEngine.SceneManagement;

public class CastleIntroGate : MonoBehaviour
{
    [Header("Gate")]
    public string nextSceneName = "one";
    public bool loadNextScene = false;
    public float interactDistance = 5f;

    [Header("Placement")]
    public bool autoPlaceAtCastleFrontGate = true;
    public Vector3 castleFrontGatePosition = new Vector3(0f, 2.05f, -16.25f);
    public Vector3 castleFrontGateRotation = Vector3.zero;
    public Vector3 blockerSize = new Vector3(5.2f, 4.2f, 0.40f);

    [Header("Visual")]
    public bool createVisibleBlocker = true;
    public bool showDebugBarrier = false;

    private Transform player;
    private GameProgressManager progress;
    private L0GateBlocker blocker;

    private string gateMessage = "";
    private float gateMessageUntil;
    private GUIStyle messageStyle;

    private void Start()
    {
        progress = GameProgressManager.Ensure();
        EnsureFlow();

        if (autoPlaceAtCastleFrontGate)
        {
            transform.position = castleFrontGatePosition;
            transform.rotation = Quaternion.Euler(castleFrontGateRotation);
        }

        SetupBlocker();
        FindPlayer();
        RefreshGate();
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (player == null)
            FindPlayer();

        RefreshGate();

        if (player == null)
            return;

        if (Vector3.Distance(player.position, transform.position) <= interactDistance && Input.GetKeyDown(KeyCode.F))
            TryUseGate();
    }

    private void EnsureFlow()
    {
        if (FindFirstObjectByType<Level0GameFlow>() == null)
        {
            GameObject obj = new GameObject("LEVEL0_GAME_FLOW");
            obj.AddComponent<Level0GameFlow>();
        }
    }

    private void SetupBlocker()
    {
        if (!createVisibleBlocker)
            return;

        blocker = GetComponent<L0GateBlocker>();
        if (blocker == null)
            blocker = gameObject.AddComponent<L0GateBlocker>();

        blocker.size = blockerSize;
        blocker.Build();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void TryUseGate()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (!progress.IsCastleIntroCompleted())
        {
            ShowGateMessage("Ворота закрыты. Сначала помоги жителям, проверь оружие и возьми указ короля.", 4.5f);
            L0StoryAudio.Ensure().PlayBlocked();
            return;
        }

        if (!progress.castleGateRaidStarted)
        {
            Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
            if (flow != null)
                flow.StartGateRaid();

            ShowGateMessage("Тревога! Сначала отбей атаку орков у ворот.", 4f);
            return;
        }

        if (progress.castleGateOpened && !progress.castleGateRaidCleared)
        {
            ShowGateMessage("Ворота открыты. Удержи проход вместе с защитниками.", 4f);
            return;
        }

        if (!progress.castleGateRaidCleared)
        {
            ShowGateMessage("Ворота закрыты. Орки ещё у ворот. Отбей атаку.", 4f);
            L0StoryAudio.Ensure().PlayBlocked();
            return;
        }

        OpenGate();
    }

    private void OpenGate()
    {
        progress.MarkCastleGateOpened();

        Level0GameFlow flow = FindFirstObjectByType<Level0GameFlow>();
        if (flow != null)
            flow.MarkCastleGateOpened();

        UnlockGateBlocker();

        ShowGateMessage("Ворота открыты. Иди к логову орков.", 5f);
        Level0OrcVillageManager village = FindFirstObjectByType<Level0OrcVillageManager>();
        Transform target = village != null ? village.GetObjectiveTarget() : transform;
        L0ObjectiveGuide.Ensure().SetObjective(progress.GetCurrentCastleObjective(), target);

        if (loadNextScene)
            Debug.Log("[CastleIntroGate] loadNextScene is ignored on Level0. Final transition is handled by Level0ExitPortal.");
    }

    public void OpenGateFromRaid()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        progress.MarkCastleGateOpened();
        loadNextScene = false;

        UnlockGateBlocker();

        L0StoryAudio.Ensure().PlayGate();
        ShowGateMessage("Ворота открыты! Защитники выходят на помощь!", 4f);
        L0ObjectiveGuide.Ensure().SetObjective("Удержи ворота вместе с защитниками", transform);
    }

    private void UnlockGateBlocker()
    {
        if (blocker == null)
            return;

        blocker.SetLocked(false);

        Collider[] colliders = blocker.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider != null)
                collider.enabled = false;
        }

        Renderer[] renderers = blocker.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void RefreshGate()
    {
        if (blocker == null)
            return;

        bool locked = progress == null || !progress.castleGateOpened;
        blocker.SetLocked(locked);
    }

    private void ShowGateMessage(string text, float duration)
    {
        gateMessage = text;
        gateMessageUntil = Time.time + duration;
        Debug.Log("[CastleIntroGate] " + text);

        if (CastlePrologueBuilder.Instance != null)
            CastlePrologueBuilder.Instance.ShowMessage(text, duration);
    }

    private void OnGUI()
    {
        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.box);
            messageStyle.fontSize = 16;
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.wordWrap = true;
        }

        if (player != null && Vector3.Distance(player.position, transform.position) <= interactDistance)
        {
            string prompt = progress != null && progress.castleGateOpened && !progress.castleGateRaidCleared
                ? "Ворота открыты — удержи проход"
                : progress != null && progress.castleGateRaidCleared
                ? "F — открыть ворота"
                : "F — проверить ворота";

            GUI.Box(new Rect(Screen.width / 2f - 230f, Screen.height - 70f, 460f, 36f), prompt);
        }

        if (!string.IsNullOrEmpty(gateMessage) && Time.time < gateMessageUntil)
            GUI.Box(new Rect(Screen.width / 2f - 350f, Screen.height - 220f, 700f, 44f), gateMessage, messageStyle);
    }
}
