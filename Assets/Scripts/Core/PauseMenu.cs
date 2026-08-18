using UnityEngine;
using UnityEngine.SceneManagement;

/// Простое статичное меню паузы — без сохранений, потому что игра теперь про один забег.
public class PauseMenu : MonoBehaviour
{
    bool _paused;
    Texture2D _panel;
    Texture2D _green;
    Texture2D _red;
    Texture2D _dark;

    void Awake()
    {
        _panel = MakeTex(2, 2, new Color(0.04f, 0.035f, 0.055f, 0.98f));
        _green = MakeTex(2, 2, new Color(0.15f, 0.55f, 0.20f, 1f));
        _red = MakeTex(2, 2, new Color(0.58f, 0.08f, 0.06f, 1f));
        _dark = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.78f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameWon) return;
            if (_paused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;
        MouseLook.Unlock();
    }

    public void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        MouseLook.Lock();
    }

    void Restart()
    {
        _paused = false;
        Time.timeScale = 1f;
        HitStop.CancelAll();
        SaveSystem.Delete();
        MouseLook.Unlock();

        RunScoreManager.Instance?.StartRun();

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "two")
            SceneBuilder.RestartCurrentRun();
        else
            SceneFader.LoadScene("Level0_Castl");
    }

    void GoMain()
    {
        _paused = false;
        Time.timeScale = 1f;
        SceneFader.LoadScene("MainMenu");
    }

    void OnGUI()
    {
        if (!_paused) return;

        int sw = Screen.width;
        int sh = Screen.height;
        GUI.DrawTexture(new Rect(0, 0, sw, sh), _dark);

        float w = 360f;
        float h = 340f;
        float x = sw * 0.5f - w * 0.5f;
        float y = sh * 0.5f - h * 0.5f;
        GUI.DrawTexture(new Rect(x, y, w, h), _panel);

        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(1f, 0.86f, 0.15f);
        GUI.Label(new Rect(x, y + 18, w, 46), "ПАУЗА", title);

        // Текущий score и kills
        var run = RunScoreManager.Instance?.Data;
        if (run != null)
        {
            GUIStyle stat = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            stat.normal.textColor = new Color(0.9f, 0.8f, 0.5f);
            string t = RunScoreManager.FormatTime(run.runTime);
            GUI.Label(new Rect(x, y + 64, w, 24),
                $"Очки: {run.score}   Убийства: {run.kills}   Серия: {run.bestStreak}   {t}", stat);
        }

        GUIStyle btn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        btn.normal.textColor = Color.white;
        btn.hover.textColor = Color.white;
        btn.active.textColor = Color.white;

        float btnY = y + 100;
        btn.normal.background = _green;
        btn.hover.background = _green;
        if (GUI.Button(new Rect(x + 45, btnY, w - 90, 48), "ПРОДОЛЖИТЬ", btn)) Resume();

        if (GUI.Button(new Rect(x + 45, btnY + 58, w - 90, 48), "НАЧАТЬ СНАЧАЛА", btn)) Restart();

        btn.normal.background = _red;
        btn.hover.background = _red;
        if (GUI.Button(new Rect(x + 45, btnY + 116, w - 90, 44), "ГЛАВНОЕ МЕНЮ", btn)) GoMain();
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
