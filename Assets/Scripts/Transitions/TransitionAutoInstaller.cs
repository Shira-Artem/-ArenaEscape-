using UnityEngine;

/// <summary>
/// Быстрый установщик перехода.
/// Вешаешь на Empty Object — получаешь мост/дорогу и портал в конце.
/// </summary>
public class TransitionAutoInstaller : MonoBehaviour
{
    [Header("Transition")]
    public string transitionName = "Путь к деревне";
    public string nextSceneName = "one";
    public BridgeStyle style = BridgeStyle.CastleRoad;
    public Vector3 startPoint = new Vector3(0f, 0f, -17f);
    public Vector3 endPoint = new Vector3(0f, 0f, -45f);
    public bool unlockedByDefault = false;

    [Header("Bridge")]
    public float width = 5f;
    public int segmentCount = 14;
    public string signText = "К деревне";

    [Header("Portal")]
    public float portalRadius = 4f;

    private GameObject portalObject;

    private void Start()
    {
        InstallTransition();
    }

    [ContextMenu("Install Transition")]
    public void InstallTransition()
    {
        TransitionBridgeBuilder builder = GetComponent<TransitionBridgeBuilder>();

        if (builder == null)
            builder = gameObject.AddComponent<TransitionBridgeBuilder>();

        builder.style = style;
        builder.startPoint = startPoint;
        builder.endPoint = endPoint;
        builder.width = width;
        builder.segmentCount = segmentCount;
        builder.signText = signText;
        builder.buildOnStart = false;
        builder.clearBeforeBuild = true;
        builder.BuildBridge();

        if (portalObject != null)
        {
            if (Application.isPlaying)
                Destroy(portalObject);
            else
                DestroyImmediate(portalObject);
        }

        portalObject = new GameObject("Portal_" + transitionName);
        portalObject.transform.SetParent(transform, false);
        portalObject.transform.position = endPoint + Vector3.up * 0.35f;

        LevelTransitionPortal portal = portalObject.AddComponent<LevelTransitionPortal>();
        portal.nextSceneName = nextSceneName;
        portal.loadSceneOnUse = false;
        portal.requireUnlock = !unlockedByDefault;
        portal.isUnlocked = unlockedByDefault;
        portal.interactionRadius = portalRadius;
        portal.lockedMessage = transitionName + " закрыт. Сначала выполни цель зоны.";
        portal.unlockedMessage = "Путь открыт: " + transitionName;

        Debug.Log("[TransitionAutoInstaller] Создан переход: " + transitionName + " → " + nextSceneName);
    }

    public LevelTransitionPortal GetPortal()
    {
        if (portalObject == null)
            return null;

        return portalObject.GetComponent<LevelTransitionPortal>();
    }

    public void UnlockPortal()
    {
        LevelTransitionPortal portal = GetPortal();

        if (portal != null)
            portal.Unlock();
    }
}
