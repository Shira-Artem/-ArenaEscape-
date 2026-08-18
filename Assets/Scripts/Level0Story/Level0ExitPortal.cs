using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelTransitionPortal))]
public class Level0ExitPortal : MonoBehaviour
{
    public string nextSceneName = "one";
    public bool loadNextScene = true;
    public bool requireOrcVillageCleared = true;
    public bool debugAllowAfterGateRaidCleared = false;

    [Header("Legacy")]
    public bool unlockAfterOrcVillageCleared = true;
    public bool loadSceneWhenCompleted = true;

    [Header("UI")]
    public bool hideUntilUnlocked = true;
    public string lockedMessage = "Выход закрыт. Сначала зачисти орочью базу.";
    public string unlockedMessage = "F — перейти на следующий уровень";

    private LevelTransitionPortal portal;
    private GameProgressManager progress;
    private Transform playerTransform;
    private bool wasUnlocked;

    private void Awake()
    {
        portal = GetComponent<LevelTransitionPortal>();
    }

    private void Start()
    {
        progress = GameProgressManager.Ensure();
        ConfigurePortal();
        RefreshPortalState();
    }

    private void Update()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        RefreshPortalState();
        HandleInteraction();
    }

    private void ConfigurePortal()
    {
        if (portal == null)
            portal = GetComponent<LevelTransitionPortal>();

        if (portal == null)
            return;

        portal.nextSceneName = nextSceneName;
        portal.loadSceneOnUse = false;
        portal.requireUnlock = true;
        portal.lockedMessage = lockedMessage;
        portal.unlockedMessage = unlockedMessage;

        // Level0ExitPortal owns the interaction so it can validate Build Settings before loading.
        portal.enabled = false;
    }

    private void RefreshPortalState()
    {
        if (portal == null)
            ConfigurePortal();

        bool shouldUnlock = IsPortalUnlocked();

        if (portal != null)
            portal.SetUnlocked(shouldUnlock);

        if (shouldUnlock)
        {
            SetPortalVisuals(true);
            wasUnlocked = true;
            return;
        }

        if (portal != null)
            portal.Lock();

        SetPortalVisuals(!hideUntilUnlocked);
        wasUnlocked = false;
    }

    private bool IsPortalUnlocked()
    {
        if (progress == null)
            return false;

        bool normalExitUnlocked = requireOrcVillageCleared
            && (progress.orcVillageCleared || progress.level0ExitUnlocked);

        bool quickExitUnlocked = debugAllowAfterGateRaidCleared
            && progress.castleGateRaidCleared;

        bool unrestrictedExit = !requireOrcVillageCleared
            && !debugAllowAfterGateRaidCleared;

        unlockAfterOrcVillageCleared = requireOrcVillageCleared;
        loadSceneWhenCompleted = loadNextScene;

        return normalExitUnlocked || quickExitUnlocked || unrestrictedExit;
    }

    private void HandleInteraction()
    {
        if (!wasUnlocked)
            return;

        FindPlayerIfNeeded();
        if (playerTransform == null)
            return;

        float interactionRadius = portal != null ? portal.interactionRadius : 4f;
        if (Vector3.Distance(transform.position, playerTransform.position) > interactionRadius)
            return;

        TransitionHintUI.Show("F — перейти на следующий уровень", 0.08f);

        if (Input.GetKeyDown(KeyCode.F))
            CompleteLevel0();
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void SetPortalVisuals(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider != null)
                collider.enabled = visible;
        }
    }

    public void CompleteLevel0()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (!IsPortalUnlocked())
            return;

        if (progress != null)
            progress.MarkLevel0Completed();

        L0ObjectiveGuide.Ensure().SetObjective(
            progress != null ? progress.GetCurrentCastleObjective() : "Уровень 0 завершён",
            transform);

        if (!loadNextScene)
        {
            TransitionHintUI.Show("Уровень 0 завершён", 3f);
            GameManager.Instance?.ShowMessage("Уровень 0 завершён", GameManager.MsgType.Kill, 3f);
            return;
        }

        if (RunScoreManager.Instance != null)
            RunScoreManager.Instance.Data.level0Cleared = true;

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("[Level0ExitPortal] Scene is not available in Build Settings: " + nextSceneName);
            TransitionHintUI.Show("Сцена " + nextSceneName + " недоступна в Build Settings", 3f);
            return;
        }

        SceneFader.LoadScene(nextSceneName);
    }

    [ContextMenu("Force Unlock Exit")]
    public void ForceUnlock()
    {
        if (progress == null)
            progress = GameProgressManager.Ensure();

        if (progress != null)
            progress.MarkOrcVillageCleared();

        RefreshPortalState();
    }
}
