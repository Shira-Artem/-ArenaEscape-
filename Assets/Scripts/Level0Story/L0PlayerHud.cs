using UnityEngine;

public class L0PlayerHud : MonoBehaviour
{
    public bool showHud = true;
    public Vector2 bottomLeftOffset = new Vector2(16f, 18f);
    public Vector2 panelSize = new Vector2(210f, 48f);

    private PlayerHealth playerHealth;
    private float nextPlayerSearchTime;
    private GUIStyle labelStyle;
    private GUIStyle shadowStyle;

    public static L0PlayerHud Ensure()
    {
        L0PlayerHud hud = FindFirstObjectByType<L0PlayerHud>();
        if (hud != null)
            return hud;

        GameObject obj = new GameObject("L0_PLAYER_HUD");
        return obj.AddComponent<L0PlayerHud>();
    }

    private void Start()
    {
        FindPlayerHealth();
    }

    private void Update()
    {
        if (playerHealth == null && Time.time >= nextPlayerSearchTime)
            FindPlayerHealth();
    }

    private void FindPlayerHealth()
    {
        nextPlayerSearchTime = Time.time + 1f;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerHealth = playerObject != null ? playerObject.GetComponent<PlayerHealth>() : null;
    }

    private void InitStyles()
    {
        if (labelStyle != null)
            return;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        shadowStyle = new GUIStyle(labelStyle);
        shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.72f);
    }

    private void OnGUI()
    {
        if (!showHud)
            return;

        if (playerHealth == null)
            FindPlayerHealth();

        if (playerHealth == null)
            return;

        InitStyles();

        int maxHealth = Mathf.Max(1, playerHealth.maxHealth);
        int currentHealth = Mathf.Clamp(playerHealth.currentHealth, 0, maxHealth);
        float fill = Mathf.Clamp01((float)currentHealth / maxHealth);

        Rect panelRect = new Rect(
            bottomLeftOffset.x,
            Screen.height - bottomLeftOffset.y - panelSize.y,
            panelSize.x,
            panelSize.y);

        Color oldColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.58f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);

        Rect textRect = new Rect(panelRect.x + 10f, panelRect.y + 5f, panelRect.width - 20f, 20f);
        GUI.color = Color.white;
        GUI.Label(new Rect(textRect.x + 1f, textRect.y + 1f, textRect.width, textRect.height), "HP: " + currentHealth + " / " + maxHealth, shadowStyle);
        GUI.Label(textRect, "HP: " + currentHealth + " / " + maxHealth, labelStyle);

        Rect barBack = new Rect(panelRect.x + 10f, panelRect.y + 29f, panelRect.width - 20f, 10f);
        GUI.color = new Color(0.18f, 0.04f, 0.04f, 0.95f);
        GUI.DrawTexture(barBack, Texture2D.whiteTexture);

        Rect barFill = new Rect(barBack.x, barBack.y, barBack.width * fill, barBack.height);
        GUI.color = Color.Lerp(new Color(0.95f, 0.12f, 0.08f), new Color(0.1f, 0.95f, 0.22f), fill);
        GUI.DrawTexture(barFill, Texture2D.whiteTexture);

        GUI.color = oldColor;
    }
}
