using UnityEngine;

public class L0ObjectiveGuide : MonoBehaviour
{
    [Header("Objective")]
    public bool useGameProgressObjective = true;
    public string objectiveOverride = "";
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 2.4f, 0f);

    [Header("Marker")]
    public bool drawScreenMarker = true;
    public string markerSymbol = "◆";
    public Vector2 markerScreenOffset = new Vector2(0f, -34f);
    public bool createMarker = false;
    public float markerBobHeight = 0.25f;
    public float markerBobSpeed = 2.2f;
    public Color markerColor = new Color(1f, 0.82f, 0.16f, 0.9f);

    private GameObject marker;
    private GUIStyle objectiveStyle;
    private GUIStyle shadowStyle;
    private GUIStyle markerStyle;

    public static L0ObjectiveGuide Ensure()
    {
        L0ObjectiveGuide guide = FindFirstObjectByType<L0ObjectiveGuide>();
        if (guide != null)
            return guide;

        GameObject obj = new GameObject("L0_OBJECTIVE_GUIDE");
        return obj.AddComponent<L0ObjectiveGuide>();
    }

    private void Start()
    {
        GameProgressManager.Ensure();

        if (createMarker)
            EnsureMarker();
    }

    private void Update()
    {
        if (createMarker)
            UpdateMarker();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetObjective(string text)
    {
        objectiveOverride = text;
        useGameProgressObjective = string.IsNullOrEmpty(text);
    }

    public void SetObjective(string text, Transform newTarget)
    {
        objectiveOverride = text;
        target = newTarget;
        useGameProgressObjective = string.IsNullOrEmpty(text);
    }

    public void ClearObjectiveOverride()
    {
        objectiveOverride = "";
        useGameProgressObjective = true;
    }

    private string GetObjectiveText()
    {
        if (!useGameProgressObjective && !string.IsNullOrEmpty(objectiveOverride))
            return objectiveOverride;

        GameProgressManager progress = GameProgressManager.Instance;
        if (progress == null)
            progress = GameProgressManager.Ensure();

        return progress != null ? progress.GetCurrentCastleObjective() : "Найди следующую цель";
    }

    private void EnsureMarker()
    {
        if (marker != null)
            return;

        marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "L0_Objective_Marker";
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * 0.35f;

        Collider col = marker.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = markerColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", markerColor * 1.4f);
            renderer.material = mat;
        }

        marker.SetActive(false);
    }

    private void UpdateMarker()
    {
        EnsureMarker();

        if (marker == null)
            return;

        if (target == null)
        {
            marker.SetActive(false);
            return;
        }

        marker.SetActive(true);
        Vector3 bob = Vector3.up * (Mathf.Sin(Time.time * markerBobSpeed) * markerBobHeight);
        marker.transform.position = target.position + targetOffset + bob;
        marker.transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
    }

    private void InitStyles()
    {
        if (objectiveStyle != null)
            return;

        objectiveStyle = new GUIStyle(GUI.skin.label);
        objectiveStyle.fontSize = 20;
        objectiveStyle.fontStyle = FontStyle.Bold;
        objectiveStyle.alignment = TextAnchor.MiddleCenter;
        objectiveStyle.wordWrap = true;
        objectiveStyle.normal.textColor = new Color(1f, 0.86f, 0.20f);

        shadowStyle = new GUIStyle(objectiveStyle);
        shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.75f);

        markerStyle = new GUIStyle(GUI.skin.label);
        markerStyle.fontSize = 34;
        markerStyle.fontStyle = FontStyle.Bold;
        markerStyle.alignment = TextAnchor.MiddleCenter;
        markerStyle.normal.textColor = markerColor;
    }

    private void OnGUI()
    {
        InitStyles();

        string text = GetObjectiveText();

        // R4: во время фазы удержания ворот показываем ТОЛЬКО шкалу L0GateDefense —
        // верхнюю строку цели не рисуем, чтобы убрать наложение HUD-ов.
        bool holdHud = L0GateDefense.Instance != null && L0GateDefense.Instance.HoldPhaseActive;

        if (!holdHud && !string.IsNullOrEmpty(text))
        {
            float width = Mathf.Min(760f, Screen.width - 40f);
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, 16f, width, 54f);
            Rect shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);

            GUI.Label(shadowRect, text, shadowStyle);
            GUI.Label(rect, text, objectiveStyle);
        }

        DrawTargetMarker();
    }

    private void DrawTargetMarker()
    {
        if (!drawScreenMarker || target == null || Camera.main == null)
            return;

        Vector3 world = target.position + targetOffset;
        Vector3 screen = Camera.main.WorldToScreenPoint(world);
        if (screen.z <= 0f)
            return;

        float size = 46f;
        Rect markerRect = new Rect(
            screen.x - size * 0.5f + markerScreenOffset.x,
            Screen.height - screen.y - size * 0.5f + markerScreenOffset.y,
            size,
            size);

        GUI.Label(markerRect, markerSymbol, markerStyle);
    }
}
