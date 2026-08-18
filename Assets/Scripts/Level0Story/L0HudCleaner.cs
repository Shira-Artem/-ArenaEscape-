using UnityEngine;

public class L0HudCleaner : MonoBehaviour
{
    public bool cleanOnStart = true;
    public bool keepGameManagerMessages = true;
    public bool hideGameManagerHud = false;
    public bool hidePlayerHealthHud = false;
    public bool ensureLevel0PlayerHud = false;
    public bool hideStoryWorldLabels = true;
    public bool hideAllWorldSpaceTextMeshes = false;

    private void Start()
    {
        if (cleanOnStart)
            Apply();
    }

    [ContextMenu("Apply Level0 HUD Cleanup")]
    public void Apply()
    {
        if (hideGameManagerHud)
        {
            GameManager[] managers = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameManager manager in managers)
            {
                if (manager != null)
                    manager.suppressLegacyHud = true;
            }
        }

        if (hidePlayerHealthHud)
        {
            PlayerHealth[] healthComponents = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (PlayerHealth health in healthComponents)
            {
                if (health != null)
                    health.suppressLegacyHud = true;
            }
        }

        if (!keepGameManagerMessages && GameManager.Instance != null)
            GameManager.Instance.enabled = false;

        if (ensureLevel0PlayerHud)
            L0PlayerHud.Ensure();

        if (hideStoryWorldLabels)
            HideStoryLabels();

        if (hideAllWorldSpaceTextMeshes)
            HideAllTextMeshes();
    }

    private void HideStoryLabels()
    {
        L0StoryLabel[] labels = FindObjectsByType<L0StoryLabel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (L0StoryLabel label in labels)
        {
            if (label == null)
                continue;

            TextMesh mesh = label.GetComponent<TextMesh>();
            SetTextMeshVisible(mesh, false);

            Renderer renderer = label.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void HideAllTextMeshes()
    {
        TextMesh[] meshes = FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMesh mesh in meshes)
        {
            if (mesh == null)
                continue;

            SetTextMeshVisible(mesh, false);

            Renderer renderer = mesh.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void SetTextMeshVisible(TextMesh textMesh, bool visible)
    {
        if (textMesh == null)
            return;

        Renderer renderer = textMesh.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = visible;
    }

    [ContextMenu("Restore Legacy HUD")]
    public void Restore()
    {
        GameManager[] managers = FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameManager manager in managers)
        {
            if (manager != null)
            {
                manager.enabled = true;
                manager.suppressLegacyHud = false;
            }
        }

        PlayerHealth[] healthComponents = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PlayerHealth health in healthComponents)
        {
            if (health != null)
                health.suppressLegacyHud = false;
        }

        L0StoryLabel[] labels = FindObjectsByType<L0StoryLabel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (L0StoryLabel label in labels)
        {
            if (label == null)
                continue;

            TextMesh mesh = label.GetComponent<TextMesh>();
            SetTextMeshVisible(mesh, true);

            Renderer renderer = label.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = true;
        }
    }
}
